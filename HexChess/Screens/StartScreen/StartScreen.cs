using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Myra;
using Myra.Graphics2D.UI;

namespace HexChess.Screens.StartScreen
{
    internal class StartScreen : GameScreen
    {
        private Game1 _game => (Game1)Game;

        private Desktop _desktop;

        private ComboView _whitePlayerCombo;
        private ComboView _blackPlayerCombo;
        private TextBox _fenText;

        public StartScreen(Game game) : base(game)
        {
            
        }

        public override void LoadContent()
        {
            var grid = new Grid()
            {
                RowSpacing = 8,
                ColumnSpacing = 8,
                Left = 800,
                Top = 200
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // Start Game Label
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // White Player selection
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // Black Player selection
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // FEN string
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // Start Game
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto)); // Open Piece Value Viewer

            var playLabel = new Label
            {
                Id = "playLabel",
                Text = "Play",
                Scale = new Vector2(1.5f, 1.5f)
            };
            Grid.SetRow(playLabel, 0);
            Grid.SetColumn(playLabel, 1);
            Grid.SetColumnSpan(playLabel, 2);

            grid.Widgets.Add(playLabel);

            var whitePlayerLabel = new Label
            {
                Id = "whitePlayerLabel",
                Text = "White Player"
            };
            Grid.SetRow(whitePlayerLabel, 1);
            Grid.SetColumn(whitePlayerLabel, 0);
            grid.Widgets.Add(whitePlayerLabel);

            _whitePlayerCombo = new ComboView
            {
                Id = "whitePlayerCombo"
            };
            _whitePlayerCombo.Widgets.Add(new Label { Id = "Human", Text = "Human" });
            _whitePlayerCombo.Widgets.Add(new Label { Id = "DefaultAi", Text = "DefaultAi" });
            _whitePlayerCombo.SelectedIndex = 0;
            Grid.SetRow(_whitePlayerCombo, 1);
            Grid.SetColumn(_whitePlayerCombo, 1);
            grid.Widgets.Add(_whitePlayerCombo);

            var blackPlayerLabel = new Label
            {
                Id = "blackPlayerLabel",
                Text = "Black Player"
            };
            Grid.SetRow(blackPlayerLabel, 2);
            Grid.SetColumn(blackPlayerLabel, 0);
            grid.Widgets.Add(blackPlayerLabel);

            _blackPlayerCombo = new ComboView
            {
                Id = "blackPlayerCombo"
            };
            _blackPlayerCombo.Widgets.Add(new Label { Id = "Human", Text = "Human" });
            _blackPlayerCombo.Widgets.Add(new Label { Id = "DefaultAi", Text = "DefaultAi" });
            _blackPlayerCombo.SelectedIndex = 0;
            Grid.SetRow(_blackPlayerCombo, 2);
            Grid.SetColumn(_blackPlayerCombo, 1);
            grid.Widgets.Add(_blackPlayerCombo);

            var fenLabel = new Label
            {
                Id = "fenLabel",
                Text = "FEN String"
            };
            Grid.SetRow(fenLabel, 3);
            Grid.SetColumn(fenLabel, 0);
            grid.Widgets.Add(fenLabel);

            _fenText = new TextBox
            {
                Id = "fenText",
                MinWidth = 400
            };
            Grid.SetRow(_fenText, 3);
            Grid.SetColumn(_fenText, 1);
            grid.Widgets.Add(_fenText);

            var startButton = new Button
            {
                Id = "startButton",
                Content = new Label { Text = "Start" },
                HorizontalAlignment = HorizontalAlignment.Center
            };
            startButton.Click += StartButton_Click;
            Grid.SetRow(startButton, 4);
            Grid.SetColumn(startButton, 0);
            Grid.SetColumnSpan(startButton, 2);
            grid.Widgets.Add(startButton);

            var pieceCellTableButton = new Button
            {
                Id = "pieceCellTableButton",
                Content = new Label { Text = "Piece Cell Table Viewer" },
                HorizontalAlignment = HorizontalAlignment.Center
            };
            pieceCellTableButton.Click += PieceCellTableButton_Click;
            Grid.SetRow(pieceCellTableButton, 6);
            Grid.SetColumn(pieceCellTableButton, 0);
            Grid.SetColumnSpan(pieceCellTableButton, 2);
            grid.Widgets.Add(pieceCellTableButton);

            // Add it to the desktop
            _desktop = new Desktop();
            _desktop.Root = grid;
        }

        private void PieceCellTableButton_Click(object sender, EventArgs e)
        {
            _game.LoadPieceCellTableScreen();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            var options = new ChessScreen.InitialisationOptions
            {
                WhitePlayer = ParsePlayerType(_whitePlayerCombo.SelectedItem.Id),
                BlackPlayer = ParsePlayerType(_blackPlayerCombo.SelectedItem.Id),
                FenString = _fenText.Text
            };

            _game.LoadChessScreen(options);
        }

        private ChessScreen.InitialisationOptions.PlayerType ParsePlayerType(string playerTypeString)
        {
            if (playerTypeString == "DefaultAi")
            {
                return ChessScreen.InitialisationOptions.PlayerType.Ai;
            }

            return ChessScreen.InitialisationOptions.PlayerType.Human;
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _desktop.Render();
        }

        public override void Update(GameTime gameTime)
        {
            //throw new NotImplementedException();
        }
    }
}
