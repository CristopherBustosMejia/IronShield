using FluentAssertions;
using IronShield.Core.Models;
using IronShield.Storage.Serialization;

namespace IronShield.Storage.Tests.Serialization;

public sealed class BinaryIronBlockSerializerTests
{
    private readonly BinaryIronBlockSerializer _serializer = new();

    [Fact]
    public void Should_Serialize_And_Deserialize_PublicMetadata()
    {
        PublicMetadata metadata = new PublicMetadata
        {
            OriginalFileName = "secret.env",
            OriginalFileSize = 1024,
            CreatedUtc = DateTimeOffset.UtcNow.AddMinutes(-90),

            AuthorInfo = new AuthorInfo
            {
                CreatedBy = "crisred",
                ApplicationName = "IronShield CLI",
                ApplicationVersion = "1.0.0"
            }
        };

        byte[] bytes = _serializer.Serialize(metadata);

        PublicMetadata result = _serializer.Deserialize<PublicMetadata>(bytes);

        result.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public void Should_Serialize_And_Deserialize_IntegrityData()
    {
        IntegrityData integrity = new IntegrityData
        {
            HashAlgorithm = "SHA-256",
            Hash = [1, 2, 3, 4, 5]
        };

        byte[] bytes = _serializer.Serialize(integrity);

        IntegrityData result = _serializer.Deserialize<IntegrityData>(bytes);

        result.Should().BeEquivalentTo(integrity);
    }

    [Fact]
    public void Should_Serialize_And_Deserialize_EncryptionInfo()
    {
        EncryptionInfo encryption = new EncryptionInfo
        {
            EncryptionAlgorithm = "AES-256-GCM",

            KeyDerivationParameters = new Argon2idParameters
            {
                Salt = [1],
                MemorySizeKb = 65536,
                Iterations = 4,
                Parallelism = 2,
                KeySize = 32
            }
        };

        byte[] bytes = _serializer.Serialize(encryption);

        EncryptionInfo result = _serializer.Deserialize<EncryptionInfo>(bytes);

        result.Should().BeEquivalentTo(encryption);
    }

    [Fact]
    public void Should_Serialize_And_Deserialize_FileContent()
    {
        FileContent content = new FileContent
        {
            Content = [10, 20, 30, 255, 0, 128]
        };

        byte[] bytes = _serializer.Serialize(content);

        FileContent result = _serializer.Deserialize<FileContent>(bytes);

        result.Should().BeEquivalentTo(content);
    }

    [Fact]
    public void Should_Serialize_And_Deserialize_EncryptedPayload()
    {
        EncryptedPayload payload = new EncryptedPayload
        {
            CipherText = [10, 20, 30],

            Parameters =
            [
                new EncryptionParameter
                {
                    Name = "Nonce",
                    Value = [1, 2, 3]
                },

            new EncryptionParameter
                {
                    Name = "Tag",
                    Value = [4, 5, 6, 7, 8]
                }
            ]
        };

        byte[] bytes = _serializer.Serialize(payload);

        EncryptedPayload result = _serializer.Deserialize<EncryptedPayload>(bytes);

        result.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public void Should_Serialize_FileContent_Without_Base64_Overhead()
    {
        FileContent content = new FileContent
        {
            Content = [1, 2, 3]
        };

        byte[] bytes = _serializer.Serialize(content);

        bytes.Length.Should().Be(4 + 3);
    }

    [Fact]
    public void Should_Produce_Compact_PublicMetadata()
    {
        PublicMetadata metadata = new PublicMetadata
        {
            OriginalFileName = "secret.env",
            OriginalFileSize = 1024,
            CreatedUtc = DateTimeOffset.UtcNow,

            AuthorInfo = new AuthorInfo
            {
                CreatedBy = "crisred",
                ApplicationName = "IronShield CLI",
                ApplicationVersion = "1.0.0"
            }
        };

        byte[] bytes = _serializer.Serialize(metadata);

        bytes.Length.Should().BeLessThan(80);
    }

    [Fact]
    public void Should_Produce_Compact_IntegrityData()
    {
        IntegrityData integrity = new IntegrityData
        {
            HashAlgorithm = "SHA-256",
            Hash = new byte[32]
        };

        byte[] bytes = _serializer.Serialize(integrity);

        bytes.Length.Should().BeLessThan(50);
    }

    [Fact]
    public void Should_Produce_Compact_EncryptionInfo()
    {
        EncryptionInfo encryption = new EncryptionInfo
        {
            EncryptionAlgorithm = "AES-256-GCM",

            KeyDerivationParameters = new Argon2idParameters
            {
                Salt = new byte[32],
                MemorySizeKb = 65536,
                Iterations = 4,
                Parallelism = 2,
                KeySize = 32
            }
        };

        byte[] bytes = _serializer.Serialize(encryption);

        bytes.Length.Should().BeLessThan(70);
    }

    [Fact]
    public void Should_Produce_Compact_EncryptedPayload()
    {
        EncryptedPayload payload = new EncryptedPayload
        {
            CipherText = [1, 2, 3],

            Parameters =
            [
                new EncryptionParameter
                {
                    Name = "Nonce",
                    Value = new byte[12]
                },

            new EncryptionParameter
                {
                    Name = "Tag",
                    Value = new byte[16]
                }
            ]
        };

        byte[] bytes = _serializer.Serialize(payload);

        bytes.Length.Should().BeLessThan(70);
    }

    [Fact]
    public void Serialize_Should_Throw_When_Value_Is_Null()
    {
        PublicMetadata? metadata = null;

        Action action = () => _serializer.Serialize(metadata!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialize_Should_Throw_When_Data_Is_Null()
    {
        byte[]? data = null;

        Action action = () => _serializer.Deserialize<PublicMetadata>(data!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deserialize_Should_Throw_When_Data_Is_Truncated()
    {
        FileContent content = new FileContent
        {
            Content = [1, 2, 3, 4, 5]
        };

        byte[] bytes = _serializer.Serialize(content);
        byte[] truncated = bytes[..^2];

        Action action = () => _serializer.Deserialize<FileContent>(truncated);

        action.Should().Throw<InvalidDataException>();
    }
}