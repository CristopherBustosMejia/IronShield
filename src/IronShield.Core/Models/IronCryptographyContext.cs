namespace IronShield.Core.Models;

public sealed class IronCryptographyContext
{
    public required byte[] EncryptionKey { get; init; }

    public required EncryptionInfo EncryptionInfo { get; init; }
}