using EyeInTheSky.ClassLibrary;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Globalization;
using System.Net.Http.Json;

namespace EyeInTheSky.Pages;

public partial class WeatherPage : ContentPage
{
    private string weatherApiKey = ApiKeys.Weather_API_KEY;
    public WeatherPage()
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
    private void OnLabelTapped(object sender, EventArgs e)
    {
        DisplayAlert("Detalii", descriereAirPollution, "OK");
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
                googleMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(44.4268, 26.1025), Distance.FromKilometers(10)));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
    }
    private async void GetWeatherDetails(object sender, EventArgs e)
    {
        Location selectedPin = new Location(googleMap.Pins[0].Location.Latitude, googleMap.Pins[0].Location.Longitude);
        if (selectedPin == null)
        {
            await DisplayAlert("Info", "Selecteaza un punct pe harta", "OK");
            return;
        }

        var weather = await GetWeatherInfo(selectedPin.Latitude, selectedPin.Longitude);
        var airPollution = await GetAirPollution(selectedPin.Latitude, selectedPin.Longitude);
        var item = airPollution.list.FirstOrDefault();
        string qualityDescription = item.main.aqi switch
        {
            1 => "Excelentă",
            2 => "Bună",
            3 => "Moderată",
            4 => "Slabă",
            5 => "Foarte slabă",
            _ => "Necunoscută"
        };
        airPollutionLabel.Text = $"Calitate aer: {qualityDescription}\n" +
                                 $"CO (monoxid de carbon): {item.components.co} µg/m³\n" +
                                 $"NO (monoxid de azot): {item.components.no} µg/m³\n" +
                                 $"NO2 (dioxid de azot): {item.components.no2} µg/m³\n" +
                                 $"O3 (ozon): {item.components.o3} µg/m³\n" +
                                 $"SO2 (dioxid de sulf): {item.components.so2} µg/m³\n" +
                                 $"PM2.5 (particule fine): {item.components.pm2_5} µg/m³\n" +
                                 $"PM10 (particule): {item.components.pm10} µg/m³\n" +
                                 $"NH3 (amoniac): {item.components.nh3} µg/m³\n";
        btnForecast.IsVisible = true;
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
            Pressure = $"Presiune: {Math.Round(item.main.pressure * 0.75006375541921, 1)} mmHg",
            Humidity = $"Umiditate: {item.main.humidity}%",
            Description = item.weather.FirstOrDefault()?.description ?? "",
            IconUrl = $"https://openweathermap.org/img/wn/{item.weather.FirstOrDefault()?.icon}@2x.png"
        }).ToList();

        ForecastList.ItemsSource = displayList;
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
    private async Task<AirPollution> GetAirPollution(double lat, double lon)
    {
        var _httpClient = new HttpClient();
        var url = $"https://api.openweathermap.org/data/2.5/air_pollution?lat={lat}&lon={lon}&appid={weatherApiKey}";
        return await _httpClient.GetFromJsonAsync<AirPollution>(url);
    }



    public string descriereAirPollution = "PM2.5 (Particule fine ≤ 2.5 µm)\r\n" +
    "Ce este: pulberi foarte fine provenite din gaze de esapament, industrie, fum etc.\r\n" +
    "Pericol: patrund adanc in plamani si chiar in sange.\r\n" +
    "Efecte: probleme respiratorii, cardiovasculare, agravarea astmului, risc crescut de cancer.\r\n\n" +
    "PM10 (Particule ≤ 10 µm)\r\n" +
    "Ce este: pulberi mai mari (praf, polen, mucegai).\r\n" +
    "Pericol: afecteaza sistemul respirator superior (nas, gat).\r\n" +
    "Efecte: iritatii, tuse, exacerbarea alergiilor.\r\n\n" +
    "O₃ (Ozon)\r\n" +
    "Ce este: gaz rezultat din reactii chimice intre poluanti si radiatia solara.\r\n" +
    "Pericol: toxic la nivelul solului (desi in stratosfera ne protejeaza).\r\n" +
    "Efecte: iritatii respiratorii, dificultati de respiratie, periculos pentru copii si batrani.\r\n\n" +
    "CO (Monoxid de Carbon)\r\n" +
    "Ce este: gaz incolor, inodor, provenit din ardere incompleta.\r\n" +
    "Pericol: reduce capacitatea sangelui de a transporta oxigen.\r\n" +
    "Efecte: dureri de cap, ameteala, in concentratii mari – letal.\r\n\n" +
    "NO (Monoxid de Azot) & NO₂ (Dioxid de Azot)\r\n" +
    "Ce este: emis de vehicule si centrale termice.\r\n" +
    "Pericol: iritant pentru plamani.\r\n" +
    "Efecte: inflamatii, agraveaza bolile pulmonare.\r\n\n" +
    "SO₂ (Dioxid de Sulf)\r\n" +
    "Ce este: provine din arderea carbunelui si petrolului.\r\n" +
    "Pericol: formeaza ploi acide.\r\n" +
    "Efecte: iritatii ale cailor respiratorii, in special la astmatici.\r\n\n" +
    "NH₃ (Amoniac)\r\n" +
    "Ce este: rezultat din activitati agricole (gunoi de grajd, ingrasaminte).\r\n" +
    "Pericol: iritant pentru ochi, nas, gat.\r\n" +
    "Efecte: tuse, dificultati de respiratie in concentratii mari.\r\n";
}