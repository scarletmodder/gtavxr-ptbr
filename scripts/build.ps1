# Builds the Windows installer: compiles and tests the tool and the in-game plugin, packs them with Inno Setup and
# writes SHA256SUMS.txt next to them. The version comes from src\GtavxrPtbr\GtavxrPtbr.csproj; the installer and the
# in-game mod package (for players who add it to GTAVXR themselves) go to dist\.
[CmdletBinding()]
param([string]$Iscc, [string]$Vcvars, [switch]$SkipTests)

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

# 3. The in-game plugin (C++, Microsoft C++ x64 tools), tested against the stand-in tables.
if (-not $Vcvars) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path -LiteralPath $vswhere) {
        $Vcvars = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -find 'VC\Auxiliary\Build\vcvars64.bat' | Select-Object -First 1
    }
}
if (-not $Vcvars -or -not (Test-Path -LiteralPath $Vcvars)) { throw 'vcvars64.bat (Microsoft C++ x64 build tools) not found. Pass -Vcvars.' }
python scripts\gen_asi_texts.py
if ($LASTEXITCODE -ne 0) { throw 'Could not generate the in-game texts' }
$asi = Join-Path $root 'work\asi'
if (Test-Path -LiteralPath $asi) { Remove-Item -LiteralPath $asi -Recurse -Force }
cmd /c "`"$(Join-Path $root 'scripts\build-asi.cmd')`" `"$Vcvars`" $version `"$asi`""
if ($LASTEXITCODE -ne 0) { throw "The in-game plugin failed to build ($LASTEXITCODE)" }
& (Join-Path $asi 'test_patch.exe') (Join-Path $asi 'fake_tables.dll') (Join-Path $asi 'fake_tables.dll') 5 4
if ($LASTEXITCODE -ne 0) { throw 'The in-game plugin failed its test' }
& (Join-Path $asi 'test_patch.exe') --refuse (Join-Path $asi 'fake_wider.dll')
if ($LASTEXITCODE -ne 0) { throw 'The in-game plugin accepted a table laid out differently' }
$package = Join-Path $asi 'package'
New-Item -ItemType Directory -Force -Path $package | Out-Null
$zip = Join-Path $package 'GTAVXR PT-BR menus do jogo.zip'
Compress-Archive -LiteralPath (Join-Path $asi 'GtavxrPtbr.asi'), (Join-Path $root 'src\GtavxrPtbrAsi\README.txt') -DestinationPath $zip

# 4. The installer and the mod package. Only this build stays in dist\, so nobody picks an older one by mistake.
$dist = Join-Path $root 'dist'
New-Item -ItemType Directory -Force -Path $dist | Out-Null
Get-ChildItem -LiteralPath $dist -Filter 'GTAVXR-PTBR-*' | Remove-Item -Force
$modZip = Join-Path $dist "GTAVXR-PTBR-InGame-v$version.zip"
Copy-Item -LiteralPath $zip -Destination $modZip
& $Iscc /Qp "/DAppVersion=$version" (Join-Path $root 'installer\gtavxr-ptbr.iss')
if ($LASTEXITCODE -ne 0) { throw "ISCC failed with code $LASTEXITCODE" }

$setup = Join-Path $dist "GTAVXR-PTBR-Setup-v$version.exe"
$lines = foreach ($file in $setup, $modZip) {
    "$((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant())  $(Split-Path $file -Leaf)"
}
[IO.File]::WriteAllText((Join-Path $dist 'SHA256SUMS.txt'), ($lines -join "`n") + "`n", (New-Object Text.UTF8Encoding($false)))
Write-Host "Built $setup and $modZip"
$lines | ForEach-Object { Write-Host $_ }
