#include "WebViewToolkit/WebView.h"
#include "WebViewToolkit/WebViewManager.h"
#include "WebViewToolkit/WebViewCapture.h"

// Windows headers
// Windows headers
#include <Windows.h>
#include <objbase.h>
#include <dwmapi.h>

// WebView2
#include <WebView2.h>
#include <WebView2EnvironmentOptions.h>
#include <wrl.h>

#include <string>

#pragma comment(lib, "dwmapi.lib")
#pragma comment(lib, "shcore.lib")

namespace WebViewToolkit
{
    // Host Window Helper
    static const wchar_t* g_windowClassName = L"WebViewToolkitHostWindow";
    static bool g_windowClassRegistered = false;

    static LRESULT CALLBACK HostWindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
    {
        return DefWindowProcW(hwnd, msg, wParam, lParam);
    }

    static bool RegisterWindowClass()
    {
        if (g_windowClassRegistered) return true;

        WNDCLASSEXW wc = {};
        wc.cbSize = sizeof(wc);
        wc.lpfnWndProc = HostWindowProc;
        wc.hInstance = GetModuleHandleW(nullptr);
        wc.lpszClassName = g_windowClassName;
        wc.style = CS_HREDRAW | CS_VREDRAW;

        if (!RegisterClassExW(&wc))
        {
            if (GetLastError() != ERROR_CLASS_ALREADY_EXISTS) return false;
        }

        g_windowClassRegistered = true;
        return true;
    }

    WebView::WebView(WebViewHandle handle, const WebViewCreateParams& params, WebViewManager* manager)
        : m_handle(handle)
        , m_manager(manager)
        , m_width(params.width)
        , m_height(params.height)
        , m_devToolsEnabled(params.enableDevTools)
    {
        if (params.userDataFolder)
        {
            m_userDataFolder = params.userDataFolder;
        }
        else
        {
            wchar_t tempPath[MAX_PATH];
            GetTempPathW(MAX_PATH, tempPath);
            m_userDataFolder = std::wstring(tempPath) + L"WebViewToolkit\\";
        }

        if (params.initialUrl)
        {
            m_pendingUrl = params.initialUrl;
        }
    }

    WebView::~WebView()
    {
        Shutdown();
    }

    void WebView::Shutdown()
    {
        if (m_state == WebViewState::Destroyed) return;
        m_state = WebViewState::Destroyed;

        // Note: Actual cleanup of COM objects and windows happens here if not in global shutdown
        // Ideally this matches the logic from WebViewManager::ReleaseWebViewInstance
        // For the sake of this refactor, we are moving logic.

        // 0. Release Capture (must be first)
        if (m_capture)
        {
            m_capture->Shutdown();
            m_capture.reset();
        }

        // 1. Release Texture (must happen before RenderAPI shutdown, but after Capture)
        if (m_texturePtr && m_manager)
        {
            IRenderAPI* api = m_manager->GetRenderAPI();
            if (api)
            {
                api->DestroySharedTexture(m_texturePtr);
            }
            m_texturePtr = nullptr;
        }

        // 2. Close Controller
        if (m_controller)
        {
            auto controller = static_cast<ICoreWebView2Controller*>(m_controller);
            if (!WebViewManager::IsShuttingDown())
            {
                controller->Close();
            }
            controller->Release();
            m_controller = nullptr;
        }

        // 3. Destroy Window
        DestroyHostWindow();

        // 4. Release other COM objects
        if (m_compositionController)
        {
            static_cast<ICoreWebView2CompositionController*>(m_compositionController)->Release();
            m_compositionController = nullptr;
        }

        if (m_webView)
        {
            static_cast<ICoreWebView2*>(m_webView)->Release();
            m_webView = nullptr;
        }

        if (m_environment)
        {
            static_cast<ICoreWebView2Environment*>(m_environment)->Release();
            m_environment = nullptr;
        }
    }

    void* WebView::CreateHostWindow(uint32_t width, uint32_t height)
    {
        if (!RegisterWindowClass()) return nullptr;

        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        
        HWND hwnd = CreateWindowExW(
            WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_LAYERED | WS_EX_TRANSPARENT,
            g_windowClassName,
            L"WebViewToolkitHost",
            WS_POPUP,
            screenWidth + 100, 0,
            static_cast<int>(width),
            static_cast<int>(height),
            nullptr, nullptr,
            GetModuleHandleW(nullptr),
            nullptr
        );

        if (hwnd)
        {
            SetLayeredWindowAttributes(hwnd, 0, 1, LWA_ALPHA);
            ShowWindow(hwnd, SW_SHOWNOACTIVATE);
        }

        return hwnd;
    }

    void WebView::DestroyHostWindow()
    {
        if (m_hostWindow)
        {
            HWND hwnd = static_cast<HWND>(m_hostWindow);
            if (IsWindow(hwnd))
            {
                if (!WebViewManager::IsShuttingDown())
                {
                    ShowWindow(hwnd, SW_HIDE);
                    DestroyWindow(hwnd);
                }
            }
            m_hostWindow = nullptr;
        }
    }

    Result WebView::Initialize()
    {
        m_hostWindow = CreateHostWindow(m_width, m_height);
        if (!m_hostWindow) return Result::ErrorUnknown;

        // Create shared texture
        IRenderAPI* renderAPI = m_manager ? m_manager->GetRenderAPI() : nullptr;
        if (!renderAPI) return Result::ErrorNotInitialized; // Should not happen

        Result result = renderAPI->CreateSharedTexture(m_width, m_height, &m_texturePtr);
        if (result != Result::Success) return result;

        return InitializeWebViewEnvironment();
    }

    Result WebView::InitializeWebViewEnvironment()
    {
        m_state = WebViewState::CreatingEnvironment;
        
        auto options = Microsoft::WRL::Make<CoreWebView2EnvironmentOptions>();
        HRESULT hr = CreateCoreWebView2EnvironmentWithOptions(
            nullptr,
            m_userDataFolder.c_str(),
            options.Get(),
            Microsoft::WRL::Callback<ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler>(
                [this](HRESULT result, ICoreWebView2Environment* env) -> HRESULT
                {
                    OnEnvironmentCreated(result, env);
                    return S_OK;
                }
            ).Get()
        );

        return FAILED(hr) ? Result::ErrorWebViewCreationFailed : Result::Success;
    }

    void WebView::OnEnvironmentCreated(long result, void* environmentPtr)
    {
        if (FAILED(result) || !environmentPtr)
        {
            m_state = WebViewState::Error;
            return; // TODO: Log failure
        }

        auto environment = static_cast<ICoreWebView2Environment*>(environmentPtr);
        environment->AddRef();
        m_environment = environment;

        InitializeCompositionController();
    }

    Result WebView::InitializeCompositionController()
    {
        m_state = WebViewState::CreatingController;
        auto environment = static_cast<ICoreWebView2Environment*>(m_environment);
        
        Microsoft::WRL::ComPtr<ICoreWebView2Environment3> env3;
        if (FAILED(environment->QueryInterface(IID_PPV_ARGS(&env3))))
        {
            m_state = WebViewState::Error;
            return Result::ErrorWebViewCreationFailed;
        }

        HRESULT hr = env3->CreateCoreWebView2CompositionController(
            static_cast<HWND>(m_hostWindow),
            Microsoft::WRL::Callback<ICoreWebView2CreateCoreWebView2CompositionControllerCompletedHandler>(
                [this](HRESULT result, ICoreWebView2CompositionController* controller) -> HRESULT
                {
                    OnCompositionControllerCreated(result, controller);
                    return S_OK;
                }
            ).Get()
        );

        return FAILED(hr) ? Result::ErrorWebViewCreationFailed : Result::Success;
    }

    void WebView::OnCompositionControllerCreated(long result, void* controllerPtr)
    {
        if (FAILED(result) || !controllerPtr)
        {
            m_state = WebViewState::Error;
            return;
        }

        auto compositionController = static_cast<ICoreWebView2CompositionController*>(controllerPtr);
        compositionController->AddRef();
        m_compositionController = compositionController;

        // Get regular controller
        Microsoft::WRL::ComPtr<ICoreWebView2Controller> controller;
        if (FAILED(compositionController->QueryInterface(IID_PPV_ARGS(&controller))))
        {
            m_state = WebViewState::Error;
            return;
        }
        
        controller->AddRef();
        m_controller = controller.Get();

        // Get CoreWebView2
        Microsoft::WRL::ComPtr<ICoreWebView2> webView;
        if (FAILED(controller->get_CoreWebView2(&webView)))
        {
            m_state = WebViewState::Error;
            return;
        }

        webView->AddRef();
        m_webView = webView.Get();

        // Basic settings
        Microsoft::WRL::ComPtr<ICoreWebView2Settings> settings;
        if (SUCCEEDED(webView->get_Settings(&settings)))
        {
            settings->put_AreDevToolsEnabled(m_devToolsEnabled);
            settings->put_AreDefaultContextMenusEnabled(TRUE);
            settings->put_IsZoomControlEnabled(FALSE);
            settings->put_IsStatusBarEnabled(FALSE);
        }

        RECT bounds = { 0, 0, static_cast<LONG>(m_width), static_cast<LONG>(m_height) };
        controller->put_Bounds(bounds);
        controller->put_IsVisible(TRUE);

        m_state = WebViewState::Ready;

        // Initialize Capture
        m_capture = std::make_unique<WebViewCapture>(this, m_manager->GetRenderAPI());
        m_capture->Initialize();

        // Register events
        EventRegistrationToken token;
        auto webView2 = static_cast<ICoreWebView2*>(m_webView);

        webView2->add_NavigationCompleted(
            Microsoft::WRL::Callback<ICoreWebView2NavigationCompletedEventHandler>(
                [this](ICoreWebView2* sender, ICoreWebView2NavigationCompletedEventArgs* args) -> HRESULT
                {
                    if (m_manager)
                    {
                        BOOL isSuccess = FALSE;
                        args->get_IsSuccess(&isSuccess);
                        
                        LPWSTR uri = nullptr;
                        sender->get_Source(&uri);
                        
                        m_manager->InvokeNavigationCallback(m_handle, uri ? uri : L"", isSuccess != FALSE);
                        
                        if (uri) CoTaskMemFree(uri);
                    }
                    return S_OK;
                }
            ).Get(),
            &token
        );

        webView2->add_WebMessageReceived(
            Microsoft::WRL::Callback<ICoreWebView2WebMessageReceivedEventHandler>(
                [this](ICoreWebView2* sender, ICoreWebView2WebMessageReceivedEventArgs* args) -> HRESULT
                {
                    UNREFERENCED_PARAMETER(sender);
                    if (m_manager)
                    {
                        LPWSTR message = nullptr;
                        args->TryGetWebMessageAsString(&message);
                        
                        if (message)
                        {
                            m_manager->InvokeMessageCallback(m_handle, message);
                            CoTaskMemFree(message);
                        }
                    }
                    return S_OK;
                }
            ).Get(),
            &token
        );

        // Navigate
        if (!m_pendingUrl.empty())
        {
            webView2->Navigate(m_pendingUrl.c_str());
        }
        else
        {
            webView2->Navigate(L"about:blank");
        }
    }

    Result WebView::Navigate(const wchar_t* url)
    {
        if (!m_webView) return Result::ErrorNotInitialized;
        return SUCCEEDED(static_cast<ICoreWebView2*>(m_webView)->Navigate(url)) ? Result::Success : Result::ErrorNavigationFailed;
    }

    Result WebView::NavigateToString(const wchar_t* html)
    {
        if (!m_webView) return Result::ErrorNotInitialized;
        return SUCCEEDED(static_cast<ICoreWebView2*>(m_webView)->NavigateToString(html)) ? Result::Success : Result::ErrorNavigationFailed;
    }

    Result WebView::ExecuteScript(const wchar_t* script)
    {
         if (!m_webView) return Result::ErrorNotInitialized;
         return SUCCEEDED(static_cast<ICoreWebView2*>(m_webView)->ExecuteScript(script, nullptr)) ? Result::Success : Result::ErrorUnknown;
    }

    Result WebView::GoBack()
    {
        if (!m_webView) return Result::ErrorNotInitialized;
        HRESULT hr = static_cast<ICoreWebView2*>(m_webView)->GoBack();
        return SUCCEEDED(hr) ? Result::Success : Result::ErrorUnknown;
    }

    Result WebView::GoForward()
    {
        if (!m_webView) return Result::ErrorNotInitialized;
        HRESULT hr = static_cast<ICoreWebView2*>(m_webView)->GoForward();
        return SUCCEEDED(hr) ? Result::Success : Result::ErrorUnknown;
    }

    bool WebView::CanGoBack()
    {
        if (!m_webView) return false;
        BOOL value = FALSE;
        static_cast<ICoreWebView2*>(m_webView)->get_CanGoBack(&value);
        return value;
    }

    bool WebView::CanGoForward()
    {
        if (!m_webView) return false;
        BOOL value = FALSE;
        static_cast<ICoreWebView2*>(m_webView)->get_CanGoForward(&value);
        return value;
    }

    Result WebView::Resize(uint32_t width, uint32_t height)
    {
        if (!m_controller) return Result::ErrorNotInitialized;
        m_width = width;
        m_height = height;

        // Resize texture
        if (m_texturePtr && m_manager)
        {
            IRenderAPI* api = m_manager->GetRenderAPI();
            if (api)
            {
                void* newTexture = nullptr;
                // Note: ResizeSharedTexture destroys the old one
                Result res = api->ResizeSharedTexture(m_texturePtr, width, height, &newTexture);
                if (res == Result::Success)
                {
                    m_texturePtr = newTexture;
                }
            }
        }

        // Resize WebView2 Controller
        RECT bounds = { 0, 0, static_cast<LONG>(width), static_cast<LONG>(height) };
        static_cast<ICoreWebView2Controller*>(m_controller)->put_Bounds(bounds);

        // Resize the HWND host window
        // Windows Graphics Capture captures the window's client area,
        // so the window itself must be resized to match the new dimensions
        if (m_hostWindow)
        {
            HWND hwnd = static_cast<HWND>(m_hostWindow);
            SetWindowPos(
                hwnd,
                nullptr,
                0, 0,  // Position (we only care about size)
                static_cast<int>(width),
                static_cast<int>(height),
                SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE
            );
        }

        // Resize Capture (Visuals & FramePool)
        if (m_capture)
        {
            m_capture->Resize(width, height);
        }

        return Result::Success;
    }

    Result WebView::SendMouseEvent(const MouseEventParams& params)
    {
        if (!m_compositionController) return Result::ErrorNotInitialized;

        auto compController = static_cast<ICoreWebView2CompositionController*>(m_compositionController);
        
        // Convert normalized coordinates to pixel coordinates
        POINT point;
        point.x = static_cast<LONG>(params.x * m_width);
        point.y = static_cast<LONG>(params.y * m_height);

        COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS virtualKeys = COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_NONE;

        COREWEBVIEW2_MOUSE_EVENT_KIND kind;
        switch (params.type)
        {
        case MouseEventType::Move:
            kind = COREWEBVIEW2_MOUSE_EVENT_KIND_MOVE;
            break;
        case MouseEventType::Down:
            if (params.button == MouseButton::Left)
            {
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND_LEFT_BUTTON_DOWN;
                virtualKeys = static_cast<COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS>(virtualKeys | COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_LEFT_BUTTON);
            }
            else if (params.button == MouseButton::Right)
            {
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND_RIGHT_BUTTON_DOWN;
                virtualKeys = static_cast<COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS>(virtualKeys | COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_RIGHT_BUTTON);
            }
            else
            {
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND_MIDDLE_BUTTON_DOWN;
                virtualKeys = static_cast<COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS>(virtualKeys | COREWEBVIEW2_MOUSE_EVENT_VIRTUAL_KEYS_MIDDLE_BUTTON);
            }
            break;
        case MouseEventType::Up:
            if (params.button == MouseButton::Left)
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND_LEFT_BUTTON_UP;
            else if (params.button == MouseButton::Right)
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND_RIGHT_BUTTON_UP;
            else
                kind = COREWEBVIEW2_MOUSE_EVENT_KIND_MIDDLE_BUTTON_UP;
            break;
        case MouseEventType::Wheel:
            kind = COREWEBVIEW2_MOUSE_EVENT_KIND_WHEEL;
            break;
        case MouseEventType::Leave:
            kind = COREWEBVIEW2_MOUSE_EVENT_KIND_LEAVE;
            break;
        default:
            return Result::ErrorUnknown;
        }

        UINT32 mouseData = 0;
        if (params.type == MouseEventType::Wheel)
        {
            mouseData = static_cast<UINT32>(static_cast<int>(params.wheelDelta * WHEEL_DELTA));
        }

        HRESULT hr = compController->SendMouseInput(kind, virtualKeys, mouseData, point);
        return SUCCEEDED(hr) ? Result::Success : Result::ErrorUnknown;
    }
 
    Result WebView::SendKeyEvent(const KeyEventParams& params)
    {
        if (!m_hostWindow) return Result::ErrorNotInitialized;
        
        // Logic from WebViewManager.cpp
        HWND hwnd = static_cast<HWND>(m_hostWindow);
        LPARAM lParam = (params.scanCode << 16) | 1;
        if (!params.isKeyDown) lParam |= (1 << 30) | (1 << 31);

        if (params.isKeyDown)
             PostMessageW(hwnd, params.isSystemKey ? WM_SYSKEYDOWN : WM_KEYDOWN, params.virtualKeyCode, lParam);
        else
             PostMessageW(hwnd, params.isSystemKey ? WM_SYSKEYUP : WM_KEYUP, params.virtualKeyCode, lParam);

        return Result::Success;
    }

    void WebView::UpdateTexture()
    {
        // Must happen on render thread
        if (m_capture && m_texturePtr)
        {
            m_capture->UpdateTexture(m_texturePtr);
        }
    }

    void WebView::OnDeviceLost()
    {
        // 1. Stop capture
        if (m_capture)
        {
            m_capture->Shutdown();
        }

        // 2. Release texture pointer
        // We don't call DestroySharedTexture because the device is already gone/released
        m_texturePtr = nullptr;

        m_state = WebViewState::Error;
    }

    void WebView::OnDeviceRestored()
    {
        if (m_state == WebViewState::Destroyed) return;

        // 1. Recreate shared texture
        IRenderAPI* renderAPI = m_manager ? m_manager->GetRenderAPI() : nullptr;
        if (!renderAPI || !renderAPI->IsInitialized()) return;

        Result result = renderAPI->CreateSharedTexture(m_width, m_height, &m_texturePtr);
        if (result != Result::Success) return;

        // 2. Restart capture with new device
        m_state = WebViewState::Ready;
        if (m_capture)
        {
            m_capture->Initialize();
        }
    }

} // namespace WebViewToolkit
