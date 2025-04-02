using MachineLearningDemo.Core.Models;
using MachineLearningDemo.Detection.Chat.ChatClient;
using MachineLearningDemo.Detection.Chat.Services.WorkFlows;

namespace MachineLearningDemo.Detection.Chat.Services;

internal interface IImageObjectDetectionService
{
    Task<ImageDetectionResult[]> Detect(string fileName, ReadOnlyMemory<byte> image, string contentType);
}

internal class ImageObjectDetectionService(
    IEnumerable<ChatClientWrapper> clients,
    IObjectDetectionWorkflow objectDetectionWorkflow) 
    : IImageObjectDetectionService
{
    public async Task<ImageDetectionResult[]> Detect(string fileName, ReadOnlyMemory<byte> image, string contentType)
    {
        var tasks = clients.Select(wrapper => objectDetectionWorkflow.Detect(wrapper, fileName, image, contentType));
        var results = await Task.WhenAll(tasks);
        
        return results;
    }
}