using System.Reflection;
using FluentAssertions;
using IronShield.Core.Attributes;
using IronShield.Core.Enums;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Core.Profiles;
using IronShield.Cryptography.Encryption;
using IronShield.Cryptography.Random;
using IronShield.Storage.Factories;
using IronShield.Storage.Serialization;

namespace IronShield.Storage.Tests.Factories;

public sealed class IronContainerFactoryTests
{
    [Theory]
    [InlineData(typeof(PublicMetadata), IronBlockType.PublicMetadata)]
    [InlineData(typeof(IntegrityData), IronBlockType.IntegrityData)]
    [InlineData(typeof(EncryptionInfo), IronBlockType.EncryptionInfo)]
    [InlineData(typeof(FileContent), IronBlockType.FileContent)]
    public void BlcokData_Should_Be_Associated_With_Correct_BlockType(Type modelType, IronBlockType expectedType)
    {
        IronBlockAttribute? attribute =
        modelType.GetCustomAttribute<IronBlockAttribute>();

        attribute.Should().NotBeNull();
        attribute!.Type.Should().Be(expectedType);
    }

    [Fact]
    public void Should_Create_Container_With_Correct_Version()
    {
        IronContainerFactory factory = CreateFactory();

        IronContainer container = factory.Create(
            (byte)IronFileFormatVersion.V1,
            [],
            CreateCryptographyContext());

        container.Version.Should()
            .Be((byte)IronFileFormatVersion.V1);
    }

    [Fact]
    public void Should_Include_EncryptionInfo_Block()
    {
        IronContainerFactory factory = CreateFactory();

        IronContainer container = factory.Create(
            (byte)IronFileFormatVersion.V1,
            [],
            CreateCryptographyContext());

        container.Blocks.Should()
            .Contain(x => x.Type == IronBlockType.EncryptionInfo);
    }

    [Fact]
    public void Should_Create_Block_For_Each_BlockData()
    {
        IronContainerFactory factory = CreateFactory();

        IIronBlockData[] data =
        [
            new PublicMetadata
        {
            OriginalFileName = "test.txt",
            OriginalFileSize = 100,
            CreatedUtc = DateTimeOffset.UtcNow,
            AuthorInfo = new AuthorInfo
            {
                CreatedBy = "crisred",
                ApplicationName = "IronShield",
                ApplicationVersion = "v0.1b"
            }
        },

        new FileContent
        {
            Content = [1, 2, 3]
        }
        ];

        IronContainer container = factory.Create(
            (byte)IronFileFormatVersion.V1,
            data,
            CreateCryptographyContext());

        container.Blocks.Should()
            .HaveCount(3);
    }

    [Fact]
    public void Should_Encrypt_Blocks_According_To_Default_Profile()
    {
        IronContainerFactory factory = CreateFactory();

        IIronBlockData[] data =
        [
            new PublicMetadata
        {
            OriginalFileName = "test.txt",
            OriginalFileSize = 100,
            CreatedUtc = DateTimeOffset.UtcNow,
            AuthorInfo = new AuthorInfo
            {
                CreatedBy = "crisred",
                ApplicationName = "IronShield",
                ApplicationVersion = "v0.1b"
            }
        },

        new IntegrityData
        {
            HashAlgorithm = "SHA-256",
            Hash = [1, 2, 3]
        }
        ];

        IronContainer container = factory.Create(
            (byte)IronFileFormatVersion.V1,
            data,
            CreateCryptographyContext());

        IronBlock encryptionInfo =
            container.Blocks.Single(
                x => x.Type == IronBlockType.EncryptionInfo);

        IronBlock publicMetadata =
            container.Blocks.Single(
                x => x.Type == IronBlockType.PublicMetadata);

        IronBlock integrityData =
            container.Blocks.Single(
                x => x.Type == IronBlockType.IntegrityData);

        encryptionInfo.IsEncrypted.Should().BeFalse();
        publicMetadata.IsEncrypted.Should().BeFalse();
        integrityData.IsEncrypted.Should().BeTrue();
    }

    [Fact]
    public void Should_Throw_When_Block_Type_Is_Not_Registered()
    {
        IronContainerFactory factory = CreateFactory();

        IIronBlockData[] data =
        [
            new UnknownBlockData()
        ];

        Action action = () =>
        {
            factory.Create(
                (byte)IronFileFormatVersion.V1,
                data,
                CreateCryptographyContext());
        };

        action.Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Should_Not_Encrypt_EncryptionInfo_Block()
    {
        IronContainerFactory factory = CreateFactory();

        IronContainer container = factory.Create(
            (byte)IronFileFormatVersion.V1,
            [],
            CreateCryptographyContext());

        IronBlock encryptionInfo =
            container.Blocks.Single(
                x => x.Type == IronBlockType.EncryptionInfo);

        encryptionInfo.IsEncrypted.Should()
            .BeFalse();
    }

    private static IronContainerFactory CreateFactory()
    {
        return new IronContainerFactory(
            new JsonIronBlockSerializer(),
            new AesGcmEncryptionProvider(
                new SecureRandomProvider()),
            new DefaultIronEncryptionProfile());
    }

    private static IronCryptographyContext CreateCryptographyContext()
    {
        return new IronCryptographyContext
        {
            EncryptionKey =
            [
                1, 2, 3, 4, 5, 6, 7, 8,
            9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32
            ],

            EncryptionInfo = new EncryptionInfo
            {
                EncryptionAlgorithm = "AES-256-GCM",
                KeyDerivationParameters = new Argon2idParameters
                {
                    Salt = [1],
                    MemorySizeKb = 65536,
                    Iterations = 3,
                    Parallelism = 1,
                    KeySize = 32
                }
            }
        };
    }

    private sealed class UnknownBlockData : IIronBlockData
    {
    }
}