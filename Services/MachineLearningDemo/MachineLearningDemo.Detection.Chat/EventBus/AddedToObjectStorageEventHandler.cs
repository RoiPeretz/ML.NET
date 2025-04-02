using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.ObjectStorage.Interfaces;
using MachineLearningDemo.Detection.Chat.Services;

namespace MachineLearningDemo.Detection.Chat.EventBus;

internal class AddedToObjectStorageEventHandler(
    IImageObjectDetectionService service,
    ILogger<AddedToObjectStorageEventHandler> logger,
    IAssetsObjectStorageClient assetsObjectStorageClient)
    : IIntegrationEventHandler<AddedToObjectStorageEvent>
{
    public async Task Handle(AddedToObjectStorageEvent @event)
    {
        var asset = await assetsObjectStorageClient.GetAssetByNameAsync(@event.FileName);

        if (asset is null)
        {
            logger.LogError("Failed to get {FileName} from object storage.", @event.FileName);
            return;
        }

        await service.Detect(@event.FileName, asset.Data, asset.ContentType);
    }
}