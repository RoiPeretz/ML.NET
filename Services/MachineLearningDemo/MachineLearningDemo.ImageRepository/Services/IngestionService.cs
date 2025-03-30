using MachineLearningDemo.Core.ObjectStorage.Models;
using MachineLearningDemo.ImageRepository.Services.Workflows;

namespace MachineLearningDemo.ImageRepository.Services;

public interface IIngestionService
{
    Task<bool> IngestImageAsync(FileModel file);
}

public class IngestionService(
    IIngestImageWorkflow ingestImageWorkflow)
    : IIngestionService
{
    public Task<bool> IngestImageAsync(FileModel file)
    {
        return ingestImageWorkflow.IngestImageAsync(file);
    }
}