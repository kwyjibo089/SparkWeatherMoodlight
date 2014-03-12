using System.Collections.Generic;

namespace OpenWeatherMap
{
    public class List
    {
        public int dt { get; set; }
        public double temp { get; set; }
        public double night { get; set; }
        public double eve { get; set; }
        public double morn { get; set; }
        public double pressure { get; set; }
        public double humidity { get; set; }
        public List<Weather> weather { get; set; }
        public double speed { get; set; }
        public int deg { get; set; }
    }
}