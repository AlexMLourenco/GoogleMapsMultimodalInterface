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
            string identifier = "";
            if (local == null) {
                Random rand = new Random();
                int id = rand.Next(1, 100);
                URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin=Universidade+Aveiro&destination={0}+Aveiro+" + id + "&mode=" + mode + "&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", service);
            }
            else {
                URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin=Universidade+Aveiro&destination={0}+Aveiro" + "&mode=" + mode + "&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", local);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            try {
                string distancia = "";
                string name = "";

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    dynamic tojson2 = JsonConvert.DeserializeObject(reader.ReadToEnd());

                    // Getting real name of the id
                    identifier = (string)tojson2.geocoded_waypoints[1].place_id.ToString();
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/details/json?place_id={0}&fields=name,formatted_phone_number&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", identifier);
                    HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(URL);

                    WebResponse response2 = request2.GetResponse();
                    using (Stream responseStream2 = response2.GetResponseStream()) {
                        StreamReader reader2 = new StreamReader(responseStream2, System.Text.Encoding.UTF8);
                        dynamic tojson3 = JsonConvert.DeserializeObject(reader2.ReadToEnd());
                        name = (string)tojson3.result.name.ToString();
                    }
                    
                    distancia = (string)tojson2.routes[0].legs[0].distance.value.ToString();

                    if (local == null) { speach = (string.Format("O {0} fica a {1} metros da sua localização", name, distancia)); }
                    else { speach = (string.Format("O {0} fica a {1} metros da sua localização", name, distancia)); }
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

        public String Translate(String input) {
            String output = "";

            if (input == "CARRO") output = "driving";
            else if (input == "A PÉ") output = "walking";
            else if (input == "BICICLETA") output = "bicycling";
            else if (input == "TRANSPORTES PÚBLICOS") output = "transit";

            return output;
        }

        public String GetInfo(String tojson, String service, String local)  {

            string speach = "";
            string URL = "";
            if (local == null) {
                URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin=Universidade+Aveiro&destination={0}+Aveiro&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", service);
            }
            else {
                URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin=Universidade+Aveiro&destination={0}+Aveiro&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", local);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            try {
                string id = "";
                string name = "";
                string phone = "";

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    dynamic tojson2 = JsonConvert.DeserializeObject(reader.ReadToEnd());

                    // Getting real name of the id
                    id = (string)tojson2.geocoded_waypoints[1].place_id.ToString();
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/details/json?place_id={0}&fields=name,rating,formatted_phone_number&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", id);
                    HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(URL);

                    WebResponse response2 = request2.GetResponse();
                    using (Stream responseStream2 = response2.GetResponseStream()) {
                        StreamReader reader2 = new StreamReader(responseStream2, System.Text.Encoding.UTF8);
                        dynamic tojson3 = JsonConvert.DeserializeObject(reader2.ReadToEnd());
                        name = (string)tojson3.result.name.ToString();
                        phone = (string)tojson3.result.formatted_phone_number.ToString();
                    }

                    if (local == null) { speach = (string.Format("O contacto telefónico do {0} é {1}", name, phone)); }
                    else { speach = (string.Format("O contacto telefónico do {0} é {1}", name, phone)); }
                }
            }
            catch (WebException ex) {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
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
