using caro_game.Core.Models;

namespace caro_game.Core.Services
{
    /// <summary>
    /// Business rules of a caro match: whose turn it is, which moves are legal,
    /// when the game is won or drawn, undo and restart.
    /// </summary>
    public sealed class GameEngine
    {
        private readonly IWinChecker _winChecker;
        private readonly Board _board;
        private readonly List<Move> _moves = new();

        public GameEngine(GameRules rules, IWinChecker winChecker, Player firstPlayer = Player.X)
        {
            ArgumentNullException.ThrowIfNull(rules);
            ArgumentNullException.ThrowIfNull(winChecker);

            Rules = rules;
            _winChecker = winChecker;
            _board = new Board(rules.BoardSize);
            Restart(firstPlayer);
        }

        public GameRules Rules { get; }

        public IReadOnlyBoard Board => _board;

        public Player CurrentPlayer { get; private set; }

        public GameStatus Status { get; private set; }

        public Player Winner { get; private set; }

        public IReadOnlyList<Position> WinningLine { get; private set; } = Array.Empty<Position>();

        public IReadOnlyList<Move> Moves => _moves;

        public Move? LastMove => _moves.Count > 0 ? _moves[^1] : null;

        public bool IsGameOver => Status != GameStatus.InProgress;

        /// <summary>
        /// Undo is only allowed while the game is still running, so a finished score never changes.
        /// </summary>
        public bool CanUndo => !IsGameOver && _moves.Count > 0;

        public bool CanPlay(Position position)
        {
            return !IsGameOver && _board.IsInside(position) && _board.IsEmpty(position);
        }

        public MoveResult Play(Position position)
        {
            if (IsGameOver)
            {
                throw new InvalidOperationException("The game is already over.");
            }

            if (!_board.IsInside(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position, "The position is outside the board.");
            }

            if (!_board.IsEmpty(position))
            {
                throw new InvalidOperationException("The cell is already occupied.");
            }

            var move = new Move(position, CurrentPlayer);
            _board.Place(position, move.Player);
            _moves.Add(move);

            IReadOnlyList<Position> winningLine = _winChecker.FindWinningLine(_board, position, Rules);
            if (winningLine.Count > 0)
            {
                Status = GameStatus.Won;
                Winner = move.Player;
                WinningLine = winningLine;
            }
            else if (_board.IsFull)
            {
                Status = GameStatus.Draw;
            }
            else
            {
                CurrentPlayer = CurrentPlayer.Opponent();
            }

            return new MoveResult(move, Status, Winner, WinningLine);
        }

        /// <summary>
        /// Takes back the last move and gives the turn back to the player who made it.
        /// </summary>
        public Move Undo()
        {
            if (!CanUndo)
            {
                throw new InvalidOperationException("There is no move to undo.");
            }

            Move lastMove = _moves[^1];
            _moves.RemoveAt(_moves.Count - 1);
            _board.Remove(lastMove.Position);
            CurrentPlayer = lastMove.Player;
            return lastMove;
        }

        public void Restart(Player firstPlayer)
        {
            if (firstPlayer == Player.None)
            {
                throw new ArgumentException("The first player must be X or O.", nameof(firstPlayer));
            }

            _board.Clear();
            _moves.Clear();
            Status = GameStatus.InProgress;
            Winner = Player.None;
            WinningLine = Array.Empty<Position>();
            CurrentPlayer = firstPlayer;
        }
    }
}
