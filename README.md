# WebView Toolkit for Unity

**High-performance, native WebView2 integration for Unity with DirectX 11 & 12 support.**

WebView Toolkit provides a seamless way to embed modern web content into your Unity projects using Microsoft Edge WebView2. It supports high-performance off-screen rendering directly to Unity textures, making it ideal for in-game browsers, web-based UI, documentation viewers, and interactive web content.

## Features

### Core Capabilities
- **Native Performance**: Built with C++/WinRT for optimal integration with Unity's graphics pipeline
- **Modern Rendering**: Supports both DirectX 11 and DirectX 12 backends (with automatic D3D11On12 bridging)
- **Off-screen Rendering**: Pure texture-based rendering with no overlay windows
- **UIToolkit Integration**:
  - Native `WebViewElement` VisualElement for seamless UIToolkit workflows
  - `WebViewPanel` for full-featured browser UI with navigation controls
  - UIBuilder support for visual editor integration
- **Multiple Integration Patterns**:
  - Direct `WebViewManager` API for advanced control
  - `WebViewBehaviour` MonoBehaviour component for traditional workflows

### Interactivity & Communication
- **Full Input Support**:
  - Mouse events (move, button clicks, wheel scrolling)
  - Keyboard input forwarding
  - Normalized coordinate mapping for resolution-independent input
- **Two-way JavaScript Interop**:
  - Execute JavaScript from C# with `ExecuteScript()`
  - Receive messages from JavaScript via `window.chrome.webview.postMessage()`
  - Event-driven communication with `MessageReceived` events
- **Navigation API**:
  - Navigate to URLs or raw HTML strings
  - Back/forward history navigation
  - History state queries (`CanGoBack`, `CanGoForward`)
  - Navigation completion events

### Developer Experience
- **Chrome DevTools Support**: Optional integrated developer tools for debugging web content
- **Source Available**: Full C++ source code included for customization and extension
- **Comprehensive Samples**: Includes basic and advanced usage examples
- **Type-Safe API**: Strongly-typed C# API with proper error handling
- **Editor-Safe**: Graceful handling of Unity Editor preview modes

## Requirements

### Platform Support
- **Operating System**: Windows 10/11 (x64 only)
- **Unity Version**: Tested on Unity 6000.3
- **Graphics API**: DirectX 11 or DirectX 12
- **WebView2 Runtime**: Microsoft Edge WebView2 (usually pre-installed on modern Windows)

### Build Requirements (for source builds)
- Visual Studio 2022 with "Desktop development with C++" workload
- CMake 3.21 or later
- Git (for vcpkg dependency management)

## Installation

### Via Unity Package Manager (Recommended)

Install the package directly from the UPM branch:

1. Open Unity Package Manager (`Window` > `Package Manager`)
2. Click the `+` button and select `Add package from git URL`
3. Enter: `https://github.com/cantetfelix/WebViewToolkit.git#upm`
4. Click `Add`

**Note**: Ensure Git is installed and accessible in your system PATH.

### Via Release Archive

Download a release archive and install locally:

1. Go to [Releases](https://github.com/cantetfelix/WebViewToolkit/releases)
2. Download the latest release archive (includes native DLL and PDB)
3. Extract to your project's `Packages` folder or any location
4. In Unity Package Manager, click `+` > `Add package from disk`
5. Navigate to the extracted folder and select `package.json`

### Via Local Installation

For development or custom builds:

1. Clone or download this repository
2. Open Unity Package Manager
3. Click `+` > `Add package from disk`
4. Navigate to the `WebViewToolkit` folder and select `package.json`

## Quick Start

### Using UIToolkit Integration (Recommended)

The primary integration path uses UIToolkit VisualElements for modern UI workflows:

**In your UXML:**
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <WebViewElement
        initial-url="https://unity.com"
        enable-dev-tools="false"
        style="width: 1280px; height: 720px;" />
</ui:UXML>
```

**In your C# code:**
```csharp
using UnityEngine.UIElements;
using WebViewToolkit.UIToolkit;

var root = GetComponent<UIDocument>().rootVisualElement;
var webView = root.Q<WebViewElement>("my-webview");

// Navigate
webView.Navigate("https://example.com");

// Handle messages from JavaScript
webView.WebView.MessageReceived += (message) => {
    Debug.Log($"Message from web: {message}");
};
```

**Using UIBuilder:**

You can also add `WebViewElement` visually in UIBuilder:

1. Open your UXML document in UIBuilder
2. In the Library panel, find `WebViewElement` under Project
3. Drag it onto your canvas
4. Configure properties in the Inspector:
   - `initial-url`: Starting URL
   - `enable-dev-tools`: Enable Chrome DevTools
   - Set width and height in the style

### Using WebViewPanel (Full Browser UI)

For a complete browser experience with navigation controls:

```xml
<WebViewPanel
    initial-url="https://google.com"
    show-toolbar="true"
    show-address-bar="true"
    show-navigation-buttons="true"
    show-dev-tools-button="true" />
```

### Using WebViewBehaviour Component

For traditional Unity workflows:

```csharp
// Add WebViewBehaviour component to a GameObject
// Configure in Inspector: Initial URL, Resolution, Dev Tools

public class MyScript : MonoBehaviour
{
    public WebViewBehaviour webViewBehaviour;

    void Start()
    {
        webViewBehaviour.Navigate("https://example.com");

        webViewBehaviour.WebView.NavigationCompleted += (url, success) => {
            Debug.Log($"Navigated to {url}: {success}");
        };
    }
}
```

### Direct Manager API

For advanced scenarios requiring full control:

```csharp
using WebViewToolkit;

// Create a WebView instance
var webView = WebViewManager.Instance.CreateWebView(
    width: 1920,
    height: 1080,
    initialUrl: "https://unity.com"
);

// Use the texture
GetComponent<RawImage>().texture = webView.Texture;

// Send input events (normalized coordinates 0-1)
webView.SendMouseEvent(
    MouseEventType.Down,
    MouseButton.Left,
    normalizedX: 0.5f,
    normalizedY: 0.5f
);

// Navigate
webView.Navigate("https://example.com");

// Two-way communication
webView.MessageReceived += (message) => {
    Debug.Log($"Received: {message}");
};
webView.ExecuteScript("window.chrome.webview.postMessage('Hello Unity!')");

// Cleanup when done
webView.Dispose();
```

## Documentation

For detailed documentation, see:
- **[Package Documentation](WebViewToolkit/README.md)** - Comprehensive API reference and guides
- **[Changelog](WebViewToolkit/CHANGELOG.md)** - Version history and release notes
- **[Samples](WebViewToolkit/Samples~/)** - Example projects demonstrating various features

## Architecture

### High-Level Overview

```
Unity Application (C#)
    ├─ WebViewManager (Singleton)
    │   └─ Graphics API detection & lifecycle management
    ├─ WebViewInstance(s)
    │   ├─ Texture2D (BGRA32 format)
    │   ├─ Navigation & JavaScript API
    │   └─ Input event handling
    └─ UI Components
        ├─ WebViewBehaviour (MonoBehaviour)
        ├─ WebViewElement (UIToolkit)
        └─ WebViewPanel (Full browser UI)
            ↓ P/Invoke
Native Plugin (C++ DLL)
    ├─ Plugin Core
    │   └─ Unity graphics device event handling
    ├─ WebView Management
    │   ├─ WebView2 API integration
    │   └─ Lifecycle & instance management
    └─ Rendering Backend
        ├─ RenderAPI_D3D11 (DirectX 11)
        └─ RenderAPI_D3D12 (DirectX 12 via D3D11On12)
            ↓
    DirectX + WebView2 Runtime
```

### Key Technical Details

- **Texture Format**: BGRA32 (matches WebView2 native output)
- **Rendering Method**: Off-screen composition via DirectComposition
- **Graphics API Detection**: Automatic detection of DX11 vs DX12 at startup
- **Thread Safety**: Render operations on Unity's render thread, UI operations on main thread
- **Update Frequency**: ~30 FPS for WebView texture updates (configurable)
- **Dependencies**: WebView2 SDK and WIL (Windows Implementation Libraries) statically linked

## Building from Source

If you want to modify the native plugin, contribute, or build for development:

### Setup Dependencies

The project uses vcpkg for C++ dependency management. Run the setup script once:

```powershell
.\WebViewToolkitPlugin\Setup.ps1
```

This will:
- Bootstrap vcpkg if not already present
- Install WebView2 SDK and WIL (Windows Implementation Libraries)
- Configure CMake presets

### Build the Native Plugin

Use the build script to compile and deploy the DLL:

```powershell
# Release build (optimized)
.\WebViewToolkitPlugin\Build.ps1 -Preset release

# Debug build (with symbols)
.\WebViewToolkitPlugin\Build.ps1 -Preset debug

# Clean build
.\WebViewToolkitPlugin\Build.ps1 -Preset release -Clean
```

The build script will:
1. Compile the native C++ plugin using CMake
2. Copy the resulting `WebViewToolkit.dll` and `WebViewToolkit.pdb` to `WebViewToolkit/Runtime/Plugins/x86_64/`
3. Make the plugin immediately available in Unity

### Manual CMake Build

For advanced scenarios or CI/CD integration:

```bash
cd WebViewToolkitPlugin

# Configure
cmake --preset x64-release

# Build
cmake --build build/x64-release --config Release

# Output will be in build/x64-release/bin/Release/
```

### Available CMake Presets

- `x64-debug`: Debug build with full symbols
- `x64-release`: Optimized release build
- `x64-release-dx11-only`: Release build without DirectX 12 support (smaller binary)

## Project Structure

```
WebViewToolkit/
├── README.md                          # This file
├── LICENSE                            # MIT License
│
├── WebViewToolkit/                    # Unity Package (UPM)
│   ├── package.json                   # Package manifest (v1.0.0)
│   ├── README.md                      # Package documentation
│   ├── Runtime/                       # C# runtime code
│   │   ├── WebViewManager.cs          # Singleton manager
│   │   ├── WebViewInstance.cs         # WebView instance wrapper
│   │   ├── Components/                # MonoBehaviour components
│   │   ├── UIToolkit/                 # UIToolkit integration
│   │   ├── Native/                    # P/Invoke declarations
│   │   └── Plugins/x86_64/            # Native DLL
│   ├── Editor/                        # Editor utilities
│   ├── Tests/                         # Unit & integration tests
│   └── Samples~/                      # Usage examples
│
└── WebViewToolkitPlugin/              # Native C++ Plugin Source
    ├── CMakeLists.txt                 # Build configuration
    ├── vcpkg.json                     # Dependency manifest
    ├── Setup.ps1                      # Dependency setup script
    ├── Build.ps1                      # Build automation script
    ├── include/WebViewToolkit/        # Public headers
    ├── src/                           # Implementation
    │   ├── Plugin.cpp                 # Unity plugin interface
    │   ├── WebViewManager.cpp         # Native WebView manager
    │   ├── WebView.cpp                # WebView instance
    │   └── RenderAPI/                 # Graphics backend
    └── build/                         # Build output (generated)
```

## Samples

The package includes two comprehensive samples demonstrating different integration approaches:

### Basic WebView Sample
Location: `Samples~/BasicWebView/`

Demonstrates:
- Using `WebViewBehaviour` component
- Basic navigation
- Handling navigation events
- Simple button controls

### UIToolkit Demo Sample
Location: `Samples~/UIToolkitDemo/`

Demonstrates:
- `WebViewPanel` full browser UI
- Custom `WebViewElement` integration
- Address bar and navigation controls
- Loading indicators and status updates
- DevTools integration

To import samples:
1. Open Package Manager
2. Select WebView Toolkit
3. Expand "Samples" section
4. Click "Import" next to the desired sample

## CI/CD & Release Pipeline

The project includes automated CI/CD via GitHub Actions (`.github/workflows/cicd.yml`):

- **On every push to main**:
  - Builds native plugin (Release configuration)
  - Validates version bumps in `package.json` and `CHANGELOG.md`
  - Publishes to UPM branch with artifacts
  - Creates GitHub releases with DLL/PDB attachments

- **Version Management**:
  - Semantic versioning (MAJOR.MINOR.PATCH)
  - Changelog-driven releases
  - Git tags on UPM branch

## Performance Considerations

- **Texture Updates**: ~30 FPS update rate balances responsiveness and CPU usage
- **Static Linking**: All dependencies statically linked into single DLL (no runtime dependencies)
- **Memory**: Each WebView instance allocates texture memory (width × height × 4 bytes)
- **GPU**: Off-screen rendering uses DirectComposition, minimal GPU overhead
- **Input**: Event-driven input system with normalized coordinates

## Known Limitations

- **Platform**: Windows 10/11 x64 only (WebView2 limitation)
- **Graphics API**: DirectX 11/12 only (no OpenGL, Vulkan, Metal)
- **Architecture**: x64 only (no x86 or ARM support)
- **Threading**: WebView operations must occur on main thread (Unity limitation)
- **WebRTC/Media**: Some WebRTC features may require additional configuration

## Troubleshooting

### WebView not rendering
- Ensure WebView2 Runtime is installed (check Windows Apps & Features)
- Verify Unity is using DirectX 11 or DirectX 12 (Edit > Project Settings > Player)
- Check console for initialization errors

### Input not working
- Verify input coordinates are normalized (0-1 range)
- Ensure GameObject has a Collider for raycasting (if using UI)
- Check that mouse events are being forwarded to the WebView

### Build errors
- Ensure Visual Studio 2022 with C++ workload is installed
- Run `Setup.ps1` to install dependencies
- Verify CMake 3.21+ is in PATH
- Check vcpkg bootstrap succeeded

## Contributing

We welcome contributions! To contribute:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Update `CHANGELOG.md` with your changes
5. Ensure builds succeed locally
6. Commit your changes with descriptive messages
7. Push to your branch
8. Open a Pull Request

### Contribution Guidelines

- Follow existing code style (C++20 for native, C# conventions for managed)
- Add tests for new features
- Update documentation for API changes
- Bump version in `package.json` for breaking changes
- Add changelog entry describing your changes

## License

MIT License - see [LICENSE](LICENSE) for details.

## Acknowledgments

- Built with [Microsoft Edge WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
- Uses [Windows Implementation Libraries (WIL)](https://github.com/microsoft/wil)
- Dependency management via [vcpkg](https://vcpkg.io/)

## Support & Feedback

- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/cantetfelix/WebViewToolkit/issues)
- **Discussions**: Ask questions in [GitHub Discussions](https://github.com/cantetfelix/WebViewToolkit/discussions)
- **Documentation**: Full API reference in [Package Documentation](WebViewToolkit/README.md)

---

**Version**: 1.0.0
**Last Updated**: 2026-01-26
