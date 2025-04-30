using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeInTheSky.ClassLibrary
{
    public class WeatherInfo
    {
        public MainInfo Main { get; set; }
        public List<WeatherDescription> Weather { get; set; }
        public WindInfo Wind { get; set; }
        public string Name { get; set; }
    }

    public class MainInfo
    {
        public float Temp { get; set; }
        public float Feels_like { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
    }

    public class WeatherDescription
    {
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }
    public class WindInfo
    {
        public float Speed { get; set; }
    }
}
