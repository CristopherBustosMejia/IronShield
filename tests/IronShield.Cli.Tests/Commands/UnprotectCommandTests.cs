using System.CommandLine;
using FluentAssertions;
using IronShield.Core.Exceptions;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Cli.Commands;

namespace IronShield.Cli.Tests.Commands;

[Collection("CLI")]
public sealed class UnprotectCommandTests
{
    [Fact]
    public async Task Should_Call_Unprotect_With_Parsed_Arguments()
    {
        var mock = new MockIronShieldService();
        var cmd = UnprotectCommand.Create(mock);

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "dummy .iron content");

        try
        {
            var result = cmd.Parse(
                [tempFile, "-p", "secret", "-o", "restored.bin", "--overwrite"],
                new ParserConfiguration());

            await result.InvokeAsync(new InvocationConfiguration());

            mock.UnprotectPassword.Should().Be("secret");
            mock.UnprotectInput.Should().NotBeNull();
        }
        finally
        {
            File.Delete(tempFile);
            if (File.Exists("restored.bin")) File.Delete("restored.bin");
        }
    }

    [Fact]
    public async Task Should_Error_When_File_Not_Found()
    {
        var mock = new MockIronShieldService();
        var cmd = UnprotectCommand.Create(mock);

        var result = cmd.Parse(
            ["/nonexistent/file.iron", "-p", "secret"],
            new ParserConfiguration());

        await result.InvokeAsync(new InvocationConfiguration());

        mock.UnprotectPassword.Should().BeNull();
    }

    [Fact]
    public async Task Should_Error_When_Password_Wrong()
    {
        var mock = new MockIronShieldService
        {
            ThrowOnUnprotect = true
        };

        var cmd = UnprotectCommand.Create(mock);

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "dummy .iron content");

        try
        {
            var result = cmd.Parse(
                [tempFile, "-p", "wrong"],
                new ParserConfiguration());

            await result.InvokeAsync(new InvocationConfiguration());

            mock.WasUnprotectCalled.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private sealed class MockIronShieldService : IIronShieldService
    {
        public Boolean ThrowOnUnprotect { get; init; }
        public Boolean WasUnprotectCalled { get; private set; }
        public Stream? UnprotectInput { get; private set; }
        public String? UnprotectPassword { get; private set; }

        public void Protect(IDataSource source, String password, Stream output)
        {
        }

        public UnprotectResult Unprotect(Stream input, String password)
        {
            WasUnprotectCalled = true;
            UnprotectInput = input;
            UnprotectPassword = password;

            if (ThrowOnUnprotect)
                throw new IronPasswordException("Incorrect password.");

            return new UnprotectResult { Data = [] };
        }

        public IntegrityVerificationResult Verify(Stream input, String password)
            => throw new NotSupportedException();
    }
}
