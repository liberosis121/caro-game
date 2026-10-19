using System;
using caro_game.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace caro_game.Converters
{
    /// <summary>
    /// Player (model value) → colour of its stone (display value). The brushes are set in App.xaml.
    /// </summary>
    public sealed class PlayerToBrushConverter : IValueConverter
    {
        public Brush? XBrush { get; set; }

        public Brush? OBrush { get; set; }

        public Brush? NoneBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Brush? brush = value switch
            {
                Player.X => XBrush,
                Player.O => OBrush,
                _ => NoneBrush
            };

            return brush ?? DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
