using System;
using System.Collections.Generic;
using System.Text;

namespace Cecs475.BoardGames.Chess.Model {
	/// <summary>
	/// Represents a chess piece owned by a particular player.
	/// </summary>
	public struct ChessPiece {
		public ChessPieceType PieceType { get; }
		public sbyte Player { get; }

		public ChessPiece(ChessPieceType pieceType, int player) {
			PieceType = pieceType;
			Player = (sbyte)player;
		}

		public static ChessPiece Empty{ get; } = new ChessPiece(ChessPieceType.Empty, 0);

		public override string ToString()
		{
			string player = "Empty", piece;
			if(Player == 1)
			{
				player = "Light";
			}else if (Player == 2)
			{
				player = "Dark";
			}

			switch (PieceType)
			{
				case ChessPieceType.Pawn:
					piece = "Pawn";
					break;
				case ChessPieceType.Rook:
					piece = "Rook";
					break;
				case ChessPieceType.Bishop:
					piece = "Bishop";
					break;
				case ChessPieceType.Knight:
					piece = "Knight";
					break;
				case ChessPieceType.Queen:
					piece = "Queen";
					break;
				case ChessPieceType.King:
					piece = "King";
					break;
				default:
					piece = "Enpty";
					break;
			}
			return player + "_" + piece;
		}
	}
}
