namespace IronShield.Core.Models;
public sealed class EncryptedPayload
{
    public required byte[] CipherText { get; init; }

    public required IReadOnlyCollection<EncryptionParameter> Parameters
    {
        get;
        init;
    }
}
public sealed class EncryptionParameter
{
    public required string Name { get; init; }

    public required byte[] Value { get; init; }
}