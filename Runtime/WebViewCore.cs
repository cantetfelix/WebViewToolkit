using UnityEngine;

namespace WebViewToolkit
{
    /// <summary>
    /// Runtime driver for WebViewManager.
    /// Ensures the WebViewManager update loop runs during gameplay.
    /// </summary>
    public class WebViewCore : MonoBehaviour
    {
        private void Update()
        {
            // Drive the WebViewManager update loop
            WebViewManager.Instance.Tick();
        }

        private void OnApplicationQuit()
        {
            // Ensure proper shutdown
            WebViewManager.Instance.Shutdown();
        }
    }
}
