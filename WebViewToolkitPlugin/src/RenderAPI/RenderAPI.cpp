// ============================================================================
// WebViewToolkit - RenderAPI Factory Implementation
// ============================================================================

#include "WebViewToolkit/RenderAPI.h"
#include "RenderAPI_D3D11.h"

#ifdef WEBVIEW_TOOLKIT_DX12_SUPPORT
#include "RenderAPI_D3D12.h"
#endif

namespace WebViewToolkit
{
    std::unique_ptr<IRenderAPI> CreateRenderAPI(GraphicsAPI api)
    {
        switch (api)
        {
        case GraphicsAPI::Direct3D11:
            return std::make_unique<RenderAPI_D3D11>();

#ifdef WEBVIEW_TOOLKIT_DX12_SUPPORT
        case GraphicsAPI::Direct3D12:
            return std::make_unique<RenderAPI_D3D12>();
#endif

        default:
            return nullptr;
        }
    }

} // namespace WebViewToolkit
