namespace MachineLearningDemo.Detection.PretrainedModel.YoloParser
{
    public class DimensionsBase
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }

        public override string ToString()
        {
            return $"x: {X}, y: {Y}, height: {Height}, width: {Width}";
        }
    }
}