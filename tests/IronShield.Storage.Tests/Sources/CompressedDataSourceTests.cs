using System.IO.Compression;
using FluentAssertions;
using IronShield.Core.Interfaces;
using IronShield.Storage.Sources;

namespace IronShield.Storage.Tests.Sources;

public sealed class CompressedDataSourceTests
{
    [Fact]
    public void Should_Return_Name_Ending_With_Gz()
    {
        var inner = new MemoryDataSource("test.txt", [1, 2, 3]);
        var source = new CompressedDataSource(inner);

        source.Name.Should().Be("test.txt.gz");
    }

    [Fact]
    public void Should_Return_Negative_Length_Before_Read()
    {
        var inner = new MemoryDataSource("test.txt", [1, 2, 3]);
        var source = new CompressedDataSource(inner);

        source.Length.Should().Be(-1);
    }

    [Fact]
    public void Should_Produce_GZip_Data()
    {
        byte[] original = [1, 2, 3, 4, 5];
        var inner = new MemoryDataSource("test.bin", original);
        var source = new CompressedDataSource(inner);

        using Stream stream = source.OpenRead();
        using var decompressed = new MemoryStream();
        using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
        {
            gzip.CopyTo(decompressed);
        }

        decompressed.ToArray().Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Should_Cache_Stream_And_Return_Same_Data()
    {
        var inner = new MemoryDataSource("test.bin", [1, 2, 3]);
        var source = new CompressedDataSource(inner);

        using Stream first = source.OpenRead();
        using Stream second = source.OpenRead();

        byte[] firstBytes = ((MemoryStream)first).ToArray();
        byte[] secondBytes = ((MemoryStream)second).ToArray();

        firstBytes.Should().BeEquivalentTo(secondBytes);
        source.Length.Should().Be(firstBytes.Length);
    }

    [Fact]
    public void Should_Throw_When_Inner_Is_Null()
    {
        Action action = () => new CompressedDataSource(null!);

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
