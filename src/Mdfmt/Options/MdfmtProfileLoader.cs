﻿using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using System;
using FluentValidation;

namespace Mdfmt.Options;

internal class MdfmtProfileLoader
{
    public static MdfmtProfile Load(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, // Optional: ignore case when matching property names
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }, // Optional: Camel case support
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        };
        string json = File.ReadAllText(filePath);
        try
        {
            MdfmtProfile data = JsonSerializer.Deserialize<MdfmtProfile>(json, options);
            MdfmtProfileValidator validator = new(filePath, data);
            validator.ValidateAndThrow(data);
            return data;
        }
        catch (JsonException)
        {
            Console.WriteLine($"Error while loading Mdfmt profile JSON from {filePath}.");
            throw;
        }
    }
}
