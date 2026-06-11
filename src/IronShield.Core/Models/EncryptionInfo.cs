using IronShield.Core.Interfaces;

namespace IronShield.Core.Models;

public sealed class EncryptionInfo
{
    public required String EncryptionAlgorithm { get; init; }
    public required IKeyderivationParameters KeyderivationParameters { get; init; }
}