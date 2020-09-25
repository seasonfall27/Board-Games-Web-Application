using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CECS475.BoardGames.Chess.WpfView
{
    /// <summary>
    /// Interaction logic ChessView.xaml
    /// </summary>
    public partial class ChessView : UserControl, IWpfGameView
    {
        public ChessView()
        {
            InitializeComponent();
        }

		private void Border_MouseEnter(object sender, MouseEventArgs e)
		{
			if (!IsEnabled) { return; }

			Border b = sender as Border;
			var square = b.DataContext as ChessSquare;
			var vm = FindResource("vm") as ChessViewModel;
			if (vm.PossibleMoves.Contains(square.Position))
			{
				square.IsHighlighted = true;
			}
		}

		private void Border_MouseLeave(object sender, MouseEventArgs e)
		{
			Border b = sender as Border;
			var square = b.DataContext as ChessSquare;
			square.IsHighlighted = false;
		}
		

		public ChessViewModel ChessViewModel => FindResource("vm") as ChessViewModel;

		public Control ViewControl => this;

		public IGameViewModel ViewModel => ChessViewModel;

		private async void Border_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (!IsEnabled) { return; }

			Border b = sender as Border;
			var square = b.DataContext as ChessSquare;
			var vm = FindResource("vm") as ChessViewModel;
			if (vm.SelectedPiece == true)
			{
				IsEnabled = false;
				await vm.ApplyMove(square.Position);
				IsEnabled = true;
				square.IsHighlighted = false;

			}
			else
			{
				if (vm.PossibleMoves.Contains(square.Position))
				{
					square.IsSelected = true;
					vm.SelectedPiece = true;
				}
			}
		}
	}
}
