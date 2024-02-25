using HexChess.Core;
using HexChess.Core.AI.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.VectorDraw;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Screens.PieceCellTableScreen
{
    internal class PieceCellTableScreen : GameScreen
    {
        private Game1 _game => (Game1)Game;

        private PieceCellTableUi _ui;

        private GraphicalBoard _displayBoard;

        private PrimitiveBatch _primitiveBatch;
        private PrimitiveDrawing _primitiveDrawing;
        private Matrix _localProjection;
        private Matrix _localView;

        private SpriteFont _arial;

        private DefaultAiData _aiData;

        private int[] _values;

        private HashSet<int> _selectedCellIndices;

        public PieceCellTableSelectionType SelectionType { get; set; }

        public PieceCellTableScreen(Game game) : base(game)
        {
            _displayBoard = new GraphicalBoard(new Vector2(600, 500));

            _selectedCellIndices = new HashSet<int>();
        }

        private void LoadAiData()
        {
            var dir = System.IO.Directory.GetCurrentDirectory();
            var text = System.IO.File.ReadAllText(dir + @"\AiData\DefaultAi.json");
            _aiData = Newtonsoft.Json.JsonConvert.DeserializeObject<DefaultAiData>(text);
        }

        public void SaveAiData()
        {
            var dir = System.IO.Directory.GetCurrentDirectory();
            var path = dir + @"\AiData\DefaultAi.json";
            var text = Newtonsoft.Json.JsonConvert.SerializeObject(_aiData, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(path, text);
        }

        public override void LoadContent()
        {
            _primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            _primitiveDrawing = new PrimitiveDrawing(_primitiveBatch);
            _localProjection = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            _localView = Matrix.Identity;

            _arial = _game.Content.Load<SpriteFont>("Arial");

            LoadAiData();

            _ui = new PieceCellTableUi(this);
        }

        public void LoadPieceCellValues(Piece pieceType)
        {
            switch (pieceType)
            {
                case Piece.Bishop:
                    _values = _aiData.PieceCellTables.BishopCellTable;
                    break;
                case Piece.Knight:
                    _values = _aiData.PieceCellTables.KnightCellTable;
                    break;
                case Piece.Rook:
                    _values = _aiData.PieceCellTables.RookCellTable;
                    break;
                case Piece.Queen:
                    _values = _aiData.PieceCellTables.QueenCellTable;
                    break;
                case Piece.King:
                    _values = _aiData.PieceCellTables.KingCellTable;
                    break;
                default: // Assume pawn
                    _values = _aiData.PieceCellTables.PawnCellTable;
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            var mouse = MouseExtended.GetState();
            var keyboard = KeyboardExtended.GetState();

            // Selecting different cells
            if (mouse.WasButtonJustDown(MouseButton.Left))
            {
                var clickedCell = _displayBoard.GetClickedCell(mouse.Position.ToVector2());

                if (clickedCell != null)
                {
                    _selectedCellIndices.Clear();

                    if (SelectionType == PieceCellTableSelectionType.Chevron)
                    {
                        // Find the cell in the centre of the chevron
                        var stepDirection = (clickedCell.Q < 0) ? MovementDirection.UpRight : MovementDirection.UpLeft;

                        var centreCell = clickedCell.Step(stepDirection, Math.Abs(clickedCell.Q));

                        _selectedCellIndices.Add(centreCell.ToArrayIndex());

                        // Move down and out until reach edge of teh board
                        var steps = 0;
                        while (true)
                        {
                            steps++;

                            var currentCell = centreCell.Step(MovementDirection.DownLeft, steps);
                            if (currentCell.IsOnBoard() == false)
                            {
                                break;
                            }
                            _selectedCellIndices.Add(currentCell.ToArrayIndex());

                            currentCell = centreCell.Step(MovementDirection.DownRight, steps);
                            _selectedCellIndices.Add(currentCell.ToArrayIndex());
                        }
                    }
                    else if (SelectionType == PieceCellTableSelectionType.Ring)
                    {
                        // Max absolute value out of coordinates is ring number
                        var ringNumber = Math.Max(Math.Abs(clickedCell.Q), Math.Max(Math.Abs(clickedCell.R), Math.Abs(clickedCell.S)));

                        if (ringNumber == 0)
                        {
                            // Central 'ring' only has middle cell in it
                            _selectedCellIndices.Add(clickedCell.ToArrayIndex());
                        }
                        else
                        {
                            // Start at bottom left of ring - this lines up with the directions - U > UR > DR > D > DL > UL
                            var currentCell = new CubeCoordinate(-ringNumber, ringNumber, 0);

                            for (int directionCounter = 0; directionCounter < 6; directionCounter++)
                            {
                                var direction = (MovementDirection)directionCounter;

                                for (int steps = 0; steps < ringNumber; steps++)
                                {
                                    _selectedCellIndices.Add(currentCell.ToArrayIndex());
                                    currentCell = currentCell.Step(direction);
                                }
                            } 
                        }
                    }
                    else
                    {
                        // Single cell selection
                        _selectedCellIndices.Add(clickedCell.ToArrayIndex());
                    }
                }
            }

            if (keyboard.WasKeyJustDown(Keys.Back))
            {
                // Backspace is to delete last digit - this is truncated division by 10
                foreach (var index in _selectedCellIndices)
                {
                    _values[index] /= 10;
                }
            }

            if (keyboard.WasKeyJustDown(Keys.D0)) foreach (var index in _selectedCellIndices) _values[index] = (_values[index] * 10) + 0;
            if (keyboard.WasKeyJustDown(Keys.D1)) foreach (var index in _selectedCellIndices) _values[index] = (_values[index] * 10) + 1;
            if (keyboard.WasKeyJustDown(Keys.D2)) foreach (var index in _selectedCellIndices) _values[index] = (_values[index] * 10) + 2;
            if (keyboard.WasKeyJustDown(Keys.D3)) foreach (var index in _selectedCellIndices) _values[index] = (_values[index] * 10) + 3;
            if (keyboard.WasKeyJustDown(Keys.D4)) foreach (var index in _selectedCellIndices) _values[index] = (_values[index] * 10) + 4;
            if (keyboard.WasKeyJustDown(Keys.D5)) foreach (var index in _selectedCellIndices) _values[index] = (_values[index] * 10) + 5;
            if (keyboard.WasKeyJustDown(Keys.D6)) foreach (var index in _selectedCellIndices) _values[index] = (_values[index] * 10) + 6;
            if (keyboard.WasKeyJustDown(Keys.D7)) foreach (var index in _selectedCellIndices) _values[index] = (_values[index] * 10) + 7;
            if (keyboard.WasKeyJustDown(Keys.D8)) foreach (var index in _selectedCellIndices) _values[index] = (_values[index] * 10) + 8;
            if (keyboard.WasKeyJustDown(Keys.D9)) foreach (var index in _selectedCellIndices) _values[index] = (_values[index] * 10) + 9;
        }

        public override void Draw(GameTime gameTime)
        {
            var spriteBatch = _game.SpriteBatch;

            _game.GraphicsDevice.Clear(Color.White);

            // Draw the cells
            _primitiveBatch.Begin(ref _localProjection, ref _localView);

            var minValue = _values.Min();
            var maxValue = _values.Max();

            foreach (var cell in _displayBoard.Cells)
            {
                var lerpValue = (maxValue == minValue) ? 0f : (float)(_values[cell.ArrayIndex] - minValue) / (maxValue - minValue);
                var colour = Color.Lerp(new Color(0, 0, 255), new Color(200, 200, 255), lerpValue);
                cell.DrawFillSpecificColour(_primitiveDrawing, _displayBoard.BoardCentre, colour);
            }

            _primitiveBatch.End();

            spriteBatch.Begin();

            // Draw data values
            foreach (var cell in _displayBoard.Cells)
            {
                spriteBatch.DrawCenteredString(_arial, _values[cell.ArrayIndex].ToString(), cell.GetHexCentre(_displayBoard.BoardCentre, true), Color.Black);
            }

            // Draw any highlights
            foreach (var selectedIndex in _selectedCellIndices)
            {
                _displayBoard.Cells[selectedIndex].DrawEdge(spriteBatch, _displayBoard.BoardCentre, Color.Red);
            }

            spriteBatch.End();

            _ui.Draw();
        }
    }

    internal enum PieceCellTableSelectionType
    {
        Single,
        Chevron,
        Ring
    }
}
