using IronShield.Core.Interfaces;
using IronShield.Core.Attributes;
using IronShield.Core.Enums;

namespace IronShield.Core.Models;

[IronBlock(IronBlockType.PublicMetadata)]
public sealed class PublicMetadata : IIronBlockData
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