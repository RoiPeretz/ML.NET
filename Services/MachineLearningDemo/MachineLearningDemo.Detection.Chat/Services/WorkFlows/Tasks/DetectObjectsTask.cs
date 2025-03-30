using MachineLearningDemo.Core.Models;
using Microsoft.Extensions.AI;

namespace MachineLearningDemo.Detection.Chat.Services.WorkFlows.Tasks;

internal interface IDetectObjectsTask
{
    Task<IEnumerable<DetectedObject>> Detect(IChatClient chatClient, ReadOnlyMemory<byte> image);
}

internal class DetectObjectsTask : IDetectObjectsTask
{
    public async Task<IEnumerable<DetectedObject>> Detect(IChatClient chatClient, ReadOnlyMemory<byte> image)
    {
        var message = new ChatMessage(ChatRole.User,
            $"""
             You are an advanced image-analysis system. Given the attached image, please identify and describe all the objects you can detect. For each detected object, provide:
             
             A label or name (e.g., 'bicycle', 'dog', 'chair').
             
             A confidence score or how certain you are.
             
             Additional information such as the color, shape, or other distinguishing features.
             
             Any relationships or context (e.g., if the object is on top of another object).
             If text is visible, transcribe it if legible. 
             If the image is ambiguous or if you are uncertain about any detail, please note that uncertainty.
             Finally, provide a brief, high-level summary that could serve as an 'alt-text' description for accessibility."
             """);

        message.Contents.Add(new DataContent(image, "image/png"));

        var chatResponse = await chatClient.GetResponseAsync<IEnumerable<DetectedObject>>(message);

        return chatResponse.Result;
    }
}