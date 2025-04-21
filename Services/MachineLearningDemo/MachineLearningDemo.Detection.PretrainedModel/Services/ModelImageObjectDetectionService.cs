using MachineLearningDemo.Core.Models;
using MachineLearningDemo.Detection.PretrainedModel.Services.WorkFlows;

namespace MachineLearningDemo.Detection.PretrainedModel.Services;

internal interface IModelImageObjectDetectionService
{
    Task<ImageDetectionResult?> Detect(string fileName, ReadOnlyMemory<byte> image, string contentType);
}

internal class ModelImageObjectDetectionService(
    IObjectDetectionWorkflow objectDetectionWorkflow) 
    : IModelImageObjectDetectionService
{
    public async Task<ImageDetectionResult?> Detect(string fileName, ReadOnlyMemory<byte> image, string contentType)
    {
       return await objectDetectionWorkflow.Detect(fileName, image, contentType);
    }
}