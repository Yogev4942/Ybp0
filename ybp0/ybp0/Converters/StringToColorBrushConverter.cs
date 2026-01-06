using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ybp0.Converters
{
    /// <summary>
    /// Converts a hex color string (e.g., "#26A69A") to a SolidColorBrush.
    /// </summary>
    public class StringToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorString && !string.IsNullOrEmpty(colorString))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // Fallback to teal if parsing fails
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26A69A"));
                }
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26A69A"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
