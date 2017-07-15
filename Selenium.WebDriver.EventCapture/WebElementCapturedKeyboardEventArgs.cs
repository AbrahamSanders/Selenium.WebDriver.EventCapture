using OpenQA.Selenium;

namespace Selenium.WebDriver.EventCapture
{
    /// <summary>
    /// EventArgs for captured DOM keyboard events
    /// </summary>
    public class WebElementCapturedKeyboardEventArgs : WebElementCapturedEventArgs
    {
        /// <summary>
        /// The key code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// The key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebElementCapturedKeyboardEventArgs"/> class.
        /// </summary>
        /// <param name="driver">The source WebDriver</param>
        /// <param name="element">A WebElement representing the DOM element on which the event occured</param>
        public WebElementCapturedKeyboardEventArgs(IWebDriver driver, IWebElement element) 
            : base(driver, element)
        {
        }
    }
}
