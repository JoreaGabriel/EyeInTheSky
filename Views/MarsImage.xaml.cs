using EyeInTheSky.ClassLibrary;
using Microsoft.Maui;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace EyeInTheSky.Views;

public partial class MarsImage : ContentPage
{
    private readonly HttpClient _httpClient = new();
    private string ApiKey = ApiKeys.NASA_API_KEY;
    private int currentPage = 1;
    private const int pageSize = 5;    
    private string currentRover = "";
    private string currentDate = "";

    public ObservableCollection<MarsPhoto> Photos { get; set; } = new();
    public MarsImage()
	{
		InitializeComponent();
        imagesCollection.ItemsSource = Photos;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        Photos.Clear();
        currentPage = 1;

        if (roverPicker.SelectedItem == null)
        {
            await DisplayAlert("Atentie", "Alege roverul!", "OK");
            return;
        }

        currentRover = roverPicker.SelectedItem.ToString().ToLower();
        currentDate = earthDatePicker.Date.ToString("yyyy-MM-dd");

       
        await LoadPhotosPage();

    }
    private async Task LoadPhotosPage()
    {     
        try
        {
            string url = $"https://api.nasa.gov/mars-photos/api/v1/rovers/{currentRover}/photos?earth_date={currentDate}&page={currentPage}&api_key={ApiKey}";
            var result = await _httpClient.GetFromJsonAsync<MarsPhotoResponse>(url);

            if (result?.Photos != null && result.Photos.Any())
            {
                var photosToShow = result.Photos
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                Photos.Clear();
                foreach (var photo in photosToShow)
                {
                    Photos.Add(photo);
                }
                nextButton.IsVisible = true;
                previousButton.IsVisible = true;
                nextButton.IsEnabled = result.Photos.Count > currentPage * pageSize;
                previousButton.IsEnabled = currentPage > 1;
            }
            else
            {
                await DisplayAlert("Info", $"Nu am gasit imagini din aceasta data de la roverul {currentRover}.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }        
    }
    private async void NextButton_Clicked(object sender, EventArgs e)
    {
        currentPage++;
        await LoadPhotosPage();
    }

    private async void PreviousButton_Clicked(object sender, EventArgs e)
    {
        if (currentPage > 1)
        {
            currentPage--;
            await LoadPhotosPage();
        }
    }

    private async void Image_Tapped(object sender, EventArgs e)
    {
        if (sender is Image image && image.BindingContext is MarsPhoto photo)
        {
            try
            {
                Uri uri = new(photo.Img_src);
                await Launcher.Default.OpenAsync(uri);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Eroare", $"Nu am putut deschide imaginea: {ex.Message}", "OK");
            }
        }
    }
}