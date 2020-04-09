using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using DotNetBrowser;
using DotNetBrowser.WinForms;
using System.Windows.Forms;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;

using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Windows.Threading;
using Keys = OpenQA.Selenium.Keys;

namespace testMaps
{
    public partial class Form1 : Form
    {
        private MmiCommunication mmiC;
        IWebDriver googledriver = new ChromeDriver(@"C:\Users\manel\Desktop\IM\Projeto\GoogleMapsMultimodalInterface");
        private String startupPath = Environment.CurrentDirectory;
        private String path = "";
        private browserCommands command;
        private string[] coords = new string[2];
        private string street = "";
        private CLocation coord = new CLocation();
        private string URL = "";
        private Boolean flag = false;
        dynamic json;
        
        public Form1() {
            InitializeComponent();
            this.coord.GetLocationEvent();
            button1.Click += new EventHandler(button1_Click);
            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            command = new browserCommands();
            mmiC.Message += MmiC_Message;
            
            mmiC.Start();
        }

        private void Form1_Load(object sender, EventArgs e) {
            path = startupPath + "/../../html/googleMaps.html";
            webBrowser1.Navigate(new System.Uri(@"file:///"+path));
            googledriver.Navigate().GoToUrl("https://www.google.pt/maps");
            //googledriver.Navigate().Back();
            //id = "searchboxinput"



        }

        private void button1_Click(object sender, EventArgs e) {
            /*
            if(this.flag == false) {
                this.coords = this.coord.getCoords();
                string street = this.coord.GetCoordinates(this.coords);
                this.URL = "https://www.google.com/maps/embed/v1/place?key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU&q=" + street;
                Console.WriteLine(this.URL);
                this.street = street;
                this.flag = true;
            }
            */

            /*
            HtmlDocument html = this.webBrowser1.Document; 
            HtmlElementCollection doc = html.GetElementsByTagName("iframe").GetElementsByName("iframe1");//.SetAttribute("src", "ola");
            foreach (HtmlElement elem in doc){
                elem.SetAttribute("src", this.URL);    
            }   
            */
           
        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {

            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                //

                Console.WriteLine(e.Message);
                var doc = XDocument.Parse(e.Message);
                var com = doc.Descendants("command").FirstOrDefault().Value;
                dynamic json = JsonConvert.DeserializeObject(com);
                Console.WriteLine((string)json.ToString());
                if (json.action != null)
                {
                    switch ((string)json.action.ToString())
                    {
                        case "SEARCH":
                            this.json = json;
                            command.searchLocation(googledriver, coord, json);
                            break;
                        case "DIRECTIONS":
                            command.getDirections(googledriver, coord,json);
                            break;
                    }
                }
            });
            
        }
        public void setWebBrowser(dynamic json) {
            string tmpURL = "";
            if ((string)json.action.ToString() == "SEARCH") {
                tmpURL += "https://www.google.com/maps/embed/v1/search?key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU";
                if ((string)json.service != null) {
                    tmpURL += "&q=" + (string)json.service.ToString();
                    if ((string)json.location != null) {
                        tmpURL += "+in+"+ (string)json.location.ToString();
                    }
                } else if ((string)json.local != null) {
                    tmpURL += "&q=" + (string)json.local.ToString();
                    if ((string)json.location != null)
                        tmpURL += "+in+" + (string)json.location.ToString();
                }
                this.URL = tmpURL;    
            } else if ((string)json.action.ToString() == "DIRECTIONS") {
                tmpURL += "https://www.google.com/maps/embed/v1/directions?key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU&origin=40.637906,-8.637598&mode="+ (string)json.mode.ToString();
                if ((string)json.service != null) {
                    tmpURL += "&destination=" + (string)json.service.ToString();
                    if ((string)json.location != null)
                        tmpURL += "+in+" + (string)json.location.ToString();
                } else if ((string)json.local != null) {
                    tmpURL += "&destination=" + (string)json.local.ToString();
                    if ((string)json.location != null)
                        tmpURL += "+in+" + (string)json.location.ToString();
                }
                this.URL = tmpURL;
            }

        }  
    }
}
