using MachineLearningDemo.Core.EventBus.Abstractions;
using MachineLearningDemo.Core.EventBus.Events;
using MachineLearningDemo.Core.Models;
using MachineLearningDemo.Core.Persistence.Commands;
using MachineLearningDemo.Detection.Chat.ChatClient;
using MachineLearningDemo.Detection.Chat.Services.WorkFlows.Tasks;

namespace MachineLearningDemo.Detection.Chat.Services.WorkFlows;

internal interface IObjectDetectionWorkflow
{
    Task<ImageDetectionResult> Detect(ChatClientWrapper chatClientWrapper, string fileName, ReadOnlyMemory<byte> image, string contentType);
}

internal class ObjectDetectionWorkflow(
    IEventBus eventBus,
    IDetectObjectsTask detectObjectsTask,
    IAddImageDetectionResultCommand addImageDetectionResultCommand)
    : IObjectDetectionWorkflow
{
    public async Task<ImageDetectionResult> Detect(ChatClientWrapper chatClientWrapper, string fileName, ReadOnlyMemory<byte> image, string contentType)
    {
        var detectionStarted = new DetectionStartedEvent
        {
            FileName = fileName,
            ModelName = chatClientWrapper.ModelName
        };

        await eventBus.PublishAsync(detectionStarted); 

        var detectedObjects = await detectObjectsTask.Detect(chatClientWrapper.ChatClient, image, contentType);

        var result = new ImageDetectionResult
        {
            FileName = fileName,
            DetectedObjects = detectedObjects,
            DetectionSource = chatClientWrapper.ModelName,
            DetectionTimeMilliseconds = DateTime.UtcNow.Subtract(detectionStarted.CreationDate).TotalMilliseconds
        };

        await addImageDetectionResultCommand.Add(result);

        var detectionEnded = new DetectionEndedEvent
        {
            FileName = fileName,
            ModelName = chatClientWrapper.ModelName
        };

        await eventBus.PublishAsync(detectionStarted);

        return result;
    }
}