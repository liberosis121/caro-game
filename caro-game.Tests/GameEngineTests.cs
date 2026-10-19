using caro_game.Core.Models;
using caro_game.Core.Services;

namespace caro_game.Tests
{
    public class GameEngineTests
    {
        private static GameEngine CreateEngine(int boardSize = 15, Player firstPlayer = Player.X, bool blockBothEnds = false)
        {
            return new GameEngine(new GameRules(boardSize, blockBothEnds), new WinChecker(), firstPlayer);
        }

        [Fact]
        public void NewGame_StartsWithChosenPlayer()
        {
            GameEngine engine = CreateEngine(firstPlayer: Player.O);

            Assert.Equal(Player.O, engine.CurrentPlayer);
            Assert.Equal(GameStatus.InProgress, engine.Status);
            Assert.Equal(15, engine.Board.Size);
            Assert.Empty(engine.Moves);
            Assert.False(engine.CanUndo);
        }

        [Fact]
        public void Players_TakeTurns()
        {
            GameEngine engine = CreateEngine();

            MoveResult first = engine.Play(new Position(7, 7));
            MoveResult second = engine.Play(new Position(7, 8));

            Assert.Equal(Player.X, first.Move.Player);
            Assert.Equal(Player.O, second.Move.Player);
            Assert.Equal(Player.X, engine.CurrentPlayer);
            Assert.Equal(Player.O, engine.Board[new Position(7, 8)]);
            Assert.Equal(new Move(new Position(7, 8), Player.O), engine.LastMove);
        }

        [Fact]
        public void OccupiedCell_CannotBePlayedAgain()
        {
            GameEngine engine = CreateEngine();
            engine.Play(new Position(0, 0));

            Assert.False(engine.CanPlay(new Position(0, 0)));
            Assert.Throws<InvalidOperationException>(() => engine.Play(new Position(0, 0)));
            Assert.Equal(Player.O, engine.CurrentPlayer);
        }

        [Fact]
        public void PositionOutsideBoard_IsRejected()
        {
            GameEngine engine = CreateEngine(boardSize: 4);

            Assert.False(engine.CanPlay(new Position(4, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => engine.Play(new Position(4, 0)));
        }

        [Fact]
        public void FiveInARow_EndsTheGameWithAWinner()
        {
            GameEngine engine = CreateEngine();

            // X plays row 0, O plays row 1.
            MoveResult? result = engine.PlayAll(
                (0, 0), (1, 0),
                (0, 1), (1, 1),
                (0, 2), (1, 2),
                (0, 3), (1, 3),
                (0, 4));

            Assert.NotNull(result);
            Assert.Equal(GameStatus.Won, result.Status);
            Assert.Equal(Player.X, result.Winner);
            Assert.Equal(5, result.WinningLine.Count);
            Assert.True(engine.IsGameOver);
            Assert.Equal(Player.X, engine.Winner);
        }

        [Fact]
        public void FillingTheGap_InTheMiddleOfALine_Wins()
        {
            GameEngine engine = CreateEngine();

            MoveResult? result = engine.PlayAll(
                (5, 3), (0, 0),
                (5, 4), (0, 1),
                (5, 6), (0, 2),
                (5, 7), (0, 4),
                (5, 5));

            Assert.Equal(GameStatus.Won, result!.Status);
            Assert.Equal(Player.X, result.Winner);
        }

        [Fact]
        public void AfterTheGameEnds_NoMoreMovesOrUndo()
        {
            GameEngine engine = CreateEngine(boardSize: 4);
            engine.PlayAll((0, 0), (1, 0), (0, 1), (1, 1), (0, 2), (1, 2), (0, 3));

            Assert.False(engine.CanPlay(new Position(3, 3)));
            Assert.False(engine.CanUndo);
            Assert.Throws<InvalidOperationException>(() => engine.Play(new Position(3, 3)));
            Assert.Throws<InvalidOperationException>(() => engine.Undo());
        }

        [Fact]
        public void FullBoardWithoutALine_IsADraw()
        {
            // Final position (no four in a row anywhere):
            // X X O O
            // O O X X
            // X X O O
            // O O X X
            GameEngine engine = CreateEngine(boardSize: 4);

            MoveResult? result = engine.PlayAll(
                (0, 0), (0, 2),
                (0, 1), (0, 3),
                (1, 2), (1, 0),
                (1, 3), (1, 1),
                (2, 0), (2, 2),
                (2, 1), (2, 3),
                (3, 2), (3, 0),
                (3, 3), (3, 1));

            Assert.Equal(GameStatus.Draw, result!.Status);
            Assert.Equal(Player.None, result.Winner);
            Assert.Empty(result.WinningLine);
            Assert.True(engine.Board.IsFull);
        }

        [Fact]
        public void BlockedBothEnds_GameContinues_WhenRuleIsOn()
        {
            GameEngine engine = CreateEngine(boardSize: 10, blockBothEnds: true);

            // O blocks (0,0) and (0,6); X then fills (0,1)..(0,5).
            MoveResult? result = engine.PlayAll(
                (0, 1), (0, 0),
                (0, 2), (0, 6),
                (0, 3), (5, 5),
                (0, 4), (6, 6),
                (0, 5));

            Assert.Equal(GameStatus.InProgress, result!.Status);
            Assert.Equal(Player.O, engine.CurrentPlayer);
        }

        [Fact]
        public void Undo_RemovesLastMoveAndGivesTheTurnBack()
        {
            GameEngine engine = CreateEngine();
            engine.PlayAll((3, 3), (4, 4));

            Move undone = engine.Undo();

            Assert.Equal(new Move(new Position(4, 4), Player.O), undone);
            Assert.True(engine.Board.IsEmpty(new Position(4, 4)));
            Assert.Equal(Player.O, engine.CurrentPlayer);
            Assert.Single(engine.Moves);
            Assert.True(engine.CanPlay(new Position(4, 4)));
        }

        [Fact]
        public void Restart_ClearsTheBoard()
        {
            GameEngine engine = CreateEngine(boardSize: 4);
            engine.PlayAll((0, 0), (1, 0), (0, 1), (1, 1), (0, 2), (1, 2), (0, 3));

            engine.Restart(Player.O);

            Assert.Equal(GameStatus.InProgress, engine.Status);
            Assert.Equal(Player.None, engine.Winner);
            Assert.Empty(engine.WinningLine);
            Assert.Empty(engine.Moves);
            Assert.Equal(0, engine.Board.OccupiedCount);
            Assert.Equal(Player.O, engine.CurrentPlayer);
        }

        [Fact]
        public void FirstPlayer_MustBeXOrO()
        {
            Assert.Throws<ArgumentException>(() => CreateEngine(firstPlayer: Player.None));
        }
    }
}
