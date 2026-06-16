using IronShield.Core.Enums;
using IronShield.Core.Interfaces;
using IronShield.Core.Attributes;

namespace IronShield.Core.Models;

[IronBlock(IronBlockType.IntegrityData)]
public sealed class IntegrityData : IIronBlockData
{
    public required String HashAlgorithm { get; init; }
    public required byte[] Hash { get; init; }
}