using System.Runtime.InteropServices.ComTypes;
using Elastic.Clients.Elasticsearch;
using MachineLearningDemo.Core.Models;
using MachineLearningDemo.Core.Persistence.Commands;
using Microsoft.Extensions.Logging;

namespace MachineLearningDemo.Infrastructure.Persistence.Elasticsearch.Commands;

class AddImageDetectionResultCommand(
    ElasticsearchClient elasticClient,
    PersistenceLayerSettings settings,
    ILogger<AddImageDetectionResultCommand> logger) 
    : IAddImageDetectionResultCommand
{
    public async Task<bool> Add(ImageDetectionResult imageDetectionResult)
    {
        var response = await elasticClient.CreateAsync(
            imageDetectionResult,
            c => c
                .Index(settings.IndexName)
                .Id(imageDetectionResult.FileName)
        );

        if (response.IsValidResponse is false)
        {
            logger.LogError("Failed to index document: {DebugInformation}", response.DebugInformation);
        }

        logger.LogInformation("Added detection result {Result} to index {Index}", imageDetectionResult, response.Index);
        
        return response.IsSuccess();
    }
}