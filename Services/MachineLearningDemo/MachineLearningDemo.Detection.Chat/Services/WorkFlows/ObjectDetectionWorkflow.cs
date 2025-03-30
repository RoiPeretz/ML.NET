using MachineLearningDemo.Core.Models;
using MachineLearningDemo.Core.Persistence.Commands;
using MachineLearningDemo.Detection.Chat.ChatClient;
using MachineLearningDemo.Detection.Chat.Services.WorkFlows.Tasks;

namespace MachineLearningDemo.Detection.Chat.Services.WorkFlows;

internal interface IObjectDetectionWorkflow
{
    Task<ImageDetectionResult> Detect(ChatClientWrapper chatClientWrapper, string fileName, ReadOnlyMemory<byte> image);
}

internal class ObjectDetectionWorkflow(
    IDetectObjectsTask detectObjectsTask,
    IAddImageDetectionResultCommand addImageDetectionResultCommand)
    : IObjectDetectionWorkflow
{
    public async Task<ImageDetectionResult> Detect(ChatClientWrapper chatClientWrapper, string fileName, ReadOnlyMemory<byte> image)
    {
        var startTime = DateTime.UtcNow;

        var detectedObjects = await detectObjectsTask.Detect(chatClientWrapper.ChatClient, image);

        var result = new ImageDetectionResult
        {
            FileName = fileName,
            DetectedObjects = detectedObjects,
            DetectionSource = chatClientWrapper.ModelName,
            DetectionTimeMilliseconds = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds
        };

        await addImageDetectionResultCommand.Add(result);
        
        return result;
    }
}