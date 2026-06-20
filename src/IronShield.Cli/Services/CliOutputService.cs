using System.Security.Cryptography;
using Spectre.Console;

namespace IronShield.Cli.Services;

internal sealed class CliOutputService
{
    public void Status(String message, Action action)
    {
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Arrow3)
            .SpinnerStyle(Style.Parse("cyan"))
            .Start(message, _ => action());
    }

    public T Status<T>(String message, Func<T> action)
    {
        return AnsiConsole.Status()
            .Spinner(Spinner.Known.Arrow3)
            .SpinnerStyle(Style.Parse("cyan"))
            .Start(message, _ => action());
    }

    public async Task StatusAsync(String message, Func<Task> action)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Arrow3)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync(message, async _ => await action());
    }

    public async Task<T> StatusAsync<T>(String message, Func<Task<T>> action)
    {
        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Arrow3)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync(message, async _ => await action());
    }

    public void Success(String message)
    {
        AnsiConsole.MarkupLine($"[green bold]SUCCESS:[/] {message}");
    }

    public void Error(String message)
    {
        AnsiConsole.MarkupLine($"[red bold]ERROR:[/] {message}");
    }

    public void FatalError(Exception exception)
    {
        AnsiConsole.MarkupLine($"[red bold]FATAL:[/] An unexpected error occurred.");
        AnsiConsole.MarkupLine($"[red]{exception.Message}[/]");
    }

    public String AskPassword(String prompt)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<String>(prompt)
                .PromptStyle("yellow")
                .Secret());
    }

    public String AskPasswordOptional(String prompt)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<String>(prompt)
                .PromptStyle("yellow")
                .Secret()
                .AllowEmpty());
    }

    public bool ConfirmOverwrite(String path)
    {
        return AnsiConsole.Confirm(
            $"[yellow]The file '{path}' already exists. Overwrite?[/]",
            false);
    }

    public void WriteHeader()
    {
        AnsiConsole.Write(
            new FigletText("IronShield")
                .LeftJustified()
                .Color(Color.Cyan1));
    }

    public static async Task RunSafe(CliOutputService output, Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (AuthenticationTagMismatchException)
        {
            output.Error("Incorrect password. Decryption failed.");
        }
        catch (CryptographicException ex)
        {
            output.Error($"Cryptographic error: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            output.Error($"File not found: {ex.FileName}");
        }
        catch (DirectoryNotFoundException ex)
        {
            output.Error($"Directory not found: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            output.Error($"Access denied: {ex.Message}");
        }
        catch (Exception ex)
        {
            output.FatalError(ex);
        }
    }
}
