using IronShield.Core.Models;

namespace IronShield.Core.Interfaces;

public interface IIronIntegrityVerificationService
{
    IntegrityVerificationResult Verify(Stream input, String password);
}