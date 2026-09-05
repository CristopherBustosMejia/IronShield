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

public sealed class IronUnprotectionServiceTests
{
    private readonly IronUnprotectionService _service;
    private readonly IronProtectionService _protection;

    public IronUnprotectionServiceTests()
    {
        var random = new SecureRandomProvider();
        var encryptionProvider = new AesGcmEncryptionProvider(random);
        var keyDerivationProvider = new Argon2idKeyDerivationProvider(random);
        var serializer = new BinaryIronBlockSerializer();

        _service = new IronUnprotectionService(
            new IronContainerReader(),
            serializer,
            encryptionProvider,
            keyDerivationProvider);

        _protection = new IronProtectionService(
            new IronBlockDataFactory(new Sha256HashProvider()),
            new IronCryptographyContextFactory(encryptionProvider, keyDerivationProvider),
            new IronContainerFactory(serializer, encryptionProvider, new DefaultIronEncryptionProfile()),
            new IronContainerWriter());
    }

    [Fact]
    public void Should_Return_Correct_Data()
    {
        byte[] original = "hello"u8.ToArray();
        using var protectedStream = new MemoryStream();
        _protection.Protect(new MemoryDataSource("f.txt", original), "pass", protectedStream);

        protectedStream.Position = 0;
        UnprotectResult result = _service.Unprotect(protectedStream, "pass");

        result.Data.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Should_Return_Metadata()
    {
        byte[] original = [1, 2, 3];
        using var protectedStream = new MemoryStream();
        _protection.Protect(new MemoryDataSource("data.bin", original), "pass", protectedStream);

        protectedStream.Position = 0;
        UnprotectResult result = _service.Unprotect(protectedStream, "pass");

        result.Metadata.Should().NotBeNull();
        result.Metadata!.OriginalFileName.Should().Be("data.bin");
    }

    [Fact]
    public void Should_Throw_On_Wrong_Password()
    {
        byte[] original = [1, 2, 3];
        using var protectedStream = new MemoryStream();
        _protection.Protect(new MemoryDataSource("f.txt", original), "correct", protectedStream);

        protectedStream.Position = 0;
        Action action = () => _service.Unprotect(protectedStream, "wrong");

        action.Should().Throw<Exception>();
    }

    [Fact]
    public void Should_Throw_On_Null_Input()
    {
        Action action = () => _service.Unprotect(null!, "pass");
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Should_Throw_On_Null_Password()
    {
        Action action = () => _service.Unprotect(new MemoryStream(), null!);
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
