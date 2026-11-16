# Saves a PNG of the main window of a running process (used to take the README screenshots).
# Usage: capture-window.ps1 -ProcessId 1234 -Output shot.png
[CmdletBinding()]
param([Parameter(Mandatory = $true)][int]$ProcessId, [Parameter(Mandatory = $true)][string]$Output)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class WinCapture {
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr h, int attr, out RECT r, int size);
}
'@

$process = Get-Process -Id $ProcessId
$handle = $process.MainWindowHandle
if ($handle -eq [IntPtr]::Zero) { throw 'The process has no main window yet' }
[void][WinCapture]::SetForegroundWindow($handle)
Start-Sleep -Milliseconds 400
$rect = New-Object WinCapture+RECT
[void][WinCapture]::GetWindowRect($handle, [ref]$rect)
$width = $rect.Right - $rect.Left
$height = $rect.Bottom - $rect.Top
$bitmap = New-Object System.Drawing.Bitmap $width, $height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$dc = $graphics.GetHdc()
[void][WinCapture]::PrintWindow($handle, $dc, 2)   # 2 = PW_RENDERFULLCONTENT (WPF and DirectX content)
$graphics.ReleaseHdc($dc)
$graphics.Dispose()

# GetWindowRect includes the invisible resize borders of Windows 10/11, which PrintWindow paints black. The visible
# frame is DWMWA_EXTENDED_FRAME_BOUNDS (9): keep only that part.
$frame = New-Object WinCapture+RECT
$size = [Runtime.InteropServices.Marshal]::SizeOf([type][WinCapture+RECT])
if ([WinCapture]::DwmGetWindowAttribute($handle, 9, [ref]$frame, $size) -eq 0) {
    $crop = New-Object System.Drawing.Rectangle ($frame.Left - $rect.Left), ($frame.Top - $rect.Top), ($frame.Right - $frame.Left), ($frame.Bottom - $frame.Top)
    $visible = $bitmap.Clone($crop, $bitmap.PixelFormat)
    $bitmap.Dispose()
    $bitmap = $visible
}
$bitmap.Save($Output, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "Saved $Output ($($bitmap.Width) x $($bitmap.Height))"
$bitmap.Dispose()
