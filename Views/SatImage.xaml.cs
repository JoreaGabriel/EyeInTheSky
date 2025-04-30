using EyeInTheSky.ClassLibrary;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Globalization;
using System.Net.Http.Json;

namespace EyeInTheSky.Views;

public partial class SatImage : ContentPage
{
    
    private string weatherApiKey = ApiKeys.Weather_API_KEY;
    private string NasaApiKey = ApiKeys.NASA_API_KEY;

    public SatImage()
	{
		InitializeComponent();
        SetLocation();
    }
    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        var position = e.Location;        
        coordLabel.Text = $"Lat: {position.Latitude:F6}, Long: {position.Longitude:F6}";                
        googleMap.Pins.Clear();                
        var pin = new Pin
        {
            Label = "Punct selectat",
            Location = position,
            Type = PinType.Place
        };
        googleMap.Pins.Add(pin);
    }

    private async void searchButton_Clicked(object sender, EventArgs e)
    {
        string locationName = locationEntry.Text;
        try
        {
            var locations = await Geocoding.GetLocationsAsync(locationName);

            if (locations != null && locations.Any())
            {
                var location = locations.First();

                var mapLocation = new Location(location.Latitude, location.Longitude);

               
                googleMap.MoveToRegion(MapSpan.FromCenterAndRadius(mapLocation, Distance.FromMiles(1)));
                coordLabel.Text = $"Lat: {mapLocation.Latitude:F6}, Long: {mapLocation.Longitude:F6}";
                googleMap.Pins.Clear();
                var pin = new Pin
                {
                    Label = locationName,
                    Location = mapLocation, 
                    Type = PinType.Place
                };

                googleMap.Pins.Add(pin);
            }
            else
            {
                await DisplayAlert("Eroare", "Nu am gasit locatia introdusa.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
    }
    private async void SetLocation()
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location != null)
            {
                var position = new Location(location.Latitude, location.Longitude);
                googleMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMiles(1)));
                googleMap.Pins.Clear();

                var pin = new Pin
                {
                    Label = "Punct selectat",
                    Location = position,
                    Type = PinType.Place
                };

                googleMap.Pins.Add(pin);
            }
            else
            {
                await DisplayAlert("Eroare", "Nu am putut obtine locatia curenta.", "OK");
                googleMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(44.4268, 26.1025),Distance.FromKilometers(10)));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
    }
    private async Task LoadNasaImage(string latitude, string longitude)
    {
        string date = "2020-01-01"; 
        string dim = "0.15"; 

        string url = $"https://api.nasa.gov/planetary/earth/imagery" +
                     $"?lon={longitude}&lat={latitude}&date={date}&dim={dim}&api_key={NasaApiKey}";

        try
        {
            var httpClient = new HttpClient();
            var imageStream = await httpClient.GetStreamAsync(url);
            nasaImage.Source = ImageSource.FromStream(() => imageStream);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", $"Nu am putut incarca imaginea NASA.\n{ex.Message}", "OK");
        }
    }    

    private void showMap_Clicked(object sender, EventArgs e)
    {
        googleMap.IsVisible = !googleMap.IsVisible;
    }

    private void SwipeItem_Invoked(object sender, EventArgs e)
    {
        nasaImage.Source = null;
        LoadNasaImage(googleMap.Pins[0].Location.Latitude.ToString("F2", CultureInfo.InvariantCulture), googleMap.Pins[0].Location.Longitude.ToString("F2", CultureInfo.InvariantCulture));
        nasaImage.IsVisible = true;
        weatherLayout.IsVisible = false;
    }    

    private async Task<WeatherInfo> GetWeatherInfo(double lat, double lon)
    {
        var _httpClient = new HttpClient();
        var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={weatherApiKey}&units=metric&lang=ro";
        return await _httpClient.GetFromJsonAsync<WeatherInfo>(url);
    }
    private async void OnWeatherSwipe(object sender, EventArgs e)
    {
        Location selectedPin = new Location(googleMap.Pins[0].Location.Latitude, googleMap.Pins[0].Location.Longitude);
        if (selectedPin == null)
        {
            await DisplayAlert("Info", "Selecteaza un punct pe harta", "OK");
            return;
        }

        var weather = await GetWeatherInfo(selectedPin.Latitude, selectedPin.Longitude);

        nasaImage.IsVisible = false;
        weatherLayout.IsVisible = true;
        double pressure = Math.Round(weather.Main.Pressure * 0.75006, 1);
        string iconCode = weather.Weather[0].Icon;
        string iconUrl = $"https://openweathermap.org/img/wn/{iconCode}@2x.png";
        weatherLocation.Text = $"Locatie: {weather.Name}";
        weatherIcon.Source = ImageSource.FromUri(new Uri(iconUrl));
        weatherLabel.Text = $"Temperatura: {weather.Main.Temp} °C (Se simte ca {weather.Main.Feels_like}°C)\n" +
                                $"Presiune: {pressure} mmHg\n" +
                                $"Umiditate: {weather.Main.Humidity}%\n" +
                                $"Vant: {weather.Wind.Speed} m/s\n" +
                                $"Cer: {weather.Weather[0].Main} - {weather.Weather[0].Description}";
    }
}