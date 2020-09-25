﻿using Cecs475.BoardGames.WpfView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace Cecs475.BoardGames.WpfApp {
	/// <summary>
	/// Interaction logic for GameChoiceWindow.xaml
	/// </summary>
	public partial class GameChoiceWindow : Window {
		public GameChoiceWindow()
		{
			Type gameFactory = typeof(IWpfGameFactory);
			string inputFolderDir = @"games";

			var dlls = from someFile in Directory.EnumerateFiles(inputFolderDir, "*", SearchOption.AllDirectories)
					   select new
					   {
						   File = someFile
					   };

			foreach (var dllFile in dlls)
			{
				if(System.IO.Path.GetExtension(dllFile.File) == ".dll")
				{
					//Assembly.LoadFrom(dllFile.File);
					string assemblyname = System.IO.Path.GetFileNameWithoutExtension(dllFile.File); //assemblyname is the name of the file, without the extension.dll
					Assembly.Load(assemblyname + ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=68e71c13048d452a");
				}
				
			}

			var finalTypes = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assemb in assemblies)
			{
				var types = assemb.GetTypes();
				foreach (Type t in types)
				{
					if (gameFactory.IsAssignableFrom(t) && t.IsClass)
						finalTypes.Add(t);
				}
			}

			var gameFactories = new List<IWpfGameFactory>();
			foreach (Type t in finalTypes)
			{
				gameFactories.Add((IWpfGameFactory)Activator.CreateInstance(t));
			}

			this.Resources.Add("GameTypes", gameFactories);

			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			Button b = sender as Button;
			// Retrieve the game type bound to the button
			IWpfGameFactory gameType = b.DataContext as IWpfGameFactory;
			// Construct a GameWindow to play the game.
			var gameWindow = new GameWindow(gameType, mHuman.IsChecked.Value ? NumberOfPlayers.Two : NumberOfPlayers.One) {
				Title = gameType.GameName
			};
			// When the GameWindow closes, we want to show this window again.
			gameWindow.Closed += GameWindow_Closed;

			// Show the GameWindow, hide the Choice window.
			gameWindow.Show();
			this.Hide();
		}

		private void GameWindow_Closed(object sender, EventArgs e) {
			this.Show();
		}
	}
}
