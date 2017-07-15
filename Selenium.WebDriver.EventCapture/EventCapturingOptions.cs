using System;

namespace Selenium.WebDriver.EventCapture
{
    /// <summary>
    /// Options to configure an <see cref="EventCapturingWebDriver"/> instance.
    /// </summary>
    public class EventCapturingOptions
    {
        /// <summary>
        /// When true, captured events will be raised asynchronously using tasks. Choose this if your event handlers don't return immediately to avoid blocking new events.
        /// When false, capture events will be raised synchronously. Choose this if you event handlers generally return within a few milliseconds.
        /// Default is false.
        /// </summary>
        public bool AsyncEvents { get; set; }

        /// <summary>
        /// Controls how often the <see cref="EventCapturingWebDriver"/> checks for new events.
        /// Lower values = more frequent checking.
        /// Default is every 100ms.
        /// </summary>
        public TimeSpan EventCaptureInterval { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCapturingOptions"/> class.
        /// </summary>
        public EventCapturingOptions()
        {
            //Set defaults
            AsyncEvents = false;
            EventCaptureInterval = TimeSpan.FromMilliseconds(100);
        }
    }
}
