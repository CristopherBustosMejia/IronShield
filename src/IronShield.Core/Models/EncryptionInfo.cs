using IronShield.Core.Interfaces;
using IronShield.Core.Attributes;
using IronShield.Core.Enums;

namespace IronShield.Core.Models;

[IronBlock(IronBlockType.EncryptionInfo)]
public sealed class EncryptionInfo : IIronBlockData
{
    public required String EncryptionAlgorithm { get; init; }
    public required IKeyDerivationParameters KeyDerivationParameters { get; init; }
}