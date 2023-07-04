using System;

namespace MsuRandomizerLibrary.Configs;

/// <summary>
/// Specifies the item categories for an item type.
/// </summary>
[AttributeUsage(AttributeTargets.Property,
    Inherited = false, AllowMultiple = false)]
public sealed class Smz3TrackNumberAttribute : Attribute
{
    public int MetroidFirst { get; init; }
    public int ZeldaFirst { get; init; }
}