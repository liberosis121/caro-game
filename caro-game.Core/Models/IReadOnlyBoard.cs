namespace caro_game.Core.Models
{
    /// <summary>
    /// Read-only view of the board, so callers outside the engine cannot place stones directly.
    /// </summary>
    public interface IReadOnlyBoard
    {
        int Size { get; }

        int OccupiedCount { get; }

        bool IsFull { get; }

        Player this[Position position] { get; }

        bool IsInside(Position position);

        bool IsEmpty(Position position);
    }
}
