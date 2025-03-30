using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.ObjectStorage.Interfaces;
using MachineLearningDemo.Core.ObjectStorage.Models;

namespace MachineLearningDemo.ImageRepository.Services.Workflows;

public interface IIngestImageWorkflow
{
    Task<bool> IngestImageAsync(FileModel file);
}

public class IngestImageWorkflow(
    IEventBus eventBus,
    IAssetsObjectStorageClient assetsObjectStorageClient) 
    : IIngestImageWorkflow
{
    public async Task<bool> IngestImageAsync(FileModel file)
    {
        var addImageToObjectStorage = await assetsObjectStorageClient.AddAssetAsync(file);
        if (addImageToObjectStorage is false)
        {
            return false;
        }

        var @event = new AssetIngestedEvent
        {
            FileName = file.FileName,
        };

        await eventBus.PublishAsync(@event);
        return true;
    }
}