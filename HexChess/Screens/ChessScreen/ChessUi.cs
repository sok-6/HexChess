using HexChess.Core;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Screens.ChessScreen
{
    internal class ChessUi
    {
        private Desktop _desktop;

        private TextBox _fenText;
        private Label _moveHistory;

        public ChessUi()
        {
            // Set up the UI
            var grid = new Grid()
            {
                RowSpacing = 8,
                ColumnSpacing = 8,
                Left = 1000,
                Top = 100
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            // Current position FEN string
            var fenLabel = new Label()
            {
                Id = "fenLabel",
                Text = "Current Position",
                TextColor = Color.Black
            };

            Grid.SetRow(fenLabel, 0);
            Grid.SetColumn(fenLabel, 0);

            grid.Widgets.Add(fenLabel);

            _fenText = new TextBox()
            {
                Id = "fenText",
                MinWidth = 600,
                Readonly = true
            };

            Grid.SetRow(_fenText, 0);
            Grid.SetColumn(_fenText, 1);

            grid.Widgets.Add(_fenText);

            // Move history
            var moveLabel = new Label()
            {
                Id = "moveLabel",
                Text = "Move History",
                TextColor = Color.Black
            };

            Grid.SetRow(moveLabel, 1);
            Grid.SetColumn(moveLabel, 0);

            grid.Widgets.Add(moveLabel);

            _moveHistory = new Label()
            {
                Id = "moveHistory",
                TextColor = Color.Black
            };

            Grid.SetRow(_moveHistory, 1);
            Grid.SetColumn(_moveHistory, 1);

            grid.Widgets.Add(_moveHistory);

            _desktop = new Desktop();
            _desktop.Root = grid;
        }

        public void SetFenText(string fenString)
        {
            _fenText.Text = fenString;
        }

        public void AddMove(Move move, GameState gameState)
        {
            var sb = new StringBuilder(_moveHistory.Text);

            var captureString = move.CapturedPiece == Piece.None ? "" : "x";
            var moveEnd = gameState == GameState.Check ? "+" : gameState == GameState.Checkmate ? "#" : "";

            sb.AppendLine($"{move.StartCoordinate.ToAlgebraic()}{captureString}{move.DestinationCoordinate.ToAlgebraic()}{moveEnd}");

            _moveHistory.Text = sb.ToString();
        }

        public void Draw()
        {
            _desktop.Render();
        }
    }
}
