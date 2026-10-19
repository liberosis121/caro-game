using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using caro_game.Core.Models;
using caro_game.Core.Services;
using caro_game.Helpers;
using caro_game.Services;

namespace caro_game.ViewModels
{
    /// <summary>
    /// Game screen: turns the engine state into what the page shows (cells, turn, result, score)
    /// and exposes the player actions as commands.
    /// </summary>
    public sealed class GameViewModel : ObservableObject
    {
        private readonly GameEngine _engine;
        private readonly IDialogService _dialogService;
        private readonly CellViewModel[] _cells;
        private Player _roundFirstPlayer;
        private CellViewModel? _lastMoveCell;
        private int _roundNumber = 1;
        private int _playerXWins;
        private int _playerOWins;
        private int _draws;

        public GameViewModel(GameSettings settings, IWinChecker winChecker, IDialogService dialogService)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(winChecker);
            ArgumentNullException.ThrowIfNull(dialogService);

            Settings = settings;
            _dialogService = dialogService;
            _roundFirstPlayer = settings.FirstPlayer;
            _engine = new GameEngine(settings.Rules, winChecker, settings.FirstPlayer);

            // Cells are stored row by row: index = row * size + column.
            int size = settings.Rules.BoardSize;
            _cells = new CellViewModel[size * size];
            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    _cells[(row * size) + column] = new CellViewModel(new Position(row, column), Play, CanPlay);
                }
            }

            UndoCommand = new RelayCommand(_ => Undo(), _ => _engine.CanUndo);
            NewRoundCommand = new AsyncRelayCommand(StartNewRoundAsync, () => _engine.IsGameOver || _engine.Moves.Count > 0);
            BackToSetupCommand = new AsyncRelayCommand(BackToSetupAsync);
        }

        /// <summary>
        /// Raised when the players want to change the board size; the page navigates back.
        /// </summary>
        public event EventHandler? BackToSetupRequested;

        public GameSettings Settings { get; }

        public IReadOnlyList<CellViewModel> Cells => _cells;

        public int BoardSize => Settings.Rules.BoardSize;

        public string BoardTitle => $"Bàn cờ {BoardSize} × {BoardSize}";

        public string RulesDescription
        {
            get
            {
                string text = $"Xếp {Settings.Rules.WinLength} quân liên tiếp theo hàng ngang, dọc hoặc chéo để thắng.";
                return Settings.Rules.BlockBothEnds
                    ? text + " Luật chặn hai đầu: hàng bị quân đối thủ chặn cả hai đầu không được tính."
                    : text;
            }
        }

        public string PlayerXName => Settings.PlayerXName;

        public string PlayerOName => Settings.PlayerOName;

        public RelayCommand UndoCommand { get; }

        public AsyncRelayCommand NewRoundCommand { get; }

        public AsyncRelayCommand BackToSetupCommand { get; }

        public bool IsGameOver => _engine.IsGameOver;

        public int MoveCount => _engine.Moves.Count;

        public string RoundText => $"Ván {_roundNumber}";

        /// <summary>
        /// Player shown in the status card: whose turn it is, or the winner once the game is over.
        /// </summary>
        public Player StatusPlayer => _engine.Status switch
        {
            GameStatus.Won => _engine.Winner,
            GameStatus.Draw => Player.None,
            _ => _engine.CurrentPlayer
        };

        public string StatusSymbol => _engine.Status == GameStatus.Draw ? "=" : StatusPlayer.ToSymbol();

        public string StatusTitle => _engine.Status switch
        {
            GameStatus.Won => $"{Settings.GetPlayerName(_engine.Winner)} thắng!",
            GameStatus.Draw => "Hòa cờ!",
            _ => $"Lượt của {Settings.GetPlayerName(_engine.CurrentPlayer)}"
        };

        public string StatusDetail => _engine.Status switch
        {
            GameStatus.Won => $"Xếp được {_engine.WinningLine.Count} quân {_engine.Winner.ToSymbol()} liên tiếp sau {MoveCount} nước đi.",
            GameStatus.Draw => $"Bàn cờ đã kín {MoveCount} ô mà không ai thắng.",
            _ => $"Nước đi thứ {MoveCount + 1} · đánh quân {_engine.CurrentPlayer.ToSymbol()}"
        };

        public string ResultMessage => IsGameOver
            ? $"Ván sau {Settings.GetPlayerName(NextRoundFirstPlayer)} ({NextRoundFirstPlayer.ToSymbol()}) đi trước."
            : string.Empty;

        public int PlayerXWins
        {
            get => _playerXWins;
            private set => SetProperty(ref _playerXWins, value);
        }

        public int PlayerOWins
        {
            get => _playerOWins;
            private set => SetProperty(ref _playerOWins, value);
        }

        public int Draws
        {
            get => _draws;
            private set => SetProperty(ref _draws, value);
        }

        // Players take turns to move first in consecutive rounds.
        private Player NextRoundFirstPlayer => _roundFirstPlayer.Opponent();

        private bool CanPlay(CellViewModel cell)
        {
            return _engine.CanPlay(cell.Position);
        }

        private void Play(CellViewModel cell)
        {
            if (!_engine.CanPlay(cell.Position))
            {
                return;
            }

            MoveResult result = _engine.Play(cell.Position);
            cell.Owner = result.Move.Player;
            MarkLastMove(cell);

            if (result.Status == GameStatus.Won)
            {
                foreach (Position position in result.WinningLine)
                {
                    GetCell(position).Highlight = CellHighlight.WinningLine;
                }

                if (result.Winner == Player.X)
                {
                    PlayerXWins++;
                }
                else
                {
                    PlayerOWins++;
                }
            }
            else if (result.Status == GameStatus.Draw)
            {
                Draws++;
            }

            RefreshGameState();
        }

        private void Undo()
        {
            if (!_engine.CanUndo)
            {
                return;
            }

            Move undoneMove = _engine.Undo();
            GetCell(undoneMove.Position).Owner = Player.None;

            Move? previousMove = _engine.LastMove;
            MarkLastMove(previousMove is null ? null : GetCell(previousMove.Position));
            RefreshGameState();
        }

        private async Task StartNewRoundAsync()
        {
            if (!_engine.IsGameOver && _engine.Moves.Count > 0)
            {
                bool confirmed = await _dialogService.ConfirmAsync(
                    "Bắt đầu ván mới?",
                    "Ván hiện tại chưa kết thúc và sẽ không được tính vào tỉ số.",
                    "Ván mới",
                    "Chơi tiếp");
                if (!confirmed)
                {
                    return;
                }
            }

            _roundFirstPlayer = NextRoundFirstPlayer;
            _roundNumber++;
            _engine.Restart(_roundFirstPlayer);

            foreach (CellViewModel cell in _cells)
            {
                cell.Owner = Player.None;
                cell.Highlight = CellHighlight.None;
            }

            _lastMoveCell = null;
            OnPropertyChanged(nameof(RoundText));
            RefreshGameState();
        }

        private async Task BackToSetupAsync()
        {
            bool hasProgress = _engine.Moves.Count > 0 || PlayerXWins + PlayerOWins + Draws > 0;
            if (hasProgress)
            {
                bool confirmed = await _dialogService.ConfirmAsync(
                    "Đổi kích thước bàn cờ?",
                    "Bàn cờ và tỉ số hiện tại sẽ bị xóa khi quay lại màn hình thiết lập.",
                    "Quay lại thiết lập",
                    "Ở lại");
                if (!confirmed)
                {
                    return;
                }
            }

            BackToSetupRequested?.Invoke(this, EventArgs.Empty);
        }

        private void MarkLastMove(CellViewModel? cell)
        {
            if (_lastMoveCell is not null)
            {
                _lastMoveCell.Highlight = CellHighlight.None;
            }

            _lastMoveCell = cell;
            if (cell is not null)
            {
                cell.Highlight = CellHighlight.LastMove;
            }
        }

        private CellViewModel GetCell(Position position)
        {
            return _cells[(position.Row * BoardSize) + position.Column];
        }

        private void RefreshGameState()
        {
            OnPropertyChanged(nameof(IsGameOver));
            OnPropertyChanged(nameof(MoveCount));
            OnPropertyChanged(nameof(StatusPlayer));
            OnPropertyChanged(nameof(StatusSymbol));
            OnPropertyChanged(nameof(StatusTitle));
            OnPropertyChanged(nameof(StatusDetail));
            OnPropertyChanged(nameof(ResultMessage));

            UndoCommand.RaiseCanExecuteChanged();
            NewRoundCommand.RaiseCanExecuteChanged();
            foreach (CellViewModel cell in _cells)
            {
                cell.PlayCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
