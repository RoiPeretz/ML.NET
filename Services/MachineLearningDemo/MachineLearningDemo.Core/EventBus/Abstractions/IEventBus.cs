using MachineLearningDemo.Core.EventBus.Events;

namespace MachineLearningDemo.Core.EventBus.Abstractions;

public interface IEventBus
{
    Task PublishAsync(IntegrationEvent @event);
}
