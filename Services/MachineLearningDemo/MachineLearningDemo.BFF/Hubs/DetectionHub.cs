using MachineLearningDemo.BFF.Clients.ImageRepository;
using MachineLearningDemo.BFF.Services;
using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.Models;
using Microsoft.AspNetCore.SignalR;
using FileParameter = MachineLearningDemo.BFF.Clients.ImageRepository.FileParameter;

namespace MachineLearningDemo.BFF.Hubs;

internal class DetectionHub(
    IEventBus eventBus,
    IQueryService queryService,
    IIngestionStatusService ingestionStatusService,
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

    public async Task<IEnumerable<ImageDetectionResult>> Query(string searchTerm) => await queryService.Query(searchTerm);

    public IDictionary<string, List<IngestionStatusEvent>> GetCurrentStatus()
    {
        return ingestionStatusService.FileToEventsMap;
    }
}