using MachineLearningDemo.BFF.Services;
using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;

namespace MachineLearningDemo.BFF.EventBus;

public class IngestionStatusEventHandler (
    ILogger<IngestionStatusEventHandler> logger,
    IIngestionStatusService ingestionStatusService)
    : IIntegrationEventHandler<IngestionStatusEvent>
{
    public Task Handle(IngestionStatusEvent @event)
    {
        logger.LogInformation("Handling event {Event}", @event);
        ingestionStatusService.OnIngestionEvent(@event);
        return Task.CompletedTask;
    }
}