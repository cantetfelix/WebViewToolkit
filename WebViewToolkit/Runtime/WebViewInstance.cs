// ============================================================================
// WebViewToolkit - WebView Instance
// ============================================================================

using System;
using UnityEngine;
using WebViewToolkit.Native;

namespace WebViewToolkit
{
    /// <summary>
    /// Represents a single WebView instance with its associated texture
    /// </summary>
    public class WebViewInstance : IDisposable
    {
        /// <summary>
        /// Native handle
        /// </summary>
        public uint Handle { get; private set; }

        /// <summary>
        /// Width in pixels
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height in pixels
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The texture containing the WebView content
        /// </summary>
        public Texture2D Texture { get; private set; }

        /// <summary>
        /// Whether this instance has been destroyed
        /// </summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// Current URL
        /// </summary>
        public string CurrentUrl { get; private set; }

        /// <summary>
        /// Event fired when navigation completes
        /// </summary>
        public event Action<string, bool> NavigationCompleted;

        /// <summary>
        /// Event fired when a JavaScript message is received
        /// </summary>
        public event Action<string> MessageReceived;

        // Native texture pointer
        private IntPtr _nativeTexturePtr;

        internal WebViewInstance(uint handle, int width, int height)
        {
            Handle = handle;
            Width = width;
            Height = height;

            // Get native texture pointer
            _nativeTexturePtr = WebViewNative.WebViewToolkit_GetTexturePtr(handle);

            if (_nativeTexturePtr != IntPtr.Zero)
            {
                // Create Unity texture from native pointer
                // BGRA format matches what WebView2 produces
                Texture = Texture2D.CreateExternalTexture(
                    width,
                    height,
                    TextureFormat.BGRA32,
                    mipChain: false,
                    linear: false,
                    _nativeTexturePtr
                );

                Texture.name = $"WebViewTexture_{handle}";
                Texture.filterMode = FilterMode.Bilinear;
                Texture.wrapMode = TextureWrapMode.Clamp;
            }
            else
            {
                Debug.LogWarning($"[WebViewInstance] Native texture pointer is null for handle {handle}");
            }
        }

        /// <summary>
        /// Navigate to a URL
        /// </summary>
        public bool Navigate(string url)
        {
            if (IsDestroyed) return false;

            var result = (NativeResult)WebViewNative.WebViewToolkit_Navigate(Handle, url);
            return result == NativeResult.Success;
        }

        /// <summary>
        /// Navigate to HTML content
        /// </summary>
        public bool NavigateToString(string html)
        {
            if (IsDestroyed) return false;

            var result = (NativeResult)WebViewNative.WebViewToolkit_NavigateToString(Handle, html);
            return result == NativeResult.Success;
        }

        /// <summary>
        /// Execute JavaScript code
        /// </summary>
        public bool ExecuteScript(string script)
        {
            if (IsDestroyed) return false;

            var result = (NativeResult)WebViewNative.WebViewToolkit_ExecuteScript(Handle, script);
            return result == NativeResult.Success;
        }

        /// <summary>
        /// Navigate back in history
        /// </summary>
        public bool GoBack()
        {
            if (IsDestroyed) return false;

            var result = (NativeResult)WebViewNative.WebViewToolkit_GoBack(Handle);
            return result == NativeResult.Success;
        }

        /// <summary>
        /// Navigate forward in history
        /// </summary>
        public bool GoForward()
        {
            if (IsDestroyed) return false;

            var result = (NativeResult)WebViewNative.WebViewToolkit_GoForward(Handle);
            return result == NativeResult.Success;
        }

        /// <summary>
        /// Check if can navigate back
        /// </summary>
        public bool CanGoBack()
        {
            if (IsDestroyed) return false;
            return WebViewNative.WebViewToolkit_CanGoBack(Handle) != 0;
        }

        /// <summary>
        /// Check if can navigate forward
        /// </summary>
        public bool CanGoForward()
        {
            if (IsDestroyed) return false;
            return WebViewNative.WebViewToolkit_CanGoForward(Handle) != 0;
        }

        /// <summary>
        /// Resize the WebView
        /// </summary>
        public bool Resize(int width, int height)
        {
            if (IsDestroyed) return false;

            var result = (NativeResult)WebViewNative.WebViewToolkit_Resize(Handle, (uint)width, (uint)height);
            if (result != NativeResult.Success)
            {
                return false;
            }

            Width = width;
            Height = height;

            // Recreate texture with new dimensions
            // Unity's Texture2D doesn't support resizing, so we must create a new one
            _nativeTexturePtr = WebViewNative.WebViewToolkit_GetTexturePtr(Handle);
            if (_nativeTexturePtr != IntPtr.Zero)
            {
                // Destroy old texture if it exists
                if (Texture != null)
                {
                    UnityEngine.Object.Destroy(Texture);
                }

                // Create new texture with updated dimensions
                Texture = Texture2D.CreateExternalTexture(
                    width,
                    height,
                    TextureFormat.BGRA32,
                    mipChain: false,
                    linear: false,
                    _nativeTexturePtr
                );
                Texture.name = $"WebViewTexture_{Handle}";
                Texture.filterMode = FilterMode.Bilinear;
                Texture.wrapMode = TextureWrapMode.Clamp;
            }

            return true;
        }
 
        /// <summary>
        /// Refresh the native texture reference (e.g. after device reset)
        /// </summary>
        public void RefreshTexture()
        {
            if (IsDestroyed) return;
 
            _nativeTexturePtr = WebViewNative.WebViewToolkit_GetTexturePtr(Handle);
            if (_nativeTexturePtr != IntPtr.Zero && Texture != null)
            {
                Texture.UpdateExternalTexture(_nativeTexturePtr);
            }
        }

        /// <summary>
        /// Send a mouse event to the WebView
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="button">Mouse button</param>
        /// <param name="normalizedX">X position [0-1]</param>
        /// <param name="normalizedY">Y position [0-1]</param>
        /// <param name="wheelDelta">Wheel delta (for wheel events)</param>
        public bool SendMouseEvent(MouseEventType eventType, MouseButton button, float normalizedX, float normalizedY, float wheelDelta = 0)
        {
            if (IsDestroyed) return false;

            var result = (NativeResult)WebViewNative.WebViewToolkit_SendMouseEvent(
                Handle,
                (int)eventType,
                (int)button,
                normalizedX,
                normalizedY,
                wheelDelta
            );

            return result == NativeResult.Success;
        }

        /// <summary>
        /// Send a key event to the WebView
        /// </summary>
        public bool SendKeyEvent(uint virtualKeyCode, uint scanCode, bool isKeyDown, bool isSystemKey = false)
        {
            if (IsDestroyed) return false;

            var result = (NativeResult)WebViewNative.WebViewToolkit_SendKeyEvent(
                Handle,
                virtualKeyCode,
                scanCode,
                isKeyDown ? 1 : 0,
                isSystemKey ? 1 : 0
            );

            return result == NativeResult.Success;
        }

        /// <summary>
        /// Destroy this WebView instance
        /// </summary>
        public void Dispose()
        {
            if (IsDestroyed) return;

            WebViewManager.Instance?.DestroyWebView(this);
            DestroyInternal();
        }

        internal void DestroyInternal()
        {
            IsDestroyed = true;

            if (Texture != null)
            {
#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(Texture, true);
#else
                UnityEngine.Object.Destroy(Texture);
#endif
                Texture = null;
            }

            _nativeTexturePtr = IntPtr.Zero;
            Handle = 0;
        }

        internal void OnNavigationCompleted(string url, bool isSuccess)
        {
            CurrentUrl = url;
            NavigationCompleted?.Invoke(url, isSuccess);
        }

        internal void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(message);
        }
    }
}
