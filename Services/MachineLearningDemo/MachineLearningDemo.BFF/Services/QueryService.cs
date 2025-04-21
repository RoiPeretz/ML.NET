using MachineLearningDemo.BFF.Clients.DetectionChat;
using MachineLearningDemo.BFF.Clients.DetectionPretrainedModel;
using static System.Threading.Tasks.Task;
using DetectedObject = MachineLearningDemo.Core.Models.DetectedObject;
using ImageDetectionResult = MachineLearningDemo.Core.Models.ImageDetectionResult;

namespace MachineLearningDemo.BFF.Services;

internal interface IQueryService
{
    Task<IEnumerable<ImageDetectionResult>> Query(string searchTerm);
}

internal class QueryService(
    IDetectionChatClientGen detectionChatClient,
    IDetectionPretrainedModelClientGen detectionPretrainedModelClient) : IQueryService
{
    public async Task<IEnumerable<ImageDetectionResult>> Query(string searchTerm)
    {
        var chatResults = QueryChat(searchTerm);
        var pretrainedModelResults = QueryPretrained(searchTerm);

        var results = await WhenAll(chatResults, pretrainedModelResults);

        return results.SelectMany(result => result);
    }

    private async Task<IEnumerable<ImageDetectionResult>> QueryChat(string searchTerm)
    {
        var chatResults = await detectionChatClient.DetectGETAsync(searchTerm);

        return chatResults.Select(result =>
        {
            return new ImageDetectionResult
            {
                FileName = result.FileName,
                DetectionSource = result.DetectionSource,
                DetectionTimeMilliseconds = result.DetectionTimeMilliseconds,
                DetectedObjects = result.DetectedObjects.Select(@obj =>
                    new DetectedObject(@obj.Label, @obj.Color, @obj.AdditionalInfo))
            };
        });
    }

    private async Task<IEnumerable<ImageDetectionResult>> QueryPretrained(string searchTerm)
    {
        var pretrainedModelResults = await detectionPretrainedModelClient.DetectAllAsync(searchTerm);

        return pretrainedModelResults.Select(result =>
        {
            return new ImageDetectionResult
            {
                FileName = result.FileName,
                DetectionSource = result.DetectionSource,
                DetectionTimeMilliseconds = result.DetectionTimeMilliseconds,
                DetectedObjects = result.DetectedObjects.Select(@obj =>
                    new DetectedObject(@obj.Label, @obj.Color, @obj.AdditionalInfo))
            };
        });
    }
}