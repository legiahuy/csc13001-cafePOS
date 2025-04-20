using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace CafePOS.Converter
{
    public class BooleanToTextColorConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            bool isActive = value is bool b && b;

            if (parameter?.ToString() == "Text")
                return isActive ? "Có" : "Không";
            else if (parameter?.ToString() == "Background")
                return isActive ? new SolidColorBrush(Microsoft.UI.Colors.Transparent) : new SolidColorBrush(Microsoft.UI.Colors.LightPink);

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value.ToString() == "Có";
        }
    }
}
