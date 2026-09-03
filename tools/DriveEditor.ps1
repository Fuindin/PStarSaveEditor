<#
.SYNOPSIS
    Drives the Phantasy Star save state editor through its real user interface and
    reports what it displayed before and after an edit.

.DESCRIPTION
    Launches the built editor, picks a game from the Game menu, loads a save state
    through the Browse dialog, selects a character, reads the values on screen,
    types new ones, clicks Update Save State, dismisses the confirmation, reads the
    values again and closes the application.

    This exercises the same path a person does, so it catches problems that calling
    into the assembly directly would miss: menu wiring, panel switching, combo box
    population, the file dialog, and the update handlers.

    ALWAYS POINT THIS AT A COPY of a save state, never one you care about. The
    editor writes a <file>.bak beside it before the first edit of a session, but a
    test script should not be the only thing standing between you and a lost save.

    Two quirks of automating this application are worth knowing, because they look
    like bugs when you first hit them:

      * The Browse dialog is a classic Win32 dialog (window class #32770), and UI
        Automation cannot enumerate its children. Its file name box and Open button
        are therefore driven with raw Win32 messages instead.

      * InvokePattern.Invoke() does not return while a modal dialog is open, so any
        click that raises one (Browse, Update Save State) is posted as BM_CLICK
        rather than invoked, and the resulting dialog is handled separately.

.PARAMETER MenuItem
    Entry under the Game menu: "Phantasy Star", "Phantasy Star 2",
    "Phantasy Star 3" or "Phantasy Star 4".

.PARAMETER SaveFile
    Full path to the save state to load. Use a copy.

.PARAMETER ComboId
    Character combo box for the panel: ps1CharacterCmb .. ps4CharacterCmb.

.PARAMETER Character
    Entry to select in that combo, exactly as it appears (e.g. "Alis Landale",
    "Rolf Landale", "Rhys", "Chaz").

.PARAMETER UpdateBtnId
    Update button for the panel: upsPS1SaveStateBtn, ps2UpdSavStateBtn,
    ps3UpdSavStateBtn or ps4UpdSavStateBtn.

.PARAMETER ReadFields
    "controlId=Label" pairs read before and after the update.

.PARAMETER SetFields
    "controlId=value" pairs typed into the New value boxes.

.PARAMETER ExePath
    The editor to drive. Defaults to the Release build in this repository.

.EXAMPLE
    # Phantasy Star: two byte little-endian meseta and experience, one byte max MP
    .\DriveEditor.ps1 -MenuItem "Phantasy Star" `
        -SaveFile "D:\saves\COPY-PStar1.ssx" `
        -ComboId ps1CharacterCmb -Character "Alis Landale" `
        -UpdateBtnId upsPS1SaveStateBtn `
        -ReadFields @("ps1CurrentMesetaTb=Meseta","ps1ExpTb=Exp","ps1MaxMPTb=MaxMP") `
        -SetFields  @("ps1NewMesetaTb=50000","ps1NewExpTb=41000","ps1NewMaxMPTb=45")

.EXAMPLE
    # Phantasy Star 2: four byte big-endian meseta and experience, two byte stats
    .\DriveEditor.ps1 -MenuItem "Phantasy Star 2" `
        -SaveFile "D:\saves\COPY-PStar2.gsx" `
        -ComboId ps2CharacterCmb -Character "Rolf Landale" `
        -UpdateBtnId ps2UpdSavStateBtn `
        -ReadFields @("ps2CurMesetaTb=Meseta","ps2MaxHPTb=MaxHP","ps2StrTb=Str") `
        -SetFields  @("ps2NewMesetaTb=250000","ps2NewMaxHPTb=300","ps2NewStrTb=240")

.EXAMPLE
    # Phantasy Star 4: the single byte stat path, which a two byte write corrupts
    .\DriveEditor.ps1 -MenuItem "Phantasy Star 4" `
        -SaveFile "D:\saves\COPY-PStar4.gsx" `
        -ComboId ps4CharacterCmb -Character "Rune" `
        -UpdateBtnId ps4UpdSavStateBtn `
        -ReadFields @("ps4CurrentMesetaTb=Meseta","ps4StrTb=Str","ps4MentalTb=Mental") `
        -SetFields  @("ps4NewMesetaTb=1234567","ps4NewStrTb=120")

.NOTES
    After a run, compare the edited copy against the original at the byte level. The
    number of differing bytes should account for exactly the fields that were
    changed and nothing else; anything extra means a write is spilling into
    neighbouring data.
#>
param(
  [Parameter(Mandatory=$true)][string]$MenuItem,
  [Parameter(Mandatory=$true)][string]$SaveFile,
  [Parameter(Mandatory=$true)][string]$ComboId,
  [Parameter(Mandatory=$true)][string]$Character,
  [Parameter(Mandatory=$true)][string]$UpdateBtnId,
  [Parameter(Mandatory=$true)][string[]]$ReadFields,
  [Parameter(Mandatory=$true)][string[]]$SetFields,
  [string]$ExePath = (Join-Path $PSScriptRoot "..\PStarSaveEditor\bin\Release\net10.0-windows\PStarSaveEditor.exe")
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes

if (-not (Test-Path $ExePath)) {
  throw "Editor not found at '$ExePath'. Build it first: dotnet build -c Release"
}
if (-not (Test-Path $SaveFile)) {
  throw "Save state not found at '$SaveFile'."
}

$sig = @'
using System; using System.Text; using System.Runtime.InteropServices;
public class Nat {
  public delegate bool EnumProc(IntPtr h, IntPtr l);
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc cb, IntPtr l);
  [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr p, EnumProc cb, IntPtr l);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
  [DllImport("user32.dll", CharSet=CharSet.Auto)] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
  [DllImport("user32.dll", CharSet=CharSet.Auto)] public static extern int GetClassName(IntPtr h, StringBuilder s, int n);
  [DllImport("user32.dll")] public static extern int GetWindowThreadProcessId(IntPtr h, out int pid);
  [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, uint msg, IntPtr w, IntPtr l);
  [DllImport("user32.dll", CharSet=CharSet.Auto)] public static extern IntPtr SendMessage(IntPtr h, uint msg, IntPtr w, string l);
}
'@
Add-Type -TypeDefinition $sig

$BM_CLICK   = 0x00F5
$WM_SETTEXT = 0x000C
$AE = [Windows.Automation.AutomationElement]
$TS = [Windows.Automation.TreeScope]

function Get-TopWindows($procId) {
  $out = New-Object System.Collections.ArrayList
  $cb = [Nat+EnumProc]{
    param($h,$l)
    $p2 = 0; [void][Nat]::GetWindowThreadProcessId($h,[ref]$p2)
    if ($p2 -eq $procId -and [Nat]::IsWindowVisible($h)) {
      $sb = New-Object Text.StringBuilder 512; [void][Nat]::GetWindowText($h,$sb,512)
      $cn = New-Object Text.StringBuilder 512; [void][Nat]::GetClassName($h,$cn,512)
      [void]$out.Add([pscustomobject]@{ hwnd=$h; title=$sb.ToString(); class=$cn.ToString() })
    }
    return $true
  }
  [void][Nat]::EnumWindows($cb,[IntPtr]::Zero)
  return $out
}

function Get-ChildControls($parent) {
  $out = New-Object System.Collections.ArrayList
  $cb = [Nat+EnumProc]{
    param($h,$l)
    $cn = New-Object Text.StringBuilder 256; [void][Nat]::GetClassName($h,$cn,256)
    $tx = New-Object Text.StringBuilder 512; [void][Nat]::GetWindowText($h,$tx,512)
    [void]$out.Add([pscustomobject]@{ hwnd=$h; class=$cn.ToString(); text=$tx.ToString() })
    return $true
  }
  [void][Nat]::EnumChildWindows($parent,$cb,[IntPtr]::Zero)
  return $out
}

function Wait-Window($procId, $pattern, $timeoutSec = 10) {
  $deadline = (Get-Date).AddSeconds($timeoutSec)
  while ((Get-Date) -lt $deadline) {
    $w = Get-TopWindows $procId | Where-Object { $_.title -match $pattern }
    if ($w) { return $w[0] }
    Start-Sleep -Milliseconds 250
  }
  return $null
}

$proc = Start-Process $ExePath -PassThru
try {
  Start-Sleep -Seconds 3
  $procId = $proc.Id

  $win = $AE::RootElement.FindFirst($TS::Children,
           (New-Object Windows.Automation.PropertyCondition($AE::NameProperty,"Phantasy Star Save State Editor")))
  if (-not $win) { throw "Main window not found. Did the application start?" }

  function El($id)      { $win.FindFirst($TS::Descendants, (New-Object Windows.Automation.PropertyCondition($AE::AutomationIdProperty,$id))) }
  function GetVal($id)  { $e = El $id; if ($e) { $e.GetCurrentPattern([Windows.Automation.ValuePattern]::Pattern).Current.Value } else { '<not found>' } }
  function SetVal($id,$v) { (El $id).GetCurrentPattern([Windows.Automation.ValuePattern]::Pattern).SetValue($v) }
  function ByName($n)   { $win.FindFirst($TS::Descendants, (New-Object Windows.Automation.PropertyCondition($AE::NameProperty,$n))) }

  # --- pick the game ---------------------------------------------------------
  (ByName "Game").GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
  Start-Sleep -Milliseconds 500
  $mi = $win.FindAll($TS::Descendants,
          (New-Object Windows.Automation.PropertyCondition($AE::ControlTypeProperty,[Windows.Automation.ControlType]::MenuItem))) |
        Where-Object { $_.Current.Name -eq $MenuItem } | Select-Object -First 1
  if (-not $mi) { throw "'$MenuItem' is not on the Game menu." }
  $mi.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
  Start-Sleep -Milliseconds 900
  Write-Output "menu: selected '$MenuItem'"

  # --- load the save state ---------------------------------------------------
  # Posted, not invoked: the dialog is modal and would block Invoke().
  [void][Nat]::PostMessage([IntPtr](El 'browseBtn').Current.NativeWindowHandle, $BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero)
  $dlg = Wait-Window $procId 'save state file' 10
  if (-not $dlg) { throw "The Browse dialog never appeared." }
  # #32770 is opaque to UI Automation, so use Win32 on its children.
  $kids  = Get-ChildControls $dlg.hwnd
  $editH = ($kids | Where-Object { $_.class -eq 'Edit' } | Select-Object -First 1).hwnd
  $openH = ($kids | Where-Object { $_.class -eq 'Button' -and $_.text -match 'Open' } | Select-Object -First 1).hwnd
  [void][Nat]::SendMessage($editH, $WM_SETTEXT, [IntPtr]::Zero, $SaveFile)
  Start-Sleep -Milliseconds 300
  [void][Nat]::PostMessage($openH, $BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero)
  Start-Sleep -Seconds 2
  $loaded = GetVal 'saveStateFileTb'
  if ($loaded -ne $SaveFile) { throw "Expected '$SaveFile' to be loaded but the editor shows '$loaded'." }
  Write-Output "loaded: $loaded"

  # --- choose the character --------------------------------------------------
  $cmb = El $ComboId
  if (-not $cmb) { throw "Combo box '$ComboId' not found on this panel." }
  $cmb.GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
  Start-Sleep -Milliseconds 500
  $item = $cmb.FindAll($TS::Descendants,
            (New-Object Windows.Automation.PropertyCondition($AE::ControlTypeProperty,[Windows.Automation.ControlType]::ListItem))) |
          Where-Object { $_.Current.Name -eq $Character } | Select-Object -First 1
  if (-not $item) { throw "Character '$Character' is not in $ComboId." }
  $item.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
  Start-Sleep -Milliseconds 800
  Write-Output "character: selected '$Character'"

  Write-Output "BEFORE"
  foreach ($f in $ReadFields) {
    $id,$label = $f -split '=',2
    Write-Output ("  {0,-14}: {1}" -f $label, (GetVal $id))
  }

  foreach ($f in $SetFields) {
    $id,$v = $f -split '=',2
    SetVal $id $v
    Write-Output ("entered {0} = {1}" -f $id, $v)
  }
  Start-Sleep -Milliseconds 300

  # --- update ----------------------------------------------------------------
  # Posted for the same reason as Browse: this raises a modal message box.
  [void][Nat]::PostMessage([IntPtr](El $UpdateBtnId).Current.NativeWindowHandle, $BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero)
  $box = Wait-Window $procId '^(Information|Warning|Error)' 10
  if ($box) {
    $bk  = Get-ChildControls $box.hwnd
    $msg = ($bk | Where-Object { $_.class -eq 'Static' -and $_.text } | Select-Object -First 1).text
    Write-Output ("dialog: [{0}] {1}" -f $box.title, $msg)
    [void][Nat]::PostMessage(($bk | Where-Object { $_.class -eq 'Button' } | Select-Object -First 1).hwnd, $BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero)
    Start-Sleep -Seconds 1
  } else {
    Write-Output "dialog: (none appeared)"
  }

  Start-Sleep -Milliseconds 500
  Write-Output "AFTER"
  foreach ($f in $ReadFields) {
    $id,$label = $f -split '=',2
    Write-Output ("  {0,-14}: {1}" -f $label, (GetVal $id))
  }
}
finally {
  if ($proc -and -not $proc.HasExited) {
    $proc.CloseMainWindow() | Out-Null
    Start-Sleep -Seconds 2
    if (-not $proc.HasExited) { $proc.Kill() }
  }
  Write-Output "closed"
}
