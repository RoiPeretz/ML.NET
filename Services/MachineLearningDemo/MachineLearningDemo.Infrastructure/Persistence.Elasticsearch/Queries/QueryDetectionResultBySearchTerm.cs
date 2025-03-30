using Elastic.Clients.Elasticsearch;
using MachineLearningDemo.Core.Models;
using MachineLearningDemo.Core.Persistence.Queries;

namespace MachineLearningDemo.Infrastructure.Persistence.Elasticsearch.Queries;

class QueryDetectionResultBySearchTerm(
    ElasticsearchClient elasticClient,
    PersistenceLayerSettings settings) : IQueryDetectionResultBySearchTerm
{
    public async Task<IEnumerable<ImageDetectionResult>> Query(string searchTerm)
    {
        var searchResponse = await elasticClient.SearchAsync<ImageDetectionResult>(s => s
            .Index(settings.IndexName)
            .Query(q => q
                .QueryString(qs => qs
                    .Query(searchTerm)
                )
            )
        );
        
        return searchResponse.Documents;
    }
}