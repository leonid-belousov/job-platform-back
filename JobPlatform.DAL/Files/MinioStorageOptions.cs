namespace JobPlatform.DAL.Files;

public sealed class MinioStorageOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "recruitment-files";
    public bool UseSsl { get; set; }
}
