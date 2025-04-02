using System.Text.Json.Serialization;

namespace MachineLearningDemo.Core.EventBus.Events;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(IngestionStartedEvent), "ingestionStarted")]
[JsonDerivedType(typeof(AddedToObjectStorageEvent), "addedToObjectStorage")]
[JsonDerivedType(typeof(DetectionStartedEvent), "detectionStarted")]
[JsonDerivedType(typeof(DetectionEndedEvent), "detectionEnded")]
public record IngestionStatusEvent : IntegrationEvent
{
    [JsonInclude]
    public string FileName { get; set; } = string.Empty;
}

public record IngestionStartedEvent : IngestionStatusEvent
{
    [JsonInclude]
    public string Message { get; set; } = "Ingestion started";
}

public record AddedToObjectStorageEvent : IngestionStatusEvent
{
    [JsonInclude]
    public string Message { get; set; } = "Added to object storage";
}

public record DetectionStartedEvent : IngestionStatusEvent
{
    [JsonInclude]
    public string ModelName { get; set; } = string.Empty;

    [JsonInclude]
    public string Message { get; set; } = "Detection started";
}

public record DetectionEndedEvent : IngestionStatusEvent
{
    [JsonInclude]
    public string ModelName { get; set; } = string.Empty;

    [JsonInclude]
    public double DetectionTime { get; set; }

    [JsonInclude] 
    public string Message { get; set; } = "Detection completed";
}