namespace MachineLearningDemo.AppHost.ResourceBuilderExtensions.MinIo;

public class MinIoBuilder
{
    public int? ApiPort { get; set; }
    public int? ConsolePort { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? DataVolumePath { get; set; }

    public MinIoBuilder WithPorts(int? apiPort = null, int? consolePort = null)
    {
        ApiPort = apiPort;
        ConsolePort = consolePort;
        return this;
    }

    public MinIoBuilder WithCredentials(string accessKey, string secretKey)
    {
        AccessKey = accessKey;
        SecretKey = secretKey;
        return this;
    }

    public MinIoBuilder WithDataVolume(string path)
    {
        DataVolumePath = path;
        return this;
    }
}