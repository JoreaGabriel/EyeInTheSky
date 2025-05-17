using EyeInTheSky.ClassLibrary;
using EyeInTheSky.Pages;
using EyeInTheSky.Views;
using System.Net.Http.Json;

namespace EyeInTheSky
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _httpClient = new();

        public MainPage()
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
            GetJoke();
        }
        public async void GetJoke()
        {
            var joke = await _httpClient.GetFromJsonAsync<Joke>("https://official-joke-api.appspot.com/random_joke");
            if (joke != null)
            {
                labelQOTD.Text = $"{joke.Setup}\n\n{joke.Punchline}";
            }
        }
        private void btnSatImg_Clicked(object sender, EventArgs e)
        {
           Navigation.PushAsync(new SatImage());
        }

        private void btnMarsImg_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MarsImage());
        }

        private void btnAPOD_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Apod());
        }

        private void btnWeather_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new WeatherPage());
        }
    }

}
