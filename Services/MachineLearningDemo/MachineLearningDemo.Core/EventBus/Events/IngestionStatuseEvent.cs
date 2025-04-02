namespace MachineLearningDemo.Core.EventBus.Events;

public abstract record IngestionStatusEvent : IntegrationEvent
{
    public string FileName { get; set; } = string.Empty;
}

public record IngestionStartedEvent : IngestionStatusEvent
{ }

public record AddedToObjectStorageEvent : IngestionStatusEvent
{ }

public record DetectionStartedEvent : IngestionStatusEvent
{
    public string ModelName { get; set; } = string.Empty;
}

public record DetectionEndedEvent : IngestionStatusEvent
{
    public string ModelName { get; set; } = string.Empty;
}