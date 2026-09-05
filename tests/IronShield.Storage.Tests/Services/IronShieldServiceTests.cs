using FluentAssertions;
using IronShield.Core.Enums;
using IronShield.Core.Models;
using IronShield.Cryptography.Encryption;
using IronShield.Cryptography.Hashing;
using IronShield.Cryptography.KeyDerivation;
using IronShield.Cryptography.Random;
using IronShield.Core.Interfaces;
using IronShield.Core.Profiles;
using IronShield.Storage.Serialization;
using IronShield.Storage.Services;

namespace IronShield.Storage.Tests.Services;

public sealed class IronShieldServiceTests
{
    private readonly IronShieldService _service;

    public IronShieldServiceTests()
    {
        var random = new SecureRandomProvider();

        _service = new IronShieldService(
            new Sha256HashProvider(),
            new AesGcmEncryptionProvider(random),
            new Argon2idKeyDerivationProvider(random),
            new BinaryIronBlockSerializer(),
            new DefaultIronEncryptionProfile());
    }

    [Fact]
    public void Should_Protect_And_Unprotect_Roundtrip()
    {
        byte[] original = "Hello, IronShield!"u8.ToArray();
        var source = new MemoryDataSource("secret.txt", original);

        using var protectedStream = new MemoryStream();

        _service.Protect(source, "s3cr3t", protectedStream);

        protectedStream.Position = 0;
        UnprotectResult result = _service.Unprotect(protectedStream, "s3cr3t");

        result.Data.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Should_Preserve_Metadata_After_Roundtrip()
    {
        byte[] original = "test data"u8.ToArray();
        var source = new MemoryDataSource("myfile.txt", original);

        using var protectedStream = new MemoryStream();

        _service.Protect(source, "password", protectedStream);

        protectedStream.Position = 0;
        UnprotectResult result = _service.Unprotect(protectedStream, "password");

        result.Metadata.Should().NotBeNull();
        result.Metadata!.OriginalFileName.Should().Be("myfile.txt");
        result.Metadata.OriginalFileSize.Should().Be(original.Length);
    }

    [Fact]
    public void Should_Throw_On_Wrong_Password()
    {
        byte[] original = "sensitive data"u8.ToArray();
        var source = new MemoryDataSource("secret.txt", original);

        using var protectedStream = new MemoryStream();
        _service.Protect(source, "correct-password", protectedStream);

        protectedStream.Position = 0;
        Action action = () => _service.Unprotect(protectedStream, "wrong-password");

        action.Should().Throw<Exception>();
    }

    [Fact]
    public void Should_Throw_On_Missing_FileContent_Block()
    {
        IronContainer container = new IronContainer
        {
            Version = (byte)IronFileFormatVersion.V1,
            Blocks =
            [
                new IronBlock
                {
                    Type = IronBlockType.PublicMetadata,
                    IsEncrypted = false,
                    Data = [1, 2, 3]
                }
            ]
        };

        using var stream = new MemoryStream();
        new IronContainerWriter().Write(container, stream);
        stream.Position = 0;

        Action action = () => _service.Unprotect(stream, "any-password");
        action.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void Should_Protect_And_Unprotect_Large_Data()
    {
        byte[] original = new byte[100_000];
        new Random(42).NextBytes(original);

        var source = new MemoryDataSource("large.bin", original);

        using var protectedStream = new MemoryStream();
        _service.Protect(source, "p4ssw0rd", protectedStream);

        protectedStream.Position = 0;
        UnprotectResult result = _service.Unprotect(protectedStream, "p4ssw0rd");

        result.Data.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Should_Produce_Readable_Protected_Stream()
    {
        byte[] original = "readable test"u8.ToArray();
        var source = new MemoryDataSource("test.txt", original);

        using var protectedStream = new MemoryStream();
        _service.Protect(source, "pass", protectedStream);

        byte[] protectedBytes = protectedStream.ToArray();

        protectedBytes[0].Should().Be((byte)'I');
        protectedBytes[1].Should().Be((byte)'R');
        protectedBytes[2].Should().Be((byte)'O');
        protectedBytes[3].Should().Be((byte)'N');

        protectedBytes.Length.Should().BeGreaterThan(original.Length);
    }

    [Fact]
    public void Protect_Should_Throw_When_Source_Is_Null()
    {
        Action action = () => _service.Protect(null!, "pass", Stream.Null);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Protect_Should_Throw_When_Password_Is_Null()
    {
        var source = new MemoryDataSource("f.txt", [1]);
        Action action = () => _service.Protect(source, null!, Stream.Null);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Unprotect_Should_Throw_When_Input_Is_Null()
    {
        Action action = () => _service.Unprotect(null!, "pass");
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Unprotect_Should_Throw_When_Password_Is_Null()
    {
        Action action = () => _service.Unprotect(Stream.Null, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    private sealed class MemoryDataSource : IDataSource
    {
        public String Name { get; }
        public long Length => Data.Length;
        public byte[] Data { get; }

        public MemoryDataSource(String name, byte[] data)
        {
            Name = name;
            Data = data;
        }

        public Stream OpenRead() => new MemoryStream(Data);
    }
}
