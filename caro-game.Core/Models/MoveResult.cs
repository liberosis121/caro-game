namespace caro_game.Core.Models
{
    /// <summary>
    /// Outcome of a move: the game status after it and, when somebody won, the winning stones.
    /// </summary>
    public sealed record MoveResult(Move Move, GameStatus Status, Player Winner, IReadOnlyList<Position> WinningLine);
}
