using Microsoft.ML.Data;

namespace MachineLearningDemo.Detection.PretrainedModel.Models;

public class ImageData(string imagePath, string label)
{
    [LoadColumn(0)]
    public string ImagePath = imagePath;

    [LoadColumn(1)]
    public string Label = label;
}
