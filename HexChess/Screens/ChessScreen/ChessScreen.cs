using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Graphics;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Graphics;
using HexChess.Core;
using MonoGame.Extended.VectorDraw;
using MonoGame.Extended.Shapes;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Input;
using Microsoft.Xna.Framework.Input;
using HexChess.Core.AI;
using Myra;
using Myra.Graphics2D.UI;

namespace HexChess.Screens.ChessScreen
{
    internal class ChessScreen : GameScreen
    {
        private new Game1 _game => (Game1)Game;

        private ChessUi _ui;

        private PrimitiveDrawing _primitiveDrawing;
        PrimitiveBatch _primitiveBatch;
        private Matrix _localProjection;
        private Matrix _localView;

        private KeyboardStateExtended _keyboardState;
        private MouseStateExtended _mouseState;

        private SpriteFont _arial;

        private GraphicalBoard _displayBoard;
        private BoardState _boardState;

        private string _data = "";
        private CubeCoordinate _dragStartCell = null;

        private IChessAi _whiteAi;
        private IChessAi _blackAi;

        private Task<Move> _aiThinkTask;

        private IEnumerable<Move> _moves;

        public ChessScreen(Game game, InitialisationOptions options) : base(game)
        {
            _displayBoard = new GraphicalBoard(new Vector2(600, 500));

            _boardState = new BoardState(options.FenString);

            _whiteAi = GetAiPlayer(options.WhitePlayer);
            _blackAi = GetAiPlayer(options.BlackPlayer);

            _aiThinkTask = null;

            _moves = Core.MoveGeneration.MoveGenerator.GetMoves(_boardState);
        }

        private IChessAi GetAiPlayer(InitialisationOptions.PlayerType playerType)
        {
            switch (playerType)
            {
                case InitialisationOptions.PlayerType.Ai: return new HexChess.Core.AI.DefaultAi();
                default: return null;
            }
        }

        public override void LoadContent()
        {
            _primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            _primitiveDrawing = new PrimitiveDrawing(_primitiveBatch);
            _localProjection = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            _localView = Matrix.Identity;

            _arial = _game.Content.Load<SpriteFont>("Arial");

            _ui = new ChessUi();
        }

        public override void Update(GameTime gameTime)
        {
            _keyboardState = KeyboardExtended.GetState();
            _mouseState = MouseExtended.GetState();

            if (_keyboardState.WasKeyJustDown(Keys.S))
            {
                _displayBoard.SetPerspective(!_displayBoard.IsWhitesPerspective);
            }

            if (_aiThinkTask == null)
            {
                // Only undo move if AI not thinking
                if (_keyboardState.WasKeyJustDown(Keys.Z))
                {
                    _boardState.UnmakeMove();
                    _moves = Core.MoveGeneration.MoveGenerator.GetMoves(_boardState);
                    SetBoardColourOverrides(null);
                }

                // Complete move
                if (_mouseState.WasButtonJustUp(MouseButton.Left))
                {
                    if (_dragStartCell != null)
                    { 
                        var releasededCellCoordinate = _displayBoard.GetClickedCell(_mouseState.Position.ToVector2());

                        if (releasededCellCoordinate == null)
                        {
                            // If not on the board, put piece back but keep the valid moves highlighted
                            _dragStartCell = null;
                        }
                        else
                        {
                            _moves = Core.MoveGeneration.MoveGenerator.GetMoves(_boardState);

                            var move = _moves.FirstOrDefault(m => m.StartCoordinate == _dragStartCell && m.DestinationCoordinate == releasededCellCoordinate);

                            if (move == null)
                            {
                                // If not valid move, put piece back but keep the valid moves highlighted
                                _dragStartCell = null;
                            }
                            else
                            {
                                // Valid move, make the move on the board and highlight the start and end of the move
                                ExecuteMove(move, false);

                                //_data = _boardState.GameState == GameState.Normal ? "" : _boardState.GameState.ToString();

                                _dragStartCell = null;

                                SetBoardColourOverrides(null);
                            }
                        }
                    }
                }

                // Start move
                if (_mouseState.WasButtonJustDown(MouseButton.Left))
                {
                    var clickedCellCoordinate = _displayBoard.GetClickedCell(_mouseState.Position.ToVector2());

                    if (clickedCellCoordinate != null)
                    {
                        var piece = _boardState.GetPieceInCell(clickedCellCoordinate.ToArrayIndex());

                        // Only grab piece if it's the current player's piece
                        if (piece != Piece.None && piece.IsWhite() == _boardState.IsWhitesTurn)
                        {
                            // Clear all old selections
                            _dragStartCell = null;

                            _displayBoard.SetCellColourOverride(clickedCellCoordinate, HexCell.ColourOverride.Purple);

                            var validDestinations = CoordinateHelpers.BOARD_COORDINATES.Where(c =>
                                _moves.Any(m => m.StartCoordinate == clickedCellCoordinate && m.DestinationCoordinate == c));

                            _dragStartCell = clickedCellCoordinate;

                            SetBoardColourOverrides(validDestinations);
                        }
                    }
                }
            }
            else if (_aiThinkTask.IsCompleted)
            {
                var move = _aiThinkTask.Result;
                _aiThinkTask = null;

                ExecuteMove(move, true, (int)gameTime.TotalGameTime.TotalMilliseconds);

                SetBoardColourOverrides(null);
            }
            else
            {
                _game.Window.Title = _blackAi.ProgressString.Read();
            }
        }

        private void ExecuteMove(Move move, bool animateMove, int currentMs = 0) 
        {
            if (animateMove)
            {
                _displayBoard.InitialiseAnimation(_boardState, move, currentMs);
            }

            _boardState.MakeMove(move);

            _moves = Core.MoveGeneration.MoveGenerator.GetMoves(_boardState);

            var gameState = _boardState.UpdateGameState(_moves.Count());

            // Update the FEN string
            _ui.SetFenText(_boardState.GetFenString());
            _ui.AddMove(move, gameState);

            if (_boardState.IsWhitesTurn && _whiteAi != null)
            {
                _aiThinkTask = Task.Run<Move>(() => _whiteAi.ChooseMove(_boardState));
            }
            else if (_boardState.IsWhitesTurn == false && _blackAi != null)
            {
                _aiThinkTask = Task.Run<Move>(() => _blackAi.ChooseMove(_boardState));
            }
        }

        private void SetBoardColourOverrides(IEnumerable<CubeCoordinate> possibleDestinationCells)
        {
            _displayBoard.ClearAllCellColourOverrides();

            // Highlight last cell
            var (lastMoveHighlightIndex1, lastMoveHighlightIndex2) = _boardState.GetLastMoveHighlightIndices();
            if (lastMoveHighlightIndex1.HasValue)
            {
                _displayBoard.SetCellColourOverride(lastMoveHighlightIndex1.Value, HexCell.ColourOverride.Green);
                _displayBoard.SetCellColourOverride(lastMoveHighlightIndex2.Value, HexCell.ColourOverride.Green);
            }

            // Colour enemy king cell red if currently in check
            var kingIndex = _boardState.IsWhitesTurn ? _boardState.WhiteKingIndex : _boardState.BlackKingIndex;

            if (_boardState.Attacks.IsUnderAttack(kingIndex, !_boardState.IsWhitesTurn))
            {
                _displayBoard.SetCellColourOverride(kingIndex, HexCell.ColourOverride.Red);
            }

            if (possibleDestinationCells != null)
            {
                foreach (var destination in possibleDestinationCells)
                {
                    _displayBoard.SetCellColourOverride(destination, HexCell.ColourOverride.Blue);
                }
            }
        }

        //private IEnumerable<CubeCoordinate> GetValidDestinations(BoardState boardState, CubeCoordinate start)
        //{
        //    return CoordinateHelpers.BOARD_COORDINATES.Where(x => boardState.ValidateMove(start, x) != null);
        //}

        public override void Draw(GameTime gameTime)
        {
            var sb = _game.SpriteBatch;

            _game.GraphicsDevice.Clear(Color.White);

            // Draw the cells
            _primitiveBatch.Begin(ref _localProjection, ref _localView);
            _displayBoard.DrawCells(_primitiveDrawing);
            _primitiveBatch.End();

            sb.Begin();

            // Draw any data to be printed on the cells
            _displayBoard.DrawData(sb, _arial);

            // Draw the pieces
            _displayBoard.DrawPieces(sb, _boardState, _dragStartCell, _mouseState.Position, (int)gameTime.TotalGameTime.TotalMilliseconds);

            sb.DrawString(_arial, _data, new Vector2(10, 10), Color.Black);

            _ui.Draw();

            sb.End();
        }
    }
}
