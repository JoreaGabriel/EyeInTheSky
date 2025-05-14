using EyeInTheSky.ClassLibrary;
using System.Net.Http.Json;

namespace EyeInTheSky.Views;

public partial class Apod : ContentPage
{
    private readonly HttpClient _httpClient = new();
    private string ApiKey = ApiKeys.NASA_API_KEY;
    private string imgUrl;
    public Apod()
	{
		InitializeComponent();
        LoadApod();
	}
    private async void LoadApod()
    {
        try
        {
            var url = $"https://api.nasa.gov/planetary/apod?api_key={ApiKey}";
            var apod = await _httpClient.GetFromJsonAsync<NasaApod>(url);

            if (apod != null && apod.Media_type == "image")
            {
                titleLabel.Text = apod.Title;
                apodImage.Source = apod.Url;
                descriptionLabel.Text = apod.Explanation;
                imgUrl = apod.Url;
            }
            else
            {
                titleLabel.Text = "Nu este imagine APOD azi";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
    }
    private async void Image_Tapped(object sender, EventArgs e)
    {
           
        if (!string.IsNullOrEmpty(imgUrl))
        {
            try
            {                
                await Launcher.Default.OpenAsync(imgUrl);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Eroare", $"Nu am putut deschide imaginea: {ex.Message}", "OK");
            }
        }
    }
}