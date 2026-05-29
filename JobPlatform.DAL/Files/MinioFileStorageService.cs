using JobPlatform.DAL.Interfaces;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace JobPlatform.DAL.Files;

public sealed class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioStorageOptions _options;

    public MinioFileStorageService(IOptions<MinioStorageOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
            throw new InvalidOperationException("Minio endpoint is not configured.");
        if (string.IsNullOrWhiteSpace(_options.AccessKey))
            throw new InvalidOperationException("Minio access key is not configured.");
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InvalidOperationException("Minio secret key is not configured.");
        if (string.IsNullOrWhiteSpace(_options.BucketName))
            throw new InvalidOperationException("Minio bucket name is not configured.");

        _client = new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSsl)
            .Build();
    }

    public async Task<string> SaveAsync(
        Stream content,
        string originalFileName,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var extension = Path.GetExtension(originalFileName);
        if (extension.Length > 20)
            extension = string.Empty;

        var objectKey = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}{extension}";

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(sizeBytes)
            .WithContentType(contentType);

        await _client.PutObjectAsync(putObjectArgs, cancellationToken);
        return objectKey;
    }

    public async Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Contains("..", StringComparison.Ordinal))
            throw new InvalidOperationException("Некорректный ключ файла.");

        var memoryStream = new MemoryStream();

        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(storageKey)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await _client.GetObjectAsync(getObjectArgs, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Contains("..", StringComparison.Ordinal))
            throw new InvalidOperationException("Некорректный ключ файла.");

        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(storageKey);

        await _client.RemoveObjectAsync(removeObjectArgs, cancellationToken);
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(_options.BucketName);
        var exists = await _client.BucketExistsAsync(bucketExistsArgs, cancellationToken);

        if (exists)
            return;

        var makeBucketArgs = new MakeBucketArgs().WithBucket(_options.BucketName);
        await _client.MakeBucketAsync(makeBucketArgs, cancellationToken);
    }
}
