using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;

using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.IO;

namespace AppGui
{
    class CLocation
    {
        GeoCoordinateWatcher watcher;
        private string[] coordinates;
        public string GetLocationEvent()
        {
            this.watcher = new GeoCoordinateWatcher();
            this.watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            bool started = this.watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            if (!started)
            {
                Console.WriteLine("GeoCoordinateWatcher timed out on start.");
            }
            return "ok";
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            PrintPosition(e.Position.Location.Latitude, e.Position.Location.Longitude);
        }

        void PrintPosition(double Latitude, double Longitude)
        {
            string[] coord = { (string)Latitude.ToString().Replace(',', '.'), (string)Longitude.ToString().Replace(',', '.') };
            this.coordinates = coord;
            Console.WriteLine("Latitude: {0}, Longitude {1}", coord[0], coord[1]);
        }
        public string[] getCoords()
        {
            return this.coordinates;

        }
        public string GetCoordinates(string[] coord)
        {   
            string URL = string.Format("https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coord[0], coord[1]);
            string street = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    dynamic tojson2 = JsonConvert.DeserializeObject(reader.ReadToEnd());

                    // Getting latitude and longitude
                     street = (string)tojson2.results[0].formatted_address.ToString();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    Console.WriteLine("Algo de errado aconteceu");
                }
                throw;
            }
            return street;
        }
    }
}
