<#
.SYNOPSIS
  Builds PARAMANIACS and creates a Yak package in ./dist
.EXAMPLE
  ./scripts/package.ps1
  ./scripts/package.ps1 -Version 0.2.0 -Yak "C:\tools\yak.exe"
#>
[CmdletBinding()]
param(
    [string]$Version = "",
    [string]$Configuration = "Release",
    [string]$Yak = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

# Version: parameter (a leading "v" is allowed) or Directory.Build.props
if (-not $Version) {
    [xml]$props = Get-Content (Join-Path $root "Directory.Build.props")
    $Version = $props.Project.PropertyGroup.Version
}
$Version = $Version.TrimStart("v")
if ($Version -notmatch '^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$') {
    throw "Version '$Version' is not valid semantic versioning (example: 1.2.3 or 1.2.3-beta1)."
}

# Find the Yak CLI
if (-not $Yak) {
    $rhinoYak = "C:\Program Files\Rhino 8\System\Yak.exe"
    if (Test-Path $rhinoYak) {
        $Yak = $rhinoYak
    } else {
        $cmd = Get-Command yak -ErrorAction SilentlyContinue
        if ($cmd) { $Yak = $cmd.Source }
    }
}
if (-not $Yak) {
    throw "Yak CLI not found. Install Rhino 8, or download https://files.mcneel.com/yak/tools/latest/yak.exe and pass -Yak <path>."
}

Write-Host "Building PARAMANIACS $Version ..."
dotnet build (Join-Path $root "Paramaniacs.sln") -c $Configuration -p:Version=$Version
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed." }

# Stage the package contents
$dist = Join-Path $root "dist"
$stage = Join-Path $dist "yak"
if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
New-Item -ItemType Directory -Path $stage | Out-Null

foreach ($tfm in @("net48", "net7.0-windows")) {
    $target = Join-Path $stage $tfm
    New-Item -ItemType Directory -Path $target | Out-Null

    $rhinoOut = Join-Path $root "src\Paramaniacs.Rhino\bin\$Configuration\$tfm"
    $ghOut = Join-Path $root "src\Paramaniacs.Grasshopper\bin\$Configuration\$tfm"

    Copy-Item (Join-Path $rhinoOut "*.rhp") $target
    Copy-Item (Join-Path $rhinoOut "Paramaniacs.Core.dll") $target
    Copy-Item (Join-Path $ghOut "*.gha") $target
}

Copy-Item (Join-Path $root "LICENSE") $stage
Copy-Item (Join-Path $root "README.md") $stage

# manifest.yml with the real version (written without BOM)
$manifest = (Get-Content (Join-Path $root "yak\manifest.yml") -Raw).Replace("{{VERSION}}", $Version)
[System.IO.File]::WriteAllText((Join-Path $stage "manifest.yml"), $manifest, (New-Object System.Text.UTF8Encoding($false)))

# Build the .yak (Yak packages everything in the current directory)
Push-Location $stage
try {
    & $Yak build --platform win
    if ($LASTEXITCODE -ne 0) { throw "yak build failed." }
} finally {
    Pop-Location
}

Move-Item (Join-Path $stage "*.yak") $dist
Write-Host ""
Write-Host "Done. Package(s):"
Get-ChildItem (Join-Path $dist "*.yak") | ForEach-Object { Write-Host "  $($_.FullName)" }
