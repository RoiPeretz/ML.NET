using MachineLearningDemo.Core.Models;

namespace MachineLearningDemo.Core.Persistence.Queries;

public interface IQueryDetectionResultBySearchTerm
{
    Task<IEnumerable<ImageDetectionResult>> Query(string searchTerm);
}