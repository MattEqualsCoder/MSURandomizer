using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace MSURandomizerUI.Controls;

internal class EnumDescriptionConverter : IValueConverter
{
    private string GetEnumDescription(Enum enumObj)
    {
        FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString())!;

        object[] attribArray = fieldInfo.GetCustomAttributes(false);

        if (attribArray.Length == 0)
        {
            return enumObj.ToString();
        }
        else
        {
            DescriptionAttribute? attrib = null;

            foreach (var att in attribArray)
            {
                if (att is DescriptionAttribute attribTemp)
                    attrib = attribTemp;
            }

            return attrib != null ? attrib.Description : enumObj.ToString();
        }
    }

    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Enum myEnum = (Enum)value;
        string description = GetEnumDescription(myEnum);
        return description;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.Empty;
    }
}