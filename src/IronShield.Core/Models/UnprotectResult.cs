namespace IronShield.Core.Models;

public sealed class UnprotectResult
{
    public required byte[] Data { get; init; }

    public PublicMetadata? Metadata { get; init; }
}
