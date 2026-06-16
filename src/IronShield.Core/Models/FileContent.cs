using IronShield.Core.Interfaces;
using IronShield.Core.Attributes;
using IronShield.Core.Enums;

namespace IronShield.Core.Models;

[IronBlock(IronBlockType.FileContent)]
public sealed class FileContent : IIronBlockData
{
    public required byte[] Content { get; init; }
}