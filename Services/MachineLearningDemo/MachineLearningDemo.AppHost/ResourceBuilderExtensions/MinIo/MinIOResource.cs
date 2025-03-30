namespace MachineLearningDemo.AppHost.ResourceBuilderExtensions.MinIo;

public class MinIoResource(string name, string? accessKey = null, string? secretKey = null)
    : ContainerResource(name), IResourceWithConnectionString
{
    internal const int DefaultApiPort = 9000;
    internal const int DefaultConsolePort = 9001;
    internal const string ApiEndpointName = "api";
    internal const string ConsoleEndpointName = "console";

    private EndpointReference? _apiReference;
    private EndpointReference? _consoleReference;
    
    public string? AccessKey { get; } = accessKey;
    public string? SecretKey { get; } = secretKey;

    private EndpointReference ApiEndpoint =>
        _apiReference ??= new EndpointReference(this, ApiEndpointName);

    private EndpointReference ConsoleEndpoint =>
        _consoleReference ??= new EndpointReference(this, ConsoleEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{ApiEndpoint.Property(EndpointProperty.Host)}:{ApiEndpoint.Property(EndpointProperty.Port)};" +
            $"AccessKey={AccessKey ?? "minioadmin"};" +
            $"SecretKey={SecretKey ?? "minioadmin"}");
}