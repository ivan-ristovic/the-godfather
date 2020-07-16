#region USING_DIRECTIVES

using System.Collections.Generic;
using Newtonsoft.Json;
#endregion

namespace TheGodfather.Modules.Search.Common
{
    public class Coord
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class City
    {
        public Coord Coord { get; set; }
        public string Country { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Weather
    {
        public string Description { get; set; }
        public string Icon { get; set; }
        public int Id { get; set; }
        public string Main { get; set; }
    }

    public class Main
    {
        public double Temp { get; set; }
        public float Pressure { get; set; }
        public float Humidity { get; set; }

        [JsonProperty("temp_min")]
        public double TempMin { get; set; }

        [JsonProperty("temp_max")]
        public double TempMax { get; set; }
    }

    public class Wind
    {
        public double Speed { get; set; }
        public double Deg { get; set; }
    }

    public class Clouds
    {
        public int All { get; set; }
    }

    public class Sys
    {
        public int Type { get; set; }
        public int Id { get; set; }
        public double Message { get; set; }
        public string Country { get; set; }
        public double Sunrise { get; set; }
        public double Sunset { get; set; }
    }

    public class WeatherData
    {
        public Coord Coord { get; set; }
        public List<Weather> Weather { get; set; }
        public Main Main { get; set; }
        public int Visibility { get; set; }
        public Wind Wind { get; set; }
        public Clouds Clouds { get; set; }
        public int Dt { get; set; }
        public Sys Sys { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cod { get; set; }
    }

    public class PartialWeatherData
    {
        public List<Weather> Weather { get; set; }
        public Main Main { get; set; }
        public int Visibility { get; set; }
        public Wind Wind { get; set; }
        public Clouds Clouds { get; set; }
        public int Dt { get; set; }
        public Sys Sys { get; set; }
        public int Id { get; set; }
        public int Cod { get; set; }
    }

    public class Forecast
    {
        public City City { get; set; }

        [JsonProperty("list")]
        public List<PartialWeatherData> WeatherDataList { get; set; }
    }
}
