using caro_game.Core.Models;

namespace caro_game.Core.Services
{
    public interface IWinChecker
    {
        /// <summary>
        /// Looks for a winning row that passes through the last move.
        /// Returns the stones of that row, or an empty list when the move does not win.
        /// </summary>
        IReadOnlyList<Position> FindWinningLine(IReadOnlyBoard board, Position lastMove, GameRules rules);
    }
}
