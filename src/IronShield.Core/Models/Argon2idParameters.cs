using IronShield.Core.Interfaces;

public sealed class Argon2idParameters : IKeyderivationParameters
{
    public String Algorithm => "Argon2id";
    public required int MemorySizeKb { get; init; }
    public required int Iterations { get; init; }

    public required int Parallelism { get; init; }

    public required int KeySize { get; init; }
}