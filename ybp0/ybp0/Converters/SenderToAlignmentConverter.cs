using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ybp0.Converters
{
    /// <summary>
    /// Converts a boolean (IsSentByMe) to HorizontalAlignment for message bubbles.
    /// True = Right (sent), False = Left (received).
    /// </summary>
    public class BoolToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSentByMe && isSentByMe)
                return HorizontalAlignment.Right;
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean to a CornerRadius for chat bubbles.
    /// Sent messages get rounded top-left, top-right, bottom-left.
    /// Received messages get rounded top-left, top-right, bottom-right.
    /// </summary>
    public class BoolToBubbleCornerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSentByMe && isSentByMe)
                return new CornerRadius(16, 16, 4, 16);  // Sent: flat bottom-right
            return new CornerRadius(16, 16, 16, 4);      // Received: flat bottom-left
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
