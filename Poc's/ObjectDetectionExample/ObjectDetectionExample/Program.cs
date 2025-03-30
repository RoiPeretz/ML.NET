using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.ML;
using ObjectDetectionExample;
using ObjectDetectionExample.DataStructures;
using ObjectDetectionExample.YoloParser;
using static System.Drawing.Image;

var assetsRelativePath = @"../../../../../../Assets";
var assetsPath = GetAbsolutePath(assetsRelativePath);

var modelsRelativePath = @"../../../../../../Models";
var modelsPath = GetAbsolutePath(modelsRelativePath);
var modelFilePath = Path.Combine(modelsPath, "yolov4", "yolov4.onnx");
var cocoNamesFile = Path.Combine(modelsPath, "yolov4", "coco.names");

var imagesFolder = Path.Combine(assetsPath, "images");
var outputFolder = Path.Combine(assetsPath, "output");

// Initialize MLContext
var mlContext = new MLContext();

try
{
    // Load Data
    var images = ImageNetData.ReadFromFile(imagesFolder);
    var imageDataView = mlContext.Data.LoadFromEnumerable(images);

    // Create instance of model scorer
    var modelScorer = new OnnxModelScorer(imagesFolder, modelFilePath, mlContext);

    // Use model to score data
    var probabilities = modelScorer.Score(imageDataView)
        .Select(output => output.Boxes)
        .ToList();

    // Post-process model output
    var parser = new YoloOutputParser(cocoNamesFile);

    var boundingBoxes =
        probabilities
        .Select(probability => parser.ParseOutputs(probability))
        .Select(boxes => parser.FilterBoundingBoxes(boxes, 5, .5F));

    // Draw bounding boxes for detected objects in each of the images
    for (var i = 0; i < images.Count(); i++)
    {
        var imageFileName = images.ElementAt(i).Label;
        var detectedObjects = boundingBoxes.ElementAt(i);

        DrawBoundingBox(imagesFolder, outputFolder, imageFileName, detectedObjects);

        LogDetectedObjects(imageFileName, detectedObjects);
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

Console.WriteLine("========= End of Process..Hit any Key ========");

string GetAbsolutePath(string relativePath)
{
    var dataRoot = new FileInfo(typeof(Program).Assembly.Location);
    var assemblyFolderPath = dataRoot.Directory.FullName;

    var fullPath = Path.Combine(assemblyFolderPath, relativePath);

    return fullPath;
}

void DrawBoundingBox(string? inputImageLocation, string outputImageLocation, string imageName, IList<YoloBoundingBox> filteredBoundingBoxes)
{
    if (inputImageLocation == null) return;
    var image = FromFile(Path.Combine(inputImageLocation, imageName));

    var originalImageHeight = image.Height;
    var originalImageWidth = image.Width;

    foreach (var box in filteredBoundingBoxes)
    {
        // Get Bounding Box Dimensions
        var x = (uint)Math.Max(box.Dimensions.X, 0);
        var y = (uint)Math.Max(box.Dimensions.Y, 0);
        var width = (uint)Math.Min(originalImageWidth - x, box.Dimensions.Width);
        var height = (uint)Math.Min(originalImageHeight - y, box.Dimensions.Height);

        // Resize To Image
        x = (uint)originalImageWidth * x / OnnxModelScorer.ImageNetSettings.ImageWidth;
        y = (uint)originalImageHeight * y / OnnxModelScorer.ImageNetSettings.ImageHeight;
        width = (uint)originalImageWidth * width / OnnxModelScorer.ImageNetSettings.ImageWidth;
        height = (uint)originalImageHeight * height / OnnxModelScorer.ImageNetSettings.ImageHeight;

        // Bounding Box Text
        var text = $"{box.Label} ({(box.Confidence * 100).ToString("0")}%)";

        using var thumbnailGraphic = Graphics.FromImage(image);

        thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
        thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
        thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // Define Text Options
        var drawFont = new Font("Arial", 12, FontStyle.Bold);
        var size = thumbnailGraphic.MeasureString(text, drawFont);
        var fontBrush = new SolidBrush(Color.Black);
        var atPoint = new Point((int)x, (int)y - (int)size.Height - 1);

        // Define BoundingBox options
        var pen = new Pen(box.BoxColor, 3.2f);
        var colorBrush = new SolidBrush(box.BoxColor);

        // Draw text on image 
        thumbnailGraphic.FillRectangle(colorBrush, (int)x, (int)(y - size.Height - 1), (int)size.Width,
            (int)size.Height);
        thumbnailGraphic.DrawString(text, drawFont, fontBrush, atPoint);

        // Draw bounding box on image
        thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
    }

    if (!Directory.Exists(outputImageLocation))
    {
        Directory.CreateDirectory(outputImageLocation);
    }

    image.Save(Path.Combine(outputImageLocation, imageName));
}

void LogDetectedObjects(string imageName, IList<YoloBoundingBox> boundingBoxes)
{
    Console.WriteLine($".....The objects in the image {imageName} are detected as below....");

    foreach (var box in boundingBoxes)
    {
        Console.WriteLine($"{box.Label} and its Confidence score: {box.Confidence}");
    }

    Console.WriteLine("");
}
