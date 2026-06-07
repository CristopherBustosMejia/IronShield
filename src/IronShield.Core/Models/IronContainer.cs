namespace IronShield.Core.Models;

public sealed class IronContainer
{
    public required byte Version { get; init; }
    public required IReadOnlyCollection<IronBlock> Blocks { get; init; }
}