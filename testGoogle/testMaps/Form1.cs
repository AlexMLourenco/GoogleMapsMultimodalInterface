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
        public Form1()
        {
            InitializeComponent();
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
            HtmlDocument html = webBrowser1.Document;
            HtmlElementCollection doc = html.GetElementsByTagName("iframe").GetElementsByName("iframe1");//.SetAttribute("src", "ola");
            foreach (HtmlElement elem in doc)
            {
                elem.SetAttribute("src", "https://www.google.com/maps/embed/v1/search?key=AIzaSyCxJd14el9dRqIkvYqFwEx_zz8zwkTAlaU&q=record+stores+in+Seattle");
               
            }
        }
        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            Console.WriteLine((string)json.ToString());
            /*
            Shape _s = null;
            switch ((string)json.recognized[0].ToString())
            {
                case "SQUARE":
                    _s = rectangle;
                    break;
                case "CIRCLE":
                    _s = circle;
                    break;
                case "TRIANGLE":
                    _s = triangle;
                    break;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                switch ((string)json.recognized[1].ToString())
                {
                    case "GREEN":
                        _s.Fill = Brushes.Green;
                        break;
                    case "BLUE":
                        _s.Fill = Brushes.Blue;
                        break;
                    case "RED":
                        _s.Fill = Brushes.Red;
                        break;
                }
            });
            */


        }
    }
}
