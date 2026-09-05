using IronShield.Core.Interfaces;
using IronShield.Core.Profiles;
using IronShield.Cryptography.Random;
using IronShield.Cryptography.Hashing;
using IronShield.Cryptography.Encryption;
using IronShield.Cryptography.KeyDerivation;
using IronShield.Storage.Serialization;
using IronShield.Storage.Services;

namespace IronShield.Cli.Composition;

internal static class DependencyInjection
{
    public static IIronShieldService CreateService()
    {
        var random = new SecureRandomProvider();

        return new IronShieldService(
            new Sha256HashProvider(),
            new AesGcmEncryptionProvider(random),
            new Argon2idKeyDerivationProvider(random),
            new BinaryIronBlockSerializer(),
            new DefaultIronEncryptionProfile());
    }
}
