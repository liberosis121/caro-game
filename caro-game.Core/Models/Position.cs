namespace caro_game.Core.Models
{
    /// <summary>
    /// Zero-based coordinates of a cell on the board.
    /// </summary>
    public readonly record struct Position(int Row, int Column);
}
