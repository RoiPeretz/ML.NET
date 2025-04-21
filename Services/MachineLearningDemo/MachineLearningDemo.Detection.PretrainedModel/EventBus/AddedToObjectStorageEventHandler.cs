using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.ObjectStorage.Interfaces;
using MachineLearningDemo.Detection.PretrainedModel.Services;

namespace MachineLearningDemo.Detection.PretrainedModel.EventBus;

internal class AddedToObjectStorageEventHandler(
    IModelImageObjectDetectionService service,
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