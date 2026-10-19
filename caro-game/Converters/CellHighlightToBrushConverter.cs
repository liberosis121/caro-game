using System;
using caro_game.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace caro_game.Converters
{
    /// <summary>
    /// Cell highlight → background of the cell.
    /// "None" must stay a Transparent brush (not null) so the whole cell is still clickable.
    /// </summary>
    public sealed class CellHighlightToBrushConverter : IValueConverter
    {
        public Brush NoneBrush { get; set; } = new SolidColorBrush(Colors.Transparent);

        public Brush? LastMoveBrush { get; set; }

        public Brush? WinningLineBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Brush? brush = value switch
            {
                CellHighlight.LastMove => LastMoveBrush,
                CellHighlight.WinningLine => WinningLineBrush,
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
