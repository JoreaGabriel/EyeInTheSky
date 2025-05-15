using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeInTheSky.ClassLibrary
{
    public class WeatherForecast
    {
        public List<ForecastItem> list { get; set; }
    }   
    public class ForecastItem
    {
        public long dt { get; set; }
        public Main main { get; set; }
        public List<Weather> weather { get; set; }

    }
    public class Weather
    {
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
    public class Main
    {
        public double temp { get; set; }        
        public int pressure { get; set; }        
        public int humidity { get; set; }        
    }
    public class HourlyForecastDisplay
    {
        public string Hour { get; set; }
        public string Temp { get; set; }        
        public string Pressure { get; set; }
        public string Humidity { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
    }
}
