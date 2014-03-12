using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Xml;
using Newtonsoft.Json;
using OpenWeatherMap;

namespace roman.Controllers
{
    public class WeatherController : ApiController
    {
        private const int CityId = 6293096; // Wädenswil, according to http://openweathermap.org/help/city_list.txt
        private const string Uri = "http://api.openweathermap.org/data/2.5/weather?id={0}";
        private const double KelvinOffset = 273.15;

        // GET api/weather
        [HttpGet]
        public HttpResponseMessage Get()
        {
            return GetWeather();
        }

        // GET api/weather/5
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            return GetWeather(id);
        }

        private static HttpResponseMessage GetWeather(int cityId = CityId)
        {
            var weatherUrl = string.Format(Uri, cityId);
            var client = new WebClient();
            var openWeatherMapApiKey = System.Configuration.ConfigurationManager.AppSettings["OpenWeatherMapApiKey"];
            client.Headers.Add("User-Agent", "Nobody");
            client.Headers.Add("x-api-key", openWeatherMapApiKey);
            try
            {
                var response = client.DownloadString(weatherUrl);
                var data = JsonConvert.DeserializeObject<CurrentCondition>(response);

                Color tempColor;
                var temp = (double?)data.main.temp - KelvinOffset;
                if (temp < -10.0)
                {
                    tempColor = Color.Purple;
                }
                else if (temp < 0.0)
                {
                    tempColor = Color.DarkBlue;
                }
                else if (temp < 5.0)
                {
                    tempColor = Color.Blue;
                }
                else if (temp < 10.0)
                {
                    tempColor = Color.LightBlue;
                }
                else if (temp < 15.0)
                {
                    tempColor = Color.DarkGreen;
                }
                else if (temp < 20.0)
                {
                    tempColor = Color.LightGreen;
                }
                else if (temp < 25.0)
                {
                    tempColor = Color.Yellow;
                }
                else if (temp < 30.0)
                {
                    tempColor = Color.Orange;
                }
                else if (temp < 35.0)
                {
                    tempColor = Color.Red;
                }
                else
                {
                    // over 35 deg celsius
                    tempColor = Color.DarkViolet;
                }

                var weatherDetail = data.weather.Count == 1
                    ? (WeatherDetail) data.weather[0].id
                    : WeatherDetail.Unknown;

                var weatherColor = GetWeatherColor(weatherDetail);

                var content = string.Format("W-[{0},{1},{2}][{3},{4},{5}]",
                    new object[]
                    {
                        tempColor.R.ToString("D3"), tempColor.G.ToString("D3"), tempColor.B.ToString("D3"),
                        weatherColor.R.ToString("D3"), weatherColor.G.ToString("D3"), weatherColor.B.ToString("D3")
                    })
                       +
                       string.Format("-Temp:{0}°C;{1}-Condition:{2};{3}", new object[]
                       {
                           String.Format("{0:0.0}", temp), tempColor.Name, weatherDetail.ToString(), weatherColor.Name
                       })
                    ;

                string appDataPath1 = System.Web.HttpContext.Current.Server.MapPath("~/app_data");
                string file1 = Path.Combine(appDataPath1, "LastWeather.xml");
                var xdoc1 = new XmlDocument();
                xdoc1.Load(file1);
                var rt = xdoc1.GetElementsByTagName("weather");
                rt[0].InnerXml = content;
                xdoc1.Save(file1);

                
                return new HttpResponseMessage
                {
                    Content = new StringContent(content,
                        Encoding.UTF8,
                        "text/html"
                    )
                };
            }
            catch (WebException e)
            {
                var appDataPath1 = System.Web.HttpContext.Current.Server.MapPath("~/app_data");
                var file1 = Path.Combine(appDataPath1, "LastWeather.xml");
                var xdoc1 = new XmlDocument();
                xdoc1.Load(file1);
                var rt = xdoc1.GetElementsByTagName("weather");
                var weather = rt[0].InnerXml.Trim();
                
                var msg = string.Format("{0} - {1}", weather,
                    string.Format("message: {0} / data: {1} / inner exception? {2}", e.Message, e.Data, 
                    (e.InnerException != null ? e.InnerException.Message : "no")));
                
                return new HttpResponseMessage
                {
                    Content = new StringContent(msg,
                        Encoding.UTF8,
                        "text/html"
                    )
                };
            }
            catch (Exception e)
            {
                var appDataPath1 = System.Web.HttpContext.Current.Server.MapPath("~/app_data");
                var file1 = Path.Combine(appDataPath1, "LastWeather.xml");
                var xdoc1 = new XmlDocument();
                xdoc1.Load(file1);
                var rt = xdoc1.GetElementsByTagName("weather");
                var weather = rt[0].InnerXml.Trim();
                
                var msg = string.Format("{0} - {1}", weather,
                    e.Message + (e.InnerException != null ? " / " + e.InnerException.Message : string.Empty));
                return new HttpResponseMessage
                {
                    Content = new StringContent(msg,
                        Encoding.UTF8,
                        "text/html"
                    )
                };
            }
        }

        private static Color GetWeatherColor(WeatherDetail weatherDetail)
        {
            Color weatherColor;

            switch (weatherDetail)
            {
                case WeatherDetail.ThunderstormWithLightRain: // = 200,
                case WeatherDetail.ThunderstormWithRain: // = 201,
                case WeatherDetail.ThunderstormWithHeavyRain: // = 202,
                case WeatherDetail.LightThunderstorm: // = 210,
                case WeatherDetail.Thunderstorm: // = 211,
                case WeatherDetail.HeavyThunderstorm: // = 212,
                case WeatherDetail.RaggedThunderstorm: // = 221,
                case WeatherDetail.ThunderstormWithLightDrizzle: // = 230,
                case WeatherDetail.ThunderstormWithDrizzle: // = 231,
                case WeatherDetail.ThunderstormWithHeavyDrizzle: // = 232,
                    weatherColor = Color.Purple;
                    break;

                case WeatherDetail.LightIntensityDrizzle: // = 300,
                case WeatherDetail.Drizzle: // = 301,
                case WeatherDetail.HeavyIntensityDrizzle: // = 302,
                case WeatherDetail.LightIntensityDrizzleRain: // = 310,
                case WeatherDetail.DrizzleRain: // = 311,
                case WeatherDetail.HeavyIntensityDrizzleRain: // = 312,
                case WeatherDetail.ShowerRainAndDrizzle: // = 313,
                case WeatherDetail.HeavyShowerRainAndDrizzle: // = 314,
                case WeatherDetail.ShowerDrizzle: // = 321,
                    weatherColor = Color.LightBlue;
                    break;

                case WeatherDetail.LightRain: // = 500,
                case WeatherDetail.ModerateRain: // = 501,
                case WeatherDetail.HeavyIntensityRain: // = 502,
                case WeatherDetail.VeryHeavyRain: // = 503,
                case WeatherDetail.ExtremeRain: // = 504,
                case WeatherDetail.FreezingRain: // = 511,
                case WeatherDetail.LightIntensityShowerRain: // = 520,
                case WeatherDetail.ShowerRain: // = 521,
                case WeatherDetail.HeavyIntensityShowerRain: // = 522,
                case WeatherDetail.RaggedShowerRain: // = 531,
                    weatherColor = Color.Blue;
                    break;

                case WeatherDetail.LightSnow: // = 600,
                case WeatherDetail.Snow: // = 601,
                case WeatherDetail.HeavySnow: // = 602,
                case WeatherDetail.Sleet: // = 611,
                case WeatherDetail.ShowerSleet: // = 612,
                case WeatherDetail.LightRainAndSnow: // = 615,
                case WeatherDetail.RainAndSnow: // = 616,
                case WeatherDetail.LightShowerSnow: // = 620,
                case WeatherDetail.ShowerSnow: // = 621,
                case WeatherDetail.HeavyShowerSnow: // = 622,
                    weatherColor = Color.White;
                    break;

                case WeatherDetail.Mist: // = 701,
                case WeatherDetail.Smoke: // = 711,
                case WeatherDetail.Haze: // = 721,
                case WeatherDetail.Fog: // = 741,
                case WeatherDetail.Dust: // = 761,
                    weatherColor = Color.Gray;
                    break;

                case WeatherDetail.SandDustWhirls: // = 731,
                case WeatherDetail.Sand: // = 751,
                case WeatherDetail.VolcanicAsh: // = 762,
                case WeatherDetail.Squalls: // = 771,
                case WeatherDetail.Tornado1: // = 781,
                case WeatherDetail.Tornado: // = 900,
                case WeatherDetail.TropicalStorm: // = 901,
                case WeatherDetail.Hurricane: // = 902,
                case WeatherDetail.Cold: // = 903,
                case WeatherDetail.Hot: // = 904,
                case WeatherDetail.Windy: // = 905,
                case WeatherDetail.Hail: // = 906,
                case WeatherDetail.Setting: // = 950,

                    weatherColor = Color.Red;
                    break;

                case WeatherDetail.SkyIsClear: // = 800,
                case WeatherDetail.FewClouds: // = 801,
                case WeatherDetail.Calm: // = 951,
                case WeatherDetail.LightBreeze: // = 952,
                case WeatherDetail.GentleBreeze: // = 953,
                    weatherColor = Color.LightGreen;
                    break;

                case WeatherDetail.ScatteredClouds: // = 802,
                case WeatherDetail.BrokenClouds: // = 803,
                case WeatherDetail.OvercastClouds: // = 804,
                case WeatherDetail.ModerateBreeze: // = 954,
                case WeatherDetail.FreshBreeze: // = 955,
                    weatherColor = Color.DarkGreen;
                    break;

                case WeatherDetail.StrongBreeze: // = 956,
                case WeatherDetail.HighWindNearGale: // = 957,
                case WeatherDetail.Gale: // = 958,
                case WeatherDetail.SevereGale: // = 959,
                case WeatherDetail.Storm: // = 960,
                case WeatherDetail.ViolentStorm: // = 961,
                case WeatherDetail.Hurricane2: // = 962
                    weatherColor = Color.Orange;
                    break;
                default:
                    weatherColor = Color.Magenta;
                    break;
            }
            return weatherColor;
        }
    }
}