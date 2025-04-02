using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.ObjectStorage.Interfaces;
using MachineLearningDemo.Detection.Chat.Services;

namespace MachineLearningDemo.Detection.Chat.EventBus;

internal class AddedToObjectStorageEventHandler(
    IImageObjectDetectionService service,
    IAssetsObjectStorageClient assetsObjectStorageClient)
    : IIntegrationEventHandler<AddedToObjectStorageEvent>
{
    public async Task Handle(AddedToObjectStorageEvent @event)
    {
        var asset = await assetsObjectStorageClient.GetAssetByNameAsync(@event.FileName);
        await service.Detect(@event.FileName, asset?.Data);
    }
}