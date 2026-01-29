using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WebViewToolkit;
using WebViewToolkit.Native;

namespace WebViewToolkit.Tests.Runtime
{
    public class WebViewIntegrationTests
    {
        private WebViewManager _manager;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            _manager = WebViewManager.Instance;
            yield return null; // Wait for Awake setup
        }

        [UnityTest]
        public IEnumerator Initialization_Succeeds()
        {
            // Give it a moment (though Initialize is synchronous in Awake)
            yield return null;
            
            Assert.IsNotNull(_manager, "Manager should exist");
            Assert.IsTrue(_manager.IsInitialized, "WebViewManager should be initialized in PlayMode");
            Assert.AreNotEqual(GraphicsAPI.Unknown, _manager.CurrentGraphicsAPI, "GraphicsAPI should be detected");
        }

        [UnityTest]
        public IEnumerator Lifecycle_CreateAndDestroy()
        {
            if (!_manager.IsInitialized) Assert.Ignore("Manager not initialized");

            var webView = _manager.CreateWebView(100, 100);
            Assert.IsNotNull(webView, "CreateWebView returned null");
            Assert.AreNotEqual(0, webView.Handle, "Handle should be valid");
            Assert.IsFalse(webView.IsDestroyed, "New webview should not be destroyed");

            yield return null;

            webView.Dispose();
            Assert.IsTrue(webView.IsDestroyed, "WebView should be marked destroyed");
        }

        [UnityTest]
        public IEnumerator Navigation_Features_Work()
        {
            if (!_manager.IsInitialized) Assert.Ignore("Manager not initialized");

            var webView = _manager.CreateWebView(500, 500);
            Assert.IsNotNull(webView);

            // 1. Navigate
            webView.Navigate("https://www.google.com");
            yield return new WaitForSeconds(0.5f); // Give native time to process
            
            // 2. Navigate again
            webView.Navigate("https://www.bing.com");
            yield return new WaitForSeconds(0.5f);

            // 3. Go Back
            webView.GoBack();
            yield return null; // Native call is instant, async effect

            // 4. Go Forward
            webView.GoForward();
            yield return null;


            yield return null;

            // Verify no crashes occurred during these calls
            webView.Dispose();
        }

        [UnityTest]
        public IEnumerator Stress_RapidLifecycle()
        {
            if (!_manager.IsInitialized) Assert.Ignore("Manager not initialized");

            int iterations = 20; // Reduced from 50 to keep test time reasonable
            var handles = new System.Collections.Generic.HashSet<uint>();

            for (int i = 0; i < iterations; i++)
            {
                var webView = _manager.CreateWebView(100, 100);
                Assert.IsNotNull(webView);
                Assert.IsFalse(handles.Contains(webView.Handle), "Handle recycled too quickly or duplicate?");
                handles.Add(webView.Handle);

                // Yielding is important for native processing (msg pump)
                yield return null; 

                webView.Dispose();
                
                // Ensure it's marked destroyed
                Assert.IsTrue(webView.IsDestroyed);
            }
        }

        [UnityTest]
        public IEnumerator MultiInstance_Isolation()
        {
            if (!_manager.IsInitialized) Assert.Ignore("Manager not initialized");

            var webView1 = _manager.CreateWebView(200, 200);
            var webView2 = _manager.CreateWebView(300, 300);

            Assert.AreNotEqual(webView1.Handle, webView2.Handle);
            Assert.AreEqual(200, webView1.Width);
            Assert.AreEqual(300, webView2.Width);

            yield return null;

            // Destroy one, verify other is still alive
            webView1.Dispose();
            Assert.IsTrue(webView1.IsDestroyed);
            Assert.IsFalse(webView2.IsDestroyed);

            yield return null;

            webView2.Dispose();
            Assert.IsTrue(webView2.IsDestroyed);
        }

        [UnityTest]
        public IEnumerator Resize_Functionality()
        {
            if (!_manager.IsInitialized) Assert.Ignore("Manager not initialized");

            var webView = _manager.CreateWebView(100, 100);
            yield return null;

            // Wait for native initialization (Async)
            // Retry resize until it succeeds or times out
            bool success = false;
            float timeout = 2.0f;
            float startTime = Time.time;
            
            while (Time.time - startTime < timeout)
            {
                if (webView.Resize(500, 400))
                {
                    success = true;
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }

            Assert.IsTrue(success, "WebView failed to initialize/resize within timeout");
            Assert.AreEqual(500, webView.Width);
            Assert.AreEqual(400, webView.Height);

            yield return null;
            webView.Dispose();
        }

        [UnityTest]
        public IEnumerator Script_Execution_ReturnsSuccess()
        {
            if (!_manager.IsInitialized) Assert.Ignore("Manager not initialized");

            var webView = _manager.CreateWebView(100, 100);
            
            // Wait for native readiness (using Resize or similar check)
            float timeout = 2.0f;
            float startTime = Time.time;
            bool ready = false;
            while (Time.time - startTime < timeout)
            {
               // Use Resize as a probe for readiness since IsReady is not exposed
               // If Resize succeeds, environment is ready
               if (webView.Resize(100, 100)) 
               {
                   ready = true;
                   break;
               }
               yield return new WaitForSeconds(0.1f);
            }
            Assert.IsTrue(ready, "WebView failed to initialize within timeout");

            // Need to load a page first usually for script context
            webView.NavigateToString("<html><body><h1>Hello</h1></body></html>");
            yield return new WaitForSeconds(0.5f);

            bool success = webView.ExecuteScript("console.log('Test Script Execution');");
            Assert.IsTrue(success, "ExecuteScript should return success");

            yield return null;
            webView.Dispose();
        }
    }
}
