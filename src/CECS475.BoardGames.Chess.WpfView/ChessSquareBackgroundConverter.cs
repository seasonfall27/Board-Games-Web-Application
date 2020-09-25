using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CECS475.BoardGames.Chess.WpfView
{
    class ChessSquareBackgroundConverter : IMultiValueConverter
    {
        //private static SolidColorBrush SELECT_BRUSH = Brushes.Red;
        private static SolidColorBrush CHECK_BRUSH = Brushes.Yellow;
        private static SolidColorBrush ENDPOS_BRUSH = Brushes.LightGreen;
        private static SolidColorBrush SELECT_BRUSH = Brushes.Red;
        private static SolidColorBrush DARK_BRUSH = Brushes.LightPink;
        private static SolidColorBrush LIGHT_BRUSH = Brushes.White;

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			// This converter will receive two properties: the Position of the square, and whether it
			// is being hovered.
			BoardPosition pos = (BoardPosition)values[0];
			bool isHighlighted = (bool)values[1];

			//// Hovered squares have a specific color.
			if (isHighlighted)
			{
			return ENDPOS_BRUSH;
			}

			bool isStartPos = (bool)values[2];
			if (isStartPos)
			{
				return SELECT_BRUSH;
			}

			bool isCheck = (bool)values[3];
			if (isCheck)
			{
				return CHECK_BRUSH;
			}




			// Corner squares are very good, and drawn green.
			if ((pos.Row == 0 || pos.Row == 2 || pos.Row == 4 || pos.Row == 6))
			{
				if ((pos.Col == 0 || pos.Col == 2 || pos.Col == 4 || pos.Col == 6))
				{
					return DARK_BRUSH;
				}
				else {
					return LIGHT_BRUSH;
				}
			}
			else
			{
				if ((pos.Col == 1 || pos.Col == 3 || pos.Col == 5 || pos.Col == 7))
				{
					return DARK_BRUSH;
				}
				else
				{
					return LIGHT_BRUSH;
				}
			}
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
