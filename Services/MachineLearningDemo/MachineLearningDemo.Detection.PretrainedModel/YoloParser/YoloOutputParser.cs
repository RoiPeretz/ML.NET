﻿using System.Drawing;

namespace MachineLearningDemo.Detection.PretrainedModel.YoloParser;

internal interface IYoloOutputParser
{
    float[] ExtractClasses(float[] modelOutput, int x, int y, int channel);
    IList<YoloObject> ParseOutputs(float[] yoloModelOutputs, float threshold = .3F);
    IList<YoloObject> FilterBoundingBoxes(IList<YoloObject> boxes, int limit, float threshold);
}

internal class YoloOutputParser : IYoloOutputParser
{
    private class CellDimensions : DimensionsBase { }

    public const int RowCount = 13;
    public const int ColCount = 13;
    public const int ChannelCount = 125;
    public const int BoxesPerCell = 5;
    public const int BoxInfoFeatureCount = 5;
    public const int ClassCount = 20;
    public const float CellWidth = 32;
    public const float CellHeight = 32;

    public readonly int ChannelStride = RowCount * ColCount;

    private readonly float[] _anchors = new float[]
    {
        1.08F, 1.19F, 3.42F, 4.41F, 6.63F, 11.38F, 9.42F, 5.11F, 16.62F, 10.52F
    };

    private readonly string[] _labels = new string[]
    {
        "aeroplane", "bicycle", "bird", "boat", "bottle",
        "bus", "car", "cat", "chair", "cow",
        "diningtable", "dog", "horse", "motorbike", "person",
        "pottedplant", "sheep", "sofa", "train", "tvmonitor"
    };

    private float Sigmoid(float value)
    {
        var k = (float)Math.Exp(value);
        return k / (1.0f + k);
    }

    private float[] Softmax(float[] values)
    {
        var maxVal = values.Max();
        var exp = values.Select(v => Math.Exp(v - maxVal));
        var enumerable = exp as double[] ?? exp.ToArray();
        var sumExp = enumerable.Sum();

        return enumerable.Select(v => (float)(v / sumExp)).ToArray();
    }

    private int GetOffset(int x, int y, int channel)
    {
        // YOLO outputs a tensor that has a shape of 125x13x13, which 
        // WinML flattens into a 1D array.  To access a specific channel 
        // for a given (x,y) cell position, we need to calculate an offset
        // into the array
        return (channel * ChannelStride) + (y * ColCount) + x;
    }

    private BoundingBoxDimensions ExtractBoundingBoxDimensions(float[] modelOutput, int x, int y, int channel)
    {
        return new BoundingBoxDimensions
        {
            X = modelOutput[GetOffset(x, y, channel)],
            Y = modelOutput[GetOffset(x, y, channel + 1)],
            Width = modelOutput[GetOffset(x, y, channel + 2)],
            Height = modelOutput[GetOffset(x, y, channel + 3)]
        };
    }

    private float GetConfidence(float[] modelOutput, int x, int y, int channel)
    {
        return Sigmoid(modelOutput[GetOffset(x, y, channel + 4)]);
    }

    private CellDimensions MapBoundingBoxToCell(int x, int y, int box, BoundingBoxDimensions boxDimensions)
    {
        return new CellDimensions
        {
            X = (x + Sigmoid(boxDimensions.X)) * CellWidth,
            Y = (y + Sigmoid(boxDimensions.Y)) * CellHeight,
            Width = (float)Math.Exp(boxDimensions.Width) * CellWidth * _anchors[box * 2],
            Height = (float)Math.Exp(boxDimensions.Height) * CellHeight * _anchors[box * 2 + 1],
        };
    }

    public float[] ExtractClasses(float[] modelOutput, int x, int y, int channel)
    {
        var predictedClasses = new float[ClassCount];
        var predictedClassOffset = channel + BoxInfoFeatureCount;
        for (var predictedClass = 0; predictedClass < ClassCount; predictedClass++)
        {
            predictedClasses[predictedClass] = modelOutput[GetOffset(x, y, predictedClass + predictedClassOffset)];
        }
        return Softmax(predictedClasses);
    }

    private ValueTuple<int, float> GetTopResult(float[] predictedClasses)
    {
        return predictedClasses
            .Select((predictedClass, index) => (Index: index, Value: predictedClass))
            .OrderByDescending(result => result.Value)
            .First();
    }

    private float IntersectionOverUnion(RectangleF boundingBoxA, RectangleF boundingBoxB)
    {
        var areaA = boundingBoxA.Width * boundingBoxA.Height;

        if (areaA <= 0)
            return 0;

        var areaB = boundingBoxB.Width * boundingBoxB.Height;

        if (areaB <= 0)
            return 0;

        var minX = Math.Max(boundingBoxA.Left, boundingBoxB.Left);
        var minY = Math.Max(boundingBoxA.Top, boundingBoxB.Top);
        var maxX = Math.Min(boundingBoxA.Right, boundingBoxB.Right);
        var maxY = Math.Min(boundingBoxA.Bottom, boundingBoxB.Bottom);

        var intersectionArea = Math.Max(maxY - minY, 0) * Math.Max(maxX - minX, 0);

        return intersectionArea / (areaA + areaB - intersectionArea);
    }

    public IList<YoloObject> ParseOutputs(float[] yoloModelOutputs, float threshold = .3F)
    {
        var boxes = new List<YoloObject>();

        for (var row = 0; row < RowCount; row++)
        {
            for (var column = 0; column < ColCount; column++)
            {
                for (var box = 0; box < BoxesPerCell; box++)
                {
                    var channel = (box * (ClassCount + BoxInfoFeatureCount));

                    var boundingBoxDimensions = ExtractBoundingBoxDimensions(yoloModelOutputs, row, column, channel);

                    var confidence = GetConfidence(yoloModelOutputs, row, column, channel);

                    var mappedBoundingBox = MapBoundingBoxToCell(row, column, box, boundingBoxDimensions);

                    if (confidence < threshold)
                        continue;

                    var predictedClasses = ExtractClasses(yoloModelOutputs, row, column, channel);

                    var (topResultIndex, topResultScore) = GetTopResult(predictedClasses);
                    var topScore = topResultScore * confidence;

                    if (topScore < threshold)
                        continue;

                    var dimensions = new BoundingBoxDimensions
                    {
                        X = (mappedBoundingBox.X - mappedBoundingBox.Width / 2),
                        Y = (mappedBoundingBox.Y - mappedBoundingBox.Height / 2),
                        Width = mappedBoundingBox.Width,
                        Height = mappedBoundingBox.Height,
                    };

                    var label = _labels[topResultIndex];
                    boxes.Add(new YoloObject(dimensions, label, topScore));
                }
            }
        }
        return boxes;
    }

    public IList<YoloObject> FilterBoundingBoxes(IList<YoloObject> boxes, int limit, float threshold)
    {
        var activeCount = boxes.Count;
        var isActiveBoxes = new bool[boxes.Count];

        for (var i = 0; i < isActiveBoxes.Length; i++)
            isActiveBoxes[i] = true;

        var sortedBoxes = boxes.Select((b, i) => new { Box = b, Index = i })
            .OrderByDescending(b => b.Box.Confidence)
            .ToList();

        var results = new List<YoloObject>();

        for (var i = 0; i < boxes.Count; i++)
        {
            if (!isActiveBoxes[i]) continue;
            var boxA = sortedBoxes[i].Box;
            results.Add(boxA);

            if (results.Count >= limit)
                break;

            for (var j = i + 1; j < boxes.Count; j++)
            {
                if (!isActiveBoxes[j]) continue;
                var boxB = sortedBoxes[j].Box;

                if (!(IntersectionOverUnion(boxA.Rect, boxB.Rect) > threshold)) continue;
                isActiveBoxes[j] = false;
                activeCount--;

                if (activeCount <= 0)
                    break;
            }

            if (activeCount <= 0)
                break;
        }
        return results;
    }

}