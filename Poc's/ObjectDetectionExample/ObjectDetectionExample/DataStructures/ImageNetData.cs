using Microsoft.ML.Data;

namespace ObjectDetectionExample.DataStructures
{
    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath = string.Empty;

        [LoadColumn(1)]
        public string Label = string.Empty;

        public static IEnumerable<ImageNetData> ReadFromFile(string? imageFolder)
        {
            return Directory
                .GetFiles(imageFolder)
                .Select(filePath => new ImageNetData { ImagePath = filePath, Label = Path.GetFileName(filePath) });
        }
    }
}