using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EyeInTheSky.ClassLibrary
{
    public static class ApiKeys
    {
        public static string NASA_API_KEY { get; private set; }
        public static string Weather_API_KEY { get; private set; }

        static ApiKeys()
        {
            var json = File.ReadAllText(Path.Combine(FileSystem.AppDataDirectory, "appsettings.json"));
            var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            NASA_API_KEY = settings["NASA_API_KEY"];
            Weather_API_KEY = settings["Weather_API_KEY"];
        }
    }
}
