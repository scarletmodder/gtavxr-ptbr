# Builds the Windows installer: compiles and tests the tool, packs it with Inno Setup and writes
# SHA256SUMS.txt next to it. The version comes from src\GtavxrPtbr\GtavxrPtbr.csproj; the installer goes to dist\.
[CmdletBinding()]
param([string]$Iscc, [switch]$SkipTests)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2
$root = Split-Path $PSScriptRoot -Parent
Set-Location -LiteralPath $root

$project = [IO.File]::ReadAllText((Join-Path $root 'src\GtavxrPtbr\GtavxrPtbr.csproj'))
if ($project -notmatch '<Version>(\d+\.\d+\.\d+)</Version>') { throw 'No <Version>X.Y.Z</Version> in GtavxrPtbr.csproj' }
$version = $Matches[1]

if (-not $Iscc) {
    $candidates = @(
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'),
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe')
    )
    $Iscc = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
if (-not $Iscc) { throw 'ISCC.exe (Inno Setup 6.7 or newer) not found. Pass -Iscc.' }

# 1. Tests: the patcher and every translation file.
if (-not $SkipTests) {
    dotnet test -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw "Tests failed with code $LASTEXITCODE" }
}

# 2. The tool and its translations, in one folder (work\ is not versioned).
$publish = Join-Path $root 'work\publish'
if (Test-Path -LiteralPath $publish) { Remove-Item -LiteralPath $publish -Recurse -Force }
dotnet build src\GtavxrPtbr -c Release --nologo -o $publish
if ($LASTEXITCODE -ne 0) { throw "Build failed with code $LASTEXITCODE" }
Get-ChildItem -LiteralPath $publish -Filter '*.pdb' | Remove-Item -Force
& (Join-Path $publish 'GtavxrPtbr.exe') --version
if ($LASTEXITCODE -ne 0) { throw 'The built tool does not start' }

# 3. The installer. Only this build stays in dist\, so nobody picks an older one by mistake.
$dist = Join-Path $root 'dist'
New-Item -ItemType Directory -Force -Path $dist | Out-Null
Get-ChildItem -LiteralPath $dist -Filter 'GTAVXR-PTBR-Setup-*' | Remove-Item -Force
& $Iscc /Qp "/DAppVersion=$version" (Join-Path $root 'installer\gtavxr-ptbr.iss')
if ($LASTEXITCODE -ne 0) { throw "ISCC failed with code $LASTEXITCODE" }

$setup = Join-Path $dist "GTAVXR-PTBR-Setup-v$version.exe"
$line = "$((Get-FileHash -LiteralPath $setup -Algorithm SHA256).Hash.ToLowerInvariant())  $(Split-Path $setup -Leaf)"
[IO.File]::WriteAllText((Join-Path $dist 'SHA256SUMS.txt'), $line + "`n", (New-Object Text.UTF8Encoding($false)))
Write-Host "Built $setup"
Write-Host $line
