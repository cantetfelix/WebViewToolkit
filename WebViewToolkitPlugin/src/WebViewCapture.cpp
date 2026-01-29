#include "WebViewToolkit/WebViewCapture.h"
#include "WebViewToolkit/WebView.h"

// Windows headers
#include <Windows.h>
#include <objbase.h>
#include <wrl.h>
#include <WebView2.h>
#include <shcore.h>
#include <DispatcherQueue.h> // Move up

#include "RenderAPI/DebugLog.h"
using WebViewToolkit::DebugLog;

// WinRT headers
#include <winrt/base.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.System.h>
#include <winrt/Windows.UI.Composition.h>
#include <winrt/Windows.UI.Composition.Desktop.h>
#include <winrt/Windows.Graphics.Capture.h>
#include <winrt/Windows.Graphics.DirectX.Direct3D11.h>
#include <windows.ui.composition.interop.h>
#include <windows.graphics.capture.interop.h>
#include <Windows.Graphics.DirectX.Direct3D11.interop.h>

#include <d3d11.h>

namespace WebViewToolkit
{
    namespace winrt_impl
    {
        using namespace winrt;
        using namespace winrt::Windows::UI::Composition;
        using namespace winrt::Windows::UI::Composition::Desktop;
        using namespace winrt::Windows::Graphics;
        using namespace winrt::Windows::Graphics::Capture;
        using namespace winrt::Windows::Graphics::DirectX;
        using namespace winrt::Windows::Graphics::DirectX::Direct3D11;
    }

    struct WindowTargetWrapper { winrt_impl::DesktopWindowTarget Value{ nullptr }; };
    struct CaptureItemWrapper { winrt_impl::GraphicsCaptureItem Value{ nullptr }; };
    struct FramePoolWrapper { winrt_impl::Direct3D11CaptureFramePool Value{ nullptr }; };
    struct SessionWrapper { winrt_impl::GraphicsCaptureSession Value{ nullptr }; };


    WebViewCapture::WebViewCapture(WebView* webView, IRenderAPI* renderAPI)
        : m_webView(webView)
        , m_renderAPI(renderAPI)
    {
    }

    WebViewCapture::~WebViewCapture()
    {
        Shutdown();
    }

    void WebViewCapture::Shutdown()
    {
        try
        {
            // 1. Close Session (Stops capture)
            // 1. Close Session (Stops capture)
            if (m_session)
            {
                auto wrapper = static_cast<SessionWrapper*>(m_session);
                wrapper->Value.Close();
                delete wrapper;
                m_session = nullptr;
            }

            // 2. Close Frame Pool
            if (m_framePool)
            {
                auto wrapper = static_cast<FramePoolWrapper*>(m_framePool);
                wrapper->Value.Close();
                delete wrapper;
                m_framePool = nullptr;
            }

            // 3. Clear Item
            if (m_captureItem)
            {
                auto wrapper = static_cast<CaptureItemWrapper*>(m_captureItem);
                delete wrapper;
                m_captureItem = nullptr;
            }

            // 4. Release Visuals
            if (m_webViewVisual)
            {
                static_cast<ABI::Windows::UI::Composition::IVisual*>(m_webViewVisual)->Release();
                m_webViewVisual = nullptr;
            }

            if (m_rootVisual)
            {
                static_cast<ABI::Windows::UI::Composition::IContainerVisual*>(m_rootVisual)->Release();
                m_rootVisual = nullptr;
            }

            if (m_windowTarget)
            {
                auto wrapper = static_cast<WindowTargetWrapper*>(m_windowTarget);
                delete wrapper;
                m_windowTarget = nullptr;
            }

            if (m_compositor)
            {
                static_cast<ABI::Windows::UI::Composition::ICompositor*>(m_compositor)->Release();
                m_compositor = nullptr;
            }

            // 5. Release WinRT D3D device
            if (m_d3dDevice)
            {
                static_cast<::IInspectable*>(m_d3dDevice)->Release();
                m_d3dDevice = nullptr;
            }
        }
        catch (...)
        {
            // Log error
        }
    }

    Result WebViewCapture::Initialize()
    {
        DebugLog::Log("WebViewCapture::Initialize: Starting...");

        if (!m_webView || !m_webView->IsReady())
        {
            DebugLog::Log("WebViewCapture::Initialize: ERROR - WebView not ready");
            return Result::ErrorNotInitialized;
        }

        try
        {
            DebugLog::Log("WebViewCapture::Initialize: Calling InitializeVisualTree...");
            InitializeVisualTree();
            DebugLog::Log("WebViewCapture::Initialize: InitializeVisualTree completed");

            DebugLog::Log("WebViewCapture::Initialize: Calling InitializeGraphicsCapture...");
            InitializeGraphicsCapture();
            DebugLog::Log("WebViewCapture::Initialize: InitializeGraphicsCapture completed");

            DebugLog::Log("WebViewCapture::Initialize: Success!");
            return Result::Success;
        }
        catch(...)
        {
            DebugLog::Log("WebViewCapture::Initialize: ERROR - Exception caught");
            return Result::ErrorUnknown;
        }
    }

    void WebViewCapture::InitializeVisualTree()
    {
        // Setup WinRT Compositor and Visual tree
        try
        {
            // Objects retrieved from WebView class
            void* environmentPtr = m_webView->GetEnvironment();
            void* compositionControllerPtr = m_webView->GetCompositionController();
            HWND hwnd = static_cast<HWND>(m_webView->GetHostWindow());

            if (!environmentPtr || !compositionControllerPtr || !hwnd)
            {
                 // Invalid state
                 return;
            }

            auto compositionController = static_cast<ICoreWebView2CompositionController*>(compositionControllerPtr);

            // Create Compositor (WinRT)
            // We need to use the DispatcherQueue from WebViewManager (which initialized WinRT)
            // The compositor creation implicitly uses the thread's DispatcherQueue
            auto compositor = winrt_impl::Compositor();
            auto abiCompositor = compositor.as<ABI::Windows::UI::Composition::ICompositor>();
            abiCompositor->AddRef();
            m_compositor = abiCompositor.get();

            // Create DesktopWindowTarget to connect visual tree to HWND
            // This is REQUIRED for GraphicsCapture to work
            
            // Use the interop interface
            winrt::com_ptr<ABI::Windows::UI::Composition::Desktop::ICompositorDesktopInterop> compositorInterop;
            winrt::check_hresult(abiCompositor->QueryInterface(IID_PPV_ARGS(compositorInterop.put())));
            
            winrt::com_ptr<IUnknown> windowTargetUnk;
            winrt::check_hresult(compositorInterop->CreateDesktopWindowTarget(
                hwnd,
                FALSE,  // isTopmost
                reinterpret_cast<ABI::Windows::UI::Composition::Desktop::IDesktopWindowTarget**>(windowTargetUnk.put())
            ));
            
            auto windowTarget = windowTargetUnk.as<winrt_impl::DesktopWindowTarget>();
            
            // We need to store this to keep it alive
            // For now, we just use void* storage
            auto targetRef = new WindowTargetWrapper{ windowTarget };
            m_windowTarget = targetRef;

            // Create root container visual
            winrt::com_ptr<ABI::Windows::UI::Composition::IContainerVisual> rootVisual;
            winrt::check_hresult(abiCompositor->CreateContainerVisual(rootVisual.put()));
            
            // Get IVisual interface to setting size
            auto rootVisualAsVisual = rootVisual.as<ABI::Windows::UI::Composition::IVisual>();
            ABI::Windows::Foundation::Numerics::Vector2 size{ 
                static_cast<float>(m_webView->GetWidth()), 
                static_cast<float>(m_webView->GetHeight()) 
            };
            rootVisualAsVisual->put_Size(size);
            rootVisualAsVisual->put_IsVisible(true);
            
            rootVisual->AddRef();
            m_rootVisual = rootVisual.get();
            
            // Connect root visual to window target
            winrt_impl::Visual rtRootVisual{ nullptr };
            winrt::copy_from_abi(rtRootVisual, rootVisualAsVisual.get());
            windowTarget.Root(rtRootVisual);

            // Create child visual for WebView2
            winrt::com_ptr<ABI::Windows::UI::Composition::IContainerVisual> webViewContainer;
            winrt::check_hresult(abiCompositor->CreateContainerVisual(webViewContainer.put()));
            
            auto webViewVisual = webViewContainer.as<ABI::Windows::UI::Composition::IVisual>();
            
            // Make it fill the parent using RelativeSizeAdjustment
            auto webViewVisual2 = webViewContainer.as<ABI::Windows::UI::Composition::IVisual2>();
            if (webViewVisual2)
            {
                ABI::Windows::Foundation::Numerics::Vector2 adjustment{ 1.0f, 1.0f };
                webViewVisual2->put_RelativeSizeAdjustment(adjustment);
            }
            
            webViewVisual->AddRef();
            m_webViewVisual = webViewVisual.get();

            // Add webview visual to root's children
            winrt::com_ptr<ABI::Windows::UI::Composition::IVisualCollection> children;
            winrt::check_hresult(rootVisual->get_Children(children.put()));
            winrt::check_hresult(children->InsertAtTop(webViewVisual.get()));

            // Set the WebView's RootVisualTarget to our visual
            // This is what makes WebView2 render into our Composition Visual
            winrt::check_hresult(compositionController->put_RootVisualTarget(webViewVisual.get()));
        }
        catch (...)
        {
            // Log error
        }
    }

    void WebViewCapture::InitializeGraphicsCapture()
    {
        DebugLog::Log("InitializeGraphicsCapture: Starting...");
        try
        {
            // Get D3D11 device from RenderAPI for capture operations
            DebugLog::Log("InitializeGraphicsCapture: Getting capture D3D11 device...");
            auto d3dDevice = static_cast<ID3D11Device*>(m_renderAPI->GetCaptureD3D11Device());
            if (!d3dDevice)
            {
                DebugLog::Log("InitializeGraphicsCapture: ERROR - Capture D3D11 device is null!");
                return;
            }
            DebugLog::Log("InitializeGraphicsCapture: Got capture device: %p", d3dDevice);

            // Create WinRT D3D device with error checking
            DebugLog::Log("InitializeGraphicsCapture: Querying IDXGIDevice interface...");
            Microsoft::WRL::ComPtr<IDXGIDevice> dxgiDevice;
            HRESULT hr = d3dDevice->QueryInterface(IID_PPV_ARGS(&dxgiDevice));
            if (FAILED(hr))
            {
                DebugLog::Log("InitializeGraphicsCapture: ERROR - Failed to get DXGI device: 0x%08X", hr);
                return;
            }
            DebugLog::Log("InitializeGraphicsCapture: Got DXGI device");

            DebugLog::Log("InitializeGraphicsCapture: Creating WinRT Direct3D device...");
            winrt::com_ptr<::IInspectable> inspectable;
            hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice.Get(), inspectable.put());
            if (FAILED(hr))
            {
                DebugLog::Log("InitializeGraphicsCapture: ERROR - Failed to create WinRT Direct3D device: 0x%08X", hr);
                return;
            }
            DebugLog::Log("InitializeGraphicsCapture: Created WinRT device");

            auto rtDevice = inspectable.as<winrt_impl::IDirect3DDevice>();
            DebugLog::Log("InitializeGraphicsCapture: Cast to IDirect3DDevice succeeded");

            // Store WinRT device for reuse during resize
            // CRITICAL: We must reuse the same WinRT device wrapper, not create new ones
            inspectable->AddRef();
            m_d3dDevice = inspectable.get();
            DebugLog::Log("InitializeGraphicsCapture: Stored WinRT device for reuse");

            // Get the HWND for capture
            DebugLog::Log("InitializeGraphicsCapture: Getting HWND...");
            HWND hwnd = static_cast<HWND>(m_webView->GetHostWindow());
            if (!hwnd || !IsWindow(hwnd))
            {
                DebugLog::Log("InitializeGraphicsCapture: ERROR - Invalid HWND (%p)", hwnd);
                return;
            }
            DebugLog::Log("InitializeGraphicsCapture: Got valid HWND: %p", hwnd);

            // Create GraphicsCaptureItem from HWND
            DebugLog::Log("InitializeGraphicsCapture: Creating GraphicsCaptureItem from HWND...");
            auto interop = winrt::get_activation_factory<winrt_impl::GraphicsCaptureItem, IGraphicsCaptureItemInterop>();
            winrt::com_ptr<ABI::Windows::Graphics::Capture::IGraphicsCaptureItem> captureItemAbi;
            hr = interop->CreateForWindow(
                hwnd,
                winrt::guid_of<ABI::Windows::Graphics::Capture::IGraphicsCaptureItem>(),
                reinterpret_cast<void**>(captureItemAbi.put())
            );
            if (FAILED(hr))
            {
                DebugLog::Log("InitializeGraphicsCapture: ERROR - Failed to create GraphicsCaptureItem: 0x%08X", hr);
                return;
            }
            DebugLog::Log("InitializeGraphicsCapture: Created GraphicsCaptureItem");

            winrt_impl::GraphicsCaptureItem captureItem{ nullptr };
            winrt::copy_from_abi(captureItem, captureItemAbi.get());

            // Create frame pool with error checking
            auto pixelFormat = winrt_impl::DirectXPixelFormat::B8G8R8A8UIntNormalized;
            winrt_impl::SizeInt32 size = captureItem.Size();

            // Ensure minimum size
            if (size.Width <= 0) size.Width = static_cast<int32_t>(m_webView->GetWidth());
            if (size.Height <= 0) size.Height = static_cast<int32_t>(m_webView->GetHeight());

            DebugLog::Log("InitializeGraphicsCapture: Creating frame pool (size: %dx%d, buffers: 2)...", size.Width, size.Height);
            auto framePool = winrt_impl::Direct3D11CaptureFramePool::Create(
                rtDevice,
                pixelFormat,
                2,
                size
            );
            DebugLog::Log("InitializeGraphicsCapture: Frame pool created");

            // Create capture session
            DebugLog::Log("InitializeGraphicsCapture: Creating capture session...");
            auto session = framePool.CreateCaptureSession(captureItem);
            DebugLog::Log("InitializeGraphicsCapture: Capture session created");

            // Store objects
            m_captureItem = new CaptureItemWrapper{ captureItem };
            m_framePool = new FramePoolWrapper{ framePool };
            m_session = new SessionWrapper{ session };
            DebugLog::Log("InitializeGraphicsCapture: Objects stored");

            // Start capture - this is where crashes often occur
            DebugLog::Log("InitializeGraphicsCapture: Starting capture session...");
            session.StartCapture();
            DebugLog::Log("InitializeGraphicsCapture: Capture session started successfully!");
        }
        catch (winrt::hresult_error const& ex)
        {
            DebugLog::Log("InitializeGraphicsCapture: ERROR - WinRT exception: 0x%08X - %ls", ex.code(), ex.message().c_str());
        }
        catch (std::exception const& ex)
        {
            DebugLog::Log("InitializeGraphicsCapture: ERROR - Exception: %s", ex.what());
        }
        catch (...)
        {
            DebugLog::Log("InitializeGraphicsCapture: ERROR - Unknown exception!");
        }
    }

    void WebViewCapture::UpdateTexture(void* unityTexturePtr)
    {
        static bool firstCall = true;
        if (firstCall)
        {
            DebugLog::Log("UpdateTexture: First call");
            firstCall = false;
        }

        if (!m_framePool || !unityTexturePtr)
        {
            DebugLog::Log("UpdateTexture: Early return (framePool=%p, texturePtr=%p)", m_framePool, unityTexturePtr);
            return;
        }

        try
        {
            auto wrapper = static_cast<FramePoolWrapper*>(m_framePool);
            auto framePool = wrapper->Value;

            DebugLog::Log("UpdateTexture: Calling TryGetNextFrame...");
            auto frame = framePool.TryGetNextFrame();

            if (!frame)
            {
                DebugLog::Log("UpdateTexture: No frame available");
                return;
            }
            DebugLog::Log("UpdateTexture: Got frame");

            auto surface = frame.Surface();
            if (!surface)
            {
                DebugLog::Log("UpdateTexture: No surface");
                frame.Close();  // Explicitly close frame before returning
                return;
            }
            DebugLog::Log("UpdateTexture: Got surface");

            // Get D3D11 texture from surface
            DebugLog::Log("UpdateTexture: Getting texture interface...");
            auto access = surface.as<::Windows::Graphics::DirectX::Direct3D11::IDirect3DDxgiInterfaceAccess>();
            ID3D11Texture2D* capturedTexture = nullptr;
            access->GetInterface(IID_PPV_ARGS(&capturedTexture));

            if (capturedTexture)
            {
                DebugLog::Log("UpdateTexture: Got captured texture %p", capturedTexture);

                // Use RenderAPI to handle the copy (handles D3D12 wrapping complexity)
                DebugLog::Log("UpdateTexture: Calling CopyCapturedTextureToUnityTexture...");
                m_renderAPI->CopyCapturedTextureToUnityTexture(capturedTexture, unityTexturePtr, true);
                DebugLog::Log("UpdateTexture: Copy completed");

                capturedTexture->Release();
            }
            else
            {
                DebugLog::Log("UpdateTexture: ERROR - Failed to get captured texture interface");
            }

            // Explicitly close frame to release it immediately
            frame.Close();
            DebugLog::Log("UpdateTexture: Frame closed");
        }
        catch (winrt::hresult_error const& ex)
        {
            DebugLog::Log("UpdateTexture: ERROR - WinRT exception: 0x%08X - %ls", ex.code(), ex.message().c_str());
        }
        catch (std::exception const& ex)
        {
            DebugLog::Log("UpdateTexture: ERROR - Exception: %s", ex.what());
        }
        catch (...)
        {
            DebugLog::Log("UpdateTexture: ERROR - Unknown exception!");
        }
    }

    Result WebViewCapture::Resize(uint32_t width, uint32_t height)
    {
        DebugLog::Log("Resize: Starting resize to %ux%u", width, height);

        try
        {
            // Prepare new size for both root and webview visuals
            ABI::Windows::Foundation::Numerics::Vector2 size{
                static_cast<float>(width),
                static_cast<float>(height)
            };

            // Update Visual Size
            if (m_rootVisual)
            {
                DebugLog::Log("Resize: Updating root visual size");
                auto rootVisual = static_cast<ABI::Windows::UI::Composition::IVisual*>(m_rootVisual);
                HRESULT hr = rootVisual->put_Size(size);
                if (FAILED(hr))
                {
                    DebugLog::Log("Resize: ERROR - Failed to update visual size: 0x%08X", hr);
                    return Result::ErrorUnknown;
                }
                DebugLog::Log("Resize: Root visual size updated successfully");
            }

            // Update WebView visual - keep explicit size at (0,0) since it uses RelativeSizeAdjustment
            // The webview visual is set up with RelativeSizeAdjustment(1.0, 1.0) during init
            // This makes it automatically match the parent size without needing explicit size updates
            // Setting explicit size here would ADD to the RelativeSizeAdjustment, making it too large
            // No action needed here - the RelativeSizeAdjustment automatically tracks parent size

            // Recreate capture setup from scratch
            // Note: framePool.Recreate() doesn't work reliably - it crashes when called while session is active
            // Solution: Destroy everything and recreate from scratch
            if (m_framePool && m_captureItem && m_d3dDevice)
            {
                DebugLog::Log("Resize: Recreating capture setup from scratch");

                // Step 1: Close and destroy existing session
                if (m_session)
                {
                    DebugLog::Log("Resize: Closing existing session...");
                    auto sessionWrapper = static_cast<SessionWrapper*>(m_session);
                    sessionWrapper->Value.Close();
                    delete sessionWrapper;
                    m_session = nullptr;
                    DebugLog::Log("Resize: Session closed and deleted");
                }

                // Step 2: Close and destroy existing frame pool
                DebugLog::Log("Resize: Closing existing frame pool...");
                auto oldFramePoolWrapper = static_cast<FramePoolWrapper*>(m_framePool);
                oldFramePoolWrapper->Value.Close();
                delete oldFramePoolWrapper;
                m_framePool = nullptr;
                DebugLog::Log("Resize: Frame pool closed and deleted");

                // Step 3: Get the stored WinRT device
                auto inspectable = static_cast<::IInspectable*>(m_d3dDevice);
                winrt::com_ptr<::IInspectable> devicePtr;
                devicePtr.copy_from(inspectable);
                auto rtDevice = devicePtr.as<winrt_impl::IDirect3DDevice>();

                // Step 4: Get the capture item
                auto captureItemWrapper = static_cast<CaptureItemWrapper*>(m_captureItem);
                auto captureItem = captureItemWrapper->Value;

                // Step 5: Create new frame pool with new size
                winrt_impl::SizeInt32 newSize;
                newSize.Width = static_cast<int32_t>(width);
                newSize.Height = static_cast<int32_t>(height);
                DebugLog::Log("Resize: Creating new frame pool (size: %dx%d)...", newSize.Width, newSize.Height);

                auto pixelFormat = winrt_impl::DirectXPixelFormat::B8G8R8A8UIntNormalized;
                auto newFramePool = winrt_impl::Direct3D11CaptureFramePool::Create(
                    rtDevice,
                    pixelFormat,
                    2,
                    newSize
                );
                m_framePool = new FramePoolWrapper{ newFramePool };
                DebugLog::Log("Resize: New frame pool created");

                // Step 6: Create new session from new frame pool
                DebugLog::Log("Resize: Creating new capture session...");
                auto newSession = newFramePool.CreateCaptureSession(captureItem);
                m_session = new SessionWrapper{ newSession };
                DebugLog::Log("Resize: New session created");

                // Step 7: Start the new session
                DebugLog::Log("Resize: Starting new capture session...");
                newSession.StartCapture();
                DebugLog::Log("Resize: Capture setup recreated successfully");
            }
            else
            {
                DebugLog::Log("Resize: Skipping capture recreation (framePool=%p, captureItem=%p, d3dDevice=%p)",
                    m_framePool, m_captureItem, m_d3dDevice);
            }

            DebugLog::Log("Resize: Resize completed successfully");
            return Result::Success;
        }
        catch (winrt::hresult_error const& ex)
        {
            DebugLog::Log("Resize: ERROR - WinRT exception: 0x%08X - %ls", ex.code(), ex.message().c_str());
            return Result::ErrorUnknown;
        }
        catch (std::exception const& ex)
        {
            DebugLog::Log("Resize: ERROR - Exception: %s", ex.what());
            return Result::ErrorUnknown;
        }
        catch (...)
        {
            DebugLog::Log("Resize: ERROR - Unknown exception!");
            return Result::ErrorUnknown;
        }
    }

} // namespace WebViewToolkit
