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
using System.Windows.Shapes;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace Cecs475.BoardGames.WpfApp
{
    /// <summary>
    /// Interaction logic for LoadingScreen.xaml
    /// </summary>
    public partial class LoadingScreen : Window
    {
        public LoadingScreen()
        {
            InitializeComponent();
        }

        public class BoardGames
        {
            public Game[] Games
            {
                get; set;
            }
        }
        public class GameFiles
        {
            public string Name
            {
                get; set;
            }
            public string Url
            {
                get; set;
            }
            public string Version
            {
                get; set;
            }
            public string Key
            {
                get; set;
            }
        }
        public class Game
        {
            public string GameName
            {
                get; set;
            }

            public GameFiles[] Files
            {
                get; set;
            }
        }

        public async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var client = new RestClient("https://cecs475-boardamges.herokuapp.com/api/games");
            var request = new RestRequest("https://cecs475-boardamges.herokuapp.com/api/games", Method.GET);
            var response = await client.ExecuteAsync(request);
            var loaded = JsonConvert.DeserializeObject<List<Game>>(response.Content);
            //iterate through each game in the list (don't assume there is only one game, as there is currently).
            //For each game, you will need to construct a WebClient object and use the DownloadFileTaskAsync method to downlaod the URL associated with each file in the game.           
            foreach(Game game in loaded)
            {
                WebClient webClient = new WebClient();
                foreach(GameFiles files in game.Files)
                {
                    await webClient.DownloadFileTaskAsync(files.Url, @"games\" + files.Url.Split('/').Last());
                }
            }
            //When the downlaods have finished, show the game choice window, and close the "Loading" window
            new GameChoiceWindow().Show();
            Close();
        }
    }
}
