using FluentAssertions;
using IronShield.Core.Enums;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Core.Profiles;
using IronShield.Cryptography.Encryption;
using IronShield.Cryptography.Hashing;
using IronShield.Cryptography.KeyDerivation;
using IronShield.Cryptography.Random;
using IronShield.Storage.Factories;
using IronShield.Storage.Serialization;
using IronShield.Storage.Services;
using IronShield.Storage.Sources;

namespace IronShield.Storage.Tests.Services;

public sealed class IronProtectionServiceTests
{
    private readonly IronProtectionService _service;

    public IronProtectionServiceTests()
    {
        var random = new SecureRandomProvider();
        var hashProvider = new Sha256HashProvider();
        var encryptionProvider = new AesGcmEncryptionProvider(random);
        var keyDerivationProvider = new Argon2idKeyDerivationProvider(random);
        var serializer = new JsonIronBlockSerializer();

        _service = new IronProtectionService(
            new IronBlockDataFactory(hashProvider),
            new IronCryptographyContextFactory(encryptionProvider, keyDerivationProvider),
            new IronContainerFactory(serializer, encryptionProvider, new DefaultIronEncryptionProfile()),
            new IronContainerWriter());
    }

    [Fact]
    public void Should_Produce_Valid_Container()
    {
        var source = new MemoryDataSource("test.txt", [1, 2, 3]);
        using var output = new MemoryStream();

        _service.Protect(source, "password", output);

        output.ToArray()[..4].Should().BeEquivalentTo("IRON"u8.ToArray());
    }

    [Fact]
    public void Should_Throw_When_Source_Is_Null()
    {
        Action action = () => _service.Protect(null!, "pass", Stream.Null);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Should_Throw_When_Password_Is_Null()
    {
        var source = new MemoryDataSource("f.txt", [1]);
        Action action = () => _service.Protect(source, null!, Stream.Null);
        action.Should().Throw<ArgumentNullException>();
    }

    private sealed class MemoryDataSource : IDataSource
    {
        public string Name { get; }
        public long Length => Data.Length;
        public byte[] Data { get; }

        public MemoryDataSource(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }

        public Stream OpenRead() => new MemoryStream(Data);
    }
}
