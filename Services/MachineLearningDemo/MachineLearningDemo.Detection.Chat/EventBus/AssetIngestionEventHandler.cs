using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.ObjectStorage.Interfaces;
using MachineLearningDemo.Detection.Chat.Services;

namespace MachineLearningDemo.Detection.Chat.EventBus;

internal class AssetIngestionEventHandler(
    IImageObjectDetectionService service,
    IAssetsObjectStorageClient assetsObjectStorageClient)
    : IIntegrationEventHandler<AssetIngestedEvent>
{
    public async Task Handle(AssetIngestedEvent @event)
    {
        var asset = await assetsObjectStorageClient.GetAssetByNameAsync(@event.FileName);
        await service.Detect(@event.FileName, asset?.Data);
    }
}