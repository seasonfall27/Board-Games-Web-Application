using System;
using System.Collections.Generic;
using System.Text;
using Cecs475.BoardGames.Model;
using System.Linq;

namespace Cecs475.BoardGames.Chess.Model
{
    /// <summary>
    /// Represents the board state of a game of chess. Tracks which squares of the 8x8 board are occupied
    /// by which player's pieces.
    /// </summary>
    public class ChessBoard : IGameBoard
    {
        #region Member fields.
        // The history of moves applied to the board.
        private List<ChessMove> mMoveHistory = new List<ChessMove>();
        private bool whiteKingCastle = false, blackKingCastle = false;
        private bool whiteLeftRook = false, whiteRightRook = false;
        private bool blackLeftRook = false, blackRightRook = false;
        int wkc = 0, bkc = 0, wlr = 0, wrr = 0, blr = 0, brr = 0;
        public const int BoardSize = 8;
        private int mCurrentPlayer = 1;
        private int mAdvantageValue = 0;
        private int prevDrawCounter = 0;
        private IEnumerable<ChessMove> mPossibleMoves;
        //public long BoardWeight => CurrentAdvantage.Player == 1 ? CurrentAdvantage.Advantage : -CurrentAdvantage.Advantage;

        // TODO: create a field for the board position array. You can hand-initialize
        // the starting entries of the array, or set them in the constructor.

        //public byte[] chessBoard = {10101011, 11001101, 11101100, 10111010,
        //							10011001, 10011001, 10011001, 10011001,
        //							00000000, 00000000, 00000000, 00000000,
        //							00000000, 00000000, 00000000, 00000000,
        //							00000000, 00000000, 00000000, 00000000,
        //							00000000, 00000000, 00000000, 00000000,
        //							00010001, 00010001, 00010001, 00010001,
        //							00100011, 01000101, 01100100, 00110010 };

        public byte[] chessBoard = {171, 205, 236, 186,
                                    153, 153, 153, 153,
                                    00000000, 00000000, 00000000, 00000000,
                                    00000000, 00000000, 00000000, 00000000,
                                    00000000, 00000000, 00000000, 00000000,
                                    00000000, 00000000, 00000000, 00000000,
                                    17, 17, 17, 17,
                                    35, 69, 100, 50 };

        //public byte[] chessBoard = {0b_1010_1011, 0b_1100_1101, 0b_1110_1100, 0b_1011_1010,
        //							0b_1001_1001, 0b_1001_1001, 0b_1001_1001, 0b_1001_1001,
        //							0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        //							0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        //							0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        //							0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        //							0b_0001_0001, 0b_0001_0001, 0b_0001_0001, 0b_0001_0001,
        //							0b_0010_0011, 0b_0100_0101, 0b_0110_0100, 0b_0011_0010 };

        private List<byte[]> chessBoardHistory = new List<byte[]>();


        // TODO: Add a means of tracking miscellaneous board state, like captured pieces and the 50-move rule.
        public int moveCount
        {
            get; private set;
        }



        // TODO: add a field for tracking the current player and the board advantage.		
        public int CurrentPlayer { get { return mCurrentPlayer == 1 ? 1 : 2; } }

        public GameAdvantage CurrentAdvantage
        {
            get; private set;

        }



        #endregion

        #region Properties.
        // TODO: implement these properties.
        // You can choose to use auto properties, computed properties, or normal properties 
        // using a private field to back the property.

        // You can add set bodies if you think that is appropriate, as long as you justify
        // the access level (public, private).


        public long BoardWeight
        {
            get
            {
                // Calculates points for the ownership of each piece from CurrentAdvantage
                long bw = CurrentAdvantage.Player == 1 ? CurrentAdvantage.Advantage : -CurrentAdvantage.Advantage;

                long lightPoints = 0;
                long darkPoints = 0;

                var boardPositions = BoardPosition.GetRectangularPositions(8, 8);
                List<BoardPosition> attackedPositions = new List<BoardPosition>();

                //Iterates through each board position
                foreach (BoardPosition pos in boardPositions)
                {
                    ChessPiece currPiece = GetPieceAtPosition(pos);
                    ISet<BoardPosition> attackPositionsForPiece = null;
                    int threatPoints = 0;

                    switch (currPiece.PieceType)
                    {
                        case ChessPieceType.Pawn:
                            // Gets attack position of Pawn at current position
                            attackPositionsForPiece = GetAttackPositionsForPawn(pos, currPiece);

                            // Calculates how much the pawn has moved and adds to board weight
                            if (currPiece.Player == 1)
                            {
                                lightPoints += (6 - pos.Row);
                            }
                            if (currPiece.Player == 2)
                            {
                                darkPoints += (pos.Row - 1);
                            }
                            break;
                        case ChessPieceType.Rook:
                            attackPositionsForPiece = GetAttackPositionsForRook(pos); //Gets attack positions of Rook
                            break;
                        case ChessPieceType.Knight:
                            attackPositionsForPiece = GetAttackPositionsForKnight(pos); //Gets attack positions of Knight
                            break;
                        case ChessPieceType.Bishop:
                            attackPositionsForPiece = GetAttackPositionsForBishop(pos); //Gets attack positions of Bishop
                            break;
                        case ChessPieceType.Queen:
                            attackPositionsForPiece = GetAttackPositionsForQueen(pos); //Gets attack positions of Queen
                            break;
                        case ChessPieceType.King:
                            attackPositionsForPiece = GetAttackPositionsForKing(pos); //Gets attack positions of King
                            break;
                    }

                    //Checks if attacked positions are protected by friendly pieces
                    if (attackPositionsForPiece != null)
                    {
                        foreach (BoardPosition position in attackPositionsForPiece)
                        {
                            int attackedPlayer = GetPieceAtPosition(position).Player;
                            ChessPieceType attackedPieceType = GetPieceAtPosition(position).PieceType;

                            // Adds points for each friendly piece that is protecting players Knight or Bishop
                            if (attackedPlayer == currPiece.Player && attackedPieceType == ChessPieceType.Knight || attackedPieceType == ChessPieceType.Bishop)
                            {
                                if (currPiece.Player == 1)
                                {
                                    lightPoints++;
                                }
                                if (currPiece.Player == 2)
                                {
                                    darkPoints++;
                                }
                            }
                            // Checks attack positions if current piece if there is an opponent's piece threatened
                            else if (attackedPlayer != currPiece.Player && attackedPieceType != ChessPieceType.Empty && attackedPositions.Contains(position) == false)
                            {
                                switch (attackedPieceType)
                                {
                                    case ChessPieceType.Rook:
                                        threatPoints = 2;
                                        break;
                                    case ChessPieceType.Knight:
                                        threatPoints = 1;
                                        break;
                                    case ChessPieceType.Bishop:
                                        threatPoints = 1;
                                        break;
                                    case ChessPieceType.Queen:
                                        threatPoints = 5;
                                        break;
                                    case ChessPieceType.King:
                                        threatPoints = 4;
                                        break;
                                    default:
                                        threatPoints = 0;
                                        break;
                                }

                                // Adds points based on player
                                if (currPiece.Player == 1)
                                {
                                    lightPoints += threatPoints;
                                }
                                else if (currPiece.Player == 2)
                                {
                                    darkPoints += threatPoints;
                                }
                                attackedPositions.Add(position);
                            }
                        }
                    }
                }
                // Sum all the values for each player and subtract Light's score minus Dark's Score
                bw += (lightPoints - darkPoints);
                return bw;
            }
        }

        public bool IsFinished
        {
            get
            {
                return !GetPossibleMoves().Any() | IsDraw;
                //throw new NotImplementedException("You must implement this property.");
                
            }
        }


        private void setAdvantage()
        {
            mAdvantageValue = 0;
            List<ChessPiece> allChessPieces = GetAllChessPiecesOnBoard();
            foreach (ChessPiece item in allChessPieces)
            {
                if (item.Player == 1)
                {
                    if (item.PieceType == ChessPieceType.Pawn)
                    {
                        mAdvantageValue += 1;
                    }
                    else if (item.PieceType == ChessPieceType.Bishop)
                    {
                        mAdvantageValue += 3;
                    }
                    else if (item.PieceType == ChessPieceType.Knight)
                    {
                        mAdvantageValue += 3;
                    }
                    else if (item.PieceType == ChessPieceType.Rook)
                    {
                        mAdvantageValue += 5;
                    }
                    else if (item.PieceType == ChessPieceType.Queen)
                    {
                        mAdvantageValue += 9;
                    }
                }
                if (item.Player == 2)
                {
                    if (item.PieceType == ChessPieceType.Pawn)
                    {
                        mAdvantageValue -= 1;
                    }
                    else if (item.PieceType == ChessPieceType.Bishop)
                    {
                        mAdvantageValue -= 3;
                    }
                    else if (item.PieceType == ChessPieceType.Knight)
                    {
                        mAdvantageValue -= 3;
                    }
                    else if (item.PieceType == ChessPieceType.Rook)
                    {
                        mAdvantageValue -= 5;
                    }
                    else if (item.PieceType == ChessPieceType.Queen)
                    {
                        mAdvantageValue -= 9;
                    }

                }


            }
            CurrentAdvantage = new GameAdvantage(mAdvantageValue > 0 ? 1 : mAdvantageValue < 0 ? 2 : 0,
                Math.Abs(mAdvantageValue));
            //Console.WriteLine(mAdvantageValue);
        }

        public IReadOnlyList<ChessMove> MoveHistory => mMoveHistory;

        // TODO: implement IsCheck, IsCheckmate, IsStalemate
        private bool isAttacked()
        {
            mCurrentPlayer = -mCurrentPlayer;
            List<ChessMove> possMoves = new List<ChessMove>();

            // Get all pieces on the board
            List<ChessPiece> boardList = GetAllChessPiecesOnBoard();

            // Get the board positions for all the pieces
            ISet<BoardPosition> boardPosList = GetBoardPositionsOfPieces(boardList);

            //Console.WriteLine("Print out current player: " + CurrentPlayer);

            foreach (var bp in boardPosList)
            {
                //mCurrentPlayer = -mCurrentPlayer;
                if (GetPieceAtPosition(bp).Player == CurrentPlayer)
                {
                    //Console.WriteLine(GetPieceAtPosition(bp).Player.ToString()+ bp.Row);
                    //Console.WriteLine("Get Piece At Position: " + Convert.ToByte(GetPieceAtPosition(bp).PieceType));
                    // Move Logic here
                    switch (Convert.ToInt32(GetPieceAtPosition(bp).PieceType))
                    {
                        case 1:
                            IEnumerable<ChessMove> pawnPossMoves = GetPossibleMovesPawn(bp);
                            foreach (var pos in pawnPossMoves)
                            {
                                //Console.WriteLine("Pawn pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        case 2:
                            IEnumerable<ChessMove> rookPossMoves = GetPossibleMovesRook(bp);
                            foreach (var pos in rookPossMoves)
                            {
                                //Console.WriteLine("Rook pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        case 3:
                            IEnumerable<ChessMove> knightPossMoves = GetPossibleMovesKnight(bp);
                            foreach (var pos in knightPossMoves)
                            {
                                //Console.WriteLine("Knight pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        case 4:
                            IEnumerable<ChessMove> bishopPossMoves = GetPossibleMovesBishop(bp);
                            foreach (var pos in bishopPossMoves)
                            {
                                //Console.WriteLine("Bishop pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        case 5:
                            IEnumerable<ChessMove> queenPossMoves = GetPossibleMovesQueen(bp);
                            foreach (var pos in queenPossMoves)
                            {
                                //Console.WriteLine("Queen pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        case 6:
                            IEnumerable<ChessMove> kingPossMoves = GetPossibleMovesKing(bp);
                            foreach (var pos in kingPossMoves)
                            {
                                //Console.WriteLine("King pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        default:
                            //Console.WriteLine("Current piece is Empty - get possible moves");
                            break;
                    }
                }
            }
            mCurrentPlayer = -mCurrentPlayer;

            foreach(var x in possMoves)
            {
                if(GetPieceAtPosition(x.EndPosition).Player != GetPieceAtPosition(x.StartPosition).Player && GetPieceAtPosition(x.EndPosition).PieceType == ChessPieceType.King)
                {
                    return true;
                }
            }

            return false ;


        }
        public bool IsCheck
        {

            get
            {
                mCurrentPlayer = -mCurrentPlayer;
                IEnumerable<ChessMove> possMoves = GetPossibleMoves();
                foreach (var poss in possMoves)
                {
                    if (GetPieceAtPosition(poss.EndPosition).Player != GetPieceAtPosition(poss.StartPosition).Player && GetPieceAtPosition(poss.EndPosition).PieceType == ChessPieceType.King)
                    {
                        mCurrentPlayer = -mCurrentPlayer;
                        if(GetPossibleMoves().Count() == 0)
                        {
                            return false;
                        }
                        //Console.WriteLine("True: King is in check " + CurrentPlayer);
                        return true;
                    }

                }
                
                mCurrentPlayer = -mCurrentPlayer;
                //Console.WriteLine("False: King is not in check " + CurrentPlayer);
                return false;
                //throw new NotImplementedException("You must implement this property."); }
            }
        }

        public bool IsCheckmate
        {
            get
            {
                if(isAttacked() == true && GetPossibleMoves().Count()== 0)
                {
                    return true;
                }
                return false;
                //throw new NotImplementedException("You must implement this property."); }
            }
        }

        public bool IsStalemate
        {
            get
            {
                //Console.WriteLine(CurrentPlayer);
                if(IsCheck == false && GetPossibleMoves().Count() == 0 && IsCheckmate == false)
                {
                    return true;
                }
                return false;
                //throw new NotImplementedException("You must implement this property."); }
            }
        }

        public bool IsDraw
        {
            get
            {
                if (DrawCounter == 100)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Tracks the current draw counter, which goes up by 1 for each non-capturing, non-pawn move, and resets to 0
        /// for other moves. If the counter reaches 100 (50 full turns), the game is a draw.
        /// </summary>
        public int DrawCounter { get; set; } = 0;
        #endregion


        #region Public methods.
        public IEnumerable<ChessMove> GetPossibleMoves()
        {
            if (mPossibleMoves != null)
            {
                return mPossibleMoves;
            }

            //Console.WriteLine(blackRightRook);
            List<ChessMove> possMoves = new List<ChessMove>();

            // Get all pieces on the board
            //List<ChessPiece> boardList = GetAllChessPiecesOnBoard();

            // Get the board positions for all the pieces
            //ISet<BoardPosition> boardPosList = GetBoardPositionsOfPieces(boardList);

            //Console.WriteLine("Print out current player: " + CurrentPlayer);

            foreach (var bp in BoardPosition.GetRectangularPositions(8,8))
            {
                if (GetPieceAtPosition(bp).Player == CurrentPlayer)
                {
                    //Console.WriteLine(GetPieceAtPosition(bp).Player.ToString()+ bp.Row);
                    //Console.WriteLine("Get Piece At Position: " + Convert.ToByte(GetPieceAtPosition(bp).PieceType));
                    // Move Logic here
                    switch (Convert.ToInt32(GetPieceAtPosition(bp).PieceType))
                    {
                        case 1:
                            IEnumerable<ChessMove> pawnPossMoves = GetPossibleMovesPawn(bp);
                            foreach (var pos in pawnPossMoves)
                            {
                                //Console.WriteLine("Pawn pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        case 2:
                            IEnumerable<ChessMove> rookPossMoves = GetPossibleMovesRook(bp);
                            foreach (var pos in rookPossMoves)
                            {
                                //Console.WriteLine("Rook pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        case 3:
                            IEnumerable<ChessMove> knightPossMoves = GetPossibleMovesKnight(bp);
                            foreach (var pos in knightPossMoves)
                            {
                                //Console.WriteLine("Knight pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        case 4:
                            IEnumerable<ChessMove> bishopPossMoves = GetPossibleMovesBishop(bp);
                            foreach (var pos in bishopPossMoves)
                            {
                                //Console.WriteLine("Bishop pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        case 5:
                            IEnumerable<ChessMove> queenPossMoves = GetPossibleMovesQueen(bp);
                            foreach (var pos in queenPossMoves)
                            {
                                //Console.WriteLine("Queen pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        case 6:
                            IEnumerable<ChessMove> kingPossMoves = GetPossibleMovesKing(bp);
                            foreach (var pos in kingPossMoves)
                            {
                                //Console.WriteLine("King pos moves: " + pos.ToString());
                                possMoves.Add(pos);
                            }
                            break;
                        default:
                            //Console.WriteLine("Current piece is Empty - get possible moves");
                            break;
                    }
                }
            }
            if (isAttacked()) {
                for (int i = 0; i < possMoves.Count; i++)
                {
                    if (possMoves[i].MoveType == ChessMoveType.CastleKingSide || possMoves[i].MoveType == ChessMoveType.CastleQueenSide)
                    {
                        possMoves.RemoveAt(i);
                    }
                }
            }
            List<ChessMove> returnMoves = new List<ChessMove>();
            GameAdvantage test = CurrentAdvantage;
            int currDrawCounter = DrawCounter;
            foreach (ChessMove x in possMoves)
            {
                ApplyMove(x);
                mCurrentPlayer = -mCurrentPlayer;
                if (isAttacked() == false)
                {
                    //Console.WriteLine("Move: " + x);
                    returnMoves.Add(x);
                }
                UndoLastMove();
                mCurrentPlayer = -mCurrentPlayer;
            }

            if (currDrawCounter != DrawCounter)
            {
                DrawCounter = currDrawCounter;
            }
            List<ChessMove> returnMoves2 = new List<ChessMove>();
            foreach (ChessMove x in returnMoves)
            {
                if (x.MoveType == ChessMoveType.CastleKingSide)
                {
                    ApplyMove(new ChessMove(x.StartPosition, new BoardPosition(x.StartPosition.Row, x.StartPosition.Col + 1)));
                    mCurrentPlayer = -mCurrentPlayer;
                    if (isAttacked() == false)
                    {
                        returnMoves2.Add(x);
                    }
                    UndoLastMove();
                    mCurrentPlayer = -mCurrentPlayer;
                }
                else if (x.MoveType == ChessMoveType.CastleQueenSide)
                {
                    ApplyMove(new ChessMove(x.StartPosition, new BoardPosition(x.EndPosition.Row, x.EndPosition.Col + 1)));
                    mCurrentPlayer = -mCurrentPlayer;
                    if (isAttacked() == false)
                    {
                        returnMoves2.Add(x);
                    }
                    UndoLastMove();
                    mCurrentPlayer = -mCurrentPlayer;
                }
                else
                {
                    returnMoves2.Add(x);
                }
            }
            //Console.WriteLine("Possible Moves Count: " + returnMoves.Count + " for player :" + CurrentPlayer);
            //mAdvantageValue = 0;
            CurrentAdvantage = test;
            //Console.WriteLine(whiteKingCastle);
            //Console.WriteLine(whiteLeftRook);
            return mPossibleMoves = returnMoves2;
            //throw new NotImplementedException("You must implement this method.");
        }

        public void ApplyMove(ChessMove m)
        {
            
            mMoveHistory.Add(m);
            if (chessBoardHistory.Count == 0)
            {
                byte[] adding = {  0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0};
                for (int i = 0; i < adding.Length; i++)
                {
                    adding[i] = chessBoard[i];
                }
                chessBoardHistory.Add(adding);
            }

            // STRONG RECOMMENDATION: any mutation to the board state should be run
            // through the method SetPieceAtPosition.

            ChessPiece startPiece = GetPieceAtPosition(m.StartPosition);
            //if(m.MoveType == ChessMoveType.CastleQueenSide)
            //{
            //    SetPieceAtPosition(m.EndPosition, startPiece);
            //    SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
            //}

            // If move is a Pawn Promotion
            ChessPiece capturedPiece = GetPieceAtPosition(m.EndPosition);
            if (m.MoveType == ChessMoveType.PawnPromote)
            {
                //Console.WriteLine(m.Player);
                ChessPiece promotedPawn = new ChessPiece(m.PromotePiece, CurrentPlayer);
                SetPieceAtPosition(m.EndPosition, promotedPawn);
                SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
            }
            else if(m.MoveType == ChessMoveType.CastleKingSide)
            {
                if(CurrentPlayer == 1)
                {
                    if(wkc == 0)
                    {
                        wkc = mMoveHistory.Count-1;
                    }
                    whiteKingCastle = true;
                    whiteRightRook = true;
                    SetPieceAtPosition(m.EndPosition, startPiece);
                    SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
                    SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col - 1), new ChessPiece(ChessPieceType.Rook, CurrentPlayer));
                    SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col + 1), new ChessPiece(ChessPieceType.Empty, 0));
                }
                else
                {
                    if(bkc == 0)
                    {
                        bkc = mMoveHistory.Count - 1;
                    }
                    blackKingCastle = true;
                    blackRightRook = true;
                    SetPieceAtPosition(m.EndPosition, startPiece);
                    SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
                    SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col - 1), new ChessPiece(ChessPieceType.Rook, CurrentPlayer));
                    SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col + 1), new ChessPiece(ChessPieceType.Empty, 0));
                }
            }
            else if (m.MoveType == ChessMoveType.CastleQueenSide)
            {
                //Console.WriteLine("aaa");
                if (CurrentPlayer == 1)
                {
                    if(wkc == 0)
                    {
                        wkc = mMoveHistory.Count-1;
                    }
                    whiteKingCastle = true;
                    whiteLeftRook = true;
                    SetPieceAtPosition(m.EndPosition, startPiece);
                    SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
                    SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col + 1), new ChessPiece(ChessPieceType.Rook, CurrentPlayer));
                    SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col - 2), new ChessPiece(ChessPieceType.Empty, 0));
                }
                else
                {
                    if (bkc == 0)
                    {
                        bkc = mMoveHistory.Count() - 1;
                    }
                    blackKingCastle = true;
                    blackLeftRook = true;
                    SetPieceAtPosition(m.EndPosition, startPiece);
                    SetPieceAtPosition(m.StartPosition, new ChessPiece(ChessPieceType.Empty, 0));
                    SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col + 1), new ChessPiece(ChessPieceType.Rook, CurrentPlayer));
                    SetPieceAtPosition(new BoardPosition(m.EndPosition.Row, m.EndPosition.Col - 2), new ChessPiece(ChessPieceType.Empty, 0));
                }
            }
            else if (m.MoveType == ChessMoveType.EnPassant)
            {
                if(CurrentPlayer == 1)
                {
                    SetPieceAtPosition(m.EndPosition, startPiece);
                    SetPieceAtPosition(m.StartPosition, ChessPiece.Empty);
                    SetPieceAtPosition(new BoardPosition(m.EndPosition.Row + 1, m.EndPosition.Col), ChessPiece.Empty);
                }
                else
                {
                    SetPieceAtPosition(m.EndPosition, startPiece);
                    SetPieceAtPosition(m.StartPosition, ChessPiece.Empty);
                    SetPieceAtPosition(new BoardPosition(m.EndPosition.Row - 1, m.EndPosition.Col), ChessPiece.Empty);
                }
            }
            
            else
            {
                if(startPiece.PieceType == ChessPieceType.King)
                {
                    if(startPiece.Player == 1)
                    {
                        if(wkc == 0)
                        {
                            wkc = mMoveHistory.Count - 1;
                        }
                        whiteKingCastle = true;
                    }
                    else
                    {
                        if(bkc == 0)
                        {
                            bkc = mMoveHistory.Count - 1;
                        }
                        blackKingCastle = true;
                    }
                }
                else if(startPiece.PieceType == ChessPieceType.Rook)
                {
                    if(startPiece.Player == 1)
                    {
                        if(m.StartPosition.Col == 0)
                        {
                            if(wlr == 0)
                            {
                                wlr = mMoveHistory.Count - 1;
                            }
                            whiteLeftRook = true;
                        }
                        else
                        {
                            if (wrr == 0)
                            {
                                wrr = mMoveHistory.Count - 1;
                            }
                            whiteRightRook = true;
                            //Console.WriteLine("test" + whiteRightRook);
                        }
                    }
                    else
                    {
                        if(m.StartPosition.Col == 0)
                        {
                            if (blr == 0)
                            {
                                blr = mMoveHistory.Count - 1;
                            }
                            blackLeftRook = true;
                        }
                        else
                        {
                            if (brr == 0)
                            {
                                brr = mMoveHistory.Count - 1;
                            }
                            blackRightRook = true;
                        }
                    }
                }
                //Console.WriteLine(CurrentPlayer);
                //Console.WriteLine("1234asda"+ startPiece.Player);
                SetPieceAtPosition(m.EndPosition, startPiece);
                SetPieceAtPosition(m.StartPosition, ChessPiece.Empty);
            }

            int preAdvantage = mAdvantageValue;
            mCurrentPlayer = -mCurrentPlayer;
            setAdvantage();

            // Increments draw counter which goes up by 1 for each non-capturing, non-pawn move, and resets to 0 for other moves.
            if (startPiece.PieceType != ChessPieceType.Pawn)
            {
                prevDrawCounter = DrawCounter;
                DrawCounter++;
                //Console.WriteLine("Draw Counter: " + DrawCounter);
            }
            else
            {
                DrawCounter = 0;
                //Console.WriteLine("Draw Counter 0: " + DrawCounter);
            }

            if (capturedPiece.PieceType != ChessPieceType.Empty)
            {
                DrawCounter = 0;
            }


            //Console.Write("Current Advantage: " + mAdvantageValue);
            byte[] add = {  0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0};
            for (int i = 0; i < add.Length; i++)
            {
                add[i] = chessBoard[i];
            }
            chessBoardHistory.Add(add);
            mPossibleMoves = null;
        }

        public void UndoLastMove()
        {
            if (chessBoardHistory.Count > 1)
            {
                byte[] remove = chessBoardHistory[chessBoardHistory.Count - 2];
                for (int i = 0; i < chessBoard.Length; i++)
                {
                    //Console.WriteLine(remove.Length);
                    chessBoard[i] = remove[i];
                }
                //Console.WriteLine(chessBoardHistory[chessBoardHistory.Count - 2]);

                //this.chessBoard = remove;
                chessBoardHistory.RemoveAt(chessBoardHistory.Count - 1);
                mCurrentPlayer = -mCurrentPlayer;
                

                ChessMove currMove = mMoveHistory.ElementAt(mMoveHistory.Count - 1);
                ChessPiece startPiece = GetPieceAtPosition(currMove.StartPosition);
                ChessPiece capturedPiece = GetPieceAtPosition(currMove.EndPosition);
                setAdvantage();
                // Increments draw counter which goes down by 1 for each non-capturing, non-pawn move, and resets to 0 for other moves.
                if (startPiece.PieceType != ChessPieceType.Pawn)
                {
                    DrawCounter--;
                }
                else
                {
                    DrawCounter++;
                }


                if (capturedPiece.PieceType != ChessPieceType.Empty)
                {
                    DrawCounter = prevDrawCounter;
                }

                /*
                whiteKingCastle = false;
                whiteLeftRook = false;
                whiteRightRook = false;
                blackKingCastle = false;
                blackLeftRook = false;
                blackRightRook = false;

                */
                //Console.WriteLine("test");
                mMoveHistory.RemoveAt(mMoveHistory.Count - 1);
                if (mMoveHistory.Count <= wkc)
                {
                    if (currMove.MoveType == ChessMoveType.CastleKingSide)
                    {
                        wrr = 0;
                        whiteRightRook = false;
                    }
                    if (currMove.MoveType == ChessMoveType.CastleQueenSide)
                    {
                        wlr = 0;
                        whiteLeftRook = false;
                    }
                    wkc = 0;
                    whiteKingCastle = false;
                }
                if (mMoveHistory.Count <= bkc)
                {
                    if (currMove.MoveType == ChessMoveType.CastleQueenSide)
                    {
                        blr = 0;
                        blackLeftRook = false;
                    }
                    if (currMove.MoveType == ChessMoveType.CastleKingSide)
                    {
                        brr = 0;
                        blackRightRook = false;
                    }
                    bkc = 0;
                    blackKingCastle = false;
                }
                if (mMoveHistory.Count <= wlr)
                {
                    wlr = 0;
                    whiteLeftRook = false;
                }
                if (mMoveHistory.Count <= wrr)
                {
                    wrr = 0;
                    whiteRightRook = false;
                }
                if (mMoveHistory.Count <= blr)
                {
                    blr = 0;
                    blackLeftRook = false;
                }
                if (mMoveHistory.Count <= brr)
                {
                    brr = 0;
                    blackRightRook = false;
                }
            }
            else
            {
                Console.WriteLine("cannot undo");
                throw new InvalidOperationException();
            }
            mPossibleMoves = null;
            //throw new NotImplementedException("You must implement this method.");
        }


        /// <summary>
        /// Returns a List of all possible chess moves that could be made by the given Pawn at the start BoardPosition.
        /// </summary>
        public IEnumerable<ChessMove> GetPossibleMovesPawn(BoardPosition startPos)
        {
            List<ChessMove> possMoves = new List<ChessMove>();
            ISet<BoardPosition> possBP = GetAttackPositionsForPawn(startPos, GetPieceAtPosition(startPos));
            
            // If the attack position is empty then only move forward
            int col = startPos.Col;
            int row = startPos.Row;
            ChessPiece currPiece = GetPieceAtPosition(startPos);

            if (currPiece.Player == 2) // Pawn is black
            {
                row++;
                BoardPosition endPos = new BoardPosition(row, col);

                if (endPos.Row == 7 && GetPieceAtPosition(endPos).PieceType == ChessPieceType.Empty)
                {
                    ChessMove currMove = new ChessMove(startPos, endPos, ChessPieceType.Queen);
                    possMoves.Add(currMove);

                    ChessMove currMove2 = new ChessMove(startPos, endPos, ChessPieceType.Knight);
                    possMoves.Add(currMove2);

                    ChessMove currMove3 = new ChessMove(startPos, endPos, ChessPieceType.Rook);
                    possMoves.Add(currMove3);

                    ChessMove currMove4 = new ChessMove(startPos, endPos, ChessPieceType.Bishop);
                    possMoves.Add(currMove4);
                }
                else
                {
                    ChessMove currMove = new ChessMove(startPos, endPos, ChessMoveType.Normal);
                    //Console.WriteLine("Current move Pawn 1: " + currMove.ToString());
                    if (GetPieceAtPosition(endPos).PieceType == ChessPieceType.Empty)
                    {
                        possMoves.Add(currMove);
                    }
                }

                // If pawn is at in the initial row
                if (startPos.Row == 1)
                {
                    row++;
                    BoardPosition endPos2 = new BoardPosition(row, col);
                    if (GetPieceAtPosition(endPos2).PieceType == ChessPieceType.Empty && GetPieceAtPosition(new BoardPosition((row-1),col)).PieceType == ChessPieceType.Empty )
                    {
                        ChessMove currMove2 = new ChessMove(startPos, endPos2, ChessMoveType.Normal);
                        //Console.WriteLine("Current move Pawn 2: " + currMove2.ToString());
                        possMoves.Add(currMove2);
                    }
                }

                // Iterate through attack positions and check if there is an enemy there
                foreach (var bp in possBP)
                {
                    // If the attack position is not empty
                    if (GetPieceAtPosition(bp).PieceType != ChessPieceType.Empty && GetPieceAtPosition(bp).Player != CurrentPlayer)
                    {
                        // Add position as a possible move
                        if (bp.Row == 0 || bp.Row == 7)
                        {
                            ChessMove currMove = new ChessMove(startPos, bp, ChessPieceType.Queen);
                            possMoves.Add(currMove);

                            ChessMove currMove2 = new ChessMove(startPos, bp, ChessPieceType.Knight);
                            possMoves.Add(currMove2);

                            ChessMove currMove3 = new ChessMove(startPos, bp, ChessPieceType.Rook);
                            possMoves.Add(currMove3);

                            ChessMove currMove4 = new ChessMove(startPos, bp, ChessPieceType.Bishop);
                            possMoves.Add(currMove4);
                        }
                        else
                        {
                            ChessMove currMove = new ChessMove(startPos, bp, ChessMoveType.Normal);
                            //Console.WriteLine("Current move Pawn: " + currMove.ToString());
                            possMoves.Add(currMove);
                        }
                    }
                }

                //En Passant
                col = startPos.Col;
                row = startPos.Row;
                ChessMove lastMove;
                if (mMoveHistory.Count > 0)
                {
                    lastMove = mMoveHistory[mMoveHistory.Count - 1];
                }
                else
                {
                    lastMove = null;
                }
                if (startPos.Row == 4 && currPiece.PieceType == ChessPieceType.Pawn)
                {
                    col++;
                    BoardPosition pawnLeft = new BoardPosition(row, col);
                    if (GetPieceAtPosition(pawnLeft).PieceType == ChessPieceType.Pawn && GetPieceAtPosition(pawnLeft).Player == 1 && lastMove.EndPosition == pawnLeft)
                    {
                        row++;
                        BoardPosition blackEnPassantLeftEnd = new BoardPosition(row, col);
                        ChessMove blackEnPassantLeft = new ChessMove(startPos, blackEnPassantLeftEnd, ChessMoveType.EnPassant);
                        possMoves.Add(blackEnPassantLeft);
                    }
                    col -= 2;
                    BoardPosition pawnRight = new BoardPosition(row, col);
                    if (GetPieceAtPosition(pawnRight).PieceType == ChessPieceType.Pawn && GetPieceAtPosition(pawnRight).Player == 1 && lastMove.EndPosition == pawnRight)
                    {
                        row++;
                        BoardPosition blackEnPassantRightEnd = new BoardPosition(row, col);
                        ChessMove whiteEnPassantRight = new ChessMove(startPos, blackEnPassantRightEnd, ChessMoveType.EnPassant);
                        possMoves.Add(whiteEnPassantRight);
                    }

                }


            }
            else //Pawn is white
            {
                row--;
                BoardPosition endPos = new BoardPosition(row, col);

                if (endPos.Row == 0 && GetPieceAtPosition(endPos).PieceType == ChessPieceType.Empty)
                {
                    ChessMove currMove = new ChessMove(startPos, endPos, ChessPieceType.Queen);
                    possMoves.Add(currMove);

                    ChessMove currMove2 = new ChessMove(startPos, endPos, ChessPieceType.Knight);
                    possMoves.Add(currMove2);

                    ChessMove currMove3 = new ChessMove(startPos, endPos, ChessPieceType.Rook);
                    possMoves.Add(currMove3);

                    ChessMove currMove4 = new ChessMove(startPos, endPos, ChessPieceType.Bishop);
                    possMoves.Add(currMove4);
                }
                else
                {
                    ChessMove currMove = new ChessMove(startPos, endPos, ChessMoveType.Normal);
                    //Console.WriteLine("Current move Pawn 1: " + currMove.ToString());
                    if (GetPieceAtPosition(endPos).PieceType == ChessPieceType.Empty)
                    {
                        possMoves.Add(currMove);
                    }
                }

                // If pawn is at in the initial row
                if (startPos.Row == 6)
                {
                    row--;
                    BoardPosition endPos2 = new BoardPosition(row, col);
                    if (GetPieceAtPosition(endPos2).PieceType == ChessPieceType.Empty && GetPieceAtPosition(new BoardPosition((row+1), col)).PieceType == ChessPieceType.Empty)
                    {
                        ChessMove currMove2 = new ChessMove(startPos, endPos2, ChessMoveType.Normal);
                        //Console.WriteLine("Current move Pawn 2: " + currMove2.ToString());
                        possMoves.Add(currMove2);
                    }
                }

                // Iterate through attack positions and check if there is an enemy there
                foreach (var bp in possBP)
                {
                    // If the attack position is not empty
                    if (GetPieceAtPosition(bp).PieceType != ChessPieceType.Empty && GetPieceAtPosition(bp).Player != CurrentPlayer)
                    {
                        // Add position as a possible move
                        if (bp.Row == 0 || bp.Row == 7)
                        {
                            ChessMove currMove = new ChessMove(startPos, bp, ChessPieceType.Queen);
                            possMoves.Add(currMove);

                            ChessMove currMove2 = new ChessMove(startPos, bp, ChessPieceType.Knight);
                            possMoves.Add(currMove2);

                            ChessMove currMove3 = new ChessMove(startPos, bp, ChessPieceType.Rook);
                            possMoves.Add(currMove3);

                            ChessMove currMove4 = new ChessMove(startPos, bp, ChessPieceType.Bishop);
                            possMoves.Add(currMove4);
                        }
                        else
                        {
                            ChessMove currMove = new ChessMove(startPos, bp, ChessMoveType.Normal);
                            //Console.WriteLine("Current move Pawn: " + currMove.ToString());
                            possMoves.Add(currMove);
                        }
                    }
                }

                //En Passant
                col = startPos.Col;
                row = startPos.Row;
                ChessMove lastMove;
                if (mMoveHistory.Count > 0)
                {
                    lastMove = mMoveHistory[mMoveHistory.Count - 1];
                }
                else
                {
                    lastMove = null;
                }
                if (startPos.Row == 3 && currPiece.PieceType == ChessPieceType.Pawn)
                {
                    col++;
                    BoardPosition pawnRight = new BoardPosition(row, col);
                    if(GetPieceAtPosition(pawnRight).PieceType == ChessPieceType.Pawn && GetPieceAtPosition(pawnRight).Player == 2 && lastMove.EndPosition == pawnRight)
                    {
                        row--;
                        BoardPosition whiteEnPassantRightEnd = new BoardPosition(row, col);
                        ChessMove whiteEnPassantRight = new ChessMove(startPos, whiteEnPassantRightEnd, ChessMoveType.EnPassant);
                        possMoves.Add(whiteEnPassantRight);
                    }
                    col -= 2;
                    BoardPosition pawnLeft = new BoardPosition(row, col);
                    if (GetPieceAtPosition(pawnLeft).PieceType == ChessPieceType.Pawn && GetPieceAtPosition(pawnLeft).Player == 2 && lastMove.EndPosition == pawnLeft)
                    {
                        row--;
                        BoardPosition whiteEnPassantLeftEnd = new BoardPosition(row, col);
                        ChessMove whiteEnPassantLeft = new ChessMove(startPos, whiteEnPassantLeftEnd, ChessMoveType.EnPassant);
                        possMoves.Add(whiteEnPassantLeft);
                    }

                }
            }
            return possMoves;
        }

        /// <summary>
        /// Returns a List of all possible chess moves that could be made by the given Knight at the start BoardPosition.
        /// </summary>
        public IEnumerable<ChessMove> GetPossibleMovesKnight(BoardPosition startPos)
        {
            List<ChessMove> possMoves = new List<ChessMove>();
            ISet<BoardPosition> possBP = GetAttackPositionsForKnight(startPos);
            foreach (var bp in possBP)
            {
                if (GetPieceAtPosition(bp).Player == CurrentPlayer && GetPieceAtPosition(bp).PieceType != ChessPieceType.Empty)
                {
                    //Console.WriteLine("Attack Position contains a friendly piece");
                }
                else
                {
                    ChessMove currMove = new ChessMove(startPos, bp, ChessMoveType.Normal);
                    //Console.WriteLine("Current move Knight: " + currMove.ToString());
                    possMoves.Add(currMove);
                }
            }
            return possMoves;
        }

        /// <summary>
        /// Returns a List of all possible chess moves that could be made by the given Queen at the start BoardPosition.
        /// </summary>
        public IEnumerable<ChessMove> GetPossibleMovesQueen(BoardPosition startPos)
        {
            List<ChessMove> possMoves = new List<ChessMove>();
            ISet<BoardPosition> possBP = GetAttackPositionsForBishop(startPos);
            ISet<BoardPosition> possBPRook = GetAttackPositionsForRook(startPos);
            possBP.UnionWith(possBPRook);
            foreach (var bp in possBP)
            {
                if (GetPieceAtPosition(bp).Player == CurrentPlayer && GetPieceAtPosition(bp).PieceType != ChessPieceType.Empty)
                {
                    //Console.WriteLine("Attack Position contains a friendly piece");
                }
                else
                {
                    ChessMove currMove = new ChessMove(startPos, bp, ChessMoveType.Normal);
                    //Console.WriteLine("Current move Rook: " + currMove.ToString());
                    possMoves.Add(currMove);
                }
            }
            return possMoves;
        }

        /// <summary>
        /// Returns a List of all possible chess moves that could be made by the given Bishop at the start BoardPosition.
        /// </summary>
        public IEnumerable<ChessMove> GetPossibleMovesBishop(BoardPosition startPos)
        {
            List<ChessMove> possMoves = new List<ChessMove>();
            ISet<BoardPosition> possBP = GetAttackPositionsForBishop(startPos);
            foreach (var bp in possBP)
            {
                if (GetPieceAtPosition(bp).Player == CurrentPlayer && GetPieceAtPosition(bp).PieceType != ChessPieceType.Empty)
                {
                    //Console.WriteLine("Attack Position contains a friendly piece");
                }
                else
                {
                    ChessMove currMove = new ChessMove(startPos, bp, ChessMoveType.Normal);
                    //Console.WriteLine("Current move Rook: " + currMove.ToString());
                    possMoves.Add(currMove);
                }
            }
            return possMoves;
        }

        /// <summary>
        /// Returns a List of all possible chess moves that could be made by the given Rook at the start BoardPosition.
        /// </summary>
        public IEnumerable<ChessMove> GetPossibleMovesRook(BoardPosition startPos)
        {
            List<ChessMove> possMoves = new List<ChessMove>();
            ISet<BoardPosition> possBP = GetAttackPositionsForRook(startPos);
            foreach (var bp in possBP)
            {
                if (GetPieceAtPosition(bp).Player == CurrentPlayer && GetPieceAtPosition(bp).PieceType != ChessPieceType.Empty)
                {
                    //Console.WriteLine("Attack Position contains a friendly piece");
                }
                else
                {
                    ChessMove currMove = new ChessMove(startPos, bp, ChessMoveType.Normal);
                    //Console.WriteLine("Current move Rook: " + currMove.ToString());
                    possMoves.Add(currMove);
                }
            }
            return possMoves;
        }

        /// <summary>
        /// Returns a List of all possible chess moves that could be made by the given King at the start BoardPosition.
        /// </summary>
        public IEnumerable<ChessMove> GetPossibleMovesKing(BoardPosition startPos)
        {
            List<ChessMove> possMoves = new List<ChessMove>();
            ISet<BoardPosition> possBP = GetAttackPositionsForKing(startPos);
            int col = startPos.Col;
            int row = startPos.Row;
            ChessPiece currPiece = GetPieceAtPosition(startPos);
            if (currPiece.Player == 2) // King is black
            {
                if (startPos.Row == 0 && startPos.Col == 4 && currPiece.PieceType == ChessPieceType.King)
                {
                    col--;
                    BoardPosition rookEndPosition = new BoardPosition(row, col);
                    if (GetPieceAtPosition(rookEndPosition).PieceType == ChessPieceType.Empty)
                    {
                        col--;
                        BoardPosition kingEndPosition = new BoardPosition(row, col);
                        if (GetPieceAtPosition(kingEndPosition).PieceType == ChessPieceType.Empty)
                        {
                            col--;
                            BoardPosition newPosition = new BoardPosition(row, col);
                            if (GetPieceAtPosition(newPosition).PieceType == ChessPieceType.Empty)
                            {
                                col--;
                                BoardPosition rookPosition = new BoardPosition(row, col);
                                if (GetPieceAtPosition(rookPosition).PieceType == ChessPieceType.Rook && blackKingCastle == false && blackLeftRook == false)
                                {
                                    //Console.WriteLine("test");
                                    ChessMove currMove = new ChessMove(startPos, kingEndPosition, ChessMoveType.CastleQueenSide);
                                    possMoves.Add(currMove);
                                }
                            }

                        }
                    }

                }
            }
            col = startPos.Col;
            if (currPiece.Player == 2) // King is black
            {
                if (startPos.Row == 0 && startPos.Col == 4 && currPiece.PieceType == ChessPieceType.King)
                {
                    col++;
                    BoardPosition newPosition2 = new BoardPosition(row, col);
                    if (GetPieceAtPosition(newPosition2).PieceType == ChessPieceType.Empty)
                    {
                        col++;
                        BoardPosition blackKingEndPosition2 = new BoardPosition(row, col);
                        if (GetPieceAtPosition(blackKingEndPosition2).PieceType == ChessPieceType.Empty)
                        {
                            col++;
                            BoardPosition blackRookPosition2 = new BoardPosition(row, col);
                            if (GetPieceAtPosition(blackRookPosition2).PieceType == ChessPieceType.Rook && blackRightRook == false && blackKingCastle == false)
                            {
                                //Console.WriteLine(blackKingEndPosition2);
                                ChessMove currMove = new ChessMove(startPos, blackKingEndPosition2, ChessMoveType.CastleKingSide);
                                possMoves.Add(currMove);
                            }


                        }
                    }

                }
            }
            col = startPos.Col;
            if (currPiece.Player == 1) // King is White
            {
                if (startPos.Row == 7 && startPos.Col == 4 && currPiece.PieceType == ChessPieceType.King)
                {
                    col--;
                    BoardPosition whiteRookEndPosition = new BoardPosition(row, col);
                    if (GetPieceAtPosition(whiteRookEndPosition).PieceType == ChessPieceType.Empty)
                    {
                        col--;
                        BoardPosition whiteKingEndPosition = new BoardPosition(row, col);
                        if (GetPieceAtPosition(whiteKingEndPosition).PieceType == ChessPieceType.Empty)
                        {
                            col--;
                            BoardPosition newPosition3 = new BoardPosition(row, col);
                            if (GetPieceAtPosition(newPosition3).PieceType == ChessPieceType.Empty)
                            {
                                col--;
                                BoardPosition whiteRookPosition = new BoardPosition(row, col);
                                if (GetPieceAtPosition(whiteRookPosition).PieceType == ChessPieceType.Rook && whiteKingCastle == false && whiteLeftRook == false)
                                {
                                    //Console.WriteLine(whiteKingEndPositio);
                                    ChessMove currMove = new ChessMove(startPos, whiteKingEndPosition, ChessMoveType.CastleQueenSide);
                                    possMoves.Add(currMove);
                                }
                            }

                        }
                    }

                }
            }
            col = startPos.Col;
            if (currPiece.Player == 1) // King is white
            {
                if (startPos.Row == 7 && startPos.Col == 4 && currPiece.PieceType == ChessPieceType.King)
                {
                    col++;
                    BoardPosition WhiteRookEndPosition = new BoardPosition(row, col);
                    if (GetPieceAtPosition(WhiteRookEndPosition).PieceType == ChessPieceType.Empty)
                    {
                        col++;
                        BoardPosition whiteKingEndPosition2 = new BoardPosition(row, col);
                        if (GetPieceAtPosition(whiteKingEndPosition2).PieceType == ChessPieceType.Empty)
                        {
                            col++;
                            BoardPosition whiteRookPosition2 = new BoardPosition(row, col);
                            if (GetPieceAtPosition(whiteRookPosition2).PieceType == ChessPieceType.Rook && whiteRightRook == false  && whiteKingCastle == false)
                            {
                                //Console.WriteLine(whiteRightRook);
                                ChessMove currMove = new ChessMove(startPos, whiteKingEndPosition2, ChessMoveType.CastleKingSide);
                                possMoves.Add(currMove);
                                //Console.WriteLine("Debug : ", currMove.EndPosition);

                            }


                        }
                    }

                }
            }

            foreach (var bp in possBP)
            {
                if (GetPieceAtPosition(bp).Player == CurrentPlayer && GetPieceAtPosition(bp).PieceType != ChessPieceType.Empty)
                {
                    //Console.WriteLine("Attack Position contains a friendly piece");
                }
                else
                {
                    ChessMove currMove = new ChessMove(startPos, bp, ChessMoveType.Normal);
                    //Console.WriteLine("Current move Knight: " + currMove.ToString());
                    possMoves.Add(currMove);
                }
            }
            return possMoves;
        }

        /// <summary>
        /// Returns whatever chess piece is occupying the given position.
        /// </summary>
        public ChessPiece GetPieceAtPosition(BoardPosition position)
        {

            // Getting row and column position
            int row = position.Row;
            int col = position.Col;

            // Algorithm to get position index in array
            row = row * 4;

            int remainder = col % 2;

            // The index is a full number
            if (remainder == 0)
            {
                col = col / 2;
                //Console.WriteLine("Full Num");

                // Gets byte representation of index in array chessBoard
                int posIndex = row + col;
                //Console.WriteLine("This is posIndex: " + posIndex);
                byte posByte = chessBoard[posIndex];
                //Console.WriteLine("This is posByte: " + posByte);

                // If the index is 0, then return empty Chess Piece
                if (posByte == 0)
                {
                    return ChessPiece.Empty;
                }
                else // Else a piece exists at the index, return the piece
                {
                    // Gets the binary value of the position byte
                    string binaryVal = Convert.ToString(posByte, 2).PadLeft(8, '0');
                    //Console.WriteLine("ToString: " + binaryVal);

                    // Getting leftmost 4 digits of binary since it is a Full Number
                    string leftMost = binaryVal.Substring(0, 4);
                    //Console.WriteLine("leftMost: " + leftMost);

                    // Gets first digit which represents the player as an int: 0- white, 1- black
                    int playerDigit = Int32.Parse(leftMost.Substring(0, 1));

                    // Gets the number that represents the chess piece and convert it into ChessPieceType enum
                    int pieceTypeInt = Convert.ToByte(leftMost.Substring(1, 3), 2);
                    ChessPieceType pieceTypeEnum = (ChessPieceType)pieceTypeInt;
                    if (playerDigit == 0 && pieceTypeInt == 0)
                    {
                        return ChessPiece.Empty;
                    }
                    // Creates a new ChessPiece object from extracted data and returns it
                    ChessPiece returnedPiece = new ChessPiece(pieceTypeEnum, playerDigit + 1);
                    return returnedPiece;
                }
            }
            else
            {
                col = col / 2;
                //Console.WriteLine("Half Num");

                // Gets byte representation of index in array chessBoard
                int posIndex = row + col;
                //Console.WriteLine("This is posIndex: " + posIndex);
                byte posByte = chessBoard[posIndex];
                //Console.WriteLine("This is posByte: " + posByte);

                // If the index is 0, then return empty Chess Piece
                if (posByte == 0)
                {
                    return ChessPiece.Empty;
                }
                else // Else a piece exists at the index, return the piece
                {
                    // Gets the binary value of the position byte
                    string binaryVal = Convert.ToString(posByte, 2).PadLeft(8, '0');
                    //Console.WriteLine("ToString: " + binaryVal);

                    // Getting rightmost 4 digits of binary since it is a Half Number
                    string rightMost = binaryVal.Substring(binaryVal.Length - 4);
                    //Console.WriteLine("rightMost: " + rightMost);

                    // Gets first digit which represents the player as an int: 0- white, 1- black
                    int playerDigit = Int32.Parse(rightMost.Substring(0, 1));

                    // Gets the number that represents the chess piece and convert it into ChessPieceType enum
                    int pieceTypeInt = Convert.ToByte(rightMost.Substring(1, 3), 2);
                    ChessPieceType pieceTypeEnum = (ChessPieceType)pieceTypeInt;
                    if (playerDigit == 0 && pieceTypeInt == 0)
                    {
                        return ChessPiece.Empty;
                    }
                    // Creates a new ChessPiece object from extracted data and returns it
                    ChessPiece returnedPiece = new ChessPiece(pieceTypeEnum, playerDigit + 1);
                    return returnedPiece;
                }
            }
        }

        /// <summary>
        /// Returns whatever player is occupying the given position.
        /// </summary>
        public int GetPlayerAtPosition(BoardPosition pos)
        {
            // As a hint, you should call GetPieceAtPosition.

            ChessPiece currPiece = GetPieceAtPosition(pos);
            int intOfcurrPiece = currPiece.Player;
            return intOfcurrPiece;
        }

        /// <summary>
        /// Returns true if the given position on the board is empty.
        /// </summary>
        /// <remarks>returns false if the position is not in bounds</remarks>
        public bool PositionIsEmpty(BoardPosition pos)
        {
            ChessPiece currPiece = GetPieceAtPosition(pos);

            if (!PositionInBounds(pos))
            {
                return false;
            }
            else
            {
                if (currPiece.PieceType == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns true if the given position contains a piece that is the enemy of the given player.
        /// </summary>
        /// <remarks>Returns false if the chess piece at position belongs to player</remarks>
        public bool PositionIsEnemy(BoardPosition pos, int player)
        {

            ChessPiece currPiece = GetPieceAtPosition(pos);
            int playerCurrPiece = currPiece.Player;
            if (playerCurrPiece == player || currPiece.PieceType == ChessPieceType.Empty)
            {
                
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true if the given position is in the bounds of the board.
        /// </summary>
        public static bool PositionInBounds(BoardPosition pos)
        {
            if (pos.Col >= 0 && pos.Col <= 7 && pos.Row >= 0 && pos.Row <= 7)
            {
                return true;
            }
            return false;
        }

        public List<ChessPiece> GetAllChessPiecesOnBoard()
        {
            List<ChessPiece> allChessPieces = new List<ChessPiece>();
            string rightByteString, leftByteString;

            // For each square (consists of 2 chess pieces)
            foreach (byte square in chessBoard)
            {
                byte squareByte = square;
                string twoPieces = Convert.ToString(squareByte, 2).PadLeft(8, '0');
                leftByteString = twoPieces.Substring(0, 4);
                rightByteString = twoPieces.Substring(twoPieces.Length - 4);

                // Extract leftByteString data into a new Chess Piece
                ChessPiece leftPiece = createChessPieceFromString(leftByteString);
                ChessPiece rightPiece = createChessPieceFromString(rightByteString);

                allChessPieces.Add(leftPiece);
                allChessPieces.Add(rightPiece);
            }
            return allChessPieces;
        }

        /// <summary>
        /// Returns all board positions where the given piece can be found.
        /// </summary>
        public IEnumerable<BoardPosition> GetPositionsOfPiece(ChessPieceType piece, int player)
        {
            List<ChessPiece> allChessPieces = GetAllChessPiecesOnBoard();
            ISet<BoardPosition> foundPositions = new HashSet<BoardPosition>();

            int currPosition = 0;
            int currRow, currCol;

            // Iterate through all the chess pieces on the board
            for (int i = 0; i < allChessPieces.Count(); i++)
            {
                if (i % 2 == 0)
                {
                    if (i != 0)
                    {
                        currPosition++;
                    }
                    // If element matches with the given parameters
                    if (allChessPieces.ElementAt(i).PieceType == piece && allChessPieces.ElementAt(i).Player == player)
                    {
                        // Get BoardPosition for currPosition (index)
                        double div = currPosition / 4;
                        currRow = Convert.ToInt32(Math.Floor(div));

                        currCol = (2 * (currPosition - (currRow * 4)));

                        // Add current Board Position into set of found Board Positions
                        BoardPosition currBP = new BoardPosition(currRow, currCol);
                        foundPositions.Add(currBP);
                    }
                }
                else
                {
                    if (allChessPieces.ElementAt(i).PieceType == piece && allChessPieces.ElementAt(i).Player == player)
                    {
                        // Get BoardPosition for currPosition (index)
                        double div = currPosition / 4;
                        currRow = Convert.ToInt32(Math.Floor(div));

                        currCol = (2 * (currPosition - (currRow * 4)) + 1);

                        // Add current Board Position into set of found Board Positions
                        BoardPosition currBP = new BoardPosition(currRow, currCol);
                        foundPositions.Add(currBP);
                    }
                }
            }

            foreach (var bp in foundPositions)
            {
                //Console.WriteLine(bp.ToString());
            }
            return foundPositions;
        }

        /// <summary>
        /// Returns true if the given player's pieces are attacking the given position.
        /// </summary>
        public bool PositionIsAttacked(BoardPosition position, int byPlayer)
        {
            ISet<BoardPosition> AttackedPositions = GetAttackedPositions(byPlayer);
            if (AttackedPositions.Contains(position))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ChessPiece createChessPieceFromString(string fourDigitByteString)
        {
            // Gets first digit which represents the player as an int: 0- white, 1- black
            int playerDigit = Int32.Parse(fourDigitByteString.Substring(0, 1));

            // Gets the number that represents the chess piece and convert it into ChessPieceType enum
            int pieceTypeInt = Convert.ToByte(fourDigitByteString.Substring(1, 3), 2);
            ChessPieceType pieceTypeEnum = (ChessPieceType)pieceTypeInt;

            // Creates a new ChessPiece object from extracted data and returns it
            ChessPiece piece = new ChessPiece(pieceTypeEnum, playerDigit + 1);
            return piece;
        }

        /// <summary>
        /// Returns a set of all BoardPositions that are attacked by the given King at the current BoardPosition.
        /// </summary>
        public ISet<BoardPosition> GetAttackPositionsForKing(BoardPosition currBP)
        {
            ISet<BoardPosition> AttackMovements = new HashSet<BoardPosition>();
            int x = currBP.Row;
            int y = currBP.Col;

            x--;
            y--;
            AttackMovements.Add(new BoardPosition(x, y));
            y++;
            AttackMovements.Add(new BoardPosition(x, y));
            y++;
            AttackMovements.Add(new BoardPosition(x, y));

            x++;
            AttackMovements.Add(new BoardPosition(x, y));
            y = y - 2;
            AttackMovements.Add(new BoardPosition(x, y));

            x++;
            AttackMovements.Add(new BoardPosition(x, y));
            y++;
            AttackMovements.Add(new BoardPosition(x, y));
            y++;
            AttackMovements.Add(new BoardPosition(x, y));

            ISet<BoardPosition> ExcludedAttacks = new HashSet<BoardPosition>();

            foreach (var bp in AttackMovements)
            {
                if ((bp.Col >= 0 && bp.Col <= 7) &&
                   (bp.Row >= 0 && bp.Row <= 7))
                {

                }
                else
                {
                    ExcludedAttacks.Add(bp);
                }
            }
            AttackMovements.ExceptWith(ExcludedAttacks);

            return AttackMovements;
        }

        /// <summary>
        /// Returns a set of all BoardPositions that are attacked by the given Knight at the current BoardPosition.
        /// </summary>
        public ISet<BoardPosition> GetAttackPositionsForKnight(BoardPosition currBP)
        {
            ISet<BoardPosition> AttackMovements = new HashSet<BoardPosition>();
            int x = currBP.Row;
            int y = currBP.Col;

            x = x - 2;
            y++;
            AttackMovements.Add(new BoardPosition(x, y));
            x++;
            y++;
            AttackMovements.Add(new BoardPosition(x, y));
            x = x + 2;
            AttackMovements.Add(new BoardPosition(x, y));
            x++;
            y--;
            AttackMovements.Add(new BoardPosition(x, y));
            y = y - 2;
            AttackMovements.Add(new BoardPosition(x, y));
            x--;
            y--;
            AttackMovements.Add(new BoardPosition(x, y));
            x = x - 2;
            AttackMovements.Add(new BoardPosition(x, y));
            x--;
            y++;
            AttackMovements.Add(new BoardPosition(x, y));

            ISet<BoardPosition> ExcludedAttacks = new HashSet<BoardPosition>();

            foreach (var bp in AttackMovements)
            {
                if ((bp.Col >= 0 && bp.Col <= 7) &&
                   (bp.Row >= 0 && bp.Row <= 7))
                {

                }
                else
                {
                    ExcludedAttacks.Add(bp);
                }
            }
            AttackMovements.ExceptWith(ExcludedAttacks);
            return AttackMovements;
        }

        /// <summary>
        /// Returns a set of all BoardPositions that are attacked by the given Bishop at the current BoardPosition.
        /// </summary>
        public ISet<BoardPosition> GetAttackPositionsForBishop(BoardPosition currBP)
        {
            ISet<BoardPosition> AttackMovements = new HashSet<BoardPosition>();
            int x = currBP.Col;
            int y = currBP.Row;


            while (x > 0 && y > 0)
            { //Build up left movement
                x--;
                y--;
                BoardPosition attackMovement = new BoardPosition(y, x);
                AttackMovements.Add(attackMovement);
                if (!PositionIsEmpty(attackMovement))
                {
                    break;
                }
            }

            x = currBP.Col;
            y = currBP.Row;

            while (x < 7 && y > 0)
            { //Build up right movement
                x++;
                y--;
                BoardPosition attackMovement = new BoardPosition(y, x);
                AttackMovements.Add(attackMovement);
                if (!PositionIsEmpty(attackMovement))
                {
                    break;
                }
            }

            x = currBP.Col;
            y = currBP.Row;

            while (x > 0 && y < 7)
            { //Build down left movement
                x--;
                y++;
                BoardPosition attackMovement = new BoardPosition(y, x);
                AttackMovements.Add(attackMovement);
                if (!PositionIsEmpty(attackMovement))
                {
                    break;
                }
            }

            x = currBP.Col;
            y = currBP.Row;

            while (x < 7 && y < 7)
            { //Build down RIGHT movement
                x++;
                y++;
                BoardPosition attackMovement = new BoardPosition(y, x);
                AttackMovements.Add(attackMovement);
                if (!PositionIsEmpty(attackMovement))
                {
                    break;
                }
            }

            return AttackMovements;
        }

        /// <summary>
        /// Returns a set of all BoardPositions that are attacked by the given rook at the current BoardPosition.
        /// </summary>
        public ISet<BoardPosition> GetAttackPositionsForRook(BoardPosition currBP)
        {
            ISet<BoardPosition> AttackMovements = new HashSet<BoardPosition>();
            int x = currBP.Col;
            int y = currBP.Row;

            for (int i = x; i < 7; i++)
            {
                x++;
                BoardPosition attackMovement = new BoardPosition(y, x);
                AttackMovements.Add(attackMovement);
                if (!PositionIsEmpty(attackMovement))
                {
                    break;
                }
            }
            x = currBP.Col;
            for (int i = x; i > 0; i--)
            {
                x--;
                BoardPosition attackMovement = new BoardPosition(y, x);
                AttackMovements.Add(attackMovement);
                if (!PositionIsEmpty(attackMovement))
                {
                    break;
                }
            }
            x = currBP.Col;
            for (int i = y; i < 7; i++)
            {
                y++;
                BoardPosition attackMovement = new BoardPosition(y, x);
                AttackMovements.Add(attackMovement);
                if (!PositionIsEmpty(attackMovement))
                {
                    break;
                }
            }
            y = currBP.Row; //reset the y coordinate to the board position
            for (int i = y; i > 0; i--)
            {
                y--;
                BoardPosition attackMovement = new BoardPosition(y, x);
                AttackMovements.Add(attackMovement);
                if (!PositionIsEmpty(attackMovement))
                {
                    break;
                }
            }
            return AttackMovements;
        }

        /// <summary>
        /// Returns a set of all BoardPositions that are attacked by the given pawn at the current BoardPosition.
        /// </summary>
        public ISet<BoardPosition> GetAttackPositionsForPawn(BoardPosition currBP, ChessPiece cp)
        {
            ISet<BoardPosition> AttackMovements = new HashSet<BoardPosition>();
            int y = currBP.Row;
            int x = currBP.Col;
            if (cp.Player == 1)
            { //For white pieces
                if (x == 0)
                { //Check to see if pawn is in far left 
                    x++;
                    y--;
                    BoardPosition attackMovement = new BoardPosition(y, x);
                    AttackMovements.Add(attackMovement);
                }
                else if (x == 7)
                { //Check to see if pawn is infar right column
                    x--;
                    y--;
                    BoardPosition attackMovement = new BoardPosition(y, x);
                    AttackMovements.Add(attackMovement);
                }
                else
                {
                    x--;
                    y--;
                    BoardPosition attackMovement = new BoardPosition(y, x);
                    AttackMovements.Add(attackMovement);
                    x = x + 2;
                    BoardPosition attackMovement2 = new BoardPosition(y, x);
                    AttackMovements.Add(attackMovement2);
                }
            }
            else if (cp.Player == 2)
            { //For black pieces
                if (x == 0)
                { //Check to see if pawn is in far left 
                    x++;
                    y++;
                    BoardPosition attackMovement = new BoardPosition(y, x);
                    AttackMovements.Add(attackMovement);
                }
                else if (x == 7)
                { //Check to see if pawn is infar right column
                    x--;
                    y++;
                    BoardPosition attackMovement = new BoardPosition(y, x);
                    AttackMovements.Add(attackMovement);
                }
                else
                {
                    x--;
                    y++;
                    BoardPosition attackMovement = new BoardPosition(y, x);
                    AttackMovements.Add(attackMovement);

                    x = x + 2;
                    //y++;
                    BoardPosition attackMovement2 = new BoardPosition(y, x);
                    AttackMovements.Add(attackMovement2);
                }
            }
            return AttackMovements;
        }

        /// <summary>
        /// Returns a set of all BoardPositions that are attacked by the given Queen at the current BoardPosition.
        /// </summary>
        public ISet<BoardPosition> GetAttackPositionsForQueen(BoardPosition currBP)
        {
            ISet<BoardPosition> AttackMovements = GetAttackPositionsForRook(currBP);
            ISet<BoardPosition> AttackMovementsDiagonal = GetAttackPositionsForBishop(currBP);
            AttackMovements.UnionWith(AttackMovementsDiagonal);
            return AttackMovements;
        }

        public ISet<BoardPosition> GetBoardPositionsOfPieces(List<ChessPiece> chessPieces)
        {
            int currPosition = 0;
            int currRow, currCol;
            ISet<BoardPosition> boardPositionsForP = new HashSet<BoardPosition>();

            for (int i = 0; i < chessPieces.Count(); i++)
            {
                if (i % 2 == 0)
                {
                    if (i != 0)
                    {
                        currPosition++;
                    }
                    //Console.WriteLine("Index in logic for loop: " + currPosition);


                    // Get BoardPosition for currPosition (index)
                    double div = currPosition / 4;
                    currRow = Convert.ToInt32(Math.Floor(div));

                    currCol = (2 * (currPosition - (currRow * 4)));
                    //Console.WriteLine("Current row: " + currRow);
                    //Console.WriteLine("Current col: " + currCol);
                }
                else
                {
                    //Console.WriteLine("Index in logic for loop: " + currPosition);


                    // Get BoardPosition for currPosition (index)
                    double div = currPosition / 4;
                    currRow = Convert.ToInt32(Math.Floor(div));

                    currCol = (2 * (currPosition - (currRow * 4)) + 1);
                    //Console.WriteLine("Current row: " + currRow);
                    //Console.WriteLine("Current col: " + currCol);
                }

                // Add current Board Position into set of current Board Positions
                BoardPosition currBP = new BoardPosition(currRow, currCol);
                boardPositionsForP.Add(currBP);
            }
            return boardPositionsForP;
        }

        /// <summary>
        /// Returns a set of all BoardPositions that are attacked by the given player.
        /// </summary>
        public ISet<BoardPosition> GetAttackedPositions(int byPlayer)
        {
            List<ChessPiece> allChessPieces = GetAllChessPiecesOnBoard();
            ISet<BoardPosition> attackedPositionsForP = new HashSet<BoardPosition>();

            int currPosition = 0;
            int currRow, currCol;

            for (int i = 0; i < allChessPieces.Count(); i++)
            {
                if (i % 2 == 0)
                {
                    if (i != 0)
                    {
                        currPosition++;
                    }
                    //Console.WriteLine("Index in logic for loop: " + currPosition);


                    // Get BoardPosition for currPosition (index)
                    double div = currPosition / 4;
                    currRow = Convert.ToInt32(Math.Floor(div));

                    currCol = (2 * (currPosition - (currRow * 4)));
                    //Console.WriteLine("Current row: " + currRow);
                    //Console.WriteLine("Current col: " + currCol);
                }
                else
                {
                    //Console.WriteLine("Index in logic for loop: " + currPosition);


                    // Get BoardPosition for currPosition (index)
                    double div = currPosition / 4;
                    currRow = Convert.ToInt32(Math.Floor(div));

                    currCol = (2 * (currPosition - (currRow * 4)) + 1);
                    //Console.WriteLine("Current row: " + currRow);
                    //Console.WriteLine("Current col: " + currCol);
                }

                // Add current Board Position into set of current Board Positions
                BoardPosition currBP = new BoardPosition(currRow, currCol);

                // Apply move logic to each piece based on its piece type to return a new board position
                ChessPiece cP = allChessPieces.ElementAt(i);
                //Console.WriteLine("ChessPiece Player: " + cP.Player);
                //Console.WriteLine("ChessPiece Piecetype: " + cP.PieceType);
                if (cP.PieceType != 000)
                {
                    // Depending on player value
                    if (cP.Player == byPlayer)
                    {
                        // Move Logic here
                        switch (Convert.ToInt32(cP.PieceType))
                        {
                            case 1:
                                ISet<BoardPosition> pawnAttackedPositions = GetAttackPositionsForPawn(currBP, cP);
                                foreach (var pos in pawnAttackedPositions)
                                {
                                    //Console.WriteLine("Pawn pos: " + pos.ToString());
                                    attackedPositionsForP.Add(pos);
                                }
                                break;
                            case 2:
                                ISet<BoardPosition> rookAttackedPositions = GetAttackPositionsForRook(currBP);
                                foreach (var pos in rookAttackedPositions)
                                {
                                    //Console.WriteLine("Rook pos: " + pos.ToString());
                                    attackedPositionsForP.Add(pos);
                                }
                                break;
                            case 3:
                                ISet<BoardPosition> knightAttackedPositions = GetAttackPositionsForKnight(currBP);
                                foreach (var pos in knightAttackedPositions)
                                {
                                    //Console.WriteLine("Knight pos: " + pos.ToString());
                                    attackedPositionsForP.Add(pos);
                                }
                                break;
                            case 4:
                                ISet<BoardPosition> bishopAttackedPositions = GetAttackPositionsForBishop(currBP);
                                foreach (var pos in bishopAttackedPositions)
                                {
                                    //Console.WriteLine("Bishop pos: " + pos.ToString());
                                    attackedPositionsForP.Add(pos);
                                }
                                break;
                            case 5:
                                ISet<BoardPosition> queenAttackedPositions = GetAttackPositionsForQueen(currBP);
                                foreach (var pos in queenAttackedPositions)
                                {
                                    //Console.WriteLine("Queen pos: " + pos.ToString());
                                    attackedPositionsForP.Add(pos);
                                }
                                break;
                            case 6:
                                ISet<BoardPosition> kingAttackedPositions = GetAttackPositionsForKing(currBP);
                                foreach (var pos in kingAttackedPositions)
                                {
                                    //Console.WriteLine("King pos: " + pos.ToString());
                                    attackedPositionsForP.Add(pos);
                                }
                                break;
                            default:
                                //Console.WriteLine("Current piece is Empty - get attack positions");
                                break;
                        }

                    }
                }
                //Console.WriteLine("Attack pos count: " + attackedPositionsForP.Count());
                //Console.WriteLine("");
            }

            return attackedPositionsForP;
        }
        #endregion

        #region Private methods.
        /// <summary>
        /// Mutates the board state so that the given piece is at the given position.
        /// </summary>
        private void SetPieceAtPosition(BoardPosition position, ChessPiece piece)
        {
            int curplayer = piece.Player;
            // Getting row and column position
            int row = position.Row;
            int col = position.Col;

            // Algorithm to get position index in array
            row = row * 4;
            col = col / 2;
            int posIndex = row + col;
            //Console.WriteLine(posIndex);
            //Console.WriteLine(col);

            // At posIndex in the array Gameboard, assign the chess piece to position
            byte posByte = chessBoard[posIndex];
            // Console.WriteLine(posByte);
            //byte test = 0;
            byte pieceByte = Convert.ToByte((int)piece.PieceType);
            byte currPlayer = Convert.ToByte((curplayer == 1 || piece.PieceType == 0) ? 0 : 8);
            if (position.Col % 2 == 0)
            {
                byte leftByte = (byte)((byte)((pieceByte | currPlayer) << 4) | (byte)((byte)(posByte << 4) >> 4));
                chessBoard[posIndex] = leftByte;
            }
            else
            {
                byte rightByte = (byte)(((pieceByte | currPlayer)) | (byte)((byte)(posByte >> 4) << 4));
                chessBoard[posIndex] = rightByte;
            }

        }

        #endregion

        #region Explicit IGameBoard implementations.
        IEnumerable<IGameMove> IGameBoard.GetPossibleMoves()
        {
            //Console.WriteLine("Get Attacked Positions:");
            //ISet<BoardPosition> attackPositions = GetAttackedPositions(0);
            ////IEnumerable<BoardPosition> posOfPawn = GetPositionsOfPiece(ChessPieceType.Pawn, 1);
            //int playerTest = GetPlayerAtPosition(new BoardPosition(7, 0));
            ////IEnumerable<ChessMove> getMovesRook = GetPossibleMovesRook(new BoardPosition(0, 0));
            //IEnumerable<ChessMove> getMovesQueen = GetPossibleMovesPawn(new BoardPosition(1, 0));
            //Console.WriteLine("Player Test: " + playerTest);

            //ChessPiece playerpieceTest = GetPieceAtPosition(new BoardPosition(7, 0));
            //Console.WriteLine("Player Test: " + playerpieceTest.PieceType);

            return GetPossibleMoves();
        }
        void IGameBoard.ApplyMove(IGameMove m)
        {
            ApplyMove(m as ChessMove);
        }
        IReadOnlyList<IGameMove> IGameBoard.MoveHistory => mMoveHistory;
        #endregion

        // You may or may not need to add code to this constructor.
        public ChessBoard()
        {

        }

        public ChessBoard(IEnumerable<Tuple<BoardPosition, ChessPiece>> startingPositions)
            : this()
        {
            var king1 = startingPositions.Where(t => t.Item2.Player == 1 && t.Item2.PieceType == ChessPieceType.King);
            var king2 = startingPositions.Where(t => t.Item2.Player == 2 && t.Item2.PieceType == ChessPieceType.King);
            if (king1.Count() != 1 || king2.Count() != 1)
            {
                throw new ArgumentException("A chess board must have a single king for each player");
            }

            foreach (var position in BoardPosition.GetRectangularPositions(8, 8))
            {
                SetPieceAtPosition(position, ChessPiece.Empty);
            }

            int[] values = { 0, 0 };
            // TODO: you must calculate the overall advantage for this board, in terms of the pieces
            // that the board has started with. "pos.Item2" will give you the chess piece being placed
            // on this particular position.
            foreach (var pos in startingPositions)
            {
                SetPieceAtPosition(pos.Item1, pos.Item2);
                switch (pos.Item2.PieceType)
                {
                    case ChessPieceType.Pawn:
                        if (pos.Item2.Player == 1)
                        {
                            values[0]++;
                        }
                        else
                        {
                            values[1]++;
                        }
                        break;
                    case ChessPieceType.Rook:
                        if (pos.Item2.Player == 1)
                        {
                            values[0] = values[0] + 5;
                        }
                        else
                        {
                            values[1] = values[1] + 5;
                        }
                        break;
                    case ChessPieceType.Bishop:
                        if (pos.Item2.Player == 1)
                        {
                            values[0] = values[0] + 3;
                        }
                        else
                        {
                            values[1] = values[1] + 3;
                        }
                        break;
                    case ChessPieceType.Knight:
                        if (pos.Item2.Player == 1)
                        {
                            values[0] = values[0] + 3;
                        }
                        else
                        {
                            values[1] = values[1] + 3;
                        }
                        break;
                    case ChessPieceType.Queen:
                        if (pos.Item2.Player == 1)
                        {
                            values[0] = values[0] + 9;
                        }
                        else
                        {
                            values[1] = values[1] + 9;
                        }
                        break;
                    case ChessPieceType.King:
                        if (pos.Item2.Player == 1)
                        {
                            values[0] = values[0] + 40;
                        }
                        else
                        {
                            values[1] = values[1] + 40;
                        }
                        break;
                    default:
                        break;
                }
                //Console.WriteLine("Advantage on board- White: " + values[0] + "Black: " + values[1]);
            }
            byte[] adding = {  0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0,
                            0,0,0,0};
            for (int i = 0; i < adding.Length; i++)
            {
                adding[i] = chessBoard[i];
            }
            chessBoardHistory.Add(adding);
            setAdvantage();
        }
    }
}
