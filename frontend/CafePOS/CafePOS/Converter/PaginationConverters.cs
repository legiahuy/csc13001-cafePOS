using Microsoft.UI.Xaml.Data;
using System;

namespace CafePOS.Converter
{
    public partial class GreaterThanOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int currentPage)
            {
                return currentPage > 1;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class LessThanTotalPagesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int currentPage && parameter is int totalPages)
            {
                return currentPage < totalPages;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}