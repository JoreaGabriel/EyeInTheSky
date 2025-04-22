namespace EyeInTheSky
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            EnsureSettingsFileAsync();
        }
        private async Task EnsureSettingsFileAsync()
        {
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, "appsettings.json");

            if (!File.Exists(targetPath))
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("appsettings.json");
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();
                File.WriteAllText(targetPath, content);
            }
        }
    }
}
