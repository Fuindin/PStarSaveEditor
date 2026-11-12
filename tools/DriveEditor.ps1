<#
.SYNOPSIS
    Drives the Phantasy Star save editor through its real user interface, edits one
    or more characters (and optionally meseta), saves them all at once, reloads the
    file from disk and reports what it displayed before and after.

.DESCRIPTION
    Launches the built editor, picks a game from the GAME box, opens a save state
    through the Open dialog, then for each requested edit selects the character on
    the party list, reads the field, and types the new value. When every edit is in
    it clicks SAVE ALL once, dismisses the confirmation, uses File > Reload from
    Disk so the values come back from the file itself, and reads every field again.

    This exercises the same path a person does — game switching, the file dialog,
    the party list, the field editors, pending edits carried across characters, and
    the single save — so it catches problems that calling the assembly would miss.

    ALWAYS POINT THIS AT A COPY of a save state, never one you care about. The
    editor writes a <file>.bak beside it before the first save of a session, but a
    test script should not be the only thing standing between you and a lost save.

    Two quirks of automating this application are worth knowing:

      * The Open dialog is a classic Win32 dialog (window class #32770), and UI
        Automation cannot enumerate its children. Its file name box and Open button
        are therefore driven with raw Win32 messages instead.

      * InvokePattern.Invoke() does not return while a modal dialog is open, so any
        click that raises one (Open, Save All) is posted as BM_CLICK rather than
        invoked, and the resulting dialog is handled separately.

.PARAMETER Game
    Game as listed in the GAME box: "Phantasy Star", "Phantasy Star II",
    "Phantasy Star III" or "Phantasy Star IV".

.PARAMETER SaveFile
    Full path to the save state to load. Use a copy.

.PARAMETER Edits
    "Character:field=value" entries, applied in order, plus optionally "meseta=value".
    Character is the name on the party list (for Phantasy Star III, the name stored
    in the save, e.g. "Rhys"). Field keys:

      all games   exp, hpCur, hpMax, mpCur, mpMax   (mp* is MP in PS1, TP after)
      PS1         attack, defense
      PS II       strength, mental, agility, luck, dexterity, attack, defense
      PS III      damage, defense, speed, luck, skill, poison (1 = poisoned, 0 = cured)
      PS IV       strength, mental, agility, dexterity, attack, defense

    "level" can be listed with -Read to show it, but it is not editable.

.PARAMETER Read
    Extra "Character:field" entries to report before and after without changing them.

.PARAMETER ExePath
    The editor to drive. Defaults to the Release build in this repository.

.EXAMPLE
    # Phantasy Star: two byte little-endian meseta and experience, across two characters
    .\DriveEditor.ps1 -Game "Phantasy Star" -SaveFile "D:\saves\COPY-PStar1.ss0" `
        -Edits @("meseta=50000","Alis Landale:exp=41000","Alis Landale:mpMax=45","Noah:hpMax=200")

.EXAMPLE
    # Phantasy Star II: four byte meseta and experience, two byte stats
    .\DriveEditor.ps1 -Game "Phantasy Star II" -SaveFile "D:\saves\COPY-PStar2.gs0" `
        -Edits @("meseta=250000","Rolf Landale:hpMax=300","Nei:strength=240","Amy Sage:mpMax=180")

.EXAMPLE
    # Phantasy Star III: the poison flag and single byte stats
    .\DriveEditor.ps1 -Game "Phantasy Star III" -SaveFile "D:\saves\COPY-PStar3.gs0" `
        -Edits @("Rhys:poison=0","Rhys:luck=99","Mieu:speed=80") -Read @("Rhys:level")

.EXAMPLE
    # Phantasy Star IV: the single byte stat path, which a two byte write corrupts
    .\DriveEditor.ps1 -Game "Phantasy Star IV" -SaveFile "D:\saves\COPY-PStar4.gs0" `
        -Edits @("meseta=1234567","Rune:strength=120","Chaz:mental=90","Rika:hpMax=500")

.NOTES
    After a run, compare the edited copy against its .bak at the byte level. The
    number of differing bytes should account for exactly the fields that were
    changed and nothing else; anything extra means a write is spilling into
    neighbouring data.
#>
param(
  [Parameter(Mandatory=$true)][string]$Game,
  [Parameter(Mandatory=$true)][string]$SaveFile,
  [Parameter(Mandatory=$true)][string[]]$Edits,
  [string[]]$Read = @(),
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

# ---- parse the edit list -----------------------------------------------------
$plan = New-Object System.Collections.ArrayList
foreach ($e in $Edits) {
  if ($e -match '^\s*meseta\s*=\s*(\d+)\s*$') {
    [void]$plan.Add([pscustomobject]@{ Char = $null; Field = 'meseta'; Value = $Matches[1]; Set = $true })
  } elseif ($e -match '^\s*(.+?)\s*:\s*(\w+)\s*=\s*(\d+)\s*$') {
    [void]$plan.Add([pscustomobject]@{ Char = $Matches[1]; Field = $Matches[2]; Value = $Matches[3]; Set = $true })
  } else {
    throw "Can't read edit '$e'. Use 'Character:field=value' or 'meseta=value'."
  }
}
foreach ($r in $Read) {
  if ($r -match '^\s*(.+?)\s*:\s*(\w+)\s*$') {
    [void]$plan.Add([pscustomobject]@{ Char = $Matches[1]; Field = $Matches[2]; Value = $null; Set = $false })
  } else {
    throw "Can't read '$r'. Use 'Character:field'."
  }
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
$CT = [Windows.Automation.ControlType]

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

# Clicks a button of a message box (the first one unless $button matches another)
# and returns "[title] text".
function Dismiss-MessageBox($procId, $pattern, $timeoutSec = 10, $button = '.') {
  $box = Wait-Window $procId $pattern $timeoutSec
  if (-not $box) { return $null }
  $bk  = Get-ChildControls $box.hwnd
  $msg = (($bk | Where-Object { $_.class -eq 'Static' -and $_.text }) | ForEach-Object { $_.text }) -join ' '
  $btn = $bk | Where-Object { $_.class -eq 'Button' -and ($_.text -replace '&', '') -match $button } | Select-Object -First 1
  if (-not $btn) { $btn = $bk | Where-Object { $_.class -eq 'Button' } | Select-Object -First 1 }
  [void][Nat]::PostMessage($btn.hwnd, $BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero)
  Start-Sleep -Milliseconds 800
  return ("[{0}] {1}" -f $box.title, ($msg -replace '\s+', ' '))
}

$proc = Start-Process $ExePath -PassThru
try {
  Start-Sleep -Seconds 3
  $procId = $proc.Id

  $win = $AE::RootElement.FindFirst($TS::Children,
           (New-Object Windows.Automation.PropertyCondition($AE::ProcessIdProperty, $procId)))
  if (-not $win -or $win.Current.Name -notlike 'Phantasy Star Save Editor*') {
    throw "Main window not found. Did the application start?"
  }

  function El($id) { $win.FindFirst($TS::Descendants, (New-Object Windows.Automation.PropertyCondition($AE::AutomationIdProperty,$id))) }

  # The value-bearing element for a field: the box itself, or the edit inside a NumericUpDown.
  function ValueTarget($e) {
    if ($e.Current.ControlType -ne $CT::Edit) {
      $inner = $e.FindFirst($TS::Descendants, (New-Object Windows.Automation.PropertyCondition($AE::ControlTypeProperty, $CT::Edit)))
      if ($inner) { return $inner }
    }
    $p = $null
    if ($e.TryGetCurrentPattern([Windows.Automation.ValuePattern]::Pattern, [ref]$p)) { return $e }
    return $null
  }

  function GetField($id) {
    $e = El $id
    if (-not $e) { return '<hidden: not used by this character>' }
    $t = $null
    if ($e.TryGetCurrentPattern([Windows.Automation.TogglePattern]::Pattern, [ref]$t)) {
      return [string]$t.Current.ToggleState
    }
    $v = ValueTarget $e
    if (-not $v) { return '<no value>' }
    return $v.GetCurrentPattern([Windows.Automation.ValuePattern]::Pattern).Current.Value
  }

  function SetField($id, $value) {
    $e = El $id
    if (-not $e) { throw "Field '$id' not found. Check the field key for this game." }
    $t = $null
    if ($e.TryGetCurrentPattern([Windows.Automation.TogglePattern]::Pattern, [ref]$t)) {
      $want = if ([int]$value -ne 0) { 'On' } else { 'Off' }
      for ($i = 0; $i -lt 3 -and [string]$t.Current.ToggleState -ne $want; $i++) { $t.Toggle(); Start-Sleep -Milliseconds 150 }
      return
    }
    # Focus the box and type, as a person would; focus leaving it commits the value.
    $v = ValueTarget $e
    $v.SetFocus()
    $v.GetCurrentPattern([Windows.Automation.ValuePattern]::Pattern).SetValue([string]$value)
    Start-Sleep -Milliseconds 150
    (El '_lstChars').SetFocus()
    Start-Sleep -Milliseconds 200
  }

  function Select-Character($name) {
    $lst = El '_lstChars'
    $item = $lst.FindAll($TS::Children,
              (New-Object Windows.Automation.PropertyCondition($AE::ControlTypeProperty, $CT::ListItem))) |
            Where-Object { $_.Current.Name -eq $name } | Select-Object -First 1
    if (-not $item) {
      $names = ($lst.FindAll($TS::Children, [Windows.Automation.Condition]::TrueCondition) | ForEach-Object { $_.Current.Name }) -join ', '
      throw "Character '$name' is not on the party list ($names)."
    }
    $lst.SetFocus()
    $item.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 500
  }

  function Report($title) {
    Write-Output $title
    foreach ($p in $plan) {
      if ($p.Field -eq 'meseta') {
        Write-Output ("  {0,-22}: {1}" -f 'meseta', (GetField '_numMeseta'))
      } else {
        Select-Character $p.Char
        Write-Output ("  {0,-22}: {1}" -f "$($p.Char) $($p.Field)", (GetField "field_$($p.Field)"))
      }
    }
  }

  # --- pick the game ---------------------------------------------------------
  $cbo = El '_cboGame'
  $cbo.GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
  Start-Sleep -Milliseconds 500
  $gi = $cbo.FindAll($TS::Descendants,
          (New-Object Windows.Automation.PropertyCondition($AE::ControlTypeProperty, $CT::ListItem))) |
        Where-Object { $_.Current.Name -eq $Game } | Select-Object -First 1
  if (-not $gi) { throw "'$Game' is not in the GAME box." }
  $gi.GetCurrentPattern([Windows.Automation.SelectionItemPattern]::Pattern).Select()
  $cbo.GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern).Collapse()
  Start-Sleep -Milliseconds 900
  Write-Output "game: selected '$Game'"

  # --- open the save state ---------------------------------------------------
  # Posted, not invoked: the dialog is modal and would block Invoke().
  [void][Nat]::PostMessage([IntPtr](El '_btnOpen').Current.NativeWindowHandle, $BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero)
  $dlg = Wait-Window $procId 'save state' 10
  if (-not $dlg) { throw "The Open dialog never appeared." }
  # #32770 is opaque to UI Automation, so use Win32 on its children.
  $kids  = Get-ChildControls $dlg.hwnd
  $editH = ($kids | Where-Object { $_.class -eq 'Edit' } | Select-Object -First 1).hwnd
  $openH = ($kids | Where-Object { $_.class -eq 'Button' -and $_.text -match 'Open' } | Select-Object -First 1).hwnd
  [void][Nat]::SendMessage($editH, $WM_SETTEXT, [IntPtr]::Zero, $SaveFile)
  Start-Sleep -Milliseconds 300
  [void][Nat]::PostMessage($openH, $BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero)
  Start-Sleep -Seconds 2
  $warn = Dismiss-MessageBox $procId '^(Not a matching save state|Open failed)' 1
  if ($warn) { throw "The editor refused the file: $warn" }
  $loaded = (El '_lblFile').Current.Name
  if ($loaded -ne $SaveFile) { throw "Expected '$SaveFile' to be loaded but the editor shows '$loaded'." }
  Write-Output "loaded: $loaded"

  Report "BEFORE"

  # --- make every edit, across characters, without saving in between ---------
  foreach ($p in $plan | Where-Object { $_.Set }) {
    if ($p.Field -eq 'meseta') {
      SetField '_numMeseta' $p.Value
      Write-Output ("entered meseta = {0}" -f $p.Value)
    } else {
      Select-Character $p.Char
      SetField "field_$($p.Field)" $p.Value
      Write-Output ("entered {0} {1} = {2}" -f $p.Char, $p.Field, $p.Value)
    }
  }

  Write-Output ("banner: {0}" -f (El '_lblBanner').Current.Name)

  # --- one save for everything ---------------------------------------------------
  # Posted for the same reason as Open: this raises a modal message box.
  [void][Nat]::PostMessage([IntPtr](El '_btnSave').Current.NativeWindowHandle, $BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero)
  $msg = Dismiss-MessageBox $procId '^(Saved|Save|Save failed|File changed on disk)$' 10
  Write-Output ("dialog: {0}" -f $(if ($msg) { $msg } else { '(none appeared)' }))

  # --- reload from disk so the AFTER values come from the file itself ----------
  $file = $win.FindFirst($TS::Descendants, (New-Object Windows.Automation.AndCondition(
            (New-Object Windows.Automation.PropertyCondition($AE::ControlTypeProperty, $CT::MenuItem)),
            (New-Object Windows.Automation.PropertyCondition($AE::NameProperty, 'File')))))
  $file.GetCurrentPattern([Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
  Start-Sleep -Milliseconds 500
  $reload = $win.FindAll($TS::Descendants,
              (New-Object Windows.Automation.PropertyCondition($AE::ControlTypeProperty, $CT::MenuItem))) |
            Where-Object { $_.Current.Name -like 'Reload from Disk*' } | Select-Object -First 1
  if ($reload) {
    $reload.GetCurrentPattern([Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Seconds 1
    Write-Output "reloaded from disk"
  } else {
    Write-Output "reload: menu item not found; AFTER shows the in-memory values"
  }

  Report "AFTER"
}
finally {
  if ($proc -and -not $proc.HasExited) {
    $proc.CloseMainWindow() | Out-Null
    Start-Sleep -Seconds 2
    # If the editor asked about unsaved changes, answer No.
    Dismiss-MessageBox $proc.Id '^Unsaved changes$' 1 '^No$' | Out-Null
    if (-not $proc.HasExited) { $proc.Kill() }
  }
  Write-Output "closed"
}
