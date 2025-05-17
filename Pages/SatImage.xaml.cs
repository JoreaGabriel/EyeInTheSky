using EyeInTheSky.ClassLibrary;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Globalization;
using System.Net.Http.Json;

namespace EyeInTheSky.Views;

public partial class SatImage : ContentPage
{   
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
        lblLoading.IsVisible = true;
        nasaImage.Source = null;
        await LoadNasaImage(googleMap.Pins[0].Location.Latitude.ToString("F2", CultureInfo.InvariantCulture), googleMap.Pins[0].Location.Longitude.ToString("F2", CultureInfo.InvariantCulture));
        nasaImage.IsVisible = true;        
        lblLoading.IsVisible = false;
    } 
    
}