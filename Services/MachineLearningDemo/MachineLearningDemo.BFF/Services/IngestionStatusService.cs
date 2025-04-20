using MachineLearningDemo.BFF.Hubs;
using MachineLearningDemo.Core.EventBus.Events;
using Microsoft.AspNetCore.SignalR;

namespace MachineLearningDemo.BFF.Services;

internal interface IIngestionStatusService 
{
    void OnIngestionEvent(IngestionStatusEvent @event);
    Dictionary<string, List<IngestionStatusEvent>> FileToEventsMap { get; }
}

internal class IngestionStatusService(IHubContext<DetectionHub> detectionHubContext)
    : IIngestionStatusService
{
    public Dictionary<string, List<IngestionStatusEvent>> FileToEventsMap { get; } = new();

    public void OnIngestionEvent(IngestionStatusEvent @event)
    {
        if (FileToEventsMap.ContainsKey(@event.FileName) is false)
        {
            FileToEventsMap[@event.FileName] = [];
        }

        FileToEventsMap[@event.FileName].Add(@event);

        detectionHubContext.Clients.All.SendAsync("IngestionStatusAdded", @event);
    }
}