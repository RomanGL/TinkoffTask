using System;
using Windows.UI.Xaml.Data;
using Microsoft.Toolkit.Uwp.Helpers;

namespace TinkoffTask.Converters
{
    public sealed class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string colorText = value.ToString();
            return ColorHelper.ToColor(colorText);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
