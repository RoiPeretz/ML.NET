using Microsoft.ML.Data;

namespace ObjectDetectionExample.YoloParser;

public class OnnxPredictionOutput
{
    [ColumnName("Identity:0")]
    public float[] Boxes { get; set; } = { };

    [ColumnName("Identity_1:0")]
    public float[] Labels { get; set; } = { };

    [ColumnName("Identity_2:0")]
    public float[] Scores { get; set; } = { };
}