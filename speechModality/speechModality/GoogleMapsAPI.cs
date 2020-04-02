using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.IO;

namespace speechModality {
    class GoogleMapsAPI {

        public String Nearby(String tojson, String service, String local, String mode) {

            string speach = "";
            string URL = "";
            if (local == null) {
                Random rand = new Random();
                int id = rand.Next(1, 100);
                URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin=Universidade+Aveiro&destination={0}+Aveiro+" + id + "&mode=" + mode + "&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", service);
            }
            else {
                URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin=Universidade+Aveiro&destination={0}+Aveiro" + "&mode=" + mode + "&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", local);
            }
            Console.WriteLine(URL);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            try {
                string distancia = "";
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    dynamic tojson2 = JsonConvert.DeserializeObject(reader.ReadToEnd());
                    distancia = (string)tojson2.routes[0].legs[0].distance.value.ToString();
                    //return reader.ReadToEnd();
                    if (local == null) {
                        speach = (string.Format("O {0} fica a {1} metros da sua localização", service, distancia));
                    }
                    else {
                        speach = (string.Format("O {0} fica a {1} metros da sua localização", local, distancia));
                    }
                }
            }
            catch (WebException ex) {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    speach = ("Algo de errado aconteceu");
                }
                throw;
            }
            return speach;
        }

    }
}
