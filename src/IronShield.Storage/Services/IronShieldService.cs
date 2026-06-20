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

    public IronShieldService(
        IIronProtectionService protection,
        IIronUnprotectionService unprotection)
    {
        _protection = protection;
        _unprotection = unprotection;
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

        _protection = new IronProtectionService(
            blockFactory,
            cryptoContextFactory,
            containerFactory,
            new IronContainerWriter());

        _unprotection = new IronUnprotectionService(
            new IronContainerReader(),
            serializer,
            encryptionProvider,
            keyDerivationProvider);
    }

    public void Protect(IDataSource source, string password, Stream output)
        => _protection.Protect(source, password, output);

    public UnprotectResult Unprotect(Stream input, string password)
        => _unprotection.Unprotect(input, password);
}
