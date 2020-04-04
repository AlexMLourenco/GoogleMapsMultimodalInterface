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

        private String mode = "driving";
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
                //System.Threading.Thread.Sleep(2000);
            } else {
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

                if (tojson.wake != null) wake = true;

                if (wake) {
                    if (json.Split(new string[] { "action" }, StringSplitOptions.None).Length > 3) {
                        t.Speak("Utilize só um comando de cada vez.");
                    } else {
                        App.Current.Dispatcher.Invoke(() => {
                            if (tojson.action != null) {
                                switch ((string)tojson.action.ToString()) {

                                    case "SEARCH":
                                        if ((string)tojson.info == null) {
                                            if ((string)tojson.service != null) {
                                                // Restaurante, Bar, Cafe, Padaria, Hotel, PSP, CGD
                                                if ((string)tojson.location != null)
                                                    t.Speak(api.Nearby((string)tojson.ToString(), (string)tojson.service.ToString(), null, mode, (string)tojson.location.ToString()));
                                                else t.Speak(api.Nearby((string)tojson.ToString(), (string)tojson.service.ToString(), null, mode, null));
                                            } else if ((string)tojson.local != null) {
                                                // McDonalds, Continente, Forum, Glicinias, Altice, Ria
                                                if ((string)tojson.location != null)
                                                    t.Speak(api.Nearby((string)tojson.ToString(), null, (string)tojson.local.ToString(), mode, (string)tojson.location.ToString()));
                                                else
                                                    t.Speak(api.Nearby((string)tojson.ToString(), null, (string)tojson.local.ToString(), mode, null));
                                            }
                                        } else {
                                            if ((string)tojson.service != null) {
                                                t.Speak(api.GetInfo((string)tojson.ToString(), (string)tojson.service.ToString(), null, (string)tojson.info.ToString()));
                                            } else if ((string)tojson.local != null) {
                                                if ((string)tojson.location != null)
                                                    t.Speak(api.GetInfo((string)tojson.ToString(), null, (string)tojson.local.ToString(), (string)tojson.info.ToString()));
                                                else
                                                    t.Speak(api.GetInfo((string)tojson.ToString(), null, (string)tojson.local.ToString(), null));
                                            }
                                        }
                                        break;

                                    case "CHANGE":
                                        if (tojson.subaction == "TRANSPORTE") {
                                            if ((string)tojson.transport != null) {
                                                // Default: carro; Others: pé, bicicleta, metro, comboio, transportes publicos
                                                mode = api.Translate((string)tojson.transport.ToString());
                                                t.Speak(string.Format("Modo de transporte alterado para {0}", (string)tojson.transport.ToString()));
                                            }
                                            else t.Speak("Peço desculpa, não entendi o meio de transporte.");

                                        } else if (tojson.subaction == "ORIGEM") {
                                            t.Speak("Não me perguntes isso ainda. Burro, ainda não sou assim tão avançado");
                                        }
                                        else t.Speak("Peço desculpa, não entendi a origem de partida pretendida.");
                                        break;

                                    case "SHUTDOWN":
                                        System.Environment.Exit(1);
                                        break;
                                }
                            } else { t.Speak("Olá! Como posso ajudar?"); }
                        });
                        var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime + "", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration) + "", e.Result.Confidence, json);
                        mmic.Send(exNot);
                    }
                }
            }
        }
    }
}
