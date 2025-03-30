using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace MachineLearningDemo.Infrastructure.EventBus.RabbitMq;

public sealed class RabbitMqEventBus(
    ILogger<RabbitMqEventBus> logger,
    IServiceProvider serviceProvider,
    IOptions<EventBusOptions> options,
    RabbitMQTelemetry rabbitMqTelemetry,
    IOptions<EventBusSubscriptionInfo> subscriptionOptions)
    : IEventBus, IDisposable, IHostedService
{
    private IModel? _consumerChannel;
    private IConnection? _rabbitMqConnection;
    private CancellationTokenSource? _linkedCts;
    private const string ExchangeName = "ml_event_bus";
    private readonly string _queueName = options.Value.SubscriptionClientName;
    private readonly TextMapPropagator _propagator = rabbitMqTelemetry.Propagator;
    private readonly ActivitySource _activitySource = rabbitMqTelemetry.ActivitySource;
    private readonly EventBusSubscriptionInfo _subscriptionInfo = subscriptionOptions.Value;
    private readonly ResiliencePipeline _pipeline = CreateResiliencePipeline(options.Value.RetryCount);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        _ = Task.Factory.StartNew(() =>
        {
            try
            {
                logger.LogInformation("Starting RabbitMQ connection on a background thread");

                _rabbitMqConnection = serviceProvider.GetRequiredService<IConnection>();
                if (!_rabbitMqConnection.IsOpen)
                {
                    return;
                }

                logger.LogTrace("Creating RabbitMQ consumer channel");

                _consumerChannel = _rabbitMqConnection.CreateModel();

                _consumerChannel.CallbackException += (_, ea) =>
                {
                    logger.LogWarning(ea.Exception, "Error with RabbitMQ consumer channel");
                };

                _consumerChannel.ExchangeDeclare(exchange: ExchangeName,
                                        type: "direct");

                _consumerChannel.QueueDeclare(queue: _queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                logger.LogTrace("Starting RabbitMQ basic consume");

                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += OnMessageReceived;

                _consumerChannel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);

                foreach (var (eventName, _) in _subscriptionInfo.EventTypes)
                {
                    _consumerChannel.QueueBind(
                        queue: _queueName,
                        exchange: ExchangeName,
                        routingKey: eventName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting RabbitMQ connection");
            }
        },
        _linkedCts.Token);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _linkedCts?.Cancel();
        _linkedCts?.Dispose();
        return Task.CompletedTask;
    }

    public Task PublishAsync(IntegrationEvent @event)
    {
        var routingKey = @event.GetType().Name;
        logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, routingKey);

        var channel = _rabbitMqConnection?.CreateModel() ?? throw new InvalidOperationException("RabbitMQ connection is not open");
        logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);
        
        channel.ExchangeDeclare(exchange: ExchangeName, type: "direct");
        var body = SerializeMessage(@event);

        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md
        var activityName = $"{routingKey} publish";

        return _pipeline.Execute(() =>
        {
            using var activity = _activitySource.StartActivity(activityName, ActivityKind.Client);

            ActivityContext contextToInject = default;

            if (activity != null)
            {
                contextToInject = activity.Context;
            }
            else if (Activity.Current != null)
            {
                contextToInject = Activity.Current.Context;
            }

            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;

            static void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
            {
                props.Headers ??= new Dictionary<string, object>();
                props.Headers[key] = value;
            }

            _propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), properties, InjectTraceContextIntoBasicProperties);

            if (activity is not null)
            {
                SetActivityContext(activity, routingKey, "publish");
            }

            logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

            try
            {
                channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                activity.SetExceptionTags(ex);

                throw;
            }
            finally
            {
                channel.Dispose();
            }
        });
    }

    private static void SetActivityContext(Activity? activity, string routingKey, string operation)
    {
        if (activity is null) return;
        // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
        // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md
        activity.SetTag("messaging.system", "rabbitmq");
        activity.SetTag("messaging.destination_kind", "queue");
        activity.SetTag("messaging.operation", operation);
        activity.SetTag("messaging.destination.name", routingKey);
        activity.SetTag("messaging.rabbitmq.routing_key", routingKey);
    }

    public void Dispose()
    {
        _consumerChannel?.Dispose();
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        // Extract the PropagationContext of the upstream parent from the message headers.
        var parentContext = _propagator.Extract(default, eventArgs.BasicProperties, ExtractTraceContextFromBasicProperties);
        Baggage.Current = parentContext.Baggage;

        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md
        var activityName = $"{eventArgs.RoutingKey} receive";

        using var activity = _activitySource.StartActivity(activityName, ActivityKind.Client, parentContext.ActivityContext);

        if (activity is not null)
        {
            SetActivityContext(activity, eventArgs.RoutingKey, "receive");
        }

        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            activity?.SetTag("message", message);

            if (message.Contains("throw-fake-exception", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
            }

            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error Processing message \"{Message}\"", message);

            activity.SetExceptionTags(ex);
        }

        _consumerChannel?.BasicAck(eventArgs.DeliveryTag, multiple: false);
        return;

        static IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
        {
            if (!props.Headers.TryGetValue(key, out var value)) return [];
            if (value is byte[] bytes) return [Encoding.UTF8.GetString(bytes)];
            return Array.Empty<string>();
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);
        await using var scope = serviceProvider.CreateAsyncScope();

        if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        var integrationEvent = DeserializeMessage(message, eventType);
        
        foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
        {
            if (integrationEvent is not null) await handler.Handle(integrationEvent);
        }
    }

    private IntegrationEvent? DeserializeMessage(string message, Type eventType)
    {
        return JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as IntegrationEvent;
    }

    private byte[] SerializeMessage(IntegrationEvent @event)
    {
        return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);
    }

    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<SocketException>(),
            MaxRetryAttempts = retryCount,
            DelayGenerator = context => ValueTask.FromResult(GenerateDelay(context.AttemptNumber))
        };

        return new ResiliencePipelineBuilder()
            .AddRetry(retryOptions)
            .Build();

        static TimeSpan? GenerateDelay(int attempt)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, attempt));
        }
    }
}
