using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace MSURandomizerUI.Controls;

internal class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var enumValue = (Enum)value!;
        return enumValue.GetDescription();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var enumValues = Enum.GetValues(targetType);
        if (value == null)
        {
            return enumValues.GetValue(0)!;
        }
        var descriptions = enumValues.Cast<Enum>().ToDictionary(x => x.GetDescription() as object, x => x);
        if (descriptions.TryGetValue(value, out var description))
        {
            return description;
        }
        else
        {
            return enumValues.GetValue(0)!;
        }
    }
}