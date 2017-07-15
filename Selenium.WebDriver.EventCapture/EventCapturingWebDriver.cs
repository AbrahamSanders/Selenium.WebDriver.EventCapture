using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Selenium.WebDriver.EventCapture
{
    /// <summary>
    /// An extension of Selenium's EventFiringWebDriver, allowing it to capture and fire events from the live browser DOM
    /// in addition to events triggered by the WebDriver itself.
    /// </summary>
    public class EventCapturingWebDriver : EventFiringWebDriver
    {

        #region private global members

        private EventCapturingOptions options;
        private Task eventCaptureTask;
        private bool eventCaptureEnabled;

        /// <summary>
        /// This <see cref="ManualResetEvent"/> allows suspending the event capture loop 
        /// by forcing the thread to block until signaled by the resumeEventCapture method.
        /// </summary>
        private ManualResetEvent suspendWaitHandle;

        /// <summary>
        /// This <see cref="ManualResetEvent"/> allows the suspendEventCapture method to block until
        /// the event capture loop has safely completed its current iteration. This helps avoid race conditions such as where
        /// the WebDriver navigates immediately after suspension and rips the DOM away from the event capture loop while it is still executing javascript.
        /// </summary>
        private ManualResetEvent iterationCompleteWaitHandle;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCapturingWebDriver"/> class with default options.
        /// </summary>
        /// <param name="parentDriver">The driver to capture events from</param>
        public EventCapturingWebDriver(IWebDriver parentDriver) 
            : this(parentDriver, new EventCapturingOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCapturingWebDriver"/> class with the specified options.
        /// </summary>
        /// <param name="parentDriver">The driver to capture events from</param>
        /// <param name="options">The options to use</param>
        public EventCapturingWebDriver(IWebDriver parentDriver, EventCapturingOptions options) 
            : base(parentDriver)
        {
            this.options = options;
        }

        #endregion

        #region events

        /// <summary>
        /// Fires when an DOM element click event is captured
        /// </summary>
        public event EventHandler<WebElementCapturedMouseEventArgs> ElementClickCaptured;

        /// <summary>
        /// Fires when a DOM right click (contextmenu) event is captured
        /// </summary>
        public event EventHandler<WebElementCapturedMouseEventArgs> ElementRightClickCaptured;

        /// <summary>
        /// Fires when a DOM double click event is captured
        /// </summary>
        public event EventHandler<WebElementCapturedMouseEventArgs> ElementDoubleClickCaptured;

        /// <summary>
        /// Fires when a DOM mouseover event is captured
        /// </summary>
        public event EventHandler<WebElementCapturedMouseEventArgs> ElementMouseOverCaptured;

        /// <summary>
        /// Fires when a DOM mouseleave event is captured
        /// </summary>
        public event EventHandler<WebElementCapturedMouseEventArgs> ElementMouseLeaveCaptured;

        /// <summary>
        /// Fires when a DOM keypress event is captured
        /// </summary>
        public event EventHandler<WebElementCapturedKeyboardEventArgs> ElementKeyPressCaptured;

        /// <summary>
        /// Raises the <see cref="ElementClickCaptured"/> event.
        /// </summary>
        /// <param name="e">A <see cref="WebElementCapturedMouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnElementClickCaptured(WebElementCapturedMouseEventArgs e)
        {
            ElementClickCaptured?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ElementRightClickCaptured"/> event.
        /// </summary>
        /// <param name="e">A <see cref="WebElementCapturedMouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnElementRightClickCaptured(WebElementCapturedMouseEventArgs e)
        {
            ElementRightClickCaptured?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ElementDoubleClickCaptured"/> event.
        /// </summary>
        /// <param name="e">A <see cref="WebElementCapturedMouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnElementDoubleClickCaptured(WebElementCapturedMouseEventArgs e)
        {
            ElementDoubleClickCaptured?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ElementMouseOverCaptured"/> event.
        /// </summary>
        /// <param name="e">A <see cref="WebElementCapturedMouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnElementMouseOverCaptured(WebElementCapturedMouseEventArgs e)
        {
            ElementMouseOverCaptured?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ElementMouseLeaveCaptured"/> event.
        /// </summary>
        /// <param name="e">A <see cref="WebElementCapturedMouseEventArgs"/> that contains the event data.</param>
        protected virtual void OnElementMouseLeaveCaptured(WebElementCapturedMouseEventArgs e)
        {
            ElementMouseLeaveCaptured?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ElementKeyPressCaptured"/> event.
        /// </summary>
        /// <param name="e">A <see cref="WebElementCapturedKeyboardEventArgs"/> that contains the event data.</param>
        protected virtual void OnElementKeyPressCaptured(WebElementCapturedKeyboardEventArgs e)
        {
            ElementKeyPressCaptured?.Invoke(this, e);
        }

        #endregion

        #region event capture

        /// <summary>
        /// Starts event capture if it has not yet been enabled, otherwise resumes event capture if it has been suspended.
        /// </summary>
        private void enableOrResumeEventCapture()
        {
            if (!eventCaptureEnabled)
            {
                eventCaptureEnabled = true;
                suspendWaitHandle = new ManualResetEvent(true);
                iterationCompleteWaitHandle = new ManualResetEvent(false);
                //Start the event capture loop on in a long running task as it may run for an indeterminate amount of time
                //and we do not want to hog a thread pool thread.
                eventCaptureTask = Task.Factory.StartNew(new Action(captureEvents), TaskCreationOptions.LongRunning);
            }
            else
            {
                resumeEventCapture();
            }
        }

        /// <summary>
        /// Terminates event capture if it is enabled. All resources created to support the event loop are disposed.
        /// </summary>
        private void disableEventCapture()
        {
            if (eventCaptureEnabled)
            {
                eventCaptureEnabled = false;
                resumeEventCapture();
                eventCaptureTask.Wait();

                eventCaptureTask.Dispose();
                suspendWaitHandle.Dispose();
                iterationCompleteWaitHandle.Dispose();
                eventCaptureTask = null;
                suspendWaitHandle = null;
                iterationCompleteWaitHandle = null;
            }
        }

        /// <summary>
        /// Suspends event capture if it is enabled, and blocks until the event capture loop has safely completed its current iteration
        /// </summary>
        private void suspendEventCapture()
        {
            if (suspendWaitHandle != null && iterationCompleteWaitHandle != null)
            {
                suspendWaitHandle.Reset();
                iterationCompleteWaitHandle.WaitOne();
            }
        }

        /// <summary>
        /// Resumes event capture if it is enabled by signaling to the event capture loop that it may unblock
        /// </summary>
        private void resumeEventCapture()
        {
            if (suspendWaitHandle != null)
            {
                suspendWaitHandle.Set();
            }
        }

        /// <summary>
        /// The main event capture loop. 
        /// </summary>
        private void captureEvents()
        {
            while (eventCaptureEnabled)
            {
                try
                {
                    //reseting iterationCompleteWaitHandle ensures that any call to suspendEventCapture while we are in the middle of the iteration will wait
                    //for the iteration to complete. A lock is not used because the critical section would indeterminately large due to the event delegate invocations.
                    iterationCompleteWaitHandle.Reset();

                    //Step 1: inject javascript which pulls off all events on the event queue and returns them. This is repeated for every event capture iteration.
                    //If the event queue code has not been registered, return a single key captureScriptNotRegistered to indicate that the script must be injected.
                    var events = WrappedDriver.ExecuteJavaScript<object>("return\"function\"==typeof getEvents?getEvents():[{captureScriptNotRegistered:null}]") as ReadOnlyCollection<object>;
                    if (events != null)
                    {
                        foreach (Dictionary<string, object> evn in events)
                        {
                            //If the event queue code has not been registered, inject it now and break the iteration. Next iteration will begin to capture events.
                            //this happens on initial WebDriver navigation or if a user event such as clicking on a link causes the browser to navigate or refresh.
                            if (evn.ContainsKey("captureScriptNotRegistered"))
                            {
                                WrappedDriver.ExecuteJavaScript("var eventQueue=[],nativeEvents={submit:\"HTMLEvents\",keypress:\"KeyEvents\",click:\"MouseEvents\",dblclick:\"MouseEvents\",contextmenu:\"MouseEvents\",dragstart:\"MouseEvents\",dragend:\"MouseEvents\",mouseover:\"MouseEvents\",mouseleave:\"MouseEvents\"};processEvent=function(e){return e.triggeredManually?!0:e.type in nativeEvents?(storeEvent(e),!1):void 0},storeEvent=function(e){ev=convertEvent(e),eventQueue.push(ev)},getEvents=function(e){var t=eventQueue.length;return eventQueue.splice(0,t)},convertEvent=function(e){var t={};return t.type=e.type,t.target=e.target,t.button=e.button,t.code=e.code,t.key=e.key,t.altKey=e.altKey,t.ctrlKey=e.ctrlKey,t.shiftKey=e.shiftKey,t.clientX=e.clientX,t.clientY=e.clientY,t.offsetX=e.offsetX,t.offsetY=e.offsetY,t};for(var eventName in nativeEvents)document.addEventListener(eventName,processEvent,!0);");
                                break;
                            }

                            //For each DOM event that was pulled off the event queue, raise the appropriate event.
                            string type = (string)evn["type"];
                            switch (type.ToLower())
                            {
                                case "click":
                                    raiseMouseEvent(evn, OnElementClickCaptured);
                                    break;
                                case "contextmenu":
                                    raiseMouseEvent(evn, OnElementRightClickCaptured);
                                    break;
                                case "dblclick":
                                    raiseMouseEvent(evn, OnElementDoubleClickCaptured);
                                    break;
                                case "mouseover":
                                    raiseMouseEvent(evn, OnElementMouseOverCaptured);
                                    break;
                                case "mouseleave":
                                    raiseMouseEvent(evn, OnElementMouseLeaveCaptured);
                                    break;
                                case "keypress":
                                    raiseKeyboardEvent(evn, OnElementKeyPressCaptured);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.Message);
                }
                finally
                {
                    //signalling iterationCompleteWaitHandle allows the suspendEventCapture method to unblock
                    //as the iteration has safely completed.
                    iterationCompleteWaitHandle.Set();
                    if (eventCaptureEnabled)
                    {
                        Thread.Sleep(options.EventCaptureInterval);
                    }
                }

                //if suspendEventCapture has been called, the method will block here indefinitely until resumeEventCapture is called.
                //otherwise execution will pass right through to the next iteration.
                suspendWaitHandle.WaitOne();
            }
        }

        /// <summary>
        /// Raises a mouse event with <see cref="WebElementCapturedMouseEventArgs"/>.
        /// </summary>
        /// <param name="evn">The DOM event data pulled off the event queue</param>
        /// <param name="eventMethod">A delegate pointing to the event-raising method for the specific event to be raised.</param>
        private void raiseMouseEvent(Dictionary<string, object> evn, Action<WebElementCapturedMouseEventArgs> eventMethod)
        {
            WebElementCapturedMouseEventArgs mouseArgs = new WebElementCapturedMouseEventArgs(WrappedDriver, (IWebElement)evn["target"]);
            mapCommonEventArgs(mouseArgs, evn);

            mouseArgs.Button = (long?)evn["button"];
            mouseArgs.ClientX = (long?)evn["clientX"];
            mouseArgs.ClientY = (long?)evn["clientY"];
            mouseArgs.OffsetX = (long?)evn["offsetX"];
            mouseArgs.OffsetY = (long?)evn["offsetY"];

            raiseEvent(() => eventMethod(mouseArgs));
        }

        /// <summary>
        /// Raises a keyboard event with <see cref="WebElementCapturedKeyboardEventArgs"/>.
        /// </summary>
        /// <param name="evn">The DOM event data pulled off the event queue</param>
        /// <param name="eventMethod">A delegate pointing to the event-raising method for the specific event to be raised.</param>
        private void raiseKeyboardEvent(Dictionary<string, object> evn, Action<WebElementCapturedKeyboardEventArgs> eventMethod)
        {
            WebElementCapturedKeyboardEventArgs keyboardArgs = new WebElementCapturedKeyboardEventArgs(WrappedDriver, (IWebElement)evn["target"]);
            mapCommonEventArgs(keyboardArgs, evn);

            keyboardArgs.Code = (string)evn["code"];
            keyboardArgs.Key = (string)evn["key"];

            raiseEvent(() => eventMethod(keyboardArgs));
        }

        /// <summary>
        /// Raises an event synchronously or asynchonously by either calling the event-raising method delegate directly or starting a task with it.
        /// This decision is based on the options.AsyncEvents flag provided by the caller. />
        /// </summary>
        /// <param name="eventMethod">A parameterless action delegate which calls the event-raising method for the specific event to be raised.</param>
        private void raiseEvent(Action eventMethod)
        {
            if (options.AsyncEvents)
            {
                Task.Factory.StartNew(eventMethod);
            }
            else
            {
                eventMethod();
            }
        }

        /// <summary>
        /// Maps common event properties from the DOM event data to the event args
        /// </summary>
        /// <param name="args">The event arguments for the event</param>
        /// <param name="evn">The DOM event data pulled off the event queue</param>
        private void mapCommonEventArgs(WebElementCapturedEventArgs args, Dictionary<string, object> evn)
        {
            args.AltKey = (bool?)evn["altKey"];
            args.CtrlKey = (bool?)evn["ctrlKey"];
            args.ShiftKey = (bool?)evn["shiftKey"];
        }

        /// <summary>
        /// Overrides the <see cref="EventFiringWebDriver"/>'s OnNavigating method to suspend event capture before navigation
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigating(WebDriverNavigationEventArgs e)
        {
            suspendEventCapture();
            base.OnNavigating(e);
        }

        /// <summary>
        /// Overrides the <see cref="EventFiringWebDriver"/>'s OnNavigated method to resume event capture after navigation
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigated(WebDriverNavigationEventArgs e)
        {
            enableOrResumeEventCapture();
            base.OnNavigated(e);
        }

        /// <summary>
        /// Overrides the <see cref="EventFiringWebDriver"/>'s OnNavigatingBack method to suspend event capture before navigation
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigatingBack(WebDriverNavigationEventArgs e)
        {
            suspendEventCapture();
            base.OnNavigatingBack(e);
        }

        /// <summary>
        /// Overrides the <see cref="EventFiringWebDriver"/>'s OnNavigatedBack method to resume event capture after navigation
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigatedBack(WebDriverNavigationEventArgs e)
        {
            enableOrResumeEventCapture();
            base.OnNavigatedBack(e);
        }

        /// <summary>
        /// Overrides the <see cref="EventFiringWebDriver"/>'s OnNavigatingForward method to suspend event capture before navigation
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigatingForward(WebDriverNavigationEventArgs e)
        {
            suspendEventCapture();
            base.OnNavigatingForward(e);
        }

        /// <summary>
        /// Overrides the <see cref="EventFiringWebDriver"/>'s OnNavigatedForward method to resume event capture after navigation
        /// </summary>
        /// <param name="e">The event args</param>
        protected override void OnNavigatedForward(WebDriverNavigationEventArgs e)
        {
            enableOrResumeEventCapture();
            base.OnNavigatedForward(e);
        }

        #endregion

        #region lifecycle methods

        /// <summary>
        /// If event capture is enabled, it is terminated on disposal of the <see cref="EventCapturingWebDriver"/>.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                disableEventCapture();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
