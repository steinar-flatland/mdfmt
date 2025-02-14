using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using FluentValidation;
using System;
using System.Collections.Generic;

namespace Mdfmt.Options;

/// <summary>
/// <para>
/// Static class providing a method to load and combine a sequence of JSON configuration files into
/// an instance of <see cref="MdfmtProfile"/>.
/// </para>
/// </summary>
internal static class MdfmtProfileLoader
{
    /// <summary>
    /// Configuration for deserialization details for loading JSON files.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        // Ignore case when matching property names
        PropertyNameCaseInsensitive = true,

        // Allows enums to be specified by name, with any casing, including 
        // lowercase, ALL CAPS, camelCase, PascalCase, and anything inbetween.
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },

        // Disallow the presence of extra members in the JSON that are not supported
        // by the target class in C#.
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    /// <summary>
    /// Load and combine a sequence of JSON configuration files into a new instance of
    /// <see cref="MdfmtProfile"/>. When there are multiple files being loaded, each
    /// subsequent file combines its non-null settings with the first, overriding any
    /// duplicate settings.
    /// </summary>
    /// <param name="filePaths">
    /// A list of 0 or more file paths.  Empty list and <c>null</c> are semantically
    /// equivalent, indicating that there are no files to load.
    /// </param>
    /// <returns>
    /// Loaded <see cref="MdfmtProfile"/>, or null if no files to load.
    /// </returns>
    public static MdfmtProfile Load(IReadOnlyList<string> filePaths)
    {
        if (filePaths == null) return null;
        MdfmtProfile result = null;
        foreach (string filePath in filePaths)
        {
            MdfmtProfile current = Load(filePath);
            result = (result == null) ? current : result.OverwriteOptionsFrom(current);
        }

        // Ensure result to be returned is valid
        if (result != null)
        {
            MdfmtProfileValidator validator = new(result);
            validator.ValidateAndThrow(result);
        }

        return result;
    }

    /// <summary>
    /// Load a single configuration file into a new instance of <see cref="MdfmtProfile"/>.
    /// </summary>
    /// <param name="filePath">
    /// The path of the configuration file to load.
    /// </param>
    /// <returns>
    /// <see cref="MdfmtProfile"/> instance, not yet validated.  Validation is deferred
    /// until all configurations have been combined.
    /// </returns>
    private static MdfmtProfile Load(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            MdfmtProfile mdfmtProfile = JsonSerializer.Deserialize<MdfmtProfile>(json, _jsonSerializerOptions);
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
