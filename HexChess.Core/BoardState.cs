using HexChess.Core.MoveGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public class BoardState
    {
        private const string START_FEN = "BKNRP1/QB2P2/N1B1P3/R3P4/PPPPP5/11/5ppppp/4p3r/3p1b1n/2p2bk/1prnqb w - 0 1";

        //public IReadOnlyList<Piece> Cells => _cells;
        private Piece[] _cells;

        public IReadOnlySet<int> WhitePawnIndices => _whitePawnIndices;
        private HashSet<int> _whitePawnIndices;

        public IReadOnlySet<int> WhiteBishopIndices => _whiteBishopIndices;
        private HashSet<int> _whiteBishopIndices;

        public IReadOnlySet<int> WhiteKnightIndices => _whiteKnightIndices;
        private HashSet<int> _whiteKnightIndices;

        public IReadOnlySet<int> WhiteRookIndices => _whiteRookIndices;
        private HashSet<int> _whiteRookIndices;

        public IReadOnlySet<int> WhiteQueenIndices => _whiteQueenIndices;
        private HashSet<int> _whiteQueenIndices;

        public int WhiteKingIndex { get; private set; }

        public IReadOnlySet<int> BlackPawnIndices => _blackPawnIndices;
        private HashSet<int> _blackPawnIndices;

        public IReadOnlySet<int> BlackBishopIndices => _blackBishopIndices;
        private HashSet<int> _blackBishopIndices;

        public IReadOnlySet<int> BlackKnightIndices => _blackKnightIndices;
        private HashSet<int> _blackKnightIndices;

        public IReadOnlySet<int> BlackRookIndices => _blackRookIndices;
        private HashSet<int> _blackRookIndices;

        public IReadOnlySet<int> BlackQueenIndices => _blackQueenIndices;
        private HashSet<int> _blackQueenIndices;

        public int BlackKingIndex { get; private set; }

        public bool IsWhitesTurn { get; private set; }

        public int? EnPassantIndex { get; private set; }

        public int HalfMoveClock { get; private set; }

        public int TurnNumber { get; private set; }

        public AttackManager Attacks { get; private set; }

        //public GameState GameState { get; private set; }

        private Stack<MoveRecord> _stateStack;

        /// <summary>
        /// Creates a new board state
        /// </summary>
        /// <param name="fenString">The FEN string which defines the initial position. If null, loads the start position</param>
        public BoardState(string fenString = null)
        {
            _cells = new Piece[91];

            _whitePawnIndices = new HashSet<int>();
            _whiteBishopIndices = new HashSet<int>();
            _whiteKnightIndices = new HashSet<int>();
            _whiteRookIndices = new HashSet<int>();
            _whiteQueenIndices = new HashSet<int>();

            _blackPawnIndices = new HashSet<int>();
            _blackBishopIndices = new HashSet<int>();
            _blackKnightIndices = new HashSet<int>();
            _blackRookIndices = new HashSet<int>();
            _blackQueenIndices = new HashSet<int>();

            _stateStack = new Stack<MoveRecord>();

            Attacks = new AttackManager();

            LoadFenString(fenString ?? START_FEN);
        }

        public BoardState(BoardState parent)
        {
            _cells = new Piece[91];
            for (var i = 0; i < 91; i++) _cells[i] = parent.GetPieceInCell(i);

            _whitePawnIndices = new HashSet<int>(parent.WhitePawnIndices);
            _whiteBishopIndices = new HashSet<int>(parent.WhiteBishopIndices);
            _whiteKnightIndices = new HashSet<int>(parent.WhiteKnightIndices);
            _whiteRookIndices = new HashSet<int>(parent.WhiteRookIndices);
            _whiteQueenIndices = new HashSet<int>(parent.WhiteQueenIndices);
            WhiteKingIndex = parent.WhiteKingIndex;

            _blackPawnIndices = new HashSet<int>(parent.BlackPawnIndices);
            _blackBishopIndices = new HashSet<int>(parent.BlackBishopIndices);
            _blackKnightIndices = new HashSet<int>(parent.BlackKnightIndices);
            _blackRookIndices = new HashSet<int>(parent.BlackRookIndices);
            _blackQueenIndices = new HashSet<int>(parent.BlackQueenIndices);
            BlackKingIndex = parent.BlackKingIndex;

            IsWhitesTurn = parent.IsWhitesTurn;
            EnPassantIndex = parent.EnPassantIndex;
            HalfMoveClock = parent.HalfMoveClock;
            TurnNumber = parent.TurnNumber;

            Attacks = new AttackManager();
            Attacks.Recalculate(this);

            _stateStack = new Stack<MoveRecord>();
        }

        private void LoadFenString(string fenString)
        {
            var fenParts = fenString.Split(' ');

            // First part is piece layout
            var pieceString = fenParts[0]
                .Replace("11", "-----------")
                .Replace("10", "----------")
                .Replace("9", "---------")
                .Replace("8", "--------")
                .Replace("7", "-------")
                .Replace("6", "------")
                .Replace("5", "-----")
                .Replace("4", "----")
                .Replace("3", "---")
                .Replace("2", "--")
                .Replace("1", "-")
                .Replace("/", "");

            for (int i = 0; i < 91; i++)
            {
                var piece = Piece.None;

                if (pieceString[i] == 'P')
                {
                    //_whitePawnIndices.Add(i);
                    piece = Piece.White | Piece.Pawn;
                }

                if (pieceString[i] == 'B')
                {
                    //_whiteBishopIndices.Add(i);
                    piece = Piece.White | Piece.Bishop;
                }

                if (pieceString[i] == 'N')
                {
                    //_whiteKnightIndices.Add(i);
                    piece = Piece.White | Piece.Knight;
                }

                if (pieceString[i] == 'R')
                {
                    //_whiteRookIndices.Add(i);
                    piece = Piece.White | Piece.Rook;
                }

                if (pieceString[i] == 'Q')
                {
                    //_whiteQueenIndices.Add(i);
                    piece = Piece.White | Piece.Queen;
                }

                if (pieceString[i] == 'K')
                {
                    //WhiteKingIndex = i;
                    piece = Piece.White | Piece.King;
                }

                if (pieceString[i] == 'p')
                {
                    //_blackPawnIndices.Add(i);
                    piece = Piece.Black | Piece.Pawn;
                }

                if (pieceString[i] == 'b')
                {
                    //_blackBishopIndices.Add(i);
                    piece = Piece.Black | Piece.Bishop;
                }

                if (pieceString[i] == 'n')
                {
                    //_blackKnightIndices.Add(i);
                    piece = Piece.Black | Piece.Knight;
                }

                if (pieceString[i] == 'r')
                {
                    //_blackRookIndices.Add(i);
                    piece = Piece.Black | Piece.Rook;
                }

                if (pieceString[i] == 'q')
                {
                    //_blackQueenIndices.Add(i);
                    piece = Piece.Black | Piece.Queen;
                }

                if (pieceString[i] == 'k')
                {
                    //BlackKingIndex = i;
                    piece = Piece.Black | Piece.King;
                }

                if (piece != Piece.None)
                {
                    PlacePieceInCell(piece, i); 
                }

                //_cells[i] = piece;
            }

            // Next is turn indicator
            IsWhitesTurn = fenParts[1] == "w";

            // Next is en passant target
            if (fenParts[2] == "-")
            {
                EnPassantIndex = null;
            }
            else
            {
                EnPassantIndex = CubeCoordinate.FromAlgebraic(fenParts[2]).ToArrayIndex();
            }

            // Next is half move clock
            HalfMoveClock = int.Parse(fenParts[3]);

            // Finally comes the turn number
            TurnNumber = int.Parse(fenParts[4]);
        }

        public void MakeMove(Move move)
        {
            var moveRecord = new MoveRecord()
            {
                PreviousHalfMoveClock = HalfMoveClock,
                PreviousTurnCounter = TurnNumber,
                PreviousEnPassantIndex = EnPassantIndex,
                PreviousWhiteTurnEh = IsWhitesTurn,
                //PreviousGameState = GameState,
                RemovedPieces = new List<(int, Piece)>(),
                AddedPieces = new List<(int, Piece)>()
            };

            // If move is a capture, remove target from set of pieces
            Piece capturedPiece = move.CapturedPiece;
            if (capturedPiece != Piece.None)
            {
                // En passant weirdness
                if (move.Flags == Move.MoveFlags.EnPassantCapture)
                {
                    RemovePieceFromCell(capturedPiece, move.EnPassantIndex);
                    moveRecord.RemovedPieces.Add((move.EnPassantIndex, capturedPiece));
                }
                else
                {
                    RemovePieceFromCell(capturedPiece, move.DestinationIndex);
                    moveRecord.RemovedPieces.Add((move.DestinationIndex, capturedPiece));
                }
            }

            var movingPieceType = GetPieceInCell(move.StartIndex);

            RemovePieceFromCell(movingPieceType, move.StartIndex);
            moveRecord.RemovedPieces.Add((move.StartIndex, movingPieceType));

            // If pawn promotion, add a different piece to the moving one
            if (move.Flags == Move.MoveFlags.PromoteToBishop || move.Flags == Move.MoveFlags.PromoteToKnight || move.Flags == Move.MoveFlags.PromoteToRook || move.Flags == Move.MoveFlags.PromoteToQueen)
            {
                var newPiece = Piece.Bishop;
                if (move.Flags == Move.MoveFlags.PromoteToKnight) newPiece = Piece.Knight;
                if (move.Flags == Move.MoveFlags.PromoteToRook) newPiece = Piece.Rook;
                if (move.Flags == Move.MoveFlags.PromoteToQueen) newPiece = Piece.Queen;

                newPiece |= (movingPieceType.IsWhite() ? Piece.White : Piece.Black);

                PlacePieceInCell(newPiece, move.DestinationIndex);
                moveRecord.AddedPieces.Add((move.DestinationIndex, newPiece));
            }
            else
            {
                PlacePieceInCell(movingPieceType, move.DestinationIndex);
                moveRecord.AddedPieces.Add((move.DestinationIndex, movingPieceType));
            }

            // If double pawn push, set the en passant index
            if (move.Flags == Move.MoveFlags.DoublePawnPush)
            {
                EnPassantIndex = move.EnPassantIndex;
            }
            else
            {
                EnPassantIndex = null;
            }

            // Increment half turn clock unless pawn move or capture
            if (capturedPiece != Piece.None || movingPieceType.IsSameTypeAs(Piece.Pawn))
            {
                HalfMoveClock = 0;
            }
            else
            {
                HalfMoveClock++;
            }

            // Toggle turn
            ToggleTurn();

            moveRecord.NextEnPassantIndex = EnPassantIndex;
            moveRecord.NextHalfMoveClock = HalfMoveClock;
            moveRecord.NextTurnCounter = TurnNumber;
            moveRecord.NextWhiteTurnEh = IsWhitesTurn;
            //moveRecord.NextGameState = GameState;
            moveRecord.LastMoveHighlightIndex1 = move.StartIndex;
            moveRecord.LastMoveHighlightIndex2 = move.DestinationIndex;

            _stateStack.Push(moveRecord);
        }

        public void UnmakeMove()
        {
            // Only undo if move on the stack
            if (_stateStack.Any() == false)
            {
                return;
            }

            var lastMoveRecord = _stateStack.Pop();

            HalfMoveClock = lastMoveRecord.PreviousHalfMoveClock;
            TurnNumber = lastMoveRecord.PreviousTurnCounter;
            IsWhitesTurn = lastMoveRecord.PreviousWhiteTurnEh;
            EnPassantIndex = lastMoveRecord.PreviousEnPassantIndex;
            //GameState = lastMoveRecord.PreviousGameState;

            // Remove pieces added last move
            foreach (var (i, p) in lastMoveRecord.AddedPieces)
            {
                RemovePieceFromCell(p, i);
            }

            // Add piece removed last move
            foreach (var (i, p) in lastMoveRecord.RemovedPieces)
            {
                PlacePieceInCell(p, i);
            }
        }

        public GameState UpdateGameState(int availableMoveCount)
        {
            if ((IsWhitesTurn && Attacks.IsUnderAttack(WhiteKingIndex, false)) || (!IsWhitesTurn && Attacks.IsUnderAttack(BlackKingIndex, true)))
            {
                if (availableMoveCount > 0)
                {
                    return GameState.Check;
                }
                else
                {
                    return GameState.Checkmate;
                }
            }
            else if (availableMoveCount > 0)
            {
                return GameState.Stalemate;
            }
            else if (HalfMoveClock == 100)
            {
                return GameState.Draw;
            }
            else
            {
                return GameState.Normal;
            }
        }

        private HashSet<int> GetPieceSet(Piece piece)
        {
            if (piece == (Piece.White | Piece.Pawn)) return _whitePawnIndices;
            if (piece == (Piece.White | Piece.Bishop)) return _whiteBishopIndices;
            if (piece == (Piece.White | Piece.Knight)) return _whiteKnightIndices;
            if (piece == (Piece.White | Piece.Rook)) return _whiteRookIndices;
            if (piece == (Piece.White | Piece.Queen)) return _whiteQueenIndices;

            if (piece == (Piece.Black | Piece.Pawn)) return _blackPawnIndices;
            if (piece == (Piece.Black | Piece.Bishop)) return _blackBishopIndices;
            if (piece == (Piece.Black | Piece.Knight)) return _blackKnightIndices;
            if (piece == (Piece.Black | Piece.Rook)) return _blackRookIndices;
            if (piece == (Piece.Black | Piece.Queen)) return _blackQueenIndices;

            return null;
        }

        public Piece GetPieceInCell(int cellIndex)
        {
            return _cells[cellIndex];
        }

        private void PlacePieceInCell(Piece piece, CubeCoordinate coordinate) => PlacePieceInCell(piece, coordinate.ToArrayIndex());

        private void PlacePieceInCell(Piece piece, int index)
        {
            // Add piece into cell array
            _cells[index] = piece;

            // If king, update the index
            if (piece == (Piece.King | Piece.White))
            {
                WhiteKingIndex = index;
            }
            else if (piece == (Piece.King | Piece.Black))
            {
                BlackKingIndex = index;
            }
            else
            {
                // Update the piece set
                var set = GetPieceSet(piece);

                set.Add(index);
            }

            // Update the attacked cells
            Attacks.AddPiece(index, piece, this);
        }

        private void RemovePieceFromCell(Piece piece, int index)
        {
            // Update attacks first
            Attacks.RemovePiece(index, piece, this);

            // Clear cell array
            _cells[index] = Piece.None;

            // Only remove from piece set if not a king
            if (piece.IsKing() == false)
            {
                var set = GetPieceSet(piece);

                set.Remove(index);
            }
        }

        public void ToggleTurn()
        {
            if (!IsWhitesTurn)
            {
                TurnNumber++;
            }

            IsWhitesTurn = !IsWhitesTurn;
        }

        public (int?, int?) GetLastMoveHighlightIndices()
        {
            if (_stateStack.Any())
            {
                var lastMoveRecord = _stateStack.Peek();

                return (lastMoveRecord.LastMoveHighlightIndex1, lastMoveRecord.LastMoveHighlightIndex2);
            }
            else
            {
                return (null, null);
            }
        }

        public string GetFenString()
        {
            var fenParts = new string[5];

            // Board representation
            var uncompressedString = string.Join("", _cells.Select(c =>
            {
                string p = "-";

                if (c.IsSameTypeAs(Piece.Pawn)) p = "p";
                else if (c.IsSameTypeAs(Piece.Bishop)) p = "b";
                else if (c.IsSameTypeAs(Piece.Knight)) p = "n";
                else if (c.IsSameTypeAs(Piece.Rook)) p = "r";
                else if (c.IsSameTypeAs(Piece.Queen)) p = "q";
                else if (c.IsSameTypeAs(Piece.King)) p = "k";

                if (c.IsWhite()) p = p.ToUpper();

                return p;
            }));

            var boardParts = new string[11];

            boardParts[0] = uncompressedString.Substring(0, 6);
            boardParts[1] = uncompressedString.Substring(6, 7);
            boardParts[2] = uncompressedString.Substring(13, 8);
            boardParts[3] = uncompressedString.Substring(21, 9);
            boardParts[4] = uncompressedString.Substring(30, 10);
            boardParts[5] = uncompressedString.Substring(40, 11);
            boardParts[6] = uncompressedString.Substring(51, 10);
            boardParts[7] = uncompressedString.Substring(61, 9);
            boardParts[8] = uncompressedString.Substring(70, 8);
            boardParts[9] = uncompressedString.Substring(78, 7);
            boardParts[10] = uncompressedString.Substring(85, 6);

            for (int i = 0; i < 11; i++)
            {
                var s = boardParts[i];

                boardParts[i] = s
                    .Replace("-----------", "11")
                    .Replace("----------", "10")
                    .Replace("---------", "9")
                    .Replace("--------", "8")
                    .Replace("-------", "7")
                    .Replace("------", "6")
                    .Replace("-----", "5")
                    .Replace("----", "4")
                    .Replace("---", "3")
                    .Replace("--", "2")
                    .Replace("-", "1");
            }

            fenParts[0] = string.Join('/', boardParts);

            // Turn indicator
            fenParts[1] = IsWhitesTurn ? "w" : "b";

            // En passant indicator
            fenParts[2] = EnPassantIndex.HasValue ? CubeCoordinate.FromArrayIndex(EnPassantIndex.Value).ToAlgebraic() : "-";

            // Half turn counter
            fenParts[3] = HalfMoveClock.ToString();

            // Full turn counter
            fenParts[4] = TurnNumber.ToString();

            return string.Join(" ", fenParts);
        }
    }
}
