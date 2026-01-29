using NUnit.Framework;
using UnityEngine;
using WebViewToolkit;

namespace WebViewToolkit.Tests.Editor
{
    public class WebViewManagerTests
    {
        [Test]
        public void Singleton_IsNotNull()
        {
            // Accessing Instance should create the Singleton if it doesn't exist
            var instance = WebViewManager.Instance;
            Assert.IsNotNull(instance, "WebViewManager.Instance should not be null");
            // No longer a MonoBehaviour, so no GameObject checks needed
        }
    }
}
