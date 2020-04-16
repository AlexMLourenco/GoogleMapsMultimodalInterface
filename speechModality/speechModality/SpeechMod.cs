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
            if (handler != null) { handler(this, msg); }
        }

        private LifeCycleEvents lce;
        private MmiCommunication mmic;
        private Tts t;

        private String mode = "DRIVING";
        private GoogleMapsAPI api = new GoogleMapsAPI();
        private bool wake = false;

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

            
            sre.SetInputToDefaultAudioDevice();
            sre.RecognizeAsync(RecognizeMode.Multiple);
            sre.SpeechRecognized += Sre_SpeechRecognized;
            sre.SpeechHypothesized += Sre_SpeechHypothesized;
            t = new Tts(sre);
        }

        private void Sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e) {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = false });
        }

        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            onRecognized(new SpeechEventArg(){Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true});
            
            if (e.Result.Confidence < 0.5) {
                t.Speak("Desculpe não percebi. Repita por favor.");
                //System.Threading.Thread.Sleep(2000);
            } else {
                string json = "{\n";
                foreach (var resultSemantic in e.Result.Semantics) { 
                    foreach (var key in resultSemantic.Value) {
                        json += "\"" + key.Key + "\": " + "\"" + key.Value.Value + "\",\n ";
                        
                    }
                }
                json += "\"" + "mode" + "\": " + "\"" + this.mode + "\",\n ";
                json = json.Substring(0, json.Length - 1);
                json += "\n}";
                
                dynamic tojson = JsonConvert.DeserializeObject(json);
                

                if (tojson.wake != null) {
                    wake = true;
                    // Get my atual location in the beggining
                    api.setLocation();
                }
                String name = "";
                if (wake) {
                    if (json.Split(new string[] { "action" }, StringSplitOptions.None).Length > 3) {
                        t.Speak("Utilize só um comando de cada vez.");
                    } else {
                        //App.Current.Dispatcher.Invoke(() => {
                            if (tojson.action != null) {
                            switch ((string)tojson.action.ToString()) {
                                    case "SEARCH":

                                    // Get information about a location or service
                                        if ((string)tojson.info != null) {
                                            if ((string)tojson.service != null) {
                                                t.Speak(api.GetInfo((string)tojson.ToString(), (string)tojson.service.ToString(), null, (string)tojson.info));
                                                Console.WriteLine(api.GetInfo((string)tojson.ToString(), (string)tojson.service.ToString(), null, (string)tojson.info));
                                            }
                                            else if ((string)tojson.local != null) {
                                                t.Speak(api.GetInfo((string)tojson.ToString(), null, (string)tojson.local.ToString(), (string)tojson.info));
                                                Console.WriteLine(api.GetInfo((string)tojson.ToString(), null, (string)tojson.local.ToString(), (string)tojson.info));
                                            }
                                        }

                                        // Search by restaurant, bar, coffee shop, police, amoung others
                                        else if ((string)tojson.service != null) {
                                        // When the input contains an location like, Aveiro, Coimbra, Lisbon
                                            if ((string)tojson.location != null)
                                            {
                                                // Get the closest input for that location (restaurant, coffee shop, etc)
                                                if ((string)tojson.nearby != null)
                                                {  // If there's no one in one km it return a message

                                                    name = api.GetClosestPlace((string)tojson.ToString(), (string)tojson.service.ToString(), null, (string)tojson.location.ToString());
                                                    t.Speak(api.speak(name));
                                                    Console.WriteLine(api.speak(name));
                                                }
                                                // Number of inputs(restaurants, coffee, etc) in a radious of 5km from that location
                                                else { 
                                                    t.Speak(api.GetClosestPlaceCounter((string)tojson.ToString(), (string)tojson.service.ToString(), null, (string)tojson.location.ToString()));
                                                    t.Speak(api.speak(name));
                                                    Console.WriteLine(api.speak(name));
                                                }

                                            } // When the input DOESN'T contain an location (use coordinates)
                                            else {
                                            // Get the closest input using coordinates (restaurant, coffee shop, etc)
                                                if ((string)tojson.nearby != null)
                                                {
                                                    name = (api.GetClosestPlace((string)tojson.ToString(), (string)tojson.service.ToString(), null, null));
                                                    t.Speak(api.speak(name));
                                                    Console.WriteLine(api.speak(name));
                                                }
                                                // Number of inputs (restaurants, coffee, etc) in a radious of 5km from that coordinates
                                                else
                                                    t.Speak(api.GetClosestPlaceCounter((string)tojson.ToString(), (string)tojson.service.ToString(), null, null));
                                                    Console.WriteLine(api.GetClosestPlaceCounter((string)tojson.ToString(), (string)tojson.service.ToString(), null, null));
                                                }
                                            if (name != "")
                                            {
                                                tojson.service = name;
                                                json = JsonConvert.SerializeObject(tojson);
                                            }
                                        }

                                        // Search by "store" like McDonald's, Forum, Altice, amoung others
                                        else if ((string)tojson.local != null) {
                                            // When the input contains an location like, Aveiro, Coimbra, Lisbon
                                            if ((string)tojson.location != null) {
                                            // Get the closest input for that location (McDonald's, Forum, etc)
                                                if ((string)tojson.nearby != null)
                                                {
                                                    name = (api.GetClosestPlace((string)tojson.ToString(), null, (string)tojson.local.ToString(), (string)tojson.location.ToString()));
                                                    t.Speak(api.speak(name));
                                                    Console.WriteLine(api.speak(name));
                                                }
                                                // Number of inputs(McDonald's, Forum, etc) in a radious of 5km from that location
                                                else
                                                {
                                                    t.Speak(api.GetClosestPlaceCounter((string)tojson.ToString(), null, (string)tojson.local.ToString(), (string)tojson.location.ToString()));
                                                    Console.WriteLine(api.GetClosestPlaceCounter((string)tojson.ToString(), null, (string)tojson.local.ToString(), (string)tojson.location.ToString()));

                                                }
                                            } // When the input DOESN'T contain an location (use coordinates)
                                            else {
                                            // Get the closest input using coordinates (McDonald's, Forum, etc)
                                                if ((string)tojson.nearby != null)
                                                {
                                                    name = (api.GetClosestPlace((string)tojson.ToString(), null, (string)tojson.local.ToString(), null));
                                                    t.Speak(api.speak(name));
                                                    Console.WriteLine(api.speak(name));
                                                }

                                                // Number of inputs (McDonald's, Forum, etc) in a radious of 5km from that coordinates
                                                else
                                                    t.Speak(api.GetClosestPlaceCounter((string)tojson.ToString(), null, (string)tojson.local.ToString(), null));
                                                    Console.WriteLine(api.GetClosestPlaceCounter((string)tojson.ToString(), null, (string)tojson.local.ToString(), null));
                                        }
                                            if (name != ""){
                                                tojson.local = name;
                                                json = JsonConvert.SerializeObject(tojson);
                                            }
                                        }
                                        
                                        break;

                                    case "MORE":    // More zoom

                                        if ((string)tojson.zoom != null)
                                            t.Speak("Aumentei zoom");
                                            Console.WriteLine("Aumentei o zoom");
                                        break;

                                    case "LESS":    // Less zoom

                                        if ((string)tojson.zoom != null)

                                            t.Speak("Diminui zoom");
                                            Console.WriteLine("Diminui o zoom");
                                        break;

                                    case "CHANGE":

                                        if ((string)tojson.view != null)
                                            t.Speak("Modo de visualização alterado com sucesso");

                                        else if (tojson.subaction == "TRANSPORTE") {
                                            if ((string)tojson.transport != null) {
                                                // Default: carro; Others: pé, bicicleta, metro, comboio, transportes publicos
                                                mode = (string)tojson.transport.ToString();
                                                t.Speak(string.Format("Modo de transporte alterado para {0}", api.Translate(mode)));
                                                Console.WriteLine("Modo de transporte alterado para {0}", api.Translate(mode));
                                            }
                                            else t.Speak("Peço desculpa, não entendi o meio de transporte.");
                                        }
                                        else t.Speak("Peço desculpa, não entendi o que pertende alterar.");

                                        break;

                                    case "DIRECTIONS":

                                        if ((string)tojson.service != null) {
                                        // Restaurante, Bar, Cafe, Padaria, Hotel, PSP, CGD
                                            if ((string)tojson.location != null)
                                            {
                                                t.Speak("Foram pedidas direcções para " + (string)tojson.service.ToString() + " em " + (string)tojson.location.ToString());
                                                Console.WriteLine("Foram pedidas direcções para " + (string)tojson.service.ToString() + " em " + (string)tojson.location.ToString());
                                            }
                                            else
                                            {
                                                t.Speak("Foram pedidas direcções para " + (string)tojson.service.ToString());
                                                Console.WriteLine("Foram pedidas direcções para " + (string)tojson.service.ToString());
                                            }
                                        }
                                        else if ((string)tojson.local != null) {
                                        // McDonalds, Continente, Forum, Glicinias, Altice, Ria
                                            if ((string)tojson.location != null)
                                            {
                                                t.Speak("Foram pedidas direcções para " + (string)tojson.local.ToString() + " em " + (string)tojson.location.ToString());
                                                Console.WriteLine("Foram pedidas direcções para " + (string)tojson.local.ToString() + " em " + (string)tojson.location.ToString());
                                            }
                                            else
                                            {
                                                t.Speak("Foram pedidas direcções para " + (string)tojson.local.ToString());
                                                Console.WriteLine("Foram pedidas direcções para " + (string)tojson.local.ToString());
                                            }
                                        }
                                        break;

                                    case "SHUTDOWN":

                                            System.Environment.Exit(1);
                                            break;
                                    }

                                } else {
                                    //sre.RecognizeAsyncStop();
                                    t.Speak("Olá! Como posso ajudar?");
                                    Console.WriteLine("Olá! Como posso ajudar?");
                        }
                        //});
                        var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime + "", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration) + "", e.Result.Confidence, json);
                        //Console.WriteLine((string)exNot.ToString());
                        mmic.Send(exNot);
                    }
                }
            }
        }
    }
}
