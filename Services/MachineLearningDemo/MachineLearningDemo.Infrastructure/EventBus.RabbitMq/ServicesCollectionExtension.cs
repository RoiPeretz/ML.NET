using MachineLearningDemo.Core.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MachineLearningDemo.Infrastructure.EventBus.RabbitMq;

public static class ServicesCollectionExtension
{
    private const string SectionName = "EventBus";

    public static IEventBusBuilder AddEventBus(this IHostApplicationBuilder builder, string connectionName)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddRabbitMQClient(connectionName, configureConnectionFactory: factory =>
        {
            factory.DispatchConsumersAsync = true;
        });

        builder.Services.AddOpenTelemetry()
           .WithTracing(tracing =>
           {
               tracing.AddSource(RabbitMQTelemetry.ActivitySourceName);
           });

        builder.Services.Configure<EventBusOptions>(builder.Configuration.GetSection(SectionName));

        builder.Services.AddSingleton<RabbitMQTelemetry>();
        builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();
        builder.Services.AddSingleton<IHostedService>(sp => (RabbitMqEventBus)sp.GetRequiredService<IEventBus>());

        return new EventBusBuilder(builder.Services);
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}
