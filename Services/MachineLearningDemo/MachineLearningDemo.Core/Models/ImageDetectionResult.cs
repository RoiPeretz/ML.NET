namespace MachineLearningDemo.Core.Models;

public class ImageDetectionResult
{
    public string FileName { get; init; } = string.Empty;
    public double DetectionTimeMilliseconds { get; init; } 
    public string DetectionSource { get; init; } = string.Empty;
    public IEnumerable<DetectedObject>? DetectedObjects { get; init; }
}