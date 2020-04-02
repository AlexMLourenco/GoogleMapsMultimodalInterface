using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmisharp;
using Microsoft.Speech.Recognition;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.IO;

namespace speechModality {
    public class SpeechMod {
        private SpeechRecognitionEngine sre;
        private Grammar gr;
        public event EventHandler<SpeechEventArg> Recognized;
        protected virtual void onRecognized(SpeechEventArg msg) {
            EventHandler<SpeechEventArg> handler = Recognized;
            if (handler != null) {
                handler(this, msg);
            }
        }

        private LifeCycleEvents lce;
        private MmiCommunication mmic;
        private Tts t;

        public SpeechMod() {
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("ASR", "FUSION","speech-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            //mmic = new MmiCommunication("localhost",9876,"User1", "ASR");  //PORT TO FUSION - uncomment this line to work with fusion later
            mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)

            mmic.Send(lce.NewContextRequest());

            //load pt recognizer
            sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
            gr = new Grammar(Environment.CurrentDirectory + "\\ptG.grxml", "rootRule");
            sre.LoadGrammar(gr);

            t = new Tts();
            sre.SetInputToDefaultAudioDevice();
            sre.RecognizeAsync(RecognizeMode.Multiple);
            sre.SpeechRecognized += Sre_SpeechRecognized;
            sre.SpeechHypothesized += Sre_SpeechHypothesized;

        }

        private void Sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e) {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = false });
        }

        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            onRecognized(new SpeechEventArg(){Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true});

            if (e.Result.Confidence < 0.5) {
                t.Speak("Desculpe não percebi. Repita por favor.");
            } else {
                //  {"destino": mcdonalds,
                //     "local": Forum aveiro
                //string json = "{ \"recognized\": [";
                string json = "{\n";
                foreach (var resultSemantic in e.Result.Semantics) { 
                    foreach (var key in resultSemantic.Value) {
                        json += "\"" + key.Key + "\": " + "\"" + key.Value.Value + "\",\n ";
                        Console.WriteLine(key.Key);
                    }
                }
                json = json.Substring(0, json.Length - 1);
                json += "\n}";
                Console.WriteLine(json);
                dynamic tojson = JsonConvert.DeserializeObject(json);
                Console.WriteLine(tojson);

                if (json.Split(new string[] { "action" }, StringSplitOptions.None).Length > 2) {
                    t.Speak("Utilize só um comando de cada vez.");
                }
                else {
                    App.Current.Dispatcher.Invoke(() => {
                        if (tojson.action != null) {
                            // Processamento do comando
                            switch ((string)tojson.action.ToString()) {
                                case "SEARCH":
                                    if ((string)tojson.service != null) {
                                        if ((string)tojson.service.ToString() == "RESTAURANTE")
                                            Nearby((string)tojson.ToString(), (string)tojson.service.ToString(), null);
                                        else if ((string)tojson.service.ToString() == "BAR")
                                            Nearby((string)tojson.ToString(), (string)tojson.service.ToString(), null);
                                        else if ((string)tojson.service.ToString() == "CAFÉ")
                                            Nearby((string)tojson.ToString(), (string)tojson.service.ToString(), null);
                                        else if ((string)tojson.service.ToString() == "HOTEL")
                                            Nearby((string)tojson.ToString(), (string)tojson.service.ToString(), null);
                                        else if ((string)tojson.service.ToString() == "PSP")
                                            Nearby((string)tojson.ToString(), (string)tojson.service.ToString(), null);
                                        else if ((string)tojson.service.ToString() == "CGD")
                                            Nearby((string)tojson.ToString(), (string)tojson.service.ToString(), null);

                                    } else if ((string)tojson.local != null ) {
                                        if ((string)tojson.local.ToString() == "MCDONALD'S")
                                            Nearby((string)tojson.ToString(), null, (string)tojson.local.ToString());
                                        else if ((string)tojson.local.ToString() == "CONTINENTE")
                                            Nearby((string)tojson.ToString(), null, (string)tojson.local.ToString());
                                        else if ((string)tojson.local.ToString() == "FORUM")
                                            Nearby((string)tojson.ToString(), null, (string)tojson.local.ToString());
                                        else if ((string)tojson.local.ToString() == "GLICINIAS")
                                            Nearby((string)tojson.ToString(), null, (string)tojson.local.ToString());
                                        else if ((string)tojson.local.ToString() == "ALTICE")
                                            Nearby((string)tojson.ToString(), null, (string)tojson.local.ToString());
                                        else if ((string)tojson.local.ToString() == "RIA")
                                            Nearby((string)tojson.ToString(), null, (string)tojson.local.ToString());
                                    }
                                    break;
                            }
                        } else {  t.Speak("Olá"); }
                    });

                    var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime + "", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration) + "", e.Result.Confidence, json);
                    mmic.Send(exNot);
                }   
            }
        }

        private void Nearby(String tojson, String service, String local) {

            string URL = "";
            if (local == null) {
                Random rand = new Random();
                int id = rand.Next(1, 100);
                URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin=Universidade+Aveiro&destination={0}+Aveiro+" + id + "&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", service);
            } else {
                URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin=Universidade+Aveiro&destination={0}+Aveiro&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", local);
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
                        t.Speak(string.Format("O {0} fica a {1} metros da sua localização", service, distancia));
                    } else {
                        t.Speak(string.Format("O {0} fica a {1} metros da sua localização", local, distancia));
                    }
                }
            } catch (WebException ex) {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    t.Speak("Algo de errado aconteceu");
                    // log errorText
                } throw;
            }
     
        }

    }
}
