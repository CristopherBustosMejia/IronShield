using System.CommandLine;
using FluentAssertions;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Cli.Commands;

namespace IronShield.Cli.Tests.Commands;

[Collection("CLI")]
public sealed class ProtectCommandTests
{
    [Fact]
    public async Task Should_Call_Protect_With_Parsed_Arguments()
    {
        var mock = new MockIronShieldService();
        var cmd = ProtectCommand.Create(mock);
        var tempFile = Path.GetTempFileName();

        try
        {
            var result = cmd.Parse(
                [tempFile, "-p", "secret", "-o", "out.iron", "--overwrite"],
                new ParserConfiguration());

            await result.InvokeAsync(new InvocationConfiguration());

            mock.ProtectPassword.Should().Be("secret");
            mock.ProtectedSource.Should().NotBeNull();
            mock.ProtectedSource!.Name.Should().Be(Path.GetFileName(tempFile));
        }
        finally
        {
            File.Delete(tempFile);
            if (File.Exists("out.iron")) File.Delete("out.iron");
        }
    }

    [Fact]
    public async Task Should_Error_When_Path_Does_Not_Exist()
    {
        var mock = new MockIronShieldService();
        var cmd = ProtectCommand.Create(mock);

        var result = cmd.Parse(
            ["/nonexistent/path.txt", "-p", "secret"],
            new ParserConfiguration());

        await result.InvokeAsync(new InvocationConfiguration());

        mock.ProtectedSource.Should().BeNull();
    }

    private sealed class MockIronShieldService : IIronShieldService
    {
        public IDataSource? ProtectedSource { get; private set; }
        public String? ProtectPassword { get; private set; }

        public void Protect(IDataSource source, String password, Stream output)
        {
            ProtectedSource = source;
            ProtectPassword = password;
        }

        public UnprotectResult Unprotect(Stream input, String password)
        {
            return new UnprotectResult { Data = [] };
        }

        public IntegrityVerificationResult Verify(Stream input, String password)
            => throw new NotSupportedException();
    }
}
