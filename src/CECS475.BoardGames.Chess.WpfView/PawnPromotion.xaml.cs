using Cecs475.BoardGames.Chess.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace CECS475.BoardGames.Chess.WpfView
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PawnPromotion : Window
    {
       // private int result { return}
       public ChessPieceType result
        {
            get;set;
        }
        public PawnPromotion(ObservableCollection<ChessSquare> pp)
        {
            InitializeComponent();
            BoardPositions.ItemsSource = pp;
        }
        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("vm") as ChessViewModel;
            square.IsSelected = true;
            
        }
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            var vm = FindResource("vm") as ChessViewModel;
            vm.Promotion = square.Player.PieceType;
            result = square.Player.PieceType;
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }
            private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            Border b = sender as Border;
            var square = b.DataContext as ChessSquare;
            square.IsSelected = false;
        }

        public ChessViewModel ChessViewModel => FindResource("vm") as ChessViewModel;

        public Control ViewControl => this;

    }
}
