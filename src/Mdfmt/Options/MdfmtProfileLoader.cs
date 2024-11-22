using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using System;

namespace Mdfmt.Options;

public class MdfmtProfileLoader
{
    public static MdfmtProfile Load(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, // Optional: ignore case when matching property names
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } // Optional: Camel case support
        };
        string json = File.ReadAllText(filePath);
        try
        {
            MdfmtProfile data = JsonSerializer.Deserialize<MdfmtProfile>(json, options);
            return data;
        }
        catch (JsonException)
        {
            Console.WriteLine($"Error while loading Mdfmt profile JSON from {filePath}.");
            throw;
        }
    }
}
