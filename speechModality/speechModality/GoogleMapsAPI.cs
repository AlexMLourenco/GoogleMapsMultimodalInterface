using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace speechModality {
    class GoogleMapsAPI {
        CLocation myLocation = new CLocation();

        public String Nearby(String tojson, String service, String local, String mode, String location) {

            string speach = "";
            string URL = "";
            string identifier = "";
            string[] coords = myLocation.getCoords();

            Random rand = new Random();
            int id = rand.Next(1, 100);

            if (local == null) {
                if (location != null)
                    URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2}+{3}+" + "&mode=" + mode + "&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], service, location);
                else // Corrigir o +Aveiro
                    URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={3}+Aveiro" + "&mode=" + mode + "&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], service);
            }
            else {
                if (location != null) 
                    URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2}+{3}" + "&mode=" + mode + "&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], local, location);
                else // Corrigir o +Aveiro
                    URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2}+Aveiro" + "&mode=" + mode + "&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], local);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
           
            try {
                string distancia = "";
                string name = "";

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    dynamic tojson2 = JsonConvert.DeserializeObject(reader.ReadToEnd());
                    
                    // Getting real name of the associated id
                    identifier = (string)tojson2.geocoded_waypoints[1].place_id.ToString();
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/details/json?place_id={0}&fields=name,formatted_phone_number&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", identifier);
                    HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(URL);

                    WebResponse response2 = request2.GetResponse();
                    using (Stream responseStream2 = response2.GetResponseStream()) {
                        StreamReader reader2 = new StreamReader(responseStream2, System.Text.Encoding.UTF8);
                        dynamic tojson3 = JsonConvert.DeserializeObject(reader2.ReadToEnd());
                        name = (string)tojson3.result.name.ToString();
                    }

                    distancia = CheckDistance((string)tojson2.routes[0].legs[0].distance.value.ToString());
                    if (local == null) { speach = (string.Format("O {0} fica a {1} da sua localização", name, distancia)); }
                    else { speach = (string.Format("O {0} fica a {1} da sua localização", name, distancia)); }
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

        private String CheckDistance(String input) {
            String output = "";
            double distance = Int32.Parse(input);

            if (distance > 1000) {
                distance = (double)distance / (double)1000;
                output = (string.Format("{0} quilómetros", distance.ToString("0.##")));
            } else output = (string.Format("{0} metros", input));

            return output;
        }

        public String Translate(String input) {
            String output = "";

            if (input == "DRIVING") output = "carro";
            else if (input == "WALKING") output = "a caminhar";
            else if (input == "BICYCLING") output = "bicicleta";
            else if (input == "TRANSIT") output = "transportes plúblicos";

            return output;
        }

        public String GetInfo(String tojson, String service, String local, String infotype)  {

            string speach = "";
            string URL = "";

            string[] coords = myLocation.getCoords();


            if (local == null) {
                URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2}+Aveiro&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], service);
            } else {
                URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2}+Aveiro&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], local);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            try {
                string id = "";
                string distancia = "";
                string name = "";
                string phone = "";
                double rating = 0.0;

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    dynamic tojson2 = JsonConvert.DeserializeObject(reader.ReadToEnd());

                    // Getting real name of the id
                    id = (string)tojson2.geocoded_waypoints[1].place_id.ToString();
                    distancia = CheckDistance((string)tojson2.routes[0].legs[0].distance.value.ToString());
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/details/json?place_id={0}&fields=name,rating,formatted_phone_number&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", id);
                    HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(URL);

                    WebResponse response2 = request2.GetResponse();
                    using (Stream responseStream2 = response2.GetResponseStream()) {
                        StreamReader reader2 = new StreamReader(responseStream2, System.Text.Encoding.UTF8);
                        dynamic tojson3 = JsonConvert.DeserializeObject(reader2.ReadToEnd());
                        name = (string)tojson3.result.name.ToString();
                        phone = (string)tojson3.result.formatted_phone_number.ToString();
                        rating = (double)tojson3.result.rating;
                    }

                    if (infotype == "PHONE NUMBER") {
                        speach = (string.Format("O contacto telefónico do {0} é {1}", name, phone));
                    }
                    else if (infotype == "RATING") {
                        speach = (string.Format("O {0} tem {1} de rating", name, rating));
                    }
                    else if (infotype == "INFORMAÇÃO"){
                        speach = (string.Format("O contacto telefónico do {0} é {1} e tem um rating de {2}", name, phone, rating));
                    }
                    else if (infotype == "DISTANCIA") {
                        speach = (string.Format("O {0} fica a {1}", name, distancia));
                    }
                }
            } catch (WebException ex) {
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

        public string[] GetCoordinates(String spot) {
            double lat = 0.0000000, lng = 0.0000000;
            string URL = string.Format("https://maps.googleapis.com/maps/api/geocode/json?address={0}&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", spot);
            string[] coord = new string[2];

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            try {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    dynamic tojson2 = JsonConvert.DeserializeObject(reader.ReadToEnd());

                    // Getting latitude and longitude
                    lat = (double)tojson2.results[0].geometry.location.lat;
                    lng = (double)tojson2.results[0].geometry.location.lng;
                    coord[0] = (string)lat.ToString().Replace(',', '.');
                    coord[1] = (string)lng.ToString().Replace(',', '.');

                    Console.WriteLine(lat);
                    Console.WriteLine(lng);

                }
            } catch (WebException ex) {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    Console.WriteLine("Algo de errado aconteceu");
                }
                throw;
            }
            return coord;
        }

        public String GetClosestPlace(String tojson, String service,String local, String location) {
            // chamar distancia 
            string speach = "";
            int radius = 50;
            Boolean found = false;
            string URL = "";
            string[] coords = myLocation.getCoords();
            if (location == null) {
                if (local != null) {
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&name={2}&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], local);
                }
                else if (service != null) {
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&type={2}&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], service);
                }
            }
            else {
                if (local != null) {
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&name={2}+in+{3}&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], local, location);
                }
                else if (service != null){
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&type={2}+in+{3}&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], service, location);
                }
            }
           

            while (!found && radius < 1000) {
                string URL2 = URL + string.Format("&radius={0}", radius);
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL2);
                try {
     
                    WebResponse response = request.GetResponse();
                    using (Stream responseStream = response.GetResponseStream()) {
                        StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                        dynamic tojson2 = JsonConvert.DeserializeObject(reader.ReadToEnd());
                        if((string)tojson2.status.ToString() == "OK") {
                            speach = (string)tojson2.results[0].name.ToString();  
                            //(string.Format("O local mais próximo é o {0}", (string)tojson2.results[0].name.ToString()));
                            //Console.WriteLine("O local mais próximo é o {0}", (string)tojson2.results[0].name.ToString());
                            found = true;
                        } else {
                            //Console.WriteLine("aumentei" + radius);
                            radius += 100;
                            speach = "";// "Não foi encontrado nenhum resultado num raio de 1 quilometro da sua localização.";
                        }
                    }
                } catch (WebException ex) {
                    WebResponse errorResponse = ex.Response;
                    using (Stream responseStream = errorResponse.GetResponseStream()) {
                        StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                        String errorText = reader.ReadToEnd();
                        speach = ("Algo de errado aconteceu");
                    } throw;
                }
            }
            return speach;
        }

        //get closest spot counter
        public String GetClosestPlaceCounter(String tojson, String service, String local, String location) {

            string speach = "";
            string URL = "";

            string[] coords = myLocation.getCoords();

            if(location == null)
            {
                if(local != null)
                {
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&name={2}&radius=5000&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], local);
                }
                else if(service != null)
                {
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&type={2}&radius=5000&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], service);
                }
            }
            else
            {
                if (local != null)
                {
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&name={2}+in+{3}&radius=5000&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], local,location);
                }
                else if (service != null)
                {
                    URL = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&type={2}+in+{3}&radius=5000&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", coords[0], coords[1], service, location);
                }
            }
               

            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

            try {

                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    dynamic tojson2 = JsonConvert.DeserializeObject(reader.ReadToEnd());
                    JArray items = (JArray)tojson2.results;
                    int length = items.Count;
                    for (int i = 0; i < items.Count; i++)
                    {
                        
                        //do something with item
                    }

                    if ((string)tojson2.status.ToString() == "OK")
                        speach = (string.Format("Foram encontrados {0} locais para essa pesquisa num raio de 5 quilômentros.", (string)tojson2.results.Count.ToString()));
                    else
                    {
                        speach = "Não foi encontrado nenhum resultado num raio de 5 quilômetros.";
                    }
                }
            }
            catch (WebException ex) {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    speach = ("Algo de errado aconteceu");
                } throw;
            }
            return speach;
        }

        public void setLocation() { myLocation.GetLocationEvent(); }

        public string speak(string name )
        {
            String p = "";
            if(name != "")
            {
                p = (string.Format("O local mais próximo é o {0}", name));
            }
            else
            {
                p = "Não foi encontrado nenhum resultado num raio de 1 quilometro da sua localização.";
            }

            return p;
        }
    }
}
