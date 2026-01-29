using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using WebViewToolkit.UIToolkit;

namespace WebViewToolkit.Tests.Editor
{
    public class WebViewElementTests
    {
        [Test]
        public void Construction_SetsDefaultProperties()
        {
            var element = new WebViewElement();
            Assert.AreEqual("https://www.google.com", element.InitialUrl, "Default InitialUrl should be google.com");
            Assert.IsFalse(element.EnableDevTools, "DevTools should be disabled by default");
        }

        [Test]
        public void Navigation_MethodsExposed()
        {
            var element = new WebViewElement();
            
            // These calls should leverage the underlying WebViewInstance safely
            // In Editor mode (detached), they should either warn or do nothing, but NOT crash
            Assert.DoesNotThrow(() => element.Navigate("https://example.com"));
            Assert.DoesNotThrow(() => element.NavigateToString("<html></html>"));
            
            // Warning "Size too small" is no longer emitted to reduce spam during layout
            // LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Size too small.*")); 
            
            // Explicitly call CreateWebView to trigger potential errors
            // element.CreateWebView(); 
        }
    }
}
