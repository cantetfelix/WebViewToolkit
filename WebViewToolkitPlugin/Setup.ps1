<#
.SYNOPSIS
Configures the WebViewToolkit project dependencies and build system.

.DESCRIPTION
This script prepares the project for building by:
1. Checking for VCPKG_ROOT.
2. configuring the CMake project using the specified preset.
This step typically downloads and compiles all vcpkg dependencies.

.PARAMETER Preset
The CMake preset to use. Default is "release".

.PARAMETER Clean
If specified, removes the build directory before configuring.
#>

param (
    [string]$Preset = "release",
    [switch]$Clean
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$BuildDir = Join-Path $ProjectRoot "build/$Preset"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "WebViewToolkit Setup" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Check for VCPKG_ROOT
if (-not (Test-Path Env:VCPKG_ROOT)) {
    Write-Warning "VCPKG_ROOT environment variable is not set."
    Write-Warning "Ensure vcpkg is installed and VCPKG_ROOT is available in your shell."
}

# Clean
if ($Clean) {
    Write-Host "`n[1/2] Cleaning build directory..." -ForegroundColor Yellow
    if (Test-Path $BuildDir) {
        Remove-Item -Recurse -Force $BuildDir
        Write-Host "Cleaned $BuildDir" -ForegroundColor Gray
    }
}

# Configure
Write-Host "`n[2/2] Configuring CMake (Preset: $Preset)..." -ForegroundColor Yellow
try {
    # This will trigger vcpkg install if using manifest mode
    cmake -S "$ProjectRoot" --preset $Preset
    Write-Host "`nSetup Configuration Complete!" -ForegroundColor Green
} catch {
    Write-Error "CMake configuration failed. Check vcpkg installation and logs."
}
