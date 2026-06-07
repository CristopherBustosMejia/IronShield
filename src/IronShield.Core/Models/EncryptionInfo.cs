namespace IronShield.Core.Models;

public sealed class EncryptionInfo
{
    public required String EncryptionAlgorithm { get; init; }
    public required String KeyDerivationAlgorithm { get; init; }
    public IReadOnlyDictionary<String, String>? Parameters { get; init; }
}