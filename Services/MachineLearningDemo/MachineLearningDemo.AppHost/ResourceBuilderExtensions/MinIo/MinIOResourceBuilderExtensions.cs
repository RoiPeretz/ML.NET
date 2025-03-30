namespace MachineLearningDemo.AppHost.ResourceBuilderExtensions.MinIo;

internal static class MinIoContainerImageTags
{
    internal const string Registry = "docker.io";
    internal const string Image = "minio/minio";
    internal const string Tag = "latest";
}

public static class MinIoResourceBuilderExtensions
{
    public static IResourceBuilder<MinIoResource> AddMinIo(
        this IDistributedApplicationBuilder builder,
        string name,
        Action<MinIoBuilder>? configure = null)
    {
        var options = new MinIoBuilder();
        configure?.Invoke(options);

        var resource = new MinIoResource(name, options.AccessKey, options.SecretKey);

        return builder.AddResource(resource)
            .WithImage(MinIoContainerImageTags.Image)
            .WithImageRegistry(MinIoContainerImageTags.Registry)
            .WithImageTag(MinIoContainerImageTags.Tag)
            .WithHttpEndpoint(
                targetPort: MinIoResource.DefaultApiPort,
                port: options.ApiPort,
                name: MinIoResource.ApiEndpointName)
            .WithEndpoint(
                targetPort: MinIoResource.DefaultConsolePort,
                port: options.ConsolePort,
                name: MinIoResource.ConsoleEndpointName)
            .ConfigureCredentials(options)
            .ConfigureVolume(options)
            .WithArgs("server", "/data", "--console-address", $":{MinIoResource.DefaultConsolePort}");
    }

    private static IResourceBuilder<MinIoResource> ConfigureCredentials(
        this IResourceBuilder<MinIoResource> builder,
        MinIoBuilder options)
    {
        return builder
            .WithEnvironment("MINIO_ROOT_USER", options.AccessKey ?? "minioadmin")
            .WithEnvironment("MINIO_ROOT_PASSWORD", options.SecretKey ?? "minioadmin");
    }

    private static IResourceBuilder<MinIoResource> ConfigureVolume(
        this IResourceBuilder<MinIoResource> builder,
        MinIoBuilder options)
    {
        if (!string.IsNullOrEmpty(options.DataVolumePath))
            builder = builder.WithVolume(options.DataVolumePath, "/data");
        return builder;
    }
}