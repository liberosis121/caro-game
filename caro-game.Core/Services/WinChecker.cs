using caro_game.Core.Models;

namespace caro_game.Core.Services
{
    /// <summary>
    /// Only the lines through the last move can have changed, so instead of scanning
    /// the whole board we count same-coloured stones in 4 directions from that move.
    /// </summary>
    public sealed class WinChecker : IWinChecker
    {
        // Horizontal, vertical, main diagonal (↘) and anti-diagonal (↙).
        private static readonly (int RowStep, int ColumnStep)[] Directions =
        {
            (0, 1),
            (1, 0),
            (1, 1),
            (1, -1)
        };

        public IReadOnlyList<Position> FindWinningLine(IReadOnlyBoard board, Position lastMove, GameRules rules)
        {
            ArgumentNullException.ThrowIfNull(board);
            ArgumentNullException.ThrowIfNull(rules);

            Player player = board[lastMove];
            if (player == Player.None)
            {
                return Array.Empty<Position>();
            }

            foreach ((int rowStep, int columnStep) in Directions)
            {
                var line = new List<Position> { lastMove };
                Position beforeStart = CollectStones(board, lastMove, -rowStep, -columnStep, player, line);
                Position afterEnd = CollectStones(board, lastMove, rowStep, columnStep, player, line);

                if (line.Count < rules.WinLength)
                {
                    continue;
                }

                // Vietnamese rule: a row closed by the opponent at both ends is not a win.
                // The board edge does not count as a block.
                if (rules.BlockBothEnds
                    && IsOpponentStone(board, beforeStart, player)
                    && IsOpponentStone(board, afterEnd, player))
                {
                    continue;
                }

                return line.OrderBy(p => p.Row).ThenBy(p => p.Column).ToList();
            }

            return Array.Empty<Position>();
        }

        /// <summary>
        /// Walks away from <paramref name="start"/> while the stones belong to <paramref name="player"/>,
        /// adding them to <paramref name="line"/>. Returns the first cell after the run (it may be off the board).
        /// </summary>
        private static Position CollectStones(IReadOnlyBoard board, Position start, int rowStep, int columnStep,
            Player player, List<Position> line)
        {
            var current = new Position(start.Row + rowStep, start.Column + columnStep);
            while (board.IsInside(current) && board[current] == player)
            {
                line.Add(current);
                current = new Position(current.Row + rowStep, current.Column + columnStep);
            }

            return current;
        }

        private static bool IsOpponentStone(IReadOnlyBoard board, Position position, Player player)
        {
            return board.IsInside(position) && board[position] == player.Opponent();
        }
    }
}
