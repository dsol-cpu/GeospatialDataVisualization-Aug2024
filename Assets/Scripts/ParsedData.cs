using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts
{
    public class ParsedData
    {
        private const decimal MAX_LATITUDE = 90;
        private const decimal MAX_LONGITUDE = 180;

        private const decimal ERR_LATITUDE = 91;
        private const decimal ERR_LONGITUDE = 181;

        public static List<Location> LoadCityData(string filename)
        {
            TextAsset dataAsset = Resources.Load<TextAsset>(filename);
            Debug.Log("Entered CityData File: "+ filename);
            if (dataAsset == null)
            {
                Debug.LogError("Could not find the data file in Resources.");
                return new List<Location>();
            }

            return ParseData(dataAsset.text);
        }

        static private (string, string) ParseCityInfo(string line)
        {
            line = Regex.Replace(line, @"^[^a-zA-Z0-9\u4E00-\u9FFF]+|[^a-zA-Z0-9\u4E00-\u9FFF]+$|['""]", "");
            List<string> parts = line.Split(',').Select(p => p.Trim()).ToList();
            
            if (parts.Count < 2 || string.IsNullOrWhiteSpace(parts[0]) || int.TryParse(parts[0], out _) || double.TryParse(parts[0], out _))
                return ("", "");

            return (parts[0], parts[1]);
        }

        static private (decimal, decimal) ParseCoordinates(string line)
        {
            var parts = line.Split(',').Select(p => p.Trim()).ToList();
            if (parts.Count < 2)
                return (ERR_LATITUDE, ERR_LONGITUDE);

            if (decimal.TryParse(parts[0], out decimal lat) && decimal.TryParse(parts[1], out decimal lon))
                return (lat, lon);

            return (ERR_LATITUDE, ERR_LONGITUDE);
        }

        public static List<Location> ParseData(string fileContent)
        {
            List<Location> locations = new List<Location>();
            Location location = new Location();

            string[] lines = fileContent.Split('\n');
            int length = lines.Length;
            bool found_city = false;

            for (int i = 0; i < length; ++i)
            {
                if (!found_city)
                {
                    var city_info = ParseCityInfo(lines[i]);
                    if (city_info.Item1 == "")
                        continue;
                    location.City = city_info.Item1;
                    location.Country = city_info.Item2;
                    found_city = true;
                }
                else
                {
                    var coordinate_info = ParseCoordinates(lines[i]);
                    if (coordinate_info.Item1 > MAX_LATITUDE || coordinate_info.Item2 > MAX_LONGITUDE)
                        continue;

                    location.Latitude = coordinate_info.Item1;
                    location.Longitude = coordinate_info.Item2;

                    locations.Add(location);
                    Debug.Log(location);
                    location = new Location();

                    found_city = false;
                }
            }
            return locations;
        }
    }
}