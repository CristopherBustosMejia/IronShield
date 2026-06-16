using IronShield.Core.Interfaces;
using IronShield.Core.Models;

namespace IronShield.Storage.Factories;

public sealed class IronCryptographyContextFactory
    : IIronCryptographyContextFactory
{
    private readonly IEncryptionProvider _encryptionProvider;
    private readonly IKeyDerivationProvider _keyDerivationProvider;

    public IronCryptographyContextFactory(IEncryptionProvider encryptionProvider, IKeyDerivationProvider keyDerivationProvider)
    {
        _encryptionProvider = encryptionProvider;
        _keyDerivationProvider = keyDerivationProvider;
    }

    public IronCryptographyContext Create(String password)
    {
        IKeyDerivationParameters parameters = _keyDerivationProvider.CreateParameters();

        byte[] key = _keyDerivationProvider.DeriveKey(password, parameters);

        EncryptionInfo encryptionInfo = new()
        {
            EncryptionAlgorithm = _encryptionProvider.Algorithm,
            KeyDerivationParameters = parameters
        };

        return new IronCryptographyContext
        {
            EncryptionKey = key,
            EncryptionInfo = encryptionInfo
        };
    }
}