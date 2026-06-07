namespace IronShield.Core.Models;

public sealed class IntegrityData
{
    public required String HashAlgorithm { get; init; }
    public required byte[] Hash { get; init; }
}