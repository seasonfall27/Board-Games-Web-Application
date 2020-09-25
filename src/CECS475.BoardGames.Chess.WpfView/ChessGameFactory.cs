using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace CECS475.BoardGames.Chess.WpfView
{
    /// <summary>
    /// Represents an Chess game that can be played in a WPF app.
    /// </summary>
    public class ChessGameFactory : IWpfGameFactory
    {
        public string GameName
        {
            get
            {
                return "Chess";
            }
        }

        public IValueConverter CreateBoardAdvantageConverter()
        {
            return new ChessAdvantageConverter();
        }

        public IValueConverter CreateCurrentPlayerConverter()
        {
            return new ChessCurrentPlayerConverter() ;
        }

        public IWpfGameView CreateGameView()
        {
            return new ChessView();
        }

        public IWpfGameView CreateGameView(NumberOfPlayers players)
        {
            var view = new ChessView();
            view.ChessViewModel.Players = players;
            return view;
        }
    }
}
