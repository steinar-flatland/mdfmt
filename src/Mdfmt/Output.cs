using System;

namespace Mdfmt;

/// <summary>
/// This class is used to write console output.
/// </summary>
internal static class Output
{
    public static void Error(object value, bool newLine = true)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        WriteValue(value, newLine);
        Console.ResetColor();
    }

    public static void Warn(object value, bool newLine = true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        WriteValue(value, newLine);
        Console.ResetColor();
    }

    public static void Info(object value, bool newLine = true, ConsoleColor consoleColor = ConsoleColor.Gray)
    {
        Console.ForegroundColor = consoleColor;
        WriteValue(value, newLine);
        Console.ResetColor();
    }

    public static void Emphasis(object value, bool newLine = true)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        WriteValue(value, newLine);
        Console.ResetColor();
    }

    private static void WriteValue(object value, bool newLine)
    {
        if (newLine)
        {
            Console.WriteLine(value);
        }
        else
        {
            Console.Write(value);
        }
    }
}
