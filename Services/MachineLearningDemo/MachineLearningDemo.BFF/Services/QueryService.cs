using MachineLearningDemo.BFF.Clients.DetectionChat;
using static System.Threading.Tasks.Task;

namespace MachineLearningDemo.BFF.Services;

internal interface IQueryService
{
    Task<IEnumerable<ImageDetectionResult>> Query(string searchTerm);
}

internal class QueryService(IDetectionChatClientGen detectionChatClient) : IQueryService
{
    public async Task<IEnumerable<ImageDetectionResult>> Query(string searchTerm)
    {
        var chatResults = detectionChatClient.DetectGETAsync(searchTerm);

        var results = await WhenAll(chatResults);

        return results.SelectMany(result => result);
    }
}