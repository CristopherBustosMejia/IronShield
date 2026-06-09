using FluentAssertions;
using IronShield.Core.Constants;
using IronShield.Core.Enums;
using IronShield.Core.Models;
using IronShield.Storage.Serialization;

namespace IronShield.Storage.Tests.Serialization;

public sealed class IronContainerWriterTests
{
    [Fact]
    public void Should_Write_Valid_Header()
    {
        IronContainer container = new()
        {
            Version = (byte)IronFileFormatVersion.V1,

            Blocks =
            [
                new IronBlock()
                {
                    Type = IronBlockType.PublicMetadata,
                    IsEncrypted = false,
                    Data = [1, 2]
                }
            ]
        };

        IronContainerWriter writer = new();

        using MemoryStream stream = new();

        writer.Write(container, stream);

        byte[] bytes = stream.ToArray();

        bytes[0].Should().Be((byte)'I');
        bytes[1].Should().Be((byte)'R');
        bytes[2].Should().Be((byte)'O');
        bytes[3].Should().Be((byte)'N');

        bytes[4].Should().Be((byte)IronFileFormatVersion.V1);
    }

    [Fact]
    public void Should_Write_Block_Count()
    {
        IronContainer container = new()
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

        IronContainerWriter writer = new();

        using MemoryStream stream = new();

        writer.Write(container, stream);

        byte[] bytes = stream.ToArray();

        int blockCount = BitConverter.ToInt32(bytes, 5);

        blockCount.Should().Be(1);
    }

    [Fact]
    public void Should_Write_Block_Metadata()
    {
        IronContainer container = new()
        {
            Version = (byte)IronFileFormatVersion.V1,

            Blocks =
            [
                new IronBlock
                {
                    Type = IronBlockType.PublicMetadata,
                    IsEncrypted = true,
                    Data = [10, 20, 30]
                }
            ]
        };

        IronContainerWriter writer = new();

        using MemoryStream stream = new();

        writer.Write(container, stream);

        byte[] bytes = stream.ToArray();

        bytes[9].Should().Be((byte)IronBlockType.PublicMetadata);

        bytes[10].Should().Be(1);

        BitConverter.ToInt32(bytes, 11).Should().Be(3);
    }
}