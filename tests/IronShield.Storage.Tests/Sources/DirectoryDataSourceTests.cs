using System.IO.Compression;
using FluentAssertions;
using IronShield.Storage.Sources;
using IronShield.Storage.Tests.Helpers;

namespace IronShield.Storage.Tests.Sources;

public sealed class DirectoryDataSourceTests
{
    [Fact]
    public void Should_Return_Name_Ending_With_Zip()
    {
        using var temp = new TemporaryDirectory();

        var source = new DirectoryDataSource(temp.Path);

        source.Name.Should().EndWith(".zip");
    }

    [Fact]
    public void Should_Return_Negative_Length_Before_Read()
    {
        using var temp = new TemporaryDirectory();

        var source = new DirectoryDataSource(temp.Path);

        source.Length.Should().Be(-1);
    }

    [Fact]
    public void Should_Produce_Valid_Zip_Archive()
    {
        using var temp = new TemporaryDirectory();
        File.WriteAllText(Path.Combine(temp.Path, "file1.txt"), "content1");
        File.WriteAllText(Path.Combine(temp.Path, "file2.txt"), "content2");

        var source = new DirectoryDataSource(temp.Path);

        using Stream stream = source.OpenRead();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        archive.Entries.Should().HaveCount(2);
        archive.Entries.Select(e => e.Name).Should().BeEquivalentTo("file1.txt", "file2.txt");
    }

    [Fact]
    public void Should_Contain_Correct_File_Contents()
    {
        using var temp = new TemporaryDirectory();
        File.WriteAllText(Path.Combine(temp.Path, "secret.txt"), "sensitive data");

        var source = new DirectoryDataSource(temp.Path);

        using Stream stream = source.OpenRead();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        ZipArchiveEntry entry = archive.Entries.Single();
        using StreamReader reader = new StreamReader(entry.Open());
        reader.ReadToEnd().Should().Be("sensitive data");
    }

    [Fact]
    public void Should_Cache_Stream_And_Return_Same_Data()
    {
        using var temp = new TemporaryDirectory();
        File.WriteAllText(Path.Combine(temp.Path, "file.txt"), "data");

        var source = new DirectoryDataSource(temp.Path);

        using Stream first = source.OpenRead();
        using Stream second = source.OpenRead();

        byte[] firstBytes = ((MemoryStream)first).ToArray();
        byte[] secondBytes = ((MemoryStream)second).ToArray();

        firstBytes.Should().BeEquivalentTo(secondBytes);
        source.Length.Should().Be(firstBytes.Length);
    }

    [Fact]
    public void Should_Throw_When_Directory_Not_Found()
    {
        Action action = () => new DirectoryDataSource("/nonexistent/directory");

        action.Should().Throw<DirectoryNotFoundException>();
    }

    [Fact]
    public void Should_Throw_When_Path_Is_Null()
    {
        Action action = () => new DirectoryDataSource(null!);

        action.Should().Throw<ArgumentNullException>();
    }
}
