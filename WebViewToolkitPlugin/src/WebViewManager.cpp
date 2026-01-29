// ============================================================================
// WebViewToolkit - WebView Manager Implementation
// ============================================================================

#include "WebViewToolkit/WebViewManager.h"
#include "WebViewToolkit/WebView.h"

// Windows headers
#include <Windows.h>

// WRL for COM helpers
#include <wrl.h>

// WinRT
#include <winrt/base.h>
#include <winrt/Windows.System.h>
#include <DispatcherQueue.h>

#pragma comment(lib, "windowsapp.lib")

namespace WebViewToolkit
{
    // ========================================================================
    // Static member definition
    // ========================================================================
    void* WebViewManager::s_dispatcherQueueController = nullptr;
    std::atomic<bool> WebViewManager::s_isShuttingDown(false);

    // ========================================================================
    // Helper: Convert strings
    // ========================================================================
    static std::string ToNarrow(const std::wstring& wstr)
    {
        if (wstr.empty()) return "";
        int size = WideCharToMultiByte(CP_UTF8, 0, wstr.c_str(), -1, nullptr, 0, nullptr, nullptr);
        std::string result(size - 1, 0);
        WideCharToMultiByte(CP_UTF8, 0, wstr.c_str(), -1, &result[0], size, nullptr, nullptr);
        return result;
    }

    // ========================================================================
    // WebViewManager Implementation
    // ========================================================================

    WebViewManager::WebViewManager() = default;

    WebViewManager::~WebViewManager()
    {
        Shutdown();
    }

    bool WebViewManager::IsShuttingDown()
    {
        return s_isShuttingDown.load(std::memory_order_acquire);
    }

    void WebViewManager::SignalShuttingDown()
    {
        s_isShuttingDown.store(true, std::memory_order_release);
    }

    void WebViewManager::Log(int32_t level, const char* message)
    {
        if (m_logCallback)
        {
            m_logCallback(level, message);
        }
#ifdef WEBVIEW_TOOLKIT_DEBUG
        OutputDebugStringA("[WebViewToolkit] ");
        OutputDebugStringA(message);
        OutputDebugStringA("\n");
#endif
    }

    void WebViewManager::LogW(int32_t level, const wchar_t* message)
    {
        Log(level, ToNarrow(message).c_str());
    }

    Result WebViewManager::InitializeWinRT()
    {
        try
        {
            // Initialize WinRT apartment (must be STA for UI)
            try
            {
                winrt::init_apartment(winrt::apartment_type::single_threaded);
                Log(0, "WebViewManager: WinRT apartment initialized");
            }
            catch (const winrt::hresult_error& e)
            {
                // RPC_E_CHANGED_MODE means apartment already initialized - this is OK
                if (e.code() != RPC_E_CHANGED_MODE)
                {
                    Log(2, "WebViewManager: Failed to init apartment");
                    return Result::ErrorUnknown;
                }
            }

            if (!s_dispatcherQueueController)
            {
                DispatcherQueueOptions options{};
                options.dwSize = sizeof(DispatcherQueueOptions);
                options.threadType = DQTYPE_THREAD_CURRENT;
                options.apartmentType = DQTAT_COM_STA;

                ABI::Windows::System::IDispatcherQueueController* controller = nullptr;
                HRESULT hr = CreateDispatcherQueueController(options, &controller);
                if (FAILED(hr))
                {
                    Log(2, "WebViewManager: Failed to create DispatcherQueueController");
                    return Result::ErrorUnknown;
                }
                s_dispatcherQueueController = controller;
            }

            return Result::Success;
        }
        catch (...)
        {
            return Result::ErrorUnknown;
        }
    }

    Result WebViewManager::Initialize(IRenderAPI* renderAPI)
    {
        s_isShuttingDown.store(false, std::memory_order_release);

        std::lock_guard<std::mutex> lock(m_mutex);
        if (m_initialized) return Result::ErrorAlreadyInitialized;
        if (!renderAPI || !renderAPI->IsInitialized()) return Result::ErrorNotInitialized;

        auto result = InitializeWinRT();
        if (result != Result::Success) return result;

        m_renderAPI = renderAPI;
        m_initialized = true;

        Log(0, "WebViewManager: Initialized successfully");
        return Result::Success;
    }

    void WebViewManager::InvokeNavigationCallback(WebViewHandle handle, const wchar_t* url, bool isSuccess)
    {
        if (m_navigationCallback)
        {
            m_navigationCallback(handle, url, isSuccess);
        }
    }

    void WebViewManager::InvokeMessageCallback(WebViewHandle handle, const wchar_t* message)
    {
        if (m_messageCallback)
        {
            m_messageCallback(handle, message);
        }
    }

    void WebViewManager::Shutdown()
    {
        if (m_shutdownComplete) return;

        s_isShuttingDown.store(true, std::memory_order_release);
        
        std::lock_guard<std::mutex> lock(m_mutex);

        // Abandonment strategy for stability
        m_instances.clear(); // Destructors of WebView will handle cleanup/abandonment logic
        
        m_initialized = false;
        m_shutdownComplete = true;
        Log(0, "WebViewManager: Shutdown complete");
    }

    WebViewHandle WebViewManager::GenerateHandle()
    {
        return m_nextHandle++;
    }

    WebView* WebViewManager::GetWebView(WebViewHandle handle)
    {
        std::lock_guard<std::mutex> lock(m_mutex);
        auto it = m_instances.find(handle);
        return (it != m_instances.end()) ? it->second.get() : nullptr;
    }

    Result WebViewManager::CreateWebView(const WebViewCreateParams& params, WebViewHandle& outHandle)
    {
        std::lock_guard<std::mutex> lock(m_mutex);
        if (!m_initialized) return Result::ErrorNotInitialized;

        WebViewHandle handle = GenerateHandle();
        outHandle = handle;

        auto webView = std::make_unique<WebView>(handle, params, this);
        
        Result result = webView->Initialize();
        if (result != Result::Success)
        {
            return result;
        }

        m_instances[handle] = std::move(webView);
        
        Log(0, "WebViewManager: WebView created");
        return Result::Success;
    }

    Result WebViewManager::DestroyWebView(WebViewHandle handle)
    {
        std::lock_guard<std::mutex> lock(m_mutex);
        auto it = m_instances.find(handle);
        if (it == m_instances.end()) return Result::ErrorInvalidHandle;

        m_instances.erase(it); // unique_ptr destructor calls data.Shutdown()
        Log(0, "WebViewManager: WebView destroyed");
        return Result::Success;
    }

    // ========================================================================
    // Delegated Methods
    // ========================================================================

    Result WebViewManager::ResizeWebView(WebViewHandle handle, uint32_t width, uint32_t height)
    {
        auto webView = GetWebView(handle);
        return webView ? webView->Resize(width, height) : Result::ErrorInvalidHandle;
    }

    Result WebViewManager::Navigate(WebViewHandle handle, const wchar_t* url)
    {
        auto webView = GetWebView(handle);
        return webView ? webView->Navigate(url) : Result::ErrorInvalidHandle;
    }

    Result WebViewManager::NavigateToString(WebViewHandle handle, const wchar_t* html)
    {
        auto webView = GetWebView(handle);
        return webView ? webView->NavigateToString(html) : Result::ErrorInvalidHandle;
    }

    Result WebViewManager::ExecuteScript(WebViewHandle handle, const wchar_t* script)
    {
        auto webView = GetWebView(handle);
        return webView ? webView->ExecuteScript(script) : Result::ErrorInvalidHandle;
    }

    Result WebViewManager::GoBack(WebViewHandle handle)
    {
        auto webView = GetWebView(handle);
        return webView ? webView->GoBack() : Result::ErrorInvalidHandle;
    }

    Result WebViewManager::GoForward(WebViewHandle handle)
    {
        auto webView = GetWebView(handle);
        return webView ? webView->GoForward() : Result::ErrorInvalidHandle;
    }

    bool WebViewManager::CanGoBack(WebViewHandle handle)
    {
        auto webView = GetWebView(handle);
        return webView ? webView->CanGoBack() : false;
    }

    bool WebViewManager::CanGoForward(WebViewHandle handle)
    {
        auto webView = GetWebView(handle);
        return webView ? webView->CanGoForward() : false;
    }

    Result WebViewManager::SendMouseEvent(WebViewHandle handle, const MouseEventParams& event)
    {
        auto webView = GetWebView(handle);
        return webView ? webView->SendMouseEvent(event) : Result::ErrorInvalidHandle;
    }

    Result WebViewManager::SendKeyEvent(WebViewHandle handle, const KeyEventParams& event)
    {
        auto webView = GetWebView(handle);
        return webView ? webView->SendKeyEvent(event) : Result::ErrorInvalidHandle;
    }

    void WebViewManager::UpdateTexture(WebViewHandle handle)
    {
        auto webView = GetWebView(handle);
        if (webView)
        {
            webView->UpdateTexture();
        }
    }

    void WebViewManager::UpdateAllTextures()
    {
        std::unique_lock<std::mutex> lock(m_mutex, std::try_to_lock);
        if (!lock.owns_lock()) return;

        for (auto& pair : m_instances)
        {
             pair.second->UpdateTexture(); 
        }
    }
    
    void WebViewManager::OnDeviceLost()
    {
        std::lock_guard<std::mutex> lock(m_mutex);
        Log(1, "WebViewManager: Device lost, notifying instances");
        for (auto& pair : m_instances)
        {
            pair.second->OnDeviceLost();
        }

        if (m_deviceEventCallback)
        {
            m_deviceEventCallback(DeviceEventType::DeviceLost);
        }
    }

    void WebViewManager::OnDeviceRestored()
    {
        std::lock_guard<std::mutex> lock(m_mutex);
        Log(0, "WebViewManager: Device restored, notifying instances");
        for (auto& pair : m_instances)
        {
            pair.second->OnDeviceRestored();
        }

        if (m_deviceEventCallback)
        {
            m_deviceEventCallback(DeviceEventType::DeviceRestored);
        }
    }

} // namespace WebViewToolkit
