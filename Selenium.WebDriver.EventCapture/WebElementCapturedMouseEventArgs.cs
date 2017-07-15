using OpenQA.Selenium;

namespace Selenium.WebDriver.EventCapture
{
    /// <summary>
    /// EventArgs for captured DOM mouse events
    /// </summary>
    public class WebElementCapturedMouseEventArgs : WebElementCapturedEventArgs
    {
        /// <summary>
        /// The mouse button
        /// </summary>
        public long? Button { get; set; }
        /// <summary>
        /// The client x-coordinate
        /// </summary>
        public long? ClientX { get; set; }
        /// <summary>
        /// The client y-coordinate
        /// </summary>
        public long? ClientY { get; set; }
        /// <summary>
        /// The x offset
        /// </summary>
        public long? OffsetX { get; set; }
        /// <summary>
        /// The y offset
        /// </summary>
        public long? OffsetY { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebElementCapturedMouseEventArgs"/> class.
        /// </summary>
        /// <param name="driver">The source WebDriver</param>
        /// <param name="element">A WebElement representing the DOM element on which the event occured</param>
        public WebElementCapturedMouseEventArgs(IWebDriver driver, IWebElement element) 
            : base(driver, element)
        {
        }
    }
}
