using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;

namespace Selenium.WebDriver.EventCapture
{
    /// <summary>
    /// EventArgs for captured DOM events
    /// </summary>
    public class WebElementCapturedEventArgs : WebElementEventArgs
    {
        /// <summary>
        /// Indicates if the Alt key was pressed when the event occured.
        /// </summary>
        public bool? AltKey { get; set; }
        /// <summary>
        /// Indicates if the Ctrl key was pressed when the event occured.
        /// </summary>
        public bool? CtrlKey { get; set; }
        /// <summary>
        /// Indicates if the Shift key was pressed when the event occured.
        /// </summary>
        public bool? ShiftKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebElementCapturedEventArgs"/> class.
        /// </summary>
        /// <param name="driver">The source WebDriver</param>
        /// <param name="element">A WebElement representing the DOM element on which the event occured</param>
        public WebElementCapturedEventArgs(IWebDriver driver, IWebElement element) 
            : base(driver, element)
        {
        }
    }
}
