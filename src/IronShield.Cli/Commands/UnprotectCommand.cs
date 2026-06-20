using System.CommandLine;
using IronShield.Core.Interfaces;
using IronShield.Core.Models;
using IronShield.Cli.Services;

namespace IronShield.Cli.Commands;

internal static class UnprotectCommand
{
    public static Command Create(IIronShieldService service)
    {
        var pathArg = new Argument<FileInfo>("path");

        var outputOpt = new Option<FileInfo?>("--output", ["-o"])
        {
            Description = "Output path for the restored file"
        };

        var passwordOpt = new Option<String?>("--password", ["-p"])
        {
            Description = "Decryption password"
        };

        var overwriteOpt = new Option<bool>("--overwrite", [])
        {
            Description = "Overwrite the output file if it exists"
        };

        var command = new Command("unprotect", "Restore a protected file or directory")
        {
            pathArg,
            outputOpt,
            passwordOpt,
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
                UnprotectHandler(service, outputService, path, output, password, overwrite));
        });

        return command;
    }

    private static async Task UnprotectHandler(
        IIronShieldService service,
        CliOutputService output,
        FileInfo path,
        FileInfo? outputPath,
        String? password,
        bool overwrite)
    {
        if (!path.Exists)
        {
            output.Error($"File not found: {path.FullName}");
            return;
        }

        password ??= output.AskPassword("Password:");

        if (String.IsNullOrWhiteSpace(password))
        {
            output.Error("Password cannot be empty.");
            return;
        }

        UnprotectResult result = null!;

        await output.StatusAsync("Decrypting...", async () =>
        {
            await using var input = path.OpenRead();
            result = await Task.Run(() => service.Unprotect(input, password));
        });

        String outputFilePath = GetOutputPath(path, outputPath, result);

        if (File.Exists(outputFilePath) && !overwrite)
        {
            if (!output.ConfirmOverwrite(outputFilePath))
            {
                output.Error("Operation cancelled by user.");
                return;
            }
        }

        await output.StatusAsync("Writing output...", async () =>
        {
            String resolvedFullPath = Path.GetFullPath(outputFilePath);
            String? dir = Path.GetDirectoryName(resolvedFullPath);
            if (!String.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            await File.WriteAllBytesAsync(resolvedFullPath, result.Data);
        });

        output.Success($"File restored to: {outputFilePath}");
    }

    private static String GetOutputPath(FileInfo inputPath, FileInfo? userPath, UnprotectResult result)
    {
        if (userPath is not null)
            return userPath.FullName;

        if (result.Metadata?.OriginalFileName is not null)
            return result.Metadata.OriginalFileName;

        return Path.ChangeExtension(inputPath.FullName, ".out");
    }
}
