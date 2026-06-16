using FluentAssertions;
using IronShield.Core.Enums;
using IronShield.Core.Models;
using IronShield.Storage.Serialization;

namespace IronShield.Storage.Tests.Serialization;

public sealed class JsonIronBlockSerializerTests
{
    private readonly JsonIronBlockSerializer _serializer = new();
    [Fact]
    public void Should_Serialize_And_Deserialize_PublicMetadata()
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
    public void Should_Serialize_And_Deserialize_EncryptedPayload()
    {
        EncryptedPayload payload = new EncryptedPayload
        {
            CipherText = [10, 20, 30],

            Parameters =
            [
                new EncryptionParameter
                {
                    Name = "Salt",
                    Value = [1, 2, 3]
                },

            new EncryptionParameter
                {
                    Name = "Nonce",
                    Value = [4, 5, 6]
                }
            ]
        };

        byte[] bytes = _serializer.Serialize(payload);

        EncryptedPayload result = _serializer.Deserialize<EncryptedPayload>(bytes);

        result.Should().BeEquivalentTo(payload);
    }

    [Fact]
    public void Should_Serialize_And_Deserialize_IronBlock()
    {
        IronBlock block = new IronBlock
        {
            Type = IronBlockType.PublicMetadata,
            IsEncrypted = false,
            Data = [1, 2, 3, 4]
        };

        byte[] bytes = _serializer.Serialize(block);

        IronBlock result = _serializer.Deserialize<IronBlock>(bytes);

        result.Should().BeEquivalentTo(block);
    }

    [Fact]
    public void Should_Serialize_And_Deserialize_IronContainer()
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

        byte[] bytes = _serializer.Serialize(container);

        IronContainer result = _serializer.Deserialize<IronContainer>(bytes);

        result.Should().BeEquivalentTo(container);
    }

    [Fact]
    public void Serialize_Should_Return_Non_Empty_Data()
    {
        IntegrityData integrity = new IntegrityData
        {
            HashAlgorithm = "SHA-256",
            Hash = [1, 2, 3]
        };

        byte[] bytes = _serializer.Serialize(integrity);

        bytes.Should().NotBeEmpty();
    }
}