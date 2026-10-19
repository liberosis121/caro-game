namespace caro_game.Core.Models
{
    /// <summary>
    /// Square n × n grid storing which player owns each cell.
    /// </summary>
    public sealed class Board : IReadOnlyBoard
    {
        private readonly Player[,] _cells;

        public Board(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, "Board size must be positive.");
            }

            Size = size;
            _cells = new Player[size, size];
        }

        public int Size { get; }

        public int OccupiedCount { get; private set; }

        public bool IsFull => OccupiedCount == Size * Size;

        public Player this[Position position]
        {
            get
            {
                EnsureInside(position);
                return _cells[position.Row, position.Column];
            }
        }

        public bool IsInside(Position position)
        {
            return position.Row >= 0 && position.Row < Size
                && position.Column >= 0 && position.Column < Size;
        }

        public bool IsEmpty(Position position)
        {
            return this[position] == Player.None;
        }

        public void Place(Position position, Player player)
        {
            if (player == Player.None)
            {
                throw new ArgumentException("Only X or O can be placed.", nameof(player));
            }

            if (!IsEmpty(position))
            {
                throw new InvalidOperationException($"Cell ({position.Row}, {position.Column}) is already occupied.");
            }

            _cells[position.Row, position.Column] = player;
            OccupiedCount++;
        }

        public void Remove(Position position)
        {
            if (IsEmpty(position))
            {
                throw new InvalidOperationException($"Cell ({position.Row}, {position.Column}) is already empty.");
            }

            _cells[position.Row, position.Column] = Player.None;
            OccupiedCount--;
        }

        public void Clear()
        {
            Array.Clear(_cells);
            OccupiedCount = 0;
        }

        private void EnsureInside(Position position)
        {
            if (!IsInside(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position), position,
                    $"Position is outside the {Size} × {Size} board.");
            }
        }
    }
}
