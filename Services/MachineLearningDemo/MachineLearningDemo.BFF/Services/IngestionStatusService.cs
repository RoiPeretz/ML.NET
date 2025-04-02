using MachineLearningDemo.Core.EventBus.Events;

namespace MachineLearningDemo.BFF.Services;

public interface IIngestionStatusNotificationService
{
    event EventHandler? StatusChanged;
    Dictionary<string, List<IngestionStatusEvent>> FileToEventsMap { get; }
}

public interface IIngestionStatusService 
{
    void OnIngestionEvent(IngestionStatusEvent @event);
}

public class IngestionStatusService : IIngestionStatusNotificationService, IIngestionStatusService
{
    public event EventHandler? StatusChanged;
    public Dictionary<string, List<IngestionStatusEvent>> FileToEventsMap { get; } = new();

    public void OnIngestionEvent(IngestionStatusEvent @event)
    {
        if (FileToEventsMap.ContainsKey(@event.FileName) is false)
        {
            FileToEventsMap[@event.FileName] = [];
        }

        FileToEventsMap[@event.FileName].Add(@event);
        StatusChanged?.Invoke(this, EventArgs.Empty);
    }
}