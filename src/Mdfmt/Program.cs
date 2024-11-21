using CommandLine;
using CommandLine.Text;
using FluentValidation;
using Mdfmt.Options;
using System;
using System.IO;

namespace Mdfmt;

public class Program
{
    private const string Version = "0.1.4";

    public static void Main(string[] args)
    {
        try
        {
            RunProgram(args);
        }
        catch (Exception ex)
        {
            LogException(ex);
            Environment.Exit(ExitCodes.GeneralError);
        }
    }

    private static void RunProgram(string[] args)
    {
        Parser parser = new(with => with.CaseInsensitiveEnumValues = true);
        ParserResult<CommandLineOptions> parsedResult = parser.ParseArguments<CommandLineOptions>(args);
        HandleParsed(parsedResult);
        HandleNotParsed(parsedResult);
    }

    private static void HandleParsed(ParserResult<CommandLineOptions> parsedResult)
    {
        parsedResult.WithParsed(options =>
        {
            if (!Directory.Exists(options.Path))
            {
                if (!File.Exists(options.Path))
                {
                    Console.WriteLine($"{options.Path} does not exist.");
                    Environment.Exit(ExitCodes.GeneralError);
                }
                else
                {
                    if (!options.Path.EndsWith(".md"))
                    {
                        Console.WriteLine("File cannot be processed because it is not a .md file");
                        Environment.Exit(ExitCodes.GeneralError);
                    }
                }
            }

            // Ensure options are valid.
            CommandLineOptionsValidator validator = new();
            validator.ValidateAndThrow(options);

            Processor processor = new(options);
            processor.Run();
            Environment.Exit(ExitCodes.Success);
        });
    }

    private static void HandleNotParsed(ParserResult<CommandLineOptions> parsedResult)
    {
        parsedResult.WithNotParsed(errors =>
        {
            if (errors.IsHelp())
            {
                Console.WriteLine("Displaying help:");
                HelpText helpText = HelpText.AutoBuild(parsedResult);
                Console.WriteLine(helpText);
                Environment.Exit(ExitCodes.Success);
            }
            else if (errors.IsVersion())
            {
                Console.WriteLine(Version);
                Environment.Exit(ExitCodes.Success);
            }
            else
            {
                Console.WriteLine("Error parsing arguments.");
                foreach (Error error in errors)
                {
                    Console.WriteLine(error);
                }
                Environment.Exit(ExitCodes.MisuseOfCommand);
            }
        });
    }

    private static void LogException(Exception ex)
    {
        Console.WriteLine("An error occurred:");
        Console.WriteLine($"Message: {ex.Message}");
        Console.WriteLine($"Stack Trace:{Environment.NewLine}{ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine("Inner Exception:");
            Console.WriteLine($"Message: {ex.InnerException.Message}");
            Console.WriteLine($"Stack Trace:{Environment.NewLine}{ex.InnerException.StackTrace}");
        }
    }
}
