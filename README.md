# 🌀 PARAMANIACS

**Fun, quirky and playful components for Rhino & Grasshopper.**
Games, random events and weird tools for architects and designers who need a 5-minute break from serious modeling.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

> ⚠️ **Status:** early development (v0.1). One component per host so far. Ideas and contributors are very welcome!

---

## What is PARAMANIACS?

PARAMANIACS is a free, open-source plugin for **Rhino** and **Grasshopper**. It is a collection of components and commands built purely for fun, not for productivity.

Ever been staring at a parametric definition for six hours and needed a break? Drop a component on the canvas and:

- 🎮 play a **two-player game** inside the Rhino viewport
- 🎲 trigger a **random (harmless!) event** in your Rhino/Grasshopper window
- 🧪 run a strange command that does something delightfully useless
- 🤝 share a goofy component with your classmates or colleagues

## What's in the box

| Where | Name | What it does |
|---|---|---|
| Rhino | `ParamaniacsColors` command | Gives Rhino a random new color theme (viewport, grid, command line, crosshair). Choose **Restore** to go back. |
| Grasshopper | **PARAMANIACS → Colors → Color Shuffle** | Gives the Grasshopper canvas a random new color theme when you press a Button. Connect `true` to *Restore* (or delete the component) to go back. |
| Grasshopper | **PARAMANIACS → Colors → Chameleon** | Gives every component on the canvas its own random color. Press the Button again for new colors. Your definition is never changed. Connect `true` to *Restore* (or delete the component) to go back. |

## Ideas on the roadmap

These are only ideas — nothing is promised. Add yours in [Discussions](../../discussions)!

- [ ] Two-player game components (Tic-Tac-Toe, Pong, Snake…) using the Rhino viewport
- [ ] "Random Event" component that plays a harmless surprise inside Grasshopper
- [ ] Mini-games controlled by sliders and panels
- [ ] Silly commands for the Rhino command line

## Requirements

- Rhino 8 (Windows) — other versions will be considered later
- Grasshopper (included with Rhino)

## Installation

Once a release exists:

1. In Rhino, run the `PackageManager` command, search for **paramaniacs** and install it, **or**
2. Download the latest `.yak` file from the [Releases](../../releases) page and install it with `yak install <file>`.

Restart Rhino after installing.

## Usage

- **Rhino:** type `ParamaniacsColors` in the command line and press Enter.
- **Grasshopper:** open the **PARAMANIACS** tab → **Colors** → **Color Shuffle**, connect a *Button* to `Run`, and press it.

### Something looks weird and I want my colors back

Run `ParamaniacsColors` and choose **Restore**. If Rhino ever crashes in the middle of a shuffle, open *Tools → Options → Appearance → Colors* and use **Restore Defaults**. Grasshopper colors only change in memory (and Chameleon only paints on top of the canvas), so restarting Grasshopper also resets them.

## Building from source

Requirements: Windows, Rhino 8, and the .NET SDK 8 (or Visual Studio 2022).

```bash
dotnet build Paramaniacs.sln
```

Then load the build in Rhino:

- **Rhino plug-in:** drag `src/Paramaniacs.Rhino/bin/Debug/<framework>/Paramaniacs.Rhino.rhp` into Rhino.
- **Grasshopper plug-in:** run `GrasshopperDeveloperSettings` in Rhino and add the folder `src/Paramaniacs.Grasshopper/bin/Debug/<framework>` to the list of loaded folders, then restart Rhino.

To create the Yak package locally: `./scripts/package.ps1` (output goes to `dist/`).

## Project structure

```
src/
  Paramaniacs.Core/          Shared logic with no Rhino dependency (e.g. random color themes)
  Paramaniacs.Rhino/         The Rhino plug-in (.rhp)
    Commands/<Feature>/        one folder per command
  Paramaniacs.Grasshopper/   The Grasshopper plug-in (.gha)
    Components/
      ParamaniacsComponent.cs    base class: every component inherits from it
      SubCategories.cs           sections inside the PARAMANIACS tab
      ColorShuffle/              one folder per component
      Chameleon/
yak/                         Yak package manifest
scripts/                     Packaging script
.github/workflows/           CI (build) and release (Yak + GitHub Release)
```

## Safety promise 🛡️

PARAMANIACS is meant to be fun, never harmful. Every component must follow our [safety rules](CONTRIBUTING.md#safety-rules): it only affects Rhino/Grasshopper, never touches your system or files without telling you, never uses the network silently, and never destroys your work. If you ever see a component break these rules, please report it (see [SECURITY.md](SECURITY.md)).

## Contributing

Everyone is welcome: architecture students, professors, programmers and people who just have a crazy idea. Read [CONTRIBUTING.md](CONTRIBUTING.md) to get started. You don't need to be an expert to contribute — look for issues labeled `good first issue`.

Please follow our [Code of Conduct](CODE_OF_CONDUCT.md).

## License

Released under the [MIT License](LICENSE). You can use, modify and share it freely.

## Disclaimer

PARAMANIACS is an independent community project. It is **not affiliated with, endorsed by, or sponsored by Robert McNeel & Associates**. *Rhino®*, *Rhinoceros®* and *Grasshopper®* are trademarks of Robert McNeel & Associates.
