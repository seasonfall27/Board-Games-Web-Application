using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Cecs475.BoardGames.WpfView;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using Cecs475.BoardGames.ComputerOpponent;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace CECS475.BoardGames.Chess.WpfView
{
	/// <summary>
	/// Represents one square on the Chess board grid.
	/// </summary>
	public class ChessSquare : INotifyPropertyChanged
	{
		private ChessPiece mChessPiece;
		/// <summary>
		/// The ChessPiece in the given square.
		/// </summary>
		public ChessPiece Player
		{
			get { return mChessPiece; }
			set
			{
				if (value.Player != mChessPiece.Player)
				{
					//Console.WriteLine(value.PieceType + " " + value.Player);
					mChessPiece = value;
					OnPropertyChanged(nameof(Player));
				}
			}
		}

		/// <summary>
		/// The position of the square.
		/// </summary>
		public BoardPosition Position
		{
			get; set;
		}
		
		public ChessSquare() { }

		public ChessSquare(ChessPiece player)
		{
			Player = player;
		}


		private bool mIsSelected;
		public bool IsSelected { 
			get { return mIsSelected; } 
			set {
				if (value != mIsSelected)
				{
					mIsSelected = value;
					OnPropertyChanged(nameof(IsSelected));
				}
			} 
		} 


		private bool mIsHighlighted;
		/// <summary>
		/// Whether the square should be highlighted because of a user action.
		/// </summary>
		public bool IsHighlighted
		{
			get { return mIsHighlighted; }
			set
			{
				if (value != mIsHighlighted)
				{
					mIsHighlighted = value;
					OnPropertyChanged(nameof(IsHighlighted));
				}
			}
		}

		private bool mIsStartPos;
		public bool IsStartPos
		{
			get { return mIsStartPos; }
			set
			{
				if (value != mIsStartPos)
				{
					mIsStartPos = value;
					OnPropertyChanged(nameof(IsStartPos));
				}
			}
		}

		private bool mIsCheck;
		public bool IsCheck
		{
			get { return mIsCheck; }
			set
			{
				if (value != mIsCheck)
				{
					mIsCheck = value;
					OnPropertyChanged(nameof(IsCheck));
				}
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}


	/// <summary>
	/// Represents the game state of a single othello game.
	/// </summary>
	public class ChessViewModel : INotifyPropertyChanged, IGameViewModel
	{
		private ChessPieceType mPromotion;
		public ChessPieceType Promotion
		{
			get { return mPromotion; }
			set { mPromotion = value; }
		}
		private ChessBoard mBoard;
		private ObservableCollection<ChessSquare> mSquares;
		private ObservableCollection<ChessSquare> mPawnPromotionPieces;
		public event EventHandler GameFinished;
		private const int MAX_AI_DEPTH = 4;
		private IGameAi mGameAi = new MinimaxAi(MAX_AI_DEPTH);

		public ChessViewModel()
		{
			//SelectedPiece = false;
			mBoard = new ChessBoard();

			// Initialize the squares objects based on the board's initial state.
			mSquares = new ObservableCollection<ChessSquare>(
				BoardPosition.GetRectangularPositions(8, 8)
				.Select(pos => new ChessSquare()
				{
					Position = pos,
					Player = mBoard.GetPieceAtPosition(pos)
				})
			);

			// Initialize the squares objects based on the board's initial state.
			mPawnPromotionPieces = new ObservableCollection<ChessSquare>();

			mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Knight, 1)));
			mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Bishop, 1)));
			mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Rook, 1)));
			mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Queen, 1)));
			
			if (SelectedPiece == false)
			{
				PossibleMoves = new HashSet<BoardPosition>(
					from ChessMove m in mBoard.GetPossibleMoves()
					select m.StartPosition
				);
			}
		}

		/// <summary>
		/// Applies a move for the current player at the given position.
		/// </summary>
		public async Task ApplyMove(BoardPosition position)
		{
			BoardPosition startpos = new BoardPosition();
			foreach (ChessSquare x in mSquares)
			{
				if (x.IsSelected)
				{
					startpos = x.Position;
				}
			}
			var possMoves = mBoard.GetPossibleMoves() as IEnumerable<ChessMove>;
			ChessMove a = null;
			// Validate the move as possible.
			foreach (var move in possMoves)
			{
				if (move.EndPosition.Equals(position) && move.StartPosition.Equals(startpos))
				{
					if (move.MoveType.Equals(ChessMoveType.PawnPromote))
					{
						//GameFinished?.Invoke(this, new EventArgs());
						if(CurrentPlayer == 1)
						{
							mPawnPromotionPieces.Clear();

							mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Knight, 1)));
							mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Bishop, 1)));
							mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Rook, 1)));
							mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Queen, 1)));
						}
						else if (CurrentPlayer == 2)
						{
							mPawnPromotionPieces.Clear();

							mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Knight, 2)));
							mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Bishop, 2)));
							mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Rook, 2)));
							mPawnPromotionPieces.Add(new ChessSquare(new ChessPiece(ChessPieceType.Queen, 2)));
						}
						OnPropertyChanged(nameof(PawnPromotionPieces));
						var pawnPromotionWindow = new PawnPromotion(PawnPromotionPieces);
						pawnPromotionWindow.ShowDialog();
						Promotion = pawnPromotionWindow.result;
						
						break;
					}
					else {
						a = move;

						break;
					}
					
				}
			}
			if(a == null)
			{
				foreach (var move in possMoves)
				{
					if (move.EndPosition.Equals(position) && move.StartPosition.Equals(startpos))
					{
						if (move.MoveType.Equals(ChessMoveType.PawnPromote) && move.PromotePiece == Promotion)
						{
							//GameFinished?.Invoke(this, new EventArgs());
							mBoard.ApplyMove(move);
							break;
						}
					}
				}
			}

			else
			{
				mBoard.ApplyMove(a);
			}
			foreach (ChessSquare x in mSquares)
			{
				if (x.IsSelected)
				{
					x.IsSelected = false;
				}
			}
			RebindState();

			if (Players == NumberOfPlayers.One && !mBoard.IsFinished)
			{
				var bestMove = await Task.Run(() => mGameAi.FindBestMove(mBoard));
				if(bestMove != null)
				{
					mBoard.ApplyMove(bestMove as ChessMove);
				}
				RebindState();
			}


			if (mBoard.IsFinished)
			{
				GameFinished?.Invoke(this, new EventArgs());
			}
		}

		private void RebindState()
		{
			if (isPieceSelected() == true)
			{
				BoardPosition startpos = new BoardPosition();
				foreach (ChessSquare x in mSquares)
				{
					if (x.IsSelected)
					{
						startpos = x.Position;
					}
				}
				
				// Rebind the possible moves, now that the board has changed.
				PossibleMoves = new HashSet<BoardPosition>(
					from ChessMove m in mBoard.GetPossibleMoves() where m.StartPosition == startpos
					select m.EndPosition
				);
			}
			else
			{
				PossibleMoves = new HashSet<BoardPosition>(
					from ChessMove m in mBoard.GetPossibleMoves()
					select m.StartPosition
				);
			}


			// Update the collection of squares by examining the new board state.
			var newSquares = BoardPosition.GetRectangularPositions(8, 8);
			int i = 0;
			foreach (var pos in newSquares)
			{
				mSquares[i].Player = mBoard.GetPieceAtPosition(pos);
				i++;
			}
			OnPropertyChanged(nameof(BoardAdvantage));
			OnPropertyChanged(nameof(CurrentPlayer));
			OnPropertyChanged(nameof(CanUndo));

			if (mBoard.IsCheck || mBoard.IsCheckmate)
			{
				foreach (var x in mSquares)
				{
					if (x.Player.PieceType == ChessPieceType.King && x.Player.Player == mBoard.CurrentPlayer)
					{
						x.IsCheck = true;
					}

				}
			}
			else
			{
				{
					foreach (var x in mSquares)
					{
						x.IsCheck = false;
					}
				}
			}
		}

		/// <summary>
		/// A collection of 64 ChessSquare objects representing the state of the 
		/// game board.
		/// </summary>
		public ObservableCollection<ChessSquare> Squares
		{
			get { return mSquares; }
		}

		public ObservableCollection<ChessSquare> PawnPromotionPieces
		{
			get { return mPawnPromotionPieces; }
		}

		/// <summary>
		/// A set of board positions where the current player can move.
		/// </summary>
		public HashSet<BoardPosition> PossibleMoves
		{
			get; private set;
		}

		public bool SelectedPiece { get { return isPieceSelected(); }set { RebindState(); } }
		private bool isPieceSelected()
		{
			foreach(var x in mSquares)
			{
				if (x.IsSelected)
				{
					return true;
				}
			}
			return false;
		}



		/// <summary>
		/// The player whose turn it currently is.
		/// </summary>
		public int CurrentPlayer
		{
			get { return mBoard.CurrentPlayer; }
		}

		/// <summary>
		/// The value of the  Chess board.
		/// </summary>

		public GameAdvantage BoardAdvantage => mBoard.CurrentAdvantage;

		public bool CanUndo => mBoard.MoveHistory.Any();

		public NumberOfPlayers Players
		{
			get; set;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public void UndoMove()
		{
			if (CanUndo)
			{
				mBoard.UndoLastMove();
				//In one-player mode, Undo has to remove an additional move to return to the
				//human player's turn/
				if(Players == NumberOfPlayers.One && CanUndo)
				{
					mBoard.UndoLastMove();
				}
				RebindState();
			}
		}
	}
}
