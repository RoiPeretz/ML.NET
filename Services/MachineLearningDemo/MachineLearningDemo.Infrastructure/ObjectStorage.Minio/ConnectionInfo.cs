namespace MachineLearningDemo.Infrastructure.ObjectStorage.Minio;

public class ConnectionInfo
{
    public Uri? Endpoint { get; set; } 
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    public static ConnectionInfo ParseMinioConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is empty");

        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
            throw new ArgumentException("Connection string must include URI, AccessKey, and SecretKey");

        var endpoint = parts[0];
        var accessKey = parts.FirstOrDefault(p => p.StartsWith("AccessKey=", StringComparison.OrdinalIgnoreCase));
        var secretKey = parts.FirstOrDefault(p => p.StartsWith("SecretKey=", StringComparison.OrdinalIgnoreCase));

        if (accessKey == null || secretKey == null)
            throw new ArgumentException("Missing AccessKey or SecretKey");

        return new ConnectionInfo
        {
            Endpoint = new Uri(endpoint),
            AccessKey = accessKey.Split('=')[1],
            SecretKey = secretKey.Split('=')[1]
        };
    }
}
