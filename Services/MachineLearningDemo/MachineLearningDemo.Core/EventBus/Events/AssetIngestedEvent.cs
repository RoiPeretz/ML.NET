namespace MachineLearningDemo.Core.EventBus.Events;

public record AssetIngestedEvent : IntegrationEvent
{
    public string FileName { get; set; } = string.Empty;
}