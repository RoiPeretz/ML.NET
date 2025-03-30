using MachineLearningDemo.Core.Models;

namespace MachineLearningDemo.Core.Persistence.Commands;

public interface IAddImageDetectionResultCommand
{
    Task<bool> Add(ImageDetectionResult imageDetectionResult);
}