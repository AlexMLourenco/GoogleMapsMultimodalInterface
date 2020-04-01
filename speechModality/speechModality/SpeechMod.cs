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

namespace speechModality
{
    public class SpeechMod
    {
        private SpeechRecognitionEngine sre;
        private Grammar gr;
        public event EventHandler<SpeechEventArg> Recognized;
        protected virtual void onRecognized(SpeechEventArg msg)
        {
            EventHandler<SpeechEventArg> handler = Recognized;
            if (handler != null)
            {
                handler(this, msg);
            }
        }

        private LifeCycleEvents lce;
        private MmiCommunication mmic;
        private Tts t;

        public SpeechMod()
        {
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

        private void Sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = false });
        }

        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            onRecognized(new SpeechEventArg(){Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true});

            //SEND
            // IMPORTANT TO KEEP THE FORMAT {"recognized":["SHAPE","COLOR"]}
            if (e.Result.Confidence < 0.5)
            {
                t.Speak("Desculpe não percebi. Repita por favor.");


            }
            else
            {
                //  {"destino": mcdonalds,
                //     "local": Forum aveiro
                //string json = "{ \"recognized\": [";
                string json = "{\n";
                foreach (var resultSemantic in e.Result.Semantics) { 
                    foreach (var key in resultSemantic.Value)
                    {
                        json += "\"" + key.Key + "\": " + "\"" + key.Value.Value + "\",\n ";
                        Console.WriteLine(key.Key);

                    }
                
                }
                json = json.Substring(0, json.Length - 1);
                json += "\n}";
                Console.WriteLine(json);
                dynamic tojson = JsonConvert.DeserializeObject(json);
                Console.WriteLine((string)tojson.ToString());




                // procurar mais perto
                //https://maps.googleapis.com/maps/api/place/findplacefromtext/json?input=continente&inputtype=textquery&fields=photos,formatted_address,name&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU
                // Direçoes
                string URL = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin=Universidade+Aveiro&destination={0}+Forum+Aveiro&key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU", (string)tojson.object.ToString());
                Console.WriteLine(URL);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                try
                {
                    string distancia = "";
                    WebResponse response = request.GetResponse();
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                        dynamic tojson2 = JsonConvert.DeserializeObject(reader.ReadToEnd());
                        //Console.Out.WriteLine((string)tojson2.routes[0].legs.ToString());
                        distancia = (string)tojson2.routes[0].legs[0].distance.value.ToString();
                        //return reader.ReadToEnd();
                        t.Speak(string.Format("O {0} fica a {1} metros da sua localização", (string)tojson.local.ToString(), distancia));
                    }
                }
                catch (WebException ex)
                {
                    WebResponse errorResponse = ex.Response;
                    using (Stream responseStream = errorResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                        String errorText = reader.ReadToEnd();
                        t.Speak("Algo de errado aconteceu");
                        // log errorText
                    }
                    throw;
                }
                var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime + "", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration) + "", e.Result.Confidence, json);
                mmic.Send(exNot);
                
            }
        }
    }
}
