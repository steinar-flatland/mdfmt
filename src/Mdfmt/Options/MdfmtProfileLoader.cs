using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using FluentValidation;
using System;

namespace Mdfmt.Options;

/// <summary>
/// Static class providing a method to load a .mdfmt file into an instance of  <see cref="MdfmtProfile"/>.
/// </summary>
internal static class MdfmtProfileLoader
{
    /// <summary>
    /// Load a .mdfmt file into a new instance of <see cref="MdfmtProfile"/>.
    /// Note that property names enum values in the .mdfmt file are case-insensitive,
    /// and extra properties are disallowed.
    /// </summary>
    /// <param name="filePath">The path of the .mdfmt file to load</param>
    /// <returns>
    /// Validated <see cref="MdfmtProfile"/> instance.  For details of the validation, see <see cref="MdfmtProfileValidator"/>.
    /// </returns>
    public static MdfmtProfile Load(string filePath)
    {
        // Options for JSON deserialization of .mdfmt file.
        var options = new JsonSerializerOptions
        {
            // Ignore case when matching property names
            PropertyNameCaseInsensitive = true,

            // Allows enums to be specified by name, with any casing, including 
            // lowercase, ALL CAPS, camelCase, PascalCase, and anything inbetween.
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },

            // Disallow the presence of extra properties in the JSON that are not supported
            // by the target schema in C#.
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };

        try
        {
            string json = File.ReadAllText(filePath);
            MdfmtProfile mdfmtProfile = JsonSerializer.Deserialize<MdfmtProfile>(json, options);
            MdfmtProfileValidator validator = new(filePath, mdfmtProfile);
            validator.ValidateAndThrow(mdfmtProfile);
            return mdfmtProfile;
        }
        catch (Exception)
        {
            // Report the file where there was a problem and rethrow.
            Output.Error($"Error while loading Mdfmt profile JSON from {filePath}.");
            throw;
        }
    }
}
