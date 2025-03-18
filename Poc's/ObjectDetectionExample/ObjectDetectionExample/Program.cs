using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;

namespace ObjectDetectionExample
{
    public class ImageData
    {
        public string ImagePath { get; set; } = string.Empty;
    }

    public class OnnxPredictionOutput
    {
        [ColumnName("Identity:0")]
        public float[] Boxes { get; set; } = { };

        [ColumnName("Identity_1:0")]
        public float[] Labels { get; set; } = { };

        [ColumnName("Identity_2:0")]
        public float[] Scores { get; set; } = { };
    }

    public class ObjectDetectionPrediction
    {
        public string Label { get; set; } = string.Empty;
        public float Score { get; set; }
        public BoundingBox Box { get; set; } = new();
    }

    public class BoundingBox
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    internal class Program
    {
        private static List<string>? _cocoLabels;

        static void Main()
        {
            const string imagesFolder = @"C:\Git\ML.NET\Assets"; // Update this path

            //yolov4
            //const string modelFilePath = @"C:\Git\ML.NET\Models\yolov4\yolov4.onnx"; // Update this path
            //const string modelInputName = "input_1:0"; // Replace with the actual input node name of your model
            //const string cocoNamesFile = @"C:\Git\ML.NET\Models\yolov4\coco.names";

            //yolov5
            const string modelFilePath = @"C:\Git\ML.NET\Models\yolov5\yolov5m.onnx"; // Update this path
            const string modelInputName = "input_1:0"; // Replace with the actual input node name of your model
            const string cocoNamesFile = @"C:\Git\ML.NET\Models\yolov5\coco.names";

            // Load COCO class names.
            _cocoLabels = File.ReadAllLines(cocoNamesFile).ToList();

            var mlContext = new MLContext();

            var pipeline = mlContext.Transforms.LoadImages(
                                outputColumnName: "image",
                                imageFolder: imagesFolder,
                                inputColumnName: nameof(ImageData.ImagePath))
                           .Append(mlContext.Transforms.ResizeImages(
                                outputColumnName: "image",
                                imageWidth: 416,
                                imageHeight: 416,
                                inputColumnName: "image"))
                           .Append(mlContext.Transforms.ExtractPixels(
                                outputColumnName: "image"))
                           // Copy the "image" column to the expected input node name.
                           .Append(mlContext.Transforms.CopyColumns(
                                outputColumnName: modelInputName,
                                inputColumnName: "image"))
                           .Append(mlContext.Transforms.ApplyOnnxModel(
                                modelFile: modelFilePath,
                                outputColumnNames: new[] { "Identity:0", "Identity_1:0", "Identity_2:0" },
                                inputColumnNames: new[] { modelInputName }));

            var imageFiles = Directory.GetFiles(imagesFolder, "*.*", SearchOption.TopDirectoryOnly)
                                      .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                     file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                                      .ToList();

            var images = imageFiles.Select(f => new ImageData { ImagePath = Path.GetFileName(f) }).ToList();

            var data = mlContext.Data.LoadFromEnumerable(images);
            ITransformer model = pipeline.Fit(data);

            // Create a prediction engine that outputs the raw arrays.
            var predictionEngine = mlContext.Model.CreatePredictionEngine<ImageData, OnnxPredictionOutput>(model);

            foreach (var image in images)
            {
                Console.WriteLine($"Processing image: {image.ImagePath}");
                var onnxOutput = predictionEngine.Predict(image);

                // Convert raw arrays into a list of detections.
                var detections = ConvertOnnxOutputToDetections(onnxOutput, threshold: 0.5f);
                // Apply non-maximum suppression to remove overlapping detections.
                var finalDetections = ApplyNonMaxSuppression(detections, iouThreshold: 0.5f);

                foreach (var detection in finalDetections)
                {
                    Console.WriteLine($"\tDetected object: {detection.Label} (Confidence: {detection.Score:F2})");
                    Console.WriteLine($"\tBounding Box: [X: {detection.Box.X:F1}, Y: {detection.Box.Y:F1}, Width: {detection.Box.Width:F1}, Height: {detection.Box.Height:F1}]");
                }
            }

            Console.WriteLine("Processing complete. Press any key to exit.");
            Console.ReadKey();
        }

        // Converts the raw model output to a list of ObjectDetectionPrediction objects.
        static List<ObjectDetectionPrediction> ConvertOnnxOutputToDetections(OnnxPredictionOutput output, float threshold)
        {
            var detections = new List<ObjectDetectionPrediction>();

            // Loop through each score in the output.
            for (var i = 0; i < output.Scores.Length; i++)
            {
                var score = output.Scores[i];
                if (score < threshold)
                    continue; // Filter out low-confidence detections.

                // Retrieve bounding box coordinates.
                var x = output.Boxes[i * 4];
                var y = output.Boxes[i * 4 + 1];
                var width = output.Boxes[i * 4 + 2];
                var height = output.Boxes[i * 4 + 3];

                // Convert the label value (float) to an integer and map it to a class name.
                var labelId = (long)Math.Round(output.Labels[i]);
                var labelName = GetLabelName(labelId);

                detections.Add(new ObjectDetectionPrediction
                {
                    Label = labelName,
                    Score = score,
                    Box = new BoundingBox { X = x, Y = y, Width = width, Height = height }
                });
            }

            return detections;
        }

        // Applies non-maximum suppression to filter out overlapping detections.
        static List<ObjectDetectionPrediction> ApplyNonMaxSuppression(List<ObjectDetectionPrediction> detections, float iouThreshold)
        {
            var finalDetections = new List<ObjectDetectionPrediction>();

            // Sort detections by descending score.
            var sortedDetections = detections.OrderByDescending(d => d.Score).ToList();

            while (sortedDetections.Any())
            {
                // Take the detection with the highest score.
                var bestDetection = sortedDetections.First();
                finalDetections.Add(bestDetection);
                sortedDetections.RemoveAt(0);

                // Remove detections with a high overlap (IoU) with the best detection.
                sortedDetections = sortedDetections.Where(d => IntersectionOverUnion(bestDetection.Box, d.Box) < iouThreshold).ToList();
            }

            return finalDetections;
        }

        // Computes Intersection over Union (IoU) for two bounding boxes.
        static float IntersectionOverUnion(BoundingBox boxA, BoundingBox boxB)
        {
            var xA = Math.Max(boxA.X, boxB.X);
            var yA = Math.Max(boxA.Y, boxB.Y);
            var xB = Math.Min(boxA.X + boxA.Width, boxB.X + boxB.Width);
            var yB = Math.Min(boxA.Y + boxA.Height, boxB.Y + boxB.Height);

            var interWidth = Math.Max(0, xB - xA);
            var interHeight = Math.Max(0, yB - yA);
            var interArea = interWidth * interHeight;

            var boxAArea = boxA.Width * boxA.Height;
            var boxBArea = boxB.Width * boxB.Height;

            var unionArea = boxAArea + boxBArea - interArea;
            return unionArea == 0 ? 0 : interArea / unionArea;
        }

        static string GetLabelName(long label)
        {
            if (_cocoLabels != null && label >= 0 && label < _cocoLabels.Count)
                return _cocoLabels[(int)label];
            return "unknown";
        }
    }
}
