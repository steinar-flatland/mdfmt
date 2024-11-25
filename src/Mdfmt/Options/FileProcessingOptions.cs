using System;
using System.Reflection;
using System.Text;

namespace Mdfmt.Options;

public class FileProcessingOptions
{
    public Flavor? Flavor { get; set; }

    public string HeadingNumbering {  get; set; }

    public int? MinimumEntryCount { get; set; }

    public NewlineStrategy? NewlineStrategy { get; set; }

    /// <summary>
    /// If this instance has any properties that are null, fill them in with the corresponding
    /// properties from another instance, in an effort to make them non-null.
    /// </summary>
    /// <param name="other">Another instance of this class.</param>
    public void FillGapsFrom(FileProcessingOptions other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Get all public instance properties
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Only process properties that have a getter and setter
            if (!property.CanRead || !property.CanWrite)
                continue;

            // Get the current value of the property
            var currentValue = property.GetValue(this);
            if (currentValue == null)
            {
                // Get the value from the other object and assign it
                var newValue = property.GetValue(other);
                property.SetValue(this, newValue);
            }
        }
    }

    /// <summary>
    ///   Determine whether all properties are set to a non-null value.
    /// </summary>
    /// <returns>
    ///   Returns true if and only if all properties are set to a non-null value.
    /// </returns>
    public bool IsComplete()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (!property.CanRead)
                continue;
            var currentValue = property.GetValue(this);
            if (currentValue == null)
            {
                return false;
            }
        }
        return true;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"  {nameof(Flavor)}: {Flavor}\n");
        sb.Append($"  {nameof(HeadingNumbering)}: {HeadingNumbering}\n");
        sb.Append($"  {nameof(MinimumEntryCount)}: {MinimumEntryCount}\n");
        sb.Append($"  {nameof(NewlineStrategy)}: {NewlineStrategy}");
        return sb.ToString();
    }

}
