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



namespace testMaps
{
    public partial class Form1 : Form
    {
        private MmiCommunication mmiC;
        private string URL = "";
        private int _myProperty = 0;
        public Form1()
        {
            InitializeComponent();
            button1.Click += new EventHandler(button1_Click);
            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;

            mmiC.Start();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
 
            webBrowser1.Navigate(new System.Uri(@"file:///C:/Users/manel/Desktop/IM/testGoogle/testMaps/html/googleMaps.html"));
           


        }

        private void button1_Click(object sender, EventArgs e)
        {
            HtmlDocument html = this.webBrowser1.Document;
            
            HtmlElementCollection doc = html.GetElementsByTagName("iframe").GetElementsByName("iframe1");//.SetAttribute("src", "ola");
            foreach (HtmlElement elem in doc)
            {
                elem.SetAttribute("src", this.URL);
                
            }
        }
        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);
            Console.WriteLine((string)json.ToString());
            if (json.action != null)
            {
                switch ((string)json.action.ToString()){
                    case "SEARCH":
                       
                        setWebBrowser(json);
                        break;
                    case "DIRECTIONS":
                        setWebBrowser(json);
                        break;
                
                }
            }
         

        }
        public void setWebBrowser(dynamic json)
        {
           
            if ((string)json.action.ToString() == "SEARCH")
            {
                this.URL = "https://www.google.com/maps/embed/v1/directions?" +
                                                 "key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU" +
                                                 "&origin=Oslo+Norway" +
                                                 "&destination=Telemark+Norway";
                
                _myProperty = 1;
           
            }
            else
            {
                this.URL = "https://www.google.com/maps/embed/v1/directions?" +
                                                "key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU" +
                                                "&origin=Oslo+Norway" +
                                                "&destination=Telemark+Norway";
            }
            
            
        }
        public int MyProperty
        {
            get { return _myProperty; }
            set
            {
                _myProperty = value;
                if (_myProperty == 1)
                {
                    button1_Click(this, EventArgs.Empty);
                    _myProperty = 0;
                }

            }
        }

        
    }
}
