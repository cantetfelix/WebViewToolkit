cmake_minimum_required(VERSION 3.21)

# ============================================================================
# WebView Toolkit - Native Unity Plugin for WebView2 Rendering
# ============================================================================
# Supports: DirectX 11 & DirectX 12 via abstract RenderAPI architecture
# Build: Static linking with vcpkg dependencies
# ============================================================================

project(WebViewToolkit 
    VERSION 1.0.0
    DESCRIPTION "Native WebView2 plugin for Unity UIToolkit"
    LANGUAGES CXX
)

# C++20 standard
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# ============================================================================
# Build Options
# ============================================================================
option(BUILD_SHARED_LIBS "Build as shared library (DLL)" ON)
option(ENABLE_DX12_SUPPORT "Enable DirectX 12 support via D3D11On12" ON)

# ============================================================================
# Platform Check - Windows only
# ============================================================================
if(NOT WIN32)
    message(FATAL_ERROR "WebViewToolkit only supports Windows platform")
endif()

# ============================================================================
# Static Runtime Linking (/MT instead of /MD)
# Critical: Avoids runtime DLL dependencies
# ============================================================================
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

# ============================================================================
# Find Dependencies (via vcpkg)
# ============================================================================
find_package(unofficial-webview2 CONFIG REQUIRED)
find_package(wil CONFIG REQUIRED)

# ============================================================================
# Source Files
# ============================================================================
set(PLUGIN_SOURCES
    # Core
    src/Plugin.cpp
    src/PluginExports.cpp
    src/WebViewManager.cpp
    src/WebView.cpp
    src/WebViewCapture.cpp
    
    # Render API Abstraction
    src/RenderAPI/RenderAPI.cpp
    src/RenderAPI/RenderAPI_D3D11.cpp
)

set(PLUGIN_HEADERS
    # Public Interface
    include/WebViewToolkit/Plugin.h
    include/WebViewToolkit/RenderAPI.h
    include/WebViewToolkit/WebViewManager.h
    include/WebViewToolkit/WebView.h
    include/WebViewToolkit/WebViewCapture.h
    include/WebViewToolkit/Types.h
    
    # Internal Headers
    src/RenderAPI/RenderAPI_D3D11.h
)

# Add DX12 support if enabled
if(ENABLE_DX12_SUPPORT)
    list(APPEND PLUGIN_SOURCES src/RenderAPI/RenderAPI_D3D12.cpp)
    list(APPEND PLUGIN_HEADERS src/RenderAPI/RenderAPI_D3D12.h)
    add_compile_definitions(WEBVIEW_TOOLKIT_DX12_SUPPORT=1)
endif()

# ============================================================================
# Unity Plugin Headers
# ============================================================================
# Unity headers location - user should place headers in extern/Unity/
set(UNITY_PLUGIN_API_DIR "${CMAKE_SOURCE_DIR}/extern/Unity")

if(NOT EXISTS "${UNITY_PLUGIN_API_DIR}/IUnityInterface.h")
    message(WARNING 
        "Unity Plugin API headers not found at: ${UNITY_PLUGIN_API_DIR}\n"
        "Please copy Unity headers (IUnityInterface.h, IUnityGraphics.h, etc.) to this directory.\n"
        "Headers can be found in: <Unity Installation>/Editor/Data/PluginAPI/"
    )
endif()



# ============================================================================
# Create Shared Library (DLL)
# ============================================================================
add_library(${PROJECT_NAME} SHARED
    ${PLUGIN_SOURCES}
    ${PLUGIN_HEADERS}
)

# Module definition file for explicit exports
target_sources(${PROJECT_NAME} PRIVATE
    src/WebViewToolkit.def
)

# ============================================================================
# Include Directories
# ============================================================================
target_include_directories(${PROJECT_NAME}
    PUBLIC
        $<BUILD_INTERFACE:${CMAKE_SOURCE_DIR}/include>
        $<INSTALL_INTERFACE:include>
    PRIVATE
        ${CMAKE_SOURCE_DIR}/src
        ${UNITY_PLUGIN_API_DIR}
)



# ============================================================================
# Link Libraries
# ============================================================================
target_link_libraries(${PROJECT_NAME}
    PRIVATE
        # vcpkg packages
        unofficial::webview2::webview2
        WIL::WIL
        
        # Windows/DirectX
        d3d11.lib
        dxgi.lib
        dcomp.lib           # DirectComposition for WebView2
        windowscodecs.lib
        windowsapp.lib      # WinRT for GraphicsCapture
        shcore.lib          # DPI scaling
)

# Add DX12 libraries if enabled
if(ENABLE_DX12_SUPPORT)
    target_link_libraries(${PROJECT_NAME}
        PRIVATE
            d3d12.lib
    )
endif()

# ============================================================================
# Compiler Options
# ============================================================================
target_compile_options(${PROJECT_NAME}
    PRIVATE
        /W4                 # Warning level 4
        /WX-                # Don't treat warnings as errors (for now)
        /permissive-        # Strict conformance
        /Zc:__cplusplus     # Correct __cplusplus macro
        /utf-8              # UTF-8 source encoding
        /EHsc               # Exception handling
        /await:strict       # C++20 coroutines for WinRT
        $<$<CONFIG:Release>:/O2>        # Optimize for speed
        $<$<CONFIG:Release>:/GL>        # Whole program optimization
        $<$<CONFIG:Debug>:/Zi>          # Debug info
        $<$<CONFIG:Debug>:/Od>          # No optimization
)

# Linker options for Release
target_link_options(${PROJECT_NAME}
    PRIVATE
        $<$<CONFIG:Release>:/LTCG>      # Link-time code generation
)

# ============================================================================
# Preprocessor Definitions
# ============================================================================
target_compile_definitions(${PROJECT_NAME}
    PRIVATE
        WEBVIEW_TOOLKIT_EXPORTS         # DLL export macro
        UNICODE
        _UNICODE
        WIN32_LEAN_AND_MEAN
        NOMINMAX
        $<$<CONFIG:Debug>:WEBVIEW_TOOLKIT_DEBUG=1>
)

# ============================================================================
# Output Configuration
# ============================================================================
set_target_properties(${PROJECT_NAME} PROPERTIES
    OUTPUT_NAME "WebViewToolkit"
    DEBUG_POSTFIX "_d"
    
    # Put all outputs in a clean directory
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib"
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib"
)

# ============================================================================
# Install Rules (for Unity integration)
# ============================================================================
install(TARGETS ${PROJECT_NAME}
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)

install(DIRECTORY include/
    DESTINATION include
)

# ============================================================================
# Post-Build: Copy to Unity Plugins folder (optional)
# ============================================================================
# Uncomment and set UNITY_PLUGINS_DIR to auto-copy DLL after build
# set(UNITY_PLUGINS_DIR "path/to/Unity/Assets/Plugins/x86_64")
# add_custom_command(TARGET ${PROJECT_NAME} POST_BUILD
#     COMMAND ${CMAKE_COMMAND} -E copy_if_different
#         $<TARGET_FILE:${PROJECT_NAME}>
#         "${UNITY_PLUGINS_DIR}/$<TARGET_FILE_NAME:${PROJECT_NAME}>"
#     COMMENT "Copying DLL to Unity Plugins folder..."
# )

# ============================================================================
# Summary
# ============================================================================
message(STATUS "")
message(STATUS "=== WebViewToolkit Configuration ===")
message(STATUS "Version:        ${PROJECT_VERSION}")
message(STATUS "Build Type:     ${CMAKE_BUILD_TYPE}")
message(STATUS "DX12 Support:   ${ENABLE_DX12_SUPPORT}")
message(STATUS "Unity Headers:  ${UNITY_PLUGIN_API_DIR}")
message(STATUS "Output Dir:     ${CMAKE_BINARY_DIR}/bin")
message(STATUS "====================================")
message(STATUS "")
