# UI automation helpers for testing the packaged caro app (Windows PowerShell 5.1).
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class CaroWin {
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
  [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr h, int x, int y, int w, int hh, bool repaint);
  [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr h);
  [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr h, int attr, out RECT r, int size);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr hdc, uint flags);
}
"@
[CaroWin]::SetProcessDPIAware() | Out-Null

# Identity name from "caro-game (Package)/Package.appxmanifest".
$package = Get-AppxPackage -Name 'c9545576-0d20-4800-9912-1d32d947c966'
if (-not $package) { throw 'The app package is not registered. Run "caro-game (Package)" once from Visual Studio (F5).' }
$script:Pfn = $package.PackageFamilyName
$AE = [System.Windows.Automation.AutomationElement]
$TS = [System.Windows.Automation.TreeScope]

function Start-Caro {
    Get-Process -Name 'caro-game' -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Milliseconds 500
    Start-Process "shell:AppsFolder\$($script:Pfn)!App"
    for ($i = 0; $i -lt 60; $i++) {
        Start-Sleep -Milliseconds 500
        $p = Get-Process -Name 'caro-game' -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
        if ($p) { Start-Sleep -Seconds 2; return $p }
    }
    throw 'caro-game window did not appear'
}

function Get-CaroProcess {
    Get-Process -Name 'caro-game' -ErrorAction Stop | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
}

function Get-CaroWindow {
    $p = Get-CaroProcess
    $AE::FromHandle($p.MainWindowHandle)
}

function Find-All([System.Windows.Automation.AutomationElement]$root, [string]$namePattern) {
    $all = $root.FindAll($TS::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    $result = @()
    foreach ($e in $all) { if ($e.Current.Name -like $namePattern) { $result += $e } }
    return $result
}

function Find-ById([string]$automationId) {
    $cond = New-Object System.Windows.Automation.PropertyCondition($AE::AutomationIdProperty, $automationId)
    (Get-CaroWindow).FindFirst($TS::Descendants, $cond)
}

function Invoke-Element($element) {
    $pattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
    $pattern.Invoke()
    Start-Sleep -Milliseconds 250
}

# Polls a condition so the checks do not depend on how fast the UI updates.
function Wait-Until([scriptblock]$Condition, [int]$TimeoutMs = 5000) {
    $watch = [System.Diagnostics.Stopwatch]::StartNew()
    while ($watch.ElapsedMilliseconds -lt $TimeoutMs) {
        if (& $Condition) { return $true }
        Start-Sleep -Milliseconds 200
    }
    return [bool](& $Condition)
}

function Invoke-ById([string]$automationId) {
    $found = Wait-Until { $null -ne (Find-ById $automationId) }
    if (-not $found) { throw "Element '$automationId' not found" }
    Invoke-Element (Find-ById $automationId)
}

# Cell names look like "Hàng 8, cột 8: trống"; the wildcard avoids typing Vietnamese letters here.
function Get-CellElements {
    $cond = New-Object System.Windows.Automation.PropertyCondition($AE::ControlTypeProperty, [System.Windows.Automation.ControlType]::Button)
    $buttons = (Get-CaroWindow).FindAll($TS::Descendants, $cond)
    $cells = @{}
    foreach ($b in $buttons) {
        if ($b.Current.Name -match '^H.ng (\d+), c.t (\d+): (.*)$') { $cells["$($Matches[1]),$($Matches[2])"] = $b }
    }
    return $cells
}

# Plays 1-based (row, column) pairs, e.g. Play-Cells @(@(8,8), @(8,9))
function Play-Cells($moves) {
    $cells = Get-CellElements
    foreach ($m in $moves) {
        $key = "$($m[0]),$($m[1])"
        if (-not $cells.ContainsKey($key)) { throw "Cell $key not found" }
        Invoke-Element $cells[$key]
    }
}

function Get-CellText([int]$row, [int]$column) {
    (Get-CellElements)["$row,$column"].Current.Name
}

function Set-EditText([int]$index, [string]$text) {
    $cond = New-Object System.Windows.Automation.PropertyCondition($AE::ControlTypeProperty, [System.Windows.Automation.ControlType]::Edit)
    $edits = (Get-CaroWindow).FindAll($TS::Descendants, $cond)
    $vp = $edits[$index].GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
    $vp.SetValue($text)
    Start-Sleep -Milliseconds 300
}

function Get-Texts {
    $cond = New-Object System.Windows.Automation.PropertyCondition($AE::ControlTypeProperty, [System.Windows.Automation.ControlType]::Text)
    $texts = (Get-CaroWindow).FindAll($TS::Descendants, $cond)
    $texts | ForEach-Object { $_.Current.Name } | Where-Object { $_ }
}

function Resize-Caro([int]$widthEpx, [int]$heightEpx) {
    $p = Get-CaroProcess
    $h = $p.MainWindowHandle
    [CaroWin]::ShowWindow($h, 9) | Out-Null
    $scale = [CaroWin]::GetDpiForWindow($h) / 96.0
    [CaroWin]::MoveWindow($h, 40, 40, [int]($widthEpx * $scale), [int]($heightEpx * $scale), $true) | Out-Null
    Start-Sleep -Milliseconds 800
}

function Save-Shot([string]$path) {
    $p = Get-CaroProcess
    $h = $p.MainWindowHandle
    Start-Sleep -Milliseconds 500
    # PW_RENDERFULLCONTENT (2) renders the DirectComposition content even when the window is covered.
    $r = New-Object CaroWin+RECT
    [CaroWin]::GetWindowRect($h, [ref]$r) | Out-Null
    $w = $r.Right - $r.Left; $hh = $r.Bottom - $r.Top
    $bmp = New-Object System.Drawing.Bitmap $w, $hh
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $hdc = $g.GetHdc()
    [CaroWin]::PrintWindow($h, $hdc, 2) | Out-Null
    $g.ReleaseHdc($hdc)
    # Crop the invisible resize borders reported by DWM.
    $f = New-Object CaroWin+RECT
    [CaroWin]::DwmGetWindowAttribute($h, 9, [ref]$f, [System.Runtime.InteropServices.Marshal]::SizeOf($f)) | Out-Null
    $crop = New-Object System.Drawing.Rectangle ($f.Left - $r.Left), ($f.Top - $r.Top), ($f.Right - $f.Left), ($f.Bottom - $f.Top)
    $out = $bmp.Clone($crop, $bmp.PixelFormat)
    $out.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose(); $out.Dispose()
    "saved $path ($($crop.Width) x $($crop.Height))"
}
