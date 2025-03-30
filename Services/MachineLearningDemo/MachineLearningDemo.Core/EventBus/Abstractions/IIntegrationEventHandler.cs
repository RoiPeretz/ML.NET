using MachineLearningDemo.Core.EventBus.Events;

namespace MachineLearningDemo.Core.EventBus.Abstractions;

public interface IIntegrationEventHandler
{
    Task Handle(IntegrationEvent @event);
}

public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent @event);

    Task IIntegrationEventHandler.Handle(IntegrationEvent @event) => Handle((TIntegrationEvent)@event);
}