using System;
using caro_game.Core.Models;
using caro_game.Helpers;

namespace caro_game.ViewModels
{
    public enum CellHighlight
    {
        None,
        LastMove,
        WinningLine
    }

    /// <summary>
    /// One square of the board as shown on screen. Each cell owns a command that plays it.
    /// </summary>
    public sealed class CellViewModel : ObservableObject
    {
        private Player _owner;
        private CellHighlight _highlight;

        public CellViewModel(Position position, Action<CellViewModel> play, Func<CellViewModel, bool> canPlay)
        {
            Position = position;
            PlayCommand = new RelayCommand(_ => play(this), _ => canPlay(this));
        }

        public Position Position { get; }

        public RelayCommand PlayCommand { get; }

        public Player Owner
        {
            get => _owner;
            set
            {
                if (SetProperty(ref _owner, value))
                {
                    OnPropertyChanged(nameof(Symbol));
                    OnPropertyChanged(nameof(AccessibleName));
                }
            }
        }

        public string Symbol => Owner.ToSymbol();

        public CellHighlight Highlight
        {
            get => _highlight;
            set => SetProperty(ref _highlight, value);
        }

        /// <summary>
        /// Text read by screen readers, e.g. "Hàng 3, cột 5: X".
        /// </summary>
        public string AccessibleName
        {
            get
            {
                string content = Owner == Player.None ? "trống" : Symbol;
                return $"Hàng {Position.Row + 1}, cột {Position.Column + 1}: {content}";
            }
        }
    }
}
