using FluentAssertions;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Storage.Sources;

namespace IronShield.Storage.Tests.Sources;

public sealed class IronBlockDataFactoryTests
{
    [Fact]
    public void Should_Produce_PublicMetadata_Block()
    {
        var factory = new IronBlockDataFactory();
        var source = new TestDataSource("secret.env", [1, 2, 3]);

        IReadOnlyCollection<IIronBlockData> blocks = factory.Create(source);

        blocks.Should().ContainSingle(b => b is PublicMetadata);
    }

    [Fact]
    public void Should_Produce_FileContent_Block()
    {
        var factory = new IronBlockDataFactory();
        var source = new TestDataSource("secret.env", [1, 2, 3]);

        IReadOnlyCollection<IIronBlockData> blocks = factory.Create(source);

        blocks.Should().ContainSingle(b => b is FileContent);
    }

    [Fact]
    public void Should_Set_Correct_OriginalFileName_In_Metadata()
    {
        var factory = new IronBlockDataFactory();
        var source = new TestDataSource("secret.env", [1, 2, 3]);

        IReadOnlyCollection<IIronBlockData> blocks = factory.Create(source);

        PublicMetadata metadata = (PublicMetadata)blocks.Single(b => b is PublicMetadata);
        metadata.OriginalFileName.Should().Be("secret.env");
    }

    [Fact]
    public void Should_Set_Correct_OriginalFileSize()
    {
        var factory = new IronBlockDataFactory();
        var source = new TestDataSource("data.bin", [1, 2, 3, 4, 5]);

        IReadOnlyCollection<IIronBlockData> blocks = factory.Create(source);

        PublicMetadata metadata = (PublicMetadata)blocks.Single(b => b is PublicMetadata);
        metadata.OriginalFileSize.Should().Be(5);
    }

    [Fact]
    public void Should_Store_Correct_Data_In_FileContent()
    {
        byte[] expected = [10, 20, 30, 40];
        var factory = new IronBlockDataFactory();
        var source = new TestDataSource("data.bin", expected);

        IReadOnlyCollection<IIronBlockData> blocks = factory.Create(source);

        FileContent content = (FileContent)blocks.Single(b => b is FileContent);
        content.Content.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Produce_IntegrityData_When_HashProvider_Is_Provided()
    {
        var source = new TestDataSource("file.txt", [1, 2, 3]);
        var factory = new IronBlockDataFactory(new FixedHashProvider());

        IReadOnlyCollection<IIronBlockData> blocks = factory.Create(source);

        blocks.Should().ContainSingle(b => b is IntegrityData);
    }

    [Fact]
    public void Should_Not_Produce_IntegrityData_Without_HashProvider()
    {
        var factory = new IronBlockDataFactory();
        var source = new TestDataSource("file.txt", [1, 2, 3]);

        IReadOnlyCollection<IIronBlockData> blocks = factory.Create(source);

        blocks.Should().NotContain(b => b is IntegrityData);
    }

    [Fact]
    public void Should_Set_Correct_Hash_In_IntegrityData()
    {
        var source = new TestDataSource("file.txt", [1, 2, 3]);
        var factory = new IronBlockDataFactory(new FixedHashProvider());

        IReadOnlyCollection<IIronBlockData> blocks = factory.Create(source);

        IntegrityData integrity = (IntegrityData)blocks.Single(b => b is IntegrityData);
        integrity.HashAlgorithm.Should().Be("TEST-256");
        integrity.Hash.Should().BeEquivalentTo(new byte[] { 0xAA, 0xBB });
    }

    [Fact]
    public void Should_Throw_When_Source_Is_Null()
    {
        var factory = new IronBlockDataFactory();

        Action action = () => factory.Create(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    private sealed class TestDataSource : IDataSource
    {
        public String Name { get; }
        public long Length => Data.Length;
        public byte[] Data { get; }

        public TestDataSource(String name, byte[] data)
        {
            Name = name;
            Data = data;
        }

        public Stream OpenRead() => new MemoryStream(Data);
    }

    private sealed class FixedHashProvider : IHashProvider
    {
        public String Algorithm => "TEST-256";

        public byte[] ComputeHash(byte[] data)
        {
            return [0xAA, 0xBB];
        }
    }
}
