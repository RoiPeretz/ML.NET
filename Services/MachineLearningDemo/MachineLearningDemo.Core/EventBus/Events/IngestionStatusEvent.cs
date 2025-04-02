namespace MachineLearningDemo.Core.EventBus.Events;

public abstract record IngestionStatusEvent : IntegrationEvent
{
    public string FileName { get; set; } = string.Empty;
}

public record IngestionStartedEvent : IngestionStatusEvent
{
    public string Message { get; set; } = "Ingestion started";
}

public record AddedToObjectStorageEvent : IngestionStatusEvent
{
    public string Message { get; set; } = "Added to object storage";
}

public record DetectionStartedEvent : IngestionStatusEvent
{
    public string ModelName { get; set; } = string.Empty;
    public string Message { get; set; } = "Detection started";
}

public record DetectionEndedEvent : IngestionStatusEvent
{
    public string ModelName { get; set; } = string.Empty;
    public string Message { get; set; } = "Detection completed";
}