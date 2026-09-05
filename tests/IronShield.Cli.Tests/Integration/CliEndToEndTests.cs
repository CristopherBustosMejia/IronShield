using System.CommandLine;
using FluentAssertions;
using IronShield.Core.Interfaces;
using IronShield.Cli.Commands;
using IronShield.Cli.Composition;

namespace IronShield.Cli.Tests.Integration;

[Collection("CLI")]
public sealed class CliEndToEndTests : IDisposable
{
    private readonly IIronShieldService _service = DependencyInjection.CreateService();
    private readonly String _tempDir = Path.Combine(
        Path.GetTempPath(), "IronShieldCliTests", Guid.NewGuid().ToString());

    [Fact]
    public async Task Should_Protect_And_Unprotect_File()
    {
        Directory.CreateDirectory(_tempDir);

        String originalContent = "Hello IronShield! This is a test.";
        String inputFile = Path.Combine(_tempDir, "document.txt");
        String protectedFile = Path.Combine(_tempDir, "document.txt.iron");
        String restoredFile = Path.Combine(_tempDir, "restored.txt");

        File.WriteAllText(inputFile, originalContent);

        var protectCmd = ProtectCommand.Create(_service);
        var protectResult = protectCmd.Parse(
            [inputFile, "-p", "testpassword", "-o", protectedFile, "--overwrite"],
            new ParserConfiguration());
        await protectResult.InvokeAsync(new InvocationConfiguration());

        File.Exists(protectedFile).Should().BeTrue($"{protectedFile} should exist after protect");
        new FileInfo(protectedFile).Length.Should().BeGreaterThan(0);

        var unprotectCmd = UnprotectCommand.Create(_service);
        var unprotectResult = unprotectCmd.Parse(
            [protectedFile, "-p", "testpassword", "-o", restoredFile, "--overwrite"],
            new ParserConfiguration());
        await unprotectResult.InvokeAsync(new InvocationConfiguration());

        File.Exists(restoredFile).Should().BeTrue();
        String restoredContent = File.ReadAllText(restoredFile);
        restoredContent.Should().Be(originalContent);
    }

    [Fact]
    public async Task Should_Fail_With_Wrong_Password()
    {
        Directory.CreateDirectory(_tempDir);

        String inputFile = Path.Combine(_tempDir, "secret.txt");
        String protectedFile = Path.Combine(_tempDir, "secret.txt.iron");

        File.WriteAllText(inputFile, "sensitive data");

        var protectCmd = ProtectCommand.Create(_service);
        var protectResult = protectCmd.Parse(
            [inputFile, "-p", "correct", "-o", protectedFile, "--overwrite"],
            new ParserConfiguration());
        await protectResult.InvokeAsync(new InvocationConfiguration());

        var unprotectCmd = UnprotectCommand.Create(_service);
        var unprotectResult = unprotectCmd.Parse(
            [protectedFile, "-p", "wrong", "-o", Path.Combine(_tempDir, "fail.txt"), "--overwrite"],
            new ParserConfiguration());

        var action = async () => await unprotectResult.InvokeAsync(new InvocationConfiguration());
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_Not_Throw_On_Corrupted_Iron_File()
    {
        Directory.CreateDirectory(_tempDir);

        String corruptedFile = Path.Combine(_tempDir, "corrupted.iron");
        File.WriteAllBytes(corruptedFile, "not-an-iron-file!!!"u8.ToArray());

        var unprotectCmd = UnprotectCommand.Create(_service);
        var unprotectResult = unprotectCmd.Parse(
            [corruptedFile, "-p", "any", "-o", Path.Combine(_tempDir, "out.txt"), "--overwrite"],
            new ParserConfiguration());

        var action = async () => await unprotectResult.InvokeAsync(new InvocationConfiguration());
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_Protect_And_Unprotect_Directory()
    {
        Directory.CreateDirectory(_tempDir);

        String sourceDir = Path.Combine(_tempDir, "myfiles");
        Directory.CreateDirectory(sourceDir);
        await File.WriteAllTextAsync(Path.Combine(sourceDir, "a.txt"), "file A content");
        await File.WriteAllTextAsync(Path.Combine(sourceDir, "b.txt"), "file B content");

        String protectedFile = Path.Combine(_tempDir, "myfiles.iron");
        String restoredZip = Path.Combine(_tempDir, "restored.zip");

        var protectCmd = ProtectCommand.Create(_service);
        var protectResult = protectCmd.Parse(
            [sourceDir, "-p", "dirpwd", "-o", protectedFile, "--overwrite"],
            new ParserConfiguration());
        await protectResult.InvokeAsync(new InvocationConfiguration());

        File.Exists(protectedFile).Should().BeTrue();

        var unprotectCmd = UnprotectCommand.Create(_service);
        var unprotectResult = unprotectCmd.Parse(
            [protectedFile, "-p", "dirpwd", "-o", restoredZip, "--overwrite"],
            new ParserConfiguration());
        await unprotectResult.InvokeAsync(new InvocationConfiguration());

        File.Exists(restoredZip).Should().BeTrue();

        using var archive = System.IO.Compression.ZipFile.OpenRead(restoredZip);
        var entryNames = archive.Entries.Select(e => e.Name).OrderBy(n => n).ToList();
        entryNames.Should().BeEquivalentTo(["a.txt", "b.txt"]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}
