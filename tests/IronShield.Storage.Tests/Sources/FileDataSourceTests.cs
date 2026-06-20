using FluentAssertions;
using IronShield.Storage.Sources;
using IronShield.Storage.Tests.Helpers;

namespace IronShield.Storage.Tests.Sources;

public sealed class FileDataSourceTests
{
    [Fact]
    public void Should_Return_File_Name()
    {
        using var temp = new TemporaryFile("test.txt");

        var source = new FileDataSource(temp.Path);

        source.Name.Should().Be("test.txt");
    }

    [Fact]
    public void Should_Return_Correct_Length()
    {
        using var temp = new TemporaryFile("test.txt", "hello world"u8.ToArray());

        var source = new FileDataSource(temp.Path);

        source.Length.Should().Be(11);
    }

    [Fact]
    public void Should_Read_File_Content()
    {
        byte[] expected = "hello world"u8.ToArray();
        using var temp = new TemporaryFile("test.txt", expected);

        var source = new FileDataSource(temp.Path);

        using Stream stream = source.OpenRead();
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        memoryStream.ToArray().Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Throw_When_File_Not_Found()
    {
        Action action = () => new FileDataSource("/nonexistent/file.txt");

        action.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void Should_Throw_When_Path_Is_Null()
    {
        Action action = () => new FileDataSource(null!);

        action.Should().Throw<ArgumentNullException>();
    }
}
