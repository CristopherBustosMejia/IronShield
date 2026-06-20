using System.CommandLine;
using IronShield.Core.Interfaces;
using IronShield.Cli.Services;
using IronShield.Storage.Sources;

namespace IronShield.Cli.Commands;

internal static class ProtectCommand
{
    public static Command Create(IIronShieldService service)
    {
        var pathArg = new Argument<String>("path");

        var outputOpt = new Option<FileInfo?>("--output", ["-o"])
        {
            Description = "Output path (.iron file)"
        };

        var passwordOpt = new Option<String?>("--password", ["-p"])
        {
            Description = "Encryption password"
        };

        var creatorOpt = new Option<String?>("--creator", [])
        {
            Description = "Creator name stored in metadata (reserved)"
        };

        var overwriteOpt = new Option<bool>("--overwrite", [])
        {
            Description = "Overwrite the output file if it exists"
        };

        var command = new Command("protect", "Protect a file or directory with encryption")
        {
            pathArg,
            outputOpt,
            passwordOpt,
            creatorOpt,
            overwriteOpt
        };

        command.SetAction((ParseResult parseResult) =>
        {
            var path = parseResult.GetRequiredValue(pathArg);
            var output = parseResult.GetValue(outputOpt);
            var password = parseResult.GetValue(passwordOpt);
            var overwrite = parseResult.GetValue(overwriteOpt);

            var outputService = new CliOutputService();
            return CliOutputService.RunSafe(outputService, () =>
                ProtectHandler(service, outputService, path, output, password, overwrite));
        });

        return command;
    }

    private static async Task ProtectHandler(
        IIronShieldService service,
        CliOutputService output,
        String inputPath,
        FileInfo? outputPath,
        String? password,
        bool overwrite)
    {
        if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
        {
            output.Error($"Path not found: {inputPath}");
            return;
        }

        password ??= output.AskPassword("Password:");

        if (String.IsNullOrWhiteSpace(password))
        {
            output.Error("Password cannot be empty.");
            return;
        }

        IDataSource source = CreateSource(inputPath);

        String defaultOutput = source.Name + ".iron";
        String resolvedOutput = outputPath?.FullName ?? defaultOutput;

        if (File.Exists(resolvedOutput) && !overwrite)
        {
            if (!output.ConfirmOverwrite(resolvedOutput))
            {
                output.Error("Operation cancelled by user.");
                return;
            }
        }

        await output.StatusAsync("Encrypting...", async () =>
        {
            String resolvedFullPath = Path.GetFullPath(resolvedOutput);
            String? dir = Path.GetDirectoryName(resolvedFullPath);
            if (!String.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            await using var outputStream = File.Create(resolvedFullPath);
            await Task.Run(() => service.Protect(source, password, outputStream));
            await outputStream.FlushAsync();
        });

        output.Success($"Protected file created: {resolvedOutput}");
    }

    private static IDataSource CreateSource(String path)
    {
        if (Directory.Exists(path))
            return new DirectoryDataSource(path);

        return new FileDataSource(path);
    }
}
