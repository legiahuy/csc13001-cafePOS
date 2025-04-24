using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace CafePOS.Converter
{
    public partial class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string? status = value as string;

            if (status == null)
                return new SolidColorBrush(Microsoft.UI.Colors.Gray);

            switch (status.ToLower())
            {
                case "pending":
                    return new SolidColorBrush(Color.FromArgb(255, 234, 67, 53)); // Red
                case "in_progress":
                    return new SolidColorBrush(Color.FromArgb(255, 251, 188, 5)); // Yellow
                case "completed":
                    return new SolidColorBrush(Color.FromArgb(255, 52, 168, 83)); // Green
                default:
                    return new SolidColorBrush(Microsoft.UI.Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("dd/MM/yyyy");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("HH:mm");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}