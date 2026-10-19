using System;
using caro_game.Core.Models;
using caro_game.Helpers;

namespace caro_game.ViewModels
{
    /// <summary>
    /// Setup screen: reads and validates the board size n (4 ≤ n ≤ 20) and the optional game options.
    /// </summary>
    public sealed class SetupViewModel : ObservableObject
    {
        public const string DefaultPlayerXName = "Người chơi X";
        public const string DefaultPlayerOName = "Người chơi O";

        private string _boardSizeText = "15";
        private int _boardSize = 15;
        private BoardSizeError _boardSizeError = BoardSizeError.None;
        private string _playerXName = string.Empty;
        private string _playerOName = string.Empty;
        private int _firstPlayerIndex;
        private bool _blockBothEnds;

        public SetupViewModel()
        {
            StartCommand = new RelayCommand(_ => Start(), _ => CanStart);
        }

        /// <summary>
        /// Raised with the chosen settings; the page reacts by navigating to the game page.
        /// </summary>
        public event EventHandler<GameSettings>? StartRequested;

        public RelayCommand StartCommand { get; }

        public string BoardSizeHeader => $"Kích thước bàn cờ n ({GameRules.MinBoardSize} ≤ n ≤ {GameRules.MaxBoardSize})";

        public string BoardSizeText
        {
            get => _boardSizeText;
            set
            {
                if (SetProperty(ref _boardSizeText, value))
                {
                    _boardSizeError = GameRules.ValidateBoardSize(value, out _boardSize);
                    OnPropertyChanged(nameof(IsBoardSizeValid));
                    OnPropertyChanged(nameof(HasBoardSizeError));
                    OnPropertyChanged(nameof(BoardSizeErrorMessage));
                    OnPropertyChanged(nameof(BoardSummary));
                    StartCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBoardSizeValid => _boardSizeError == BoardSizeError.None;

        public bool HasBoardSizeError => !IsBoardSizeValid;

        public string BoardSizeErrorMessage => _boardSizeError switch
        {
            BoardSizeError.Empty => "Vui lòng nhập kích thước bàn cờ.",
            BoardSizeError.NotAWholeNumber => "Kích thước phải là một số nguyên, ví dụ 15.",
            BoardSizeError.OutOfRange => $"Kích thước phải nằm trong khoảng {GameRules.MinBoardSize} đến {GameRules.MaxBoardSize}.",
            _ => string.Empty
        };

        public string BoardSummary => IsBoardSizeValid
            ? $"Bàn cờ {_boardSize} × {_boardSize} ({_boardSize * _boardSize} ô) · xếp {GameRules.GetWinLength(_boardSize)} quân liên tiếp để thắng"
            : string.Empty;

        public string PlayerXName
        {
            get => _playerXName;
            set
            {
                if (SetProperty(ref _playerXName, value))
                {
                    OnPlayerNamesChanged();
                }
            }
        }

        public string PlayerOName
        {
            get => _playerOName;
            set
            {
                if (SetProperty(ref _playerOName, value))
                {
                    OnPlayerNamesChanged();
                }
            }
        }

        public bool HasNameError => string.Equals(EffectivePlayerXName, EffectivePlayerOName, StringComparison.CurrentCultureIgnoreCase);

        public string NameErrorMessage => HasNameError ? "Tên hai người chơi phải khác nhau." : string.Empty;

        /// <summary>
        /// 0 = X moves first, 1 = O moves first (index of the RadioButtons).
        /// </summary>
        public int FirstPlayerIndex
        {
            get => _firstPlayerIndex;
            set => SetProperty(ref _firstPlayerIndex, value);
        }

        public bool BlockBothEnds
        {
            get => _blockBothEnds;
            set => SetProperty(ref _blockBothEnds, value);
        }

        private bool CanStart => IsBoardSizeValid && !HasNameError;

        private string EffectivePlayerXName => NameOrDefault(PlayerXName, DefaultPlayerXName);

        private string EffectivePlayerOName => NameOrDefault(PlayerOName, DefaultPlayerOName);

        private void OnPlayerNamesChanged()
        {
            OnPropertyChanged(nameof(HasNameError));
            OnPropertyChanged(nameof(NameErrorMessage));
            StartCommand.RaiseCanExecuteChanged();
        }

        private void Start()
        {
            if (!CanStart)
            {
                return;
            }

            var rules = new GameRules(_boardSize, BlockBothEnds);
            Player firstPlayer = FirstPlayerIndex == 1 ? Player.O : Player.X;
            var settings = new GameSettings(rules, EffectivePlayerXName, EffectivePlayerOName, firstPlayer);
            StartRequested?.Invoke(this, settings);
        }

        private static string NameOrDefault(string name, string defaultName)
        {
            return string.IsNullOrWhiteSpace(name) ? defaultName : name.Trim();
        }
    }
}
