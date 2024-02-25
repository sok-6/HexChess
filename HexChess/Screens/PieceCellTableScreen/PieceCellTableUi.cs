using HexChess.Core;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Screens.PieceCellTableScreen
{
    internal class PieceCellTableUi
    {
        private PieceCellTableScreen _parent;

        private Desktop _desktop;

        private ListView _pieceList;
        private ListView _selectionList;

        public PieceCellTableUi(PieceCellTableScreen parent)
        {
            _parent = parent;

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
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            var selectTypeLabel = new Label
            {
                Id = "selectTypeLabel",
                Text = "Select Piece Type",
                TextColor = Color.Black
            };
            Grid.SetRow(selectTypeLabel, 0);
            Grid.SetColumn(selectTypeLabel, 0);
            grid.Widgets.Add(selectTypeLabel);

            _pieceList = new ListView
            {
                Id = "pieceList"
            };
            _pieceList.Widgets.Add(new Label { Text = Piece.Pawn.ToString() });
            _pieceList.Widgets.Add(new Label { Text = Piece.Bishop.ToString() });
            _pieceList.Widgets.Add(new Label { Text = Piece.Knight.ToString() });
            _pieceList.Widgets.Add(new Label { Text = Piece.Rook.ToString() });
            _pieceList.Widgets.Add(new Label { Text = Piece.Queen.ToString() });
            _pieceList.Widgets.Add(new Label { Text = Piece.King.ToString() });
            _pieceList.SelectedIndexChanged += PieceList_SelectedIndexChanged;
            _pieceList.SelectedIndex = 0;
            Grid.SetRow(_pieceList, 0);
            Grid.SetColumn(_pieceList, 1);
            grid.Widgets.Add(_pieceList);

            var selectMultipleLabel = new Label
            {
                Id = "selectMultipleLabel",
                Text = "Cell Selection Type",
                TextColor = Color.Black
            };
            Grid.SetRow(selectMultipleLabel, 1);
            Grid.SetColumn(selectMultipleLabel, 0);
            grid.Widgets.Add(selectMultipleLabel);

            _selectionList = new ListView
            {
                Id = "selectionList"
            };
            _selectionList.Widgets.Add(new Label { Text = PieceCellTableSelectionType.Single.ToString() });
            _selectionList.Widgets.Add(new Label { Text = PieceCellTableSelectionType.Chevron.ToString() });
            _selectionList.Widgets.Add(new Label { Text = PieceCellTableSelectionType.Ring.ToString() });
            _selectionList.SelectedIndexChanged += SelectionList_SelectedIndexChanged;
            _selectionList.SelectedIndex = 0;
            Grid.SetRow(_selectionList, 1);
            Grid.SetColumn(_selectionList, 1);
            grid.Widgets.Add(_selectionList);

            var saveButton = new Button
            {
                Id = "saveButton",
                Content = new Label { Text = "Save Tables" }
            };
            saveButton.Click += SaveButton_Click;
            Grid.SetRow(saveButton, 2);
            Grid.SetColumn(saveButton, 0);
            Grid.SetColumnSpan(saveButton, 2);
            grid.Widgets.Add(saveButton);

            _desktop = new Desktop();
            _desktop.Widgets.Add(grid);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            _parent.SaveAiData();
        }

        private void SelectionList_SelectedIndexChanged(object sender, EventArgs e)
        { 
            var selectionType = Enum.Parse<PieceCellTableSelectionType>(((Label)_selectionList.SelectedItem).Text);
            _parent.SelectionType = selectionType;
        }

        private void PieceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var pieceType = Enum.Parse<Piece>(((Label)_pieceList.SelectedItem).Text);
            _parent.LoadPieceCellValues(pieceType);
        }

        public void Draw()
        {
            _desktop.Render();
        }
    }
}
