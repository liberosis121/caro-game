namespace caro_game.Core.Models
{
    /// <summary>
    /// Why a board size typed by the user was rejected.
    /// The UI turns each value into a message for the user.
    /// </summary>
    public enum BoardSizeError
    {
        None,
        Empty,
        NotAWholeNumber,
        OutOfRange
    }
}
