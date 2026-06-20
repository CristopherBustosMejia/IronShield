using System.CommandLine;
using IronShield.Cli.Commands;
using IronShield.Cli.Composition;
using IronShield.Cli.Services;

var outputService = new CliOutputService();
var service = DependencyInjection.CreateService();

outputService.WriteHeader();

var rootCommand = new RootCommand("IronShield - File encryption tool")
{
    ProtectCommand.Create(service),
    UnprotectCommand.Create(service)
};

var parseResult = rootCommand.Parse(args, new ParserConfiguration());
return await parseResult.InvokeAsync(new InvocationConfiguration());
