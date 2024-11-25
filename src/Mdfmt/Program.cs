using CommandLine;
using CommandLine.Text;
using FluentValidation;
using Mdfmt.Options;
using System;
using System.IO;
using System.Text.Json;

namespace Mdfmt;

public class Program
{
    private const string Version = "1.0.0";

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
        HandleParsed(args, parsedResult);
        HandleNotParsed(parsedResult);
    }

    private static void HandleParsed(string[] args, ParserResult<CommandLineOptions> parsedResult)
    {
        MdfmtProfile mdfmtProfile = null;

        parsedResult.WithParsed(options =>
        {
            if (Directory.Exists(options.Path))
            {
                string mdfmtFileName = Path.Combine(options.Path, ".mdfmt");
                if (File.Exists(mdfmtFileName))
                {
                    mdfmtProfile = MdfmtProfileLoader.Load(mdfmtFileName);
                }
            }
            else
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

            //TODO: It would be good to have some validation for the Mdfmt profile, to avoid silliness
            // like having an options key in CpathToOptions that doesn't go anywhere, etc.
            // It would be nice to know that the configuration makes sense before we start processing
            // a bunch of files, so we don't crash halfway through..

            MdfmtOptions mdfmtOptions = new(args, options, mdfmtProfile);

            Processor processor = new(mdfmtOptions);
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
        Console.WriteLine($"Error: {ex.GetType().Name}. {ex.Message}");
        if (ex is not JsonException)
        {
            Console.WriteLine($"Stack Trace:{Environment.NewLine}{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine("Inner Exception:");
                Console.WriteLine($"Message: {ex.InnerException.Message}");
                Console.WriteLine($"Stack Trace:{Environment.NewLine}{ex.InnerException.StackTrace}");
            }
        }
    }
}
