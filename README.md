# Selenium.WebDriver.EventCapture
Selenium.WebDriver.EventCapture is a WebDriver wrapper that adds the ability to capture DOM & browser events triggered by a live user.

## Why would I want to capture DOM events from Selenium?
Selenium is mainly used for test automation, where capturing events is not necessary. However, there are some less common alternative uses for Selenium such as recording macros, filling forms, and others where Selenium is used to automate a browser being actively used by a live user. In these scenarios it is useful to be able to capture events raised by the actual browser DOM in addition to events raised by WebDriver actions.

As of now, Selenium WebDriver lacks the facility to capture events from the DOM and allow the user to subscribe to ones he/she is interested in. There is an [EventFiringWebDriver](https://github.com/SeleniumHQ/selenium/blob/master/dotnet/src/support/Events/EventFiringWebDriver.cs) built in, however this only fires events *raised by the WebDriver itself* rather than raised by the live DOM.

There are quite a few posts from the last few years where people are looking for this functionality:
([here](https://stackoverflow.com/questions/16746757/seleniumwebdriver-is-there-a-listener-to-capture-user-actions-in-the-browser-s),
[here](https://stackoverflow.com/questions/35884230/can-my-webdriver-script-catch-a-event-from-the-webpage),
[here](https://stackoverflow.com/questions/9805508/how-to-capture-user-action-on-browser-by-java-code),
[here](https://groups.google.com/d/msg/selenium-users/29GyTVvayCE/5NjKPbc5X1wJ),
etc...), so it is only logical to assume that there is need for it.

## How does it work?
Selenium.WebDriver.EventCapture extends the existing EventFiringWebDriver to support capture of DOM events through a new subclass [EventCapturingWebDriver](https://github.com/AbrahamSanders/Selenium.WebDriver.EventCapture/blob/master/Selenium.WebDriver.EventCapture/EventCapturingWebDriver.cs). This is made possible through a javascript event queue mechanism. The implementation was inspired by [Alp's stackoverflow answer](https://stackoverflow.com/a/9814436).

In a nutshell: a script is injected into the DOM which registers a global event handler that dumps event information into a queue. Then, the EventCapturingWebDriver repeatedly polls the DOM on a very short interval to pull the latest events off the queue and return them to be raised. Calling code can subscribe to these DOM events through C# event handlers:

    EventCapturingWebDriver driver = new EventCapturingWebDriver(baseDriver);
    
    driver.ElementClickCaptured += Driver_ElementClickCaptured;
    driver.ElementMouseOverCaptured += Driver_ElementMouseOverCaptured;
    driver.ElementKeyPressCaptured += Driver_ElementKeyPressCaptured;

    driver.Navigate().GoToUrl("http://www.wikipedia.org");

    
