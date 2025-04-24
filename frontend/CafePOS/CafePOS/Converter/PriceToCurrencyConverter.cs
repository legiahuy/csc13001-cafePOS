using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace CafePOS.Converter  
{
    public partial class PriceToCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Handle both int and double
            if (value is int intPrice)
                return intPrice.ToString("C0", new CultureInfo("vi-VN"));
            else if (value is double doublePrice)
                return doublePrice.ToString("C0", new CultureInfo("vi-VN"));

            // Try parsing if it's another type
            if (value != null && decimal.TryParse(value.ToString(), out decimal decimalPrice))
                return decimalPrice.ToString("C0", new CultureInfo("vi-VN"));

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string str)
            {
                // Remove currency symbols and separators
                var cleaned = str.Replace("₫", "").Replace(".", "").Replace(",", "").Trim();

                // Convert based on the target type
                if (targetType == typeof(int) && int.TryParse(cleaned, out int intResult))
                    return intResult;
                else if (targetType == typeof(double) && double.TryParse(cleaned, out double doubleResult))
                    return doubleResult;
            }

            // Default return value based on target type
            return targetType == typeof(int) ? 0 : 0.0;
        }
    }
}
