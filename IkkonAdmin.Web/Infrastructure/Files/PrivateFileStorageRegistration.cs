using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace IkkonAdmin.Web.Infrastructure.Files;

public static class PrivateFileStorageRegistration
{
    public static IServiceCollection AddPrivateFileStorage(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var section = configuration.GetSection(PrivateFileStorageOptions.SectionName);
        var settings = section.Get<PrivateFileStorageOptions>() ?? new PrivateFileStorageOptions();
        if (!section.Exists() && environment.IsProduction())
        {
            throw new InvalidOperationException(
                "PrivateFileStorage deve ser configurado explicitamente como S3 em produção.");
        }

        services.AddOptions<PrivateFileStorageOptions>()
            .Bind(section)
            .Validate(options =>
                options.Provider.Equals(PrivateFileStorageOptions.LocalProvider, StringComparison.OrdinalIgnoreCase) ||
                options.Provider.Equals(PrivateFileStorageOptions.S3Provider, StringComparison.OrdinalIgnoreCase),
                "PrivateFileStorage:Provider deve ser Local ou S3.")
            .Validate(options =>
                !options.Provider.Equals(PrivateFileStorageOptions.S3Provider, StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrWhiteSpace(options.BucketName),
                "PrivateFileStorage:BucketName é obrigatório para S3.")
            .Validate(options =>
                string.IsNullOrWhiteSpace(options.AccessKeyId) ==
                string.IsNullOrWhiteSpace(options.SecretAccessKey),
                "PrivateFileStorage:AccessKeyId e SecretAccessKey devem ser informados juntos.")
            .ValidateOnStart();

        if (environment.IsProduction() &&
            !settings.Provider.Equals(PrivateFileStorageOptions.S3Provider, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Documentos privados exigem PrivateFileStorage:Provider=S3 em produção.");
        }

        if (settings.Provider.Equals(PrivateFileStorageOptions.S3Provider, StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IAmazonS3>(_ => CreateS3Client(settings));
            services.AddSingleton<S3PrivateFileStorageService>();
            services.AddSingleton<IPrivateFileStorageService>(provider =>
                provider.GetRequiredService<S3PrivateFileStorageService>());
            services.AddSingleton<IPrivateFileStorageHealthProbe>(provider =>
                provider.GetRequiredService<S3PrivateFileStorageService>());
            return services;
        }

        services.AddSingleton<LocalPrivateFileStorageService>();
        services.AddSingleton<IPrivateFileStorageService>(provider =>
            provider.GetRequiredService<LocalPrivateFileStorageService>());
        services.AddSingleton<IPrivateFileStorageHealthProbe>(provider =>
            provider.GetRequiredService<LocalPrivateFileStorageService>());
        return services;
    }

    private static IAmazonS3 CreateS3Client(PrivateFileStorageOptions settings)
    {
        var config = new AmazonS3Config
        {
            ForcePathStyle = settings.ForcePathStyle,
            RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region)
        };

        if (!string.IsNullOrWhiteSpace(settings.ServiceUrl))
        {
            config.ServiceURL = settings.ServiceUrl;
            config.AuthenticationRegion = settings.Region;
        }

        var hasExplicitCredentials =
            !string.IsNullOrWhiteSpace(settings.AccessKeyId) &&
            !string.IsNullOrWhiteSpace(settings.SecretAccessKey);
        return hasExplicitCredentials
            ? new AmazonS3Client(
                new BasicAWSCredentials(settings.AccessKeyId, settings.SecretAccessKey),
                config)
            : new AmazonS3Client(config);
    }
}
