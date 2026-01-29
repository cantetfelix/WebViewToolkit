# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2026-01-29

### Changed

- Updated README documentation to reflect current sample structure
  - Removed references to outdated BasicWebView and UIToolkit Demo samples
  - Added comprehensive descriptions for all six current samples (01-06)
  - Organized samples by difficulty level with estimated learning times
- Improved documentation clarity and accuracy

## [1.2.0] - 2026-01-29

### Added

- Six comprehensive sample scenes demonstrating WebView Toolkit features
  - 01_HelloWebView: Basic WebView setup and usage
  - 02_Navigation: URL navigation and history management
  - 03_JavaScriptBridge: C# ↔ JavaScript communication
  - 04_DynamicHTML: Dynamic HTML content generation
  - 05_InteractiveInput: Mouse and keyboard input handling
  - 06_MultipleWebViews: Managing multiple WebView instances
- Windows Graphics Capture implementation for both DX11 and DX12
- WinRT device wrapper storage for efficient resize operations

### Fixed

- **Critical:** WebView content cropping after resizing to larger dimensions (DX12)
  - Root cause: HWND host window was not being resized with WebView2 controller bounds
  - Solution: Added SetWindowPos call to resize HWND window client area
- **Critical:** DX11 crashes during WebView resize operations
  - Root cause: framePool.Recreate() called while Graphics Capture session was actively running
  - Solution: Replaced Recreate() with complete teardown and recreation (close session → close pool → create new pool → create new session → start capture)
- Y-flip logic in D3D12 texture copy for proper content orientation
- Texture dimension calculation for Windows Graphics Capture in DX12
- Dimension validation in DX11 to skip mismatched frames during resize transitions

### Changed

- Improved resize handling with proper Graphics Capture session lifecycle management
- Enhanced debug logging throughout capture initialization and resize operations

## [1.1.1] - 2026-01-26

### Fixed

- Force build on push to master to ensure artifacts are always up to date
- Fix master-to-master comparison issue that resulted in no changes detected
- Make conditional build only apply to PRs
- Fix gh release command: remove invalid 'assets' JSON field
- Make artifact download step PR-only to prevent push failures
- Change artifact download failure from error to warning for PRs
- Add explicit conditions to prevent pipeline failures

## [1.1.0] - 2026-01-26

### Changed

- Rewrite all README.md
- Fix VCPKG local build conflicting with required baseline argument on cicd
- Remove legacy WebViewRawImage component  
- Optimize CICD to trigger build step only if plugin source code changes detected

## [1.0.0] - 2026-01-20

### Added

- Initial release
- WebView2 integration with off-screen rendering
- DirectX 11 support
- DirectX 12 support via D3D11On12
- DirectComposition visual integration
- Mouse input forwarding
- Navigation API (Navigate, NavigateToString)
- JavaScript execution
- WebViewBehaviour MonoBehaviour component
- WebViewRawImage UI component with automatic input handling
- Sample scene and documentation
