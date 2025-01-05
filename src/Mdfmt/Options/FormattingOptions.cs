using System;
using System.Reflection;
using System.Text.Json;

namespace Mdfmt.Options;

/// <summary>
/// Options for how to format a Markdown file that can vary per Markdown file.
/// This does not include global settings that are the same for all Markdown files.
/// </summary>
internal class FormattingOptions
{
    /// <summary>
    /// Flavor of Markdown formatting to apply.  Optional / may be null.
    /// </summary>
    public Flavor? Flavor { get; set; }

    /// <summary>
    /// Whether to include heading numbers, and the style to use.  One of: ["none", "1.", "1"].
    /// Optional / may be null.
    /// </summary>
    public string HeadingNumbering {  get; set; }

    /// <summary>
    /// The minimum number of entries for which to include table of contents.  0 turns off/
    /// removes TOC.  May not be negative.  Optional / may be null.
    /// </summary>
    public int? TocThreshold { get; set; }

    /// <summary>
    /// Enumerated value indicating strategy for newlines.  One of [Unix, Windows, PreferUnix,
    /// PreferWindows].  Preferred options respect the file and take effect only if the file has a
    /// mixture.  Optional / may be null.
    /// </summary>
    public NewlineStrategy? NewlineStrategy { get; set; }

    /// <summary>
    /// If this instance has any properties that are null, fill them in with the corresponding
    /// properties from another instance, in an effort to make them non-null.
    /// </summary>
    /// <param name="other">Another instance of this class.</param>
    public void PopulateFrom(FormattingOptions other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Get all public instance properties
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Get the current value of the property
            var currentValue = property.GetValue(this);
            if (currentValue == null)
            {
                // Get the value from the other object and assign it
                property.SetValue(this, property.GetValue(other));
            }
        }
    }

    /// <summary>
    /// Determine whether all properties are set to a non-null value.
    /// </summary>
    /// <returns>
    /// Returns true if all properties are set to a non-null value and false otherwise.
    /// </returns>
    public bool IsComplete()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            var currentValue = property.GetValue(this);
            if (currentValue == null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Determine whether any properties are set to a non-null value.
    /// </summary>
    /// <returns>
    /// Returns true if at least one property is set to a non-null value and false otherwise.
    /// </returns>
    public bool AnyPropertiesArePopulated()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            var currentValue = property.GetValue(this);
            if (currentValue != null)
            {
                return true;
            }
        }
        return false;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, Constants.JsonSerializerOptions);
    }
}
