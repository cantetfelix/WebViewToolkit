================================================================================
Unity Native Plugin API Headers
================================================================================

This folder should contain the Unity Native Plugin API headers.

REQUIRED FILES:
---------------
Copy the following files from your Unity installation:

    <Unity Installation>/Editor/Data/PluginAPI/

Required headers:
    - IUnityInterface.h
    - IUnityGraphics.h
    - IUnityGraphicsD3D11.h
    - IUnityGraphicsD3D12.h (for DX12 support)

Example paths:
    Windows: C:\Program Files\Unity\Hub\Editor\<version>\Editor\Data\PluginAPI\
    macOS:   /Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/PluginAPI/

ALTERNATIVE:
------------
You can also download the headers from Unity's official repository:
https://github.com/Unity-Technologies/PluginAPI

After copying the headers, the folder structure should look like:

    extern/Unity/
        ├── IUnityInterface.h
        ├── IUnityGraphics.h
        ├── IUnityGraphicsD3D11.h
        ├── IUnityGraphicsD3D12.h
        └── README.txt (this file)

================================================================================
