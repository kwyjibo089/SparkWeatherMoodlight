using System.Collections.Generic;

namespace OpenWeatherMap
{
    public class RootObject
    {
        public string cod { get; set; }
        public City city { get; set; }
        public int cnt { get; set; }
        public string model { get; set; }
        public List<List> list { get; set; }
    }
}