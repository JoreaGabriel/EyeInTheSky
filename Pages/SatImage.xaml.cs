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
        SetPin(position, googleMap);
    }
    private void showMap_Clicked(object sender, EventArgs e)
    {
        googleMap.IsVisible = !googleMap.IsVisible;
        if (googleMap.IsVisible) showMap.ImageSource = "off.png";
        else showMap.ImageSource = "on.png";
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
                coordLabel.Text = $"Lat: {mapLocation.Latitude:F6}, Long: {mapLocation.Longitude:F6}"; 
                SetPin(mapLocation, googleMap);                
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
    public static async void SetPin(Location l, Microsoft.Maui.Controls.Maps.Map gMap)
    {
        gMap.MoveToRegion(MapSpan.FromCenterAndRadius(l, Distance.FromMiles(1)));
        gMap.Pins.Clear();

        var pin = new Pin
        {
            Label = "Punct selectat",
            Location = l,
            Type = PinType.Place
        };

        gMap.Pins.Add(pin);
    }
    private async void SetLocation()
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location != null)
            {
                var position = new Location(location.Latitude, location.Longitude);
                SetPin(position, googleMap);
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

    private async void SwipeItem_Invoked(object sender, EventArgs e)
    {
        weatherLayout.IsVisible = false;
        lblLoading.IsVisible = true;
        nasaImage.Source = null;
        await LoadNasaImage(googleMap.Pins[0].Location.Latitude.ToString("F2", CultureInfo.InvariantCulture), googleMap.Pins[0].Location.Longitude.ToString("F2", CultureInfo.InvariantCulture));
        nasaImage.IsVisible = true;        
        lblLoading.IsVisible = false;
    }    

    private async Task<WeatherInfo> GetWeatherInfo(double lat, double lon)
    {
        var _httpClient = new HttpClient();
        var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={weatherApiKey}&units=metric&lang=ro";
        return await _httpClient.GetFromJsonAsync<WeatherInfo>(url);
    }

    private async Task<WeatherForecast> GetForecast(double lat, double lon)
    {
        var _httpClient = new HttpClient();
        var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={weatherApiKey}&units=metric&lang=ro";
        return await _httpClient.GetFromJsonAsync<WeatherForecast>(url);
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
        ForecastList.IsVisible = false;
        weatherLayout.IsVisible = true;
        double pressure = Math.Round(weather.Main.Pressure * 0.75006, 1);
        DateTime sunriseTime = DateTimeOffset.FromUnixTimeSeconds(weather.Sys.sunrise).ToLocalTime().DateTime;
        DateTime sunsetTime = DateTimeOffset.FromUnixTimeSeconds(weather.Sys.sunset).ToLocalTime().DateTime;
        string iconCode = weather.Weather[0].Icon;
        string iconUrl = $"https://openweathermap.org/img/wn/{iconCode}@2x.png";
        weatherLocation.Text = $"Locatie: {weather.Name}";
        weatherIcon.Source = ImageSource.FromUri(new Uri(iconUrl));
        weatherLabel.Text = $"Temperatura: {weather.Main.Temp} °C (Se simte ca {weather.Main.Feels_like}°C)\n" +
                                $"Presiune: {pressure} mmHg\n" +
                                $"Umiditate: {weather.Main.Humidity}%\n" +
                                $"Vant: {weather.Wind.Speed} m/s\n" +
                                $"Cer: {weather.Weather[0].Main} - {weather.Weather[0].Description}\n" +
                                $"Rasarit: {sunriseTime.ToString("HH:mm")}\n" +
                                $"Apus: {sunsetTime.ToString("HH:mm")}\n";
    }
   
    private async void btnForecast_Clicked(object sender, EventArgs e)
    {
        Location selectedPin = new Location(googleMap.Pins[0].Location.Latitude, googleMap.Pins[0].Location.Longitude);
        if (selectedPin == null)
        {
            await DisplayAlert("Info", "Selecteaza un punct pe harta", "OK");
            return;
        }
        var weather = await GetForecast(selectedPin.Latitude, selectedPin.Longitude);
        ForecastList.IsVisible = true;
        var displayList = weather.list.Select(item => new HourlyForecastDisplay
        {
            Hour = DateTimeOffset.FromUnixTimeSeconds(item.dt).ToLocalTime().ToString("dd MMM yyyy, HH:mm", CultureInfo.GetCultureInfo("ro-RO")),
            Temp = $"Temperatura: {item.main.temp}°C",            
            Pressure = $"Presiune: {Math.Round(item.main.pressure * 0.75006375541921,1)} mmHg",
            Humidity = $"Umiditate: {item.main.humidity}%",
            Description = item.weather.FirstOrDefault()?.description ?? "",
            IconUrl = $"https://openweathermap.org/img/wn/{item.weather.FirstOrDefault()?.icon}@2x.png"
        }).ToList();

        ForecastList.ItemsSource = displayList;
    }
}