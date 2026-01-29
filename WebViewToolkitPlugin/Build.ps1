<#
.SYNOPSIS
Builds the WebViewToolkit plugin and deploys it to Unity.

.DESCRIPTION
This script:
1. Builds the project using the specified CMake preset.
2. Copies the output DLL (and PDB) to the Unity Package Plugins folder.

.PARAMETER Preset
The CMake preset to use. Default is "release".

.PARAMETER Config
The build configuration (Debug/Release). Default is "Release".
#>

param (
    [string]$Preset = "release",
    [string]$Config = "Release"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$BuildDir = Join-Path $ProjectRoot "build/$Preset"
$UnityPluginDir = Join-Path $ProjectRoot "../WebViewToolkit/Runtime/Plugins/x86_64"

# Artifact Names
$DllName = "WebViewToolkit.dll"
$PdbName = "WebViewToolkit.pdb"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "WebViewToolkit Build & Deploy" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

if (-not (Test-Path $BuildDir)) {
    Write-Error "Build directory '$BuildDir' not found. Please run Setup.ps1 first."
}

# 1. Build
Write-Host "`n[1/2] Building $Config configuration..." -ForegroundColor Yellow
try {
    cmake --build "$BuildDir" --config $Config
} catch {
    Write-Error "Build failed."
}

# 2. Deploy
Write-Host "`n[2/2] Copying artifacts to Unity project..." -ForegroundColor Yellow

$SourceDll = Join-Path $BuildDir "bin/$Config/$DllName"
$SourcePdb = Join-Path $BuildDir "bin/$Config/$PdbName"
$DestDll = Join-Path $UnityPluginDir $DllName
$DestPdb = Join-Path $UnityPluginDir $PdbName

if (Test-Path $SourceDll) {
    # Ensure destination directory exists
    if (-not (Test-Path $UnityPluginDir)) {
        New-Item -ItemType Directory -Force -Path $UnityPluginDir | Out-Null
    }

    # Copy DLL
    try {
        Copy-Item -Path $SourceDll -Destination $DestDll -Force
        Write-Host "Copied: $DllName" -ForegroundColor Green
    } catch {
        Write-Error "Failed to copy $DllName. The file is likely locked by another process (e.g. Unity Editor). Please close Unity or stop the play mode and try again."
        exit 1
    }

    # Copy PDB (Optional but recommended for native debugging)
    if (Test-Path $SourcePdb) {
        Copy-Item -Path $SourcePdb -Destination $DestPdb -Force
        Write-Host "Copied: $PdbName (Symbols)" -ForegroundColor Green
    } else {
        Write-Warning "PDB not found at $SourcePdb (This is normal for Release builds depending on settings)"
    }
} else {
    Write-Error "Build artifact not found at $SourceDll"
}

Write-Host "`nBuild & Deploy Complete!" -ForegroundColor Cyan
