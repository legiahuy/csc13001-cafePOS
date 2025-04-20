using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Data;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Kernel.Sketches;

namespace CafePOS
{
    public class AxisArrayToEnumerableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Axis[] axes)
            {
                return axes.Cast<ICartesianAxis>();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 