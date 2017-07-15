using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using Selenium.WebDriver.EventCapture;
using System;

namespace Example
{
    /// <summary>
    /// Example program to demonstrate the usage of EventCapturingWebDriver
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press 1 for Chrome");
            Console.WriteLine("Press 2 for Firefox");
            Console.WriteLine("Press 3 for Internet Explorer");
            Console.WriteLine("Press 4 for Edge");
            Console.WriteLine("Press any other key to quit.");
            Console.Write(">>> ");

            IWebDriver baseDriver;
            switch (Console.ReadKey().KeyChar)
            {
                case '1':
                    baseDriver = new ChromeDriver();
                    break;
                case '2':
                    baseDriver = new FirefoxDriver();
                    break;
                case '3':
                    baseDriver = new InternetExplorerDriver();
                    break;
                case '4':
                    baseDriver = new EdgeDriver();
                    break;
                default:
                    return;
            }

            Console.WriteLine();
            Console.WriteLine("Hit any key to quit.");
            Console.WriteLine();

            using (EventCapturingWebDriver driver = new EventCapturingWebDriver(baseDriver))
            {
                driver.ElementClickCaptured += Driver_ElementClickCaptured;
                driver.ElementRightClickCaptured += Driver_ElementRightClickCaptured;
                driver.ElementDoubleClickCaptured += Driver_ElementDoubleClickCaptured;
                driver.ElementMouseOverCaptured += Driver_ElementMouseOverCaptured;
                driver.ElementMouseLeaveCaptured += Driver_ElementMouseLeaveCaptured;
                driver.ElementKeyPressCaptured += Driver_ElementKeyPressCaptured;

                driver.Navigate().GoToUrl("http://www.wikipedia.org");

                Console.ReadKey();
            }
        }

        private static void Driver_ElementClickCaptured(object sender, WebElementCapturedMouseEventArgs e)
        {
            Console.WriteLine("Click at ({0}, {1}): <{2} id=\"{3}\">", e.ClientX, e.ClientY, e.Element.TagName, e.Element.GetAttribute("id"));
        }

        private static void Driver_ElementRightClickCaptured(object sender, WebElementCapturedMouseEventArgs e)
        {
            Console.WriteLine("RightClick at ({0}, {1}): <{2} id=\"{3}\">", e.ClientX, e.ClientY, e.Element.TagName, e.Element.GetAttribute("id"));
        }

        private static void Driver_ElementDoubleClickCaptured(object sender, WebElementCapturedMouseEventArgs e)
        {
            Console.WriteLine("DoubleClick at ({0}, {1}): <{2} id=\"{3}\">", e.ClientX, e.ClientY, e.Element.TagName, e.Element.GetAttribute("id"));
        }

        private static void Driver_ElementMouseOverCaptured(object sender, WebElementCapturedMouseEventArgs e)
        {
            Console.WriteLine("MouseOver at ({0}, {1}): <{2} id=\"{3}\">", e.ClientX, e.ClientY, e.Element.TagName, e.Element.GetAttribute("id"));
        }

        private static void Driver_ElementMouseLeaveCaptured(object sender, WebElementCapturedMouseEventArgs e)
        {
            Console.WriteLine("MouseLeave at ({0}, {1}): <{2} id=\"{3}\">", e.ClientX, e.ClientY, e.Element.TagName, e.Element.GetAttribute("id"));
        }

        private static void Driver_ElementKeyPressCaptured(object sender, WebElementCapturedKeyboardEventArgs e)
        {
            Console.WriteLine("KeyPress [{0}]: <{1} id=\"{2}\">", e.Key, e.Element.TagName, e.Element.GetAttribute("id"));
        }
    }
}
