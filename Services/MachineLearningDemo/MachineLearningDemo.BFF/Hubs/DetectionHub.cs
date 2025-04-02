using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;
using Microsoft.AspNetCore.SignalR;

namespace MachineLearningDemo.BFF.Hubs;

public class DetectionHub(
    IEventBus eventBus,
    IImageRepositoryClientGen imageRepositoryClient)
    : Hub
{
    public async Task Detect(string fileName, string base64String, string contentType)
    {
        var name = Guid.NewGuid() + Path.GetExtension(fileName);
        var @event= new IngestionStartedEvent
        {
            FileName = name,
        };

        await eventBus.PublishAsync(@event);

        var commaIndex = base64String.IndexOf(',');
        if (commaIndex != -1)
        {
            base64String = base64String[(commaIndex + 1)..];
        }

        var fileBytes = Convert.FromBase64String(base64String);
        var stream = new MemoryStream(fileBytes);

        var formFile = new FileParameter(stream, name, contentType);
        
        await imageRepositoryClient.IngestAsync(formFile);
    }
}