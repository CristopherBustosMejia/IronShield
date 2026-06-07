namespace IronShield.Core.Models;

public sealed class PublicMetadata
{
    public required String OriginalFileName { get; init; }
    public required long OriginalFileSize { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
    public required AuthorInfo AuthorInfo { get; init; }
}

public sealed class AuthorInfo
{
    public required String CreatedBy { get; init; }
    public required String ApplicationName { get; init; }
    public required String ApplicationVersion { get; init; }
}