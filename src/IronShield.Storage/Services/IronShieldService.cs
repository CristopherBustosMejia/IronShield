using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Storage.Factories;
using IronShield.Storage.Serialization;
using IronShield.Storage.Sources;

namespace IronShield.Storage.Services;

public sealed class IronShieldService : IIronShieldService
{
    private readonly IIronProtectionService _protection;
    private readonly IIronUnprotectionService _unprotection;
    private readonly IIronIntegrityVerificationService _verification;

    public IronShieldService(
        IIronProtectionService protection,
        IIronUnprotectionService unprotection,
        IIronIntegrityVerificationService verification)
    {
        _protection = protection;
        _unprotection = unprotection;
        _verification = verification;
    }

    public IronShieldService(
        IHashProvider hashProvider,
        IEncryptionProvider encryptionProvider,
        IKeyDerivationProvider keyDerivationProvider,
        IIronBlockSerializer serializer,
        IIronEncryptionProfile profile)
    {
        var blockFactory = new IronBlockDataFactory(hashProvider);
        var cryptoContextFactory = new IronCryptographyContextFactory(
            encryptionProvider, keyDerivationProvider);
        var containerFactory = new IronContainerFactory(
            serializer, encryptionProvider, profile);
        var reader = new IronContainerReader();

        _protection = new IronProtectionService(
            blockFactory,
            cryptoContextFactory,
            containerFactory,
            new IronContainerWriter());

        _unprotection = new IronUnprotectionService(
            reader,
            serializer,
            encryptionProvider,
            keyDerivationProvider);

        _verification = new IronIntegrityVerificationService(
            reader,
            serializer,
            encryptionProvider,
            keyDerivationProvider,
            hashProvider);
    }

    public void Protect(IDataSource source, string password, Stream output)
        => _protection.Protect(source, password, output);

    public UnprotectResult Unprotect(Stream input, string password)
        => _unprotection.Unprotect(input, password);

    public IntegrityVerificationResult Verify(Stream input, string password)
        => _verification.Verify(input, password);
}
