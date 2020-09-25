using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Cecs475.BoardGames.Chess.Model;
using Cecs475.BoardGames.Model;
using System.Windows.Media.Imaging;

namespace CECS475.BoardGames.Chess.WpfView
{
    class ChessPieceImageConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				ChessPiece? c = value as ChessPiece?;
				if (c != null)
				{
					string src = c.ToString();
					//Console.WriteLine("This is what it is converting " + src);
					return new BitmapImage(new Uri("/CECS475.BoardGames.Chess.WpfView;component/Resources/" + src + ".png", UriKind.Relative));
				}
				else
				{
					string src = c.ToString();
					return new BitmapImage(new Uri("/CECS475.BoardGames.Chess.WpfView;component/Resources/" + src + ".png", UriKind.Relative));
				}
			}
			catch (Exception e)
			{
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
