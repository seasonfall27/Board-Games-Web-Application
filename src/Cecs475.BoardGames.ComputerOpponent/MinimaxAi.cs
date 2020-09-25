using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.ComputerOpponent
{
	internal struct MinimaxBestMove
	{
		public long Weight { get; set; }
		public IGameMove Move { get; set; }
	}

	public class MinimaxAi : IGameAi
	{
		private int mMaxDepth;
		public MinimaxAi(int maxDepth)
		{
			mMaxDepth = maxDepth;
		}

		public IGameMove FindBestMove(IGameBoard b)
		{
			return FindBestMove(b, long.MinValue, long.MaxValue,
				mMaxDepth, b.CurrentPlayer == 1).Move;
		}

		private static MinimaxBestMove FindBestMove(IGameBoard b, long alpha, long beta, int depthLeft, bool isMaximizing)
		{
			if (depthLeft == 0 || b.IsFinished)
				return new MinimaxBestMove
				{
					Weight = b.BoardWeight,
					Move = null
				};

			IGameMove bestMove = null;
			var possibleMoves = b.GetPossibleMoves();
			foreach (IGameMove m in possibleMoves)
			{
				b.ApplyMove(m);
				long weight = (FindBestMove(b, alpha, beta, depthLeft - 1, !isMaximizing)).Weight;
				b.UndoLastMove();

				if (isMaximizing && weight > alpha)
				{
					alpha = weight;
					bestMove = m;
				}
				else if (!isMaximizing && weight < beta)
				{
					beta = weight;
					bestMove = m;
				}

				if (alpha >= beta)
				{
					return new MinimaxBestMove
					{
						Weight = (isMaximizing) ? beta : alpha,
						Move = bestMove
					};
				}
			}

			return new MinimaxBestMove
			{
				Weight = (isMaximizing) ? alpha : beta,
				Move = bestMove
			};

		}

	}
}