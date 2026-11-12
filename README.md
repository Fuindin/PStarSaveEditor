# PStarSaveEditor

Phantasy Star Save Editor is a save state editor for **Phantasy Star** (Sega Master System) and **Phantasy Star II**, **Phantasy Star III: Generations of Doom** and **Phantasy Star IV: The End of the Millennium** (Sega Genesis / Mega Drive).

It works with save states created by the **Kega Fusion 3.64** emulator: Master System states (`.ss0` – `.ss9`) for Phantasy Star, and Genesis states (`.gs0` – `.gs9`) for the three Genesis games. When you open a file the editor checks it fits the selected game — a Genesis state must carry Fusion's `GST` header, and every file must be large enough to hold the game's data — so you can't accidentally edit the wrong file.

## Features

- A themed interface modelled on the games' own menus — dark windows with white frames, amber headings like the series logo, a starfield header like the title screens, and HP / TP gauges on each party member's card. Each game has its own accent colour, and the panel beside the party shows the game's box art (or, if none is embedded, the Algo star system with that game's world picked out).
- Edit meseta, experience, HP / TP (MP in the first game) and stats for every party member. Phantasy Star III's poison status can be cured or set.
- **Edit as many characters as you like before saving.** Every change is kept in memory as you move between characters; edited characters are marked ● on the list, and a banner lists everything that's pending. **Save All** writes all of it in one go.
- View each character's equipment (Phantasy Star and Phantasy Star IV).
- Warns before closing, switching games or opening another file with unsaved changes, and if the emulator has rewritten the file since you opened it.
- Drag a save state onto the window to open it.

Phantasy Star III's generations reuse the same five party records, so its list shows one card per party slot, named after whoever holds it in your save.

## Using it

1. Pick the game from the **GAME** box (or the **Game** menu).
2. **File ▸ Open…** your Kega Fusion save state (or drag it onto the window).
3. Choose a character on the left and edit the amber-labelled rows on the **STATUS** tab. Grey rows are shown for reference only.
4. Repeat for any other characters, and edit **MESETA** at the top.
5. Click **Save All** (Ctrl+S), then load the state in Kega Fusion.

## Backups

The first time you save a file in a session, the editor copies the untouched save state to `<file>.bak` next to the original, so the pre-edit state can always be recovered. Even so, it's a good idea to keep your own backup of anything important.

## Box art (optional)

To show the real box art beside the party list, put images named `ps1`, `ps2`, `ps3` and `ps4` (`.jpg`, `.jpeg` or `.png`, lower-case extension) in `PStarSaveEditor\Covers\` and rebuild. They are embedded in the exe. Any game without one gets the Algo star system drawing instead.

## Requirements

- Windows
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) (to run)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (to build)

## Running

Download / extract `PStarSaveEditor.zip` from the repository root and run `PStarSaveEditor.exe`.

## Building

The project is an SDK-style .NET 10 (`net10.0-windows`) Windows Forms app.

```
dotnet build -c Release
```

or open `PStarSaveEditor.sln` in Visual Studio 2022+.

## Project layout

- `Game/` — the in-memory save state (`SaveState.cs`), one definition per game with its offsets and fields, and the item name tables.
- `MainForm.cs` / `Theme.cs` / `PhantasyTabControl.cs` — the UI.

## Tools

`tools/DriveEditor.ps1` drives the built editor through its real user interface — game selection, the Open dialog, the party list, edits to several characters, a single **Save All**, and a reload from disk — then reports the values it displayed before and after. It is useful for checking a change end to end rather than only through the code. Run `Get-Help .\tools\DriveEditor.ps1 -Full` for the parameters and worked examples for each game. Point it at a *copy* of a save state.

## Background

I created this project after wanting to play through Phantasy Star III without having to "grind". I have always been a fan of the Phantasy Star series. I played through Phantasy Star III after renting it at a local video game rental store / arcade when the game was first released, bought it not long after, and have played through it many times. When I got the itch to play it again I didn't want to grind for money or experience, and after searching for a few hours I couldn't find a tool that worked in Windows 8 / 10 — so I made my own.

I started the editor for Phantasy Star III, then added Phantasy Star II, Phantasy Star IV, and Phantasy Star. It has been a lot of fun working on this project and I hope it helps other people go back and play these games again.
