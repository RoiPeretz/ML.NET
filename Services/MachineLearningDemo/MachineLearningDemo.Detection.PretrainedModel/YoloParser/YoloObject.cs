using System.Drawing;

namespace MachineLearningDemo.Detection.PretrainedModel.YoloParser;

public class BoundingBoxDimensions : DimensionsBase { }

public class YoloObject(BoundingBoxDimensions dimensions, string label, float confidence)
{
    public BoundingBoxDimensions Dimensions { get; set; } = dimensions;

    public string Label { get; set; } = label;

    public float Confidence { get; set; } = confidence;

    public RectangleF Rect => new(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height);
}