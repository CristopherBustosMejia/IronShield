using FluentAssertions;
using IronShield.Core.Enums;
using IronShield.Core.Exceptions;
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

namespace IronShield.Storage.Tests.Services;

public sealed class IronIntegrityVerificationServiceTests
{
    private readonly SecureRandomProvider _random = new();
    private readonly AesGcmEncryptionProvider _encryptionProvider;
    private readonly Argon2idKeyDerivationProvider _keyDerivationProvider;
    private readonly Sha256HashProvider _hashProvider = new();
    private readonly BinaryIronBlockSerializer _serializer = new();
    private readonly IronIntegrityVerificationService _service;

    public IronIntegrityVerificationServiceTests()
    {
        _encryptionProvider = new AesGcmEncryptionProvider(_random);
        _keyDerivationProvider = new Argon2idKeyDerivationProvider(_random);

        _service = new IronIntegrityVerificationService(
            new IronContainerReader(),
            _serializer,
            _encryptionProvider,
            _keyDerivationProvider,
            _hashProvider);
    }

    [Fact]
    public void Should_Verify_Valid_File()
    {
        IronCryptographyContext context = CreateContext("password");
        byte[] content = [1, 2, 3];

        using MemoryStream stream = WriteContainer(context,
            new FileContent { Content = content },
            new IntegrityData
            {
                HashAlgorithm = "SHA-256",
                Hash = _hashProvider.ComputeHash(content)
            });

        IntegrityVerificationResult result = _service.Verify(stream, "password");

        result.IsAvailable.Should().BeTrue();
        result.IsValid.Should().BeTrue();
        result.HashAlgorithm.Should().Be("SHA-256");
    }

    [Fact]
    public void Should_Detect_Hash_Mismatch()
    {
        IronCryptographyContext context = CreateContext("password");
        byte[] content = [1, 2, 3];
        byte[] wrongHash = new byte[32];
        wrongHash[0] = 0xFF;

        using MemoryStream stream = WriteContainer(context,
            new FileContent { Content = content },
            new IntegrityData { HashAlgorithm = "SHA-256", Hash = wrongHash });

        IntegrityVerificationResult result = _service.Verify(stream, "password");

        result.IsAvailable.Should().BeTrue();
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Return_Not_Available_When_Integrity_Data_Is_Missing()
    {
        IronCryptographyContext context = CreateContext("password");

        using MemoryStream stream = WriteContainer(context,
            new FileContent { Content = [1, 2, 3] });

        IntegrityVerificationResult result = _service.Verify(stream, "password");

        result.IsAvailable.Should().BeFalse();
        result.IsValid.Should().BeFalse();
        result.HashAlgorithm.Should().BeNull();
    }

    [Fact]
    public void Should_Not_Be_Valid_When_Hash_Algorithm_Is_Unsupported()
    {
        IronCryptographyContext context = CreateContext("password");

        using MemoryStream stream = WriteContainer(context,
            new FileContent { Content = [1, 2, 3] },
            new IntegrityData { HashAlgorithm = "MD5", Hash = new byte[16] });

        IntegrityVerificationResult result = _service.Verify(stream, "password");

        result.IsAvailable.Should().BeTrue();
        result.IsValid.Should().BeFalse();
        result.HashAlgorithm.Should().Be("MD5");
    }

    [Fact]
    public void Should_Not_Be_Valid_When_Hash_Length_Differs()
    {
        IronCryptographyContext context = CreateContext("password");

        using MemoryStream stream = WriteContainer(context,
            new FileContent { Content = [1, 2, 3] },
            new IntegrityData { HashAlgorithm = "SHA-256", Hash = new byte[16] });

        IntegrityVerificationResult result = _service.Verify(stream, "password");

        result.IsAvailable.Should().BeTrue();
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Throw_On_Wrong_Password()
    {
        IronCryptographyContext context = CreateContext("password");
        byte[] content = [1, 2, 3];

        using MemoryStream stream = WriteContainer(context,
            new FileContent { Content = content },
            new IntegrityData
            {
                HashAlgorithm = "SHA-256",
                Hash = _hashProvider.ComputeHash(content)
            });

        Action action = () => _service.Verify(stream, "wrong");

        action.Should().Throw<IronPasswordException>();
    }

    [Fact]
    public void Should_Throw_When_Encryption_Info_Is_Missing()
    {
        IronBlock fileContent = new IronBlock
        {
            Type = IronBlockType.FileContent,
            IsEncrypted = false,
            Data = _serializer.Serialize(new FileContent { Content = [1, 2, 3] })
        };

        using MemoryStream stream = WriteRawContainer(fileContent);

        Action action = () => _service.Verify(stream, "password");

        action.Should().Throw<IronFormatException>();
    }

    [Fact]
    public void Should_Throw_When_File_Content_Is_Missing()
    {
        using MemoryStream stream = WriteContainer(CreateContext("password"),
            new IntegrityData { HashAlgorithm = "SHA-256", Hash = new byte[32] });

        Action action = () => _service.Verify(stream, "password");

        action.Should().Throw<IronFormatException>();
    }

    [Fact]
    public void Should_Throw_On_Invalid_Magic()
    {
        using MemoryStream stream = new("BAD!"u8.ToArray());

        Action action = () => _service.Verify(stream, "password");

        action.Should().Throw<IronFormatException>();
    }

    [Fact]
    public void Should_Throw_When_Input_Is_Null()
    {
        Action action = () => _service.Verify(null!, "password");

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Should_Throw_When_Password_Is_Null()
    {
        using MemoryStream stream = new();

        Action action = () => _service.Verify(stream, null!);

        action.Should().Throw<ArgumentNullException>();
    }

    private IronCryptographyContext CreateContext(String password)
    {
        IKeyDerivationParameters parameters = _keyDerivationProvider.CreateParameters();

        return new IronCryptographyContext
        {
            EncryptionKey = _keyDerivationProvider.DeriveKey(password, parameters),
            EncryptionInfo = new EncryptionInfo
            {
                EncryptionAlgorithm = "AES-256-GCM",
                KeyDerivationParameters = parameters
            }
        };
    }

    private MemoryStream WriteContainer(IronCryptographyContext context, params IIronBlockData[] data)
    {
        var factory = new IronContainerFactory(
            _serializer,
            _encryptionProvider,
            new DefaultIronEncryptionProfile());

        IronContainer container = factory.Create((byte)IronFileFormatVersion.V1, data, context);

        var stream = new MemoryStream();
        new IronContainerWriter().Write(container, stream);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream WriteRawContainer(params IronBlock[] blocks)
    {
        IronContainer container = new IronContainer
        {
            Version = (byte)IronFileFormatVersion.V1,
            Blocks = blocks
        };

        var stream = new MemoryStream();
        new IronContainerWriter().Write(container, stream);
        stream.Position = 0;
        return stream;
    }
}