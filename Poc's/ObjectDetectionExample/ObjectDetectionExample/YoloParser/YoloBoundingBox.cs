using System.Drawing;

namespace ObjectDetectionExample.YoloParser
{
    public class BoundingBoxDimensions : DimensionsBase { }

    public class YoloBoundingBox
    {
        public BoundingBoxDimensions Dimensions { get; set; } = new();

        public string Label { get; set; } = string.Empty;

        public float Confidence { get; set; }

        public RectangleF Rect => new(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height);

        public Color BoxColor { get; set; }
    }
    
}