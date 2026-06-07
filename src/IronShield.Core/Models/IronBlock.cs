using IronShield.Core.Enums;

namespace IronShield.Core.Models;

public sealed class IronBlock
{
    public required IronBlockType Type { get; init; }

    public required bool IsEncrypted { get; init; }

    public required byte[] Data { get; init; }
}