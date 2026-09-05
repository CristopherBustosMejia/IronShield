namespace IronShield.Core.Models;

public sealed class IntegrityVerificationResult
{
    public required bool IsAvailable { get; init; }

    public required bool IsValid { get; init; }

    public String? HashAlgorithm { get; init; }
}