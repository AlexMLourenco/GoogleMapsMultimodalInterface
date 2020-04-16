using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Windows.Threading;
using Keys = OpenQA.Selenium.Keys;
using System.Threading;

namespace AppGui

{
    class browserCommands
    {

        public void searchLocation(IWebDriver driver, CLocation location, dynamic json)
        {
            string tmpInput = "";
            driver.Navigate().GoToUrl("https://www.google.pt/maps");
            if ((string)json.service != null)
            {
                tmpInput += (string)json.service.ToString();
                if ((string)json.location != null)
                {
                    tmpInput += " " + (string)json.location.ToString();
                }
            }
            else if ((string)json.local != null)
            {
                tmpInput += (string)json.local.ToString();
                if ((string)json.location != null)
                    tmpInput += " " + (string)json.location.ToString();
            }
            driver.FindElement(By.Id("searchboxinput")).SendKeys(tmpInput);
            driver.FindElement(By.Id("searchboxinput")).SendKeys(Keys.Enter);
        }

        public void getDirections(IWebDriver driver, CLocation location, dynamic json)
        {
            string tmpInput = "";
            int mode = 6;
            string[] coordenadas = location.getCoords();
            switch ((string)json.mode.ToString())
            {
                case "WALKING":
                    mode = 2;
                    break;
                case "DRIVING":
                    mode = 0;
                    break;
                case "BICYCLING":
                    mode = 1;
                    break;
                case "TRANSIT":
                    mode = 3;
                    break;
            }
            if ((string)json.service != null)
            {
                tmpInput += (string)json.service.ToString();
                if ((string)json.location != null)
                {
                    tmpInput += " " + (string)json.location.ToString();
                }
            }
            else if ((string)json.local != null)
            {
                tmpInput += (string)json.local.ToString();
                if ((string)json.location != null)
                    tmpInput += " " + (string)json.location.ToString();
            }
            Console.WriteLine("mode " + mode.ToString());
            driver.Navigate().GoToUrl("https://www.google.pt/maps");
            driver.FindElement(By.Id("searchbox-directions")).Click();
            Thread.Sleep(2000);
            driver.FindElement(By.CssSelector(string.Format("div[data-travel_mode='{0}']", mode))).Click();

            Thread.Sleep(500);
            // origem
            driver.FindElement(By.XPath("//*[@id='sb_ifc51']/input")).Click();
            driver.FindElement(By.XPath("//*[@id='sb_ifc51']/input")).SendKeys(string.Format("{0},{1}", coordenadas[0], coordenadas[1]));

            // destino
            Thread.Sleep(500);
            driver.FindElement(By.XPath("//*[@id='sb_ifc52']/input")).Click();
            driver.FindElement(By.XPath("//*[@id='sb_ifc52']/input")).SendKeys(tmpInput);
            driver.FindElement(By.XPath("//*[@id='sb_ifc52']/input")).SendKeys(Keys.Enter);
        }
        public void zoomIn(IWebDriver driver, dynamic json)
        {
            if((string)json.zoom != null)
            {
                driver.FindElement(By.Id("widget-zoom-in")).Click();
            }
        }
        public void zoomOut(IWebDriver driver, dynamic json)
        {
            if ((string)json.zoom != null)
            {
                driver.FindElement(By.Id("widget-zoom-out")).Click();
            }
        }
        public void changeView(IWebDriver driver, dynamic json)
        {
            Thread.Sleep(2000);
            driver.FindElement(By.XPath("//*[@id='minimap']/div/div[2]/button")).Click();
               
            Thread.Sleep(2000);
           

        }
    }
}
