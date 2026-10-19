# Full UI regression of the caro app through UI Automation. Prints PASS/FAIL per check.
# Requirement: the app package is registered (run it once with F5 from Visual Studio, Debug | x64).
# Usage (Windows PowerShell 5.1):
#   powershell -ExecutionPolicy Bypass -File .\tests\ui\Run-UiRegression.ps1
# The script starts the app, drives it, then closes it. Do not use mouse/keyboard on it while running.
. "$PSScriptRoot\CaroUi.ps1"

$script:fails = 0
function Check([string]$name, [bool]$ok, [string]$detail = '') {
    if ($ok) { "PASS  $name" } else { $script:fails++; "FAIL  $name  $detail" }
}
function NameOf([string]$id) { $e = Find-ById $id; if ($e) { $e.Current.Name } else { '<missing>' } }
function EnabledOf([string]$id) { (Find-ById $id).Current.IsEnabled }
function EditValue([int]$index) {
    $cond = New-Object System.Windows.Automation.PropertyCondition($AE::ControlTypeProperty, [System.Windows.Automation.ControlType]::Edit)
    $edits = (Get-CaroWindow).FindAll($TS::Descendants, $cond)
    $edits[$index].GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value
}
function CellState([int]$r, [int]$c) { ((Get-CellText $r $c) -split ': ')[-1] }
function Confirm-Dialog { Invoke-ById 'PrimaryButton'; Start-Sleep -Milliseconds 1500 }

$p = Start-Caro
[CaroWin]::ShowWindow($p.MainWindowHandle, 4) | Out-Null
Resize-Caro 1280 860

# 1. Setup screen
Check '1 setup: default n = 15 is valid' ((EditValue 0) -eq '15' -and (EnabledOf 'StartButton'))

# 2-3. Board size validation
Set-EditText 0 '25'
Check '2 n=25 rejected' ((-not (EnabledOf 'StartButton')) -and (NameOf 'BoardSizeErrorText') -eq 'Kích thước phải nằm trong khoảng 4 đến 20.') (NameOf 'BoardSizeErrorText')
Set-EditText 0 '3'
Check '2 n=3 rejected' (-not (EnabledOf 'StartButton'))
Set-EditText 0 'abc'
Check '3 n=abc rejected' ((-not (EnabledOf 'StartButton')) -and (NameOf 'BoardSizeErrorText') -eq 'Kích thước phải là một số nguyên, ví dụ 15.') (NameOf 'BoardSizeErrorText')
Set-EditText 0 ''
Check '3 empty rejected' ((-not (EnabledOf 'StartButton')) -and (NameOf 'BoardSizeErrorText') -eq 'Vui lòng nhập kích thước bàn cờ.') (NameOf 'BoardSizeErrorText')
Set-EditText 0 '4'
Check '3 n=4 accepted' (EnabledOf 'StartButton')
Set-EditText 0 '20'
Check '3 n=20 accepted' (EnabledOf 'StartButton')

# 4. Player names
Set-EditText 1 'An'
Set-EditText 2 'an'
Check '4 duplicate names rejected' (-not (EnabledOf 'StartButton'))
Set-EditText 2 'Bình'
Set-EditText 0 '15'
Check '4 distinct names accepted' (EnabledOf 'StartButton')

# 5. Create 15 x 15 board and play
Invoke-ById 'StartButton'; Start-Sleep 2
Check '5 board has 225 cells' ((Get-CellElements).Count -eq 225)
Play-Cells @(@(8,8), @(7,8), @(8,9), @(7,9))
Check '5 players alternate' ((CellState 8 8) -eq 'X' -and (CellState 7 8) -eq 'O' -and (CellState 8 9) -eq 'X')
Check '5 turn shown' ((NameOf 'StatusTitleText') -eq 'Lượt của An') (NameOf 'StatusTitleText')
Check '5 occupied cell disabled' (-not (Get-CellElements)['8,8'].Current.IsEnabled)

# 6. X wins with 5 in a row
Play-Cells @(@(8,10), @(7,10), @(8,11), @(7,11), @(8,7))
Check '6 X wins' ((NameOf 'StatusTitleText') -eq 'An thắng!') (NameOf 'StatusTitleText')
Check '6 undo locked after win' (-not (EnabledOf 'UndoButton'))
Check '6 empty cells locked after win' (-not (Get-CellElements)['1,1'].Current.IsEnabled)
Check '6 score X = 1' ((Get-Texts) -contains '1')

# 7. New round keeps score, O starts
Invoke-ById 'PlayAgainButton'; Start-Sleep 1
Check '7 new round: board empty, O starts' (Wait-Until { (CellState 8 8) -eq 'trống' -and (NameOf 'StatusTitleText') -eq 'Lượt của Bình' }) (NameOf 'StatusTitleText')
Check '7 round counter' ((Get-Texts) -contains 'Ván 2')

# 8. Undo
Play-Cells @(@(5,5), @(6,6), @(5,6))
Invoke-ById 'UndoButton'
Check '8 undo clears last cell and returns turn' ((CellState 5 6) -eq 'trống' -and (CellState 6 6) -eq 'X' -and (NameOf 'StatusTitleText') -eq 'Lượt của Bình') (NameOf 'StatusTitleText')

# 9. New round during a game asks first; cancel keeps the board
Invoke-ById 'NewRoundButton'
Check '9 confirmation dialog shown' (Wait-Until { $null -ne (Find-ById 'CloseButton') })
Invoke-ById 'CloseButton'; Start-Sleep 1
Check '9 cancel keeps board' ((CellState 5 5) -eq 'O' -and (CellState 6 6) -eq 'X')

# 10. Back to setup keeps the entered values
Invoke-ById 'BackToSetupButton'; Confirm-Dialog
Check '10 back on setup page with values kept' ((Wait-Until { $null -ne (Find-ById 'StartButton') }) -and (EditValue 0) -eq '15' -and (EditValue 1) -eq 'An' -and (EditValue 2) -eq 'Bình')

# 11. 4 x 4 draw
Set-EditText 0 '4'
Invoke-ById 'StartButton'; Start-Sleep 2
Check '11 board has 16 cells' ((Get-CellElements).Count -eq 16)
Play-Cells @(@(1,1), @(1,3), @(1,2), @(1,4), @(2,3), @(2,1), @(2,4), @(2,2), @(3,1), @(3,3), @(3,2), @(3,4), @(4,3), @(4,1), @(4,4), @(4,2))
Check '11 draw detected' ((NameOf 'StatusTitleText') -eq 'Hòa cờ!') (NameOf 'StatusTitleText')

# 12. Narrow layout: all three cards inside the window
Resize-Caro 720 640
$win = (Get-CaroWindow).Current.BoundingRectangle
$back = (Find-ById 'BackToSetupButton').Current.BoundingRectangle
Check '12 narrow: actions visible inside window' ($back.Right -le $win.Right -and $back.Bottom -lt $win.Bottom -and $back.Width -gt 0) "back=$back win=$win"

# 13. 20 x 20 with blocked-both-ends rule
Resize-Caro 1280 860
Invoke-ById 'BackToSetupButton'; Confirm-Dialog
Set-EditText 0 '20'
$toggleEl = (Get-CaroWindow).FindFirst($TS::Descendants, (New-Object System.Windows.Automation.PropertyCondition($AE::ClassNameProperty, 'ToggleSwitch')))
$tp = $toggleEl.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
if ($tp.Current.ToggleState -ne 'On') { $tp.Toggle(); Start-Sleep -Milliseconds 500 }
Invoke-ById 'StartButton'; Start-Sleep 2
Check '13 board has 400 cells' ((Get-CellElements).Count -eq 400)
Play-Cells @(@(10,4), @(10,3), @(10,5), @(10,9), @(10,6), @(14,14), @(10,7), @(15,15), @(10,8))
Check '13 blocked five is not a win' ((NameOf 'StatusTitleText') -eq 'Lượt của Bình') (NameOf 'StatusTitleText')

# 14. Wide layout: side panel on the right of the board
$win = (Get-CaroWindow).Current.BoundingRectangle
$back = (Find-ById 'BackToSetupButton').Current.BoundingRectangle
Check '14 wide: side panel on the right' ($back.Left -gt ($win.Left + $win.Width * 0.6)) "back=$back win=$win"

Get-Process -Name 'caro-game' -ErrorAction SilentlyContinue | Stop-Process -Force
"FAILURES: $script:fails"
