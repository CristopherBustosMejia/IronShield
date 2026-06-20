using FluentAssertions;
using IronShield.Core.Enums;
using IronShield.Core.Models;
using IronShield.Core.Constants;
using IronShield.Storage.Serialization;

namespace IronShield.Storage.Tests.Serialization;

public sealed class IronContainerReaderTest
{
    [Fact]
    public void Should_Read_Container_Written_By_Writer()
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
                },
                new IronBlock
                {
                    Type = IronBlockType.FileContent,
                    IsEncrypted = true,
                    Data = [4, 5, 6]
                }
            ]
        };

        IronContainerWriter writer = new IronContainerWriter();
        IronContainerReader reader = new IronContainerReader();

        using MemoryStream stream = new MemoryStream();

        writer.Write(container,stream);

        stream.Position = 0;

        byte[] raw = stream.ToArray();

        Console.WriteLine(Convert.ToHexString(raw));

        IronContainer result = reader.Read(stream);

        result.Should().BeEquivalentTo(container);
    }

    [Fact]
    public void Should_Throw_When_Magic_Is_Invalid()
    {
        byte[] bytes =
        [
            (byte) 'B',
            (byte) 'A',
            (byte) 'D',
            (byte) '!',
            1,
            0,0,0,
        ];

        IronContainerReader reader = new IronContainerReader();

        using MemoryStream stream = new MemoryStream(bytes);

        byte[] raw = stream.ToArray();

        Console.WriteLine(Convert.ToHexString(raw));

        Action action = () => reader.Read(stream);

        action.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void Should_Throw_When_Block_Data_Is_Truncated()
    {
        byte[] bytes =
        [
            (byte) 'I',
            (byte) 'R',
            (byte) 'O',
            (byte) 'N',
            1,
            1,0,0,0,
            (byte)IronBlockType.PublicMetadata,
            0,
            10,0,0,0,
            1,2,3
        ];

        IronContainerReader reader = new IronContainerReader();

        using MemoryStream stream = new MemoryStream(bytes);

        Action action = () => reader.Read(stream);

        action.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void Should_Skip_Unknown_Block_Type()
    {
        byte[] bytes =
        [
            (byte)'I',
            (byte)'R',
            (byte)'O',
            (byte)'N',
            1,
            2,0,0,0,
            99,
            0,
            3,0,0,0,
            1,2,3,
            (byte)IronBlockType.PublicMetadata,
            0,
            2,0,0,0,
            4,5
        ];

        IronContainerReader reader = new();
        using MemoryStream stream = new(bytes);

        IronContainer result = reader.Read(stream);

        result.Blocks.Should().HaveCount(1);
        result.Blocks.Single().Type.Should().Be(IronBlockType.PublicMetadata);
        result.Blocks.Single().Data.Should().BeEquivalentTo(new byte[] { 4, 5 });
    }
}