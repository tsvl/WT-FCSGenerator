# WT-FCSGenerator

Custom sight generator for War Thunder ground vehicles. Generates ballistic sights with real shell trajectories, penetration values, and rangefinder markings for every vehicle in the game.

Originally created by [Assin127](https://live.warthunder.com/user/58909037/), now maintained and rewritten by [tsvl](https://github.com/tsvl).

## Download

1. Get the latest build from the [Releases](https://github.com/tsvl/WT-FCSGenerator/releases) page (look for the `.zip` under the newest release).
2. Extract the ZIP anywhere.
3. Run `FCS.exe`. If prompted, install the [.NET 10 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

## Quick start

1. Open `FCS.exe`.
2. Set **Game Path** to your War Thunder install folder (e.g. `C:\Program Files\Steam\steamapps\common\WarThunder`).
3. Pick a **Sight type** from the dropdown.
4. Click **Generate Sights**.

The tool will extract the latest vehicle data from your install, compute ballistic tables, and write sight files to the `Output` folder next to `FCS.exe`.

> [!TIP]
> You can change the output path to point directly at the game's `UserSights` folder (see below) to skip the copy step.

## Installing sights into the game

### Where to put the files

Copy the generated sight folders from `Output/{sight type}/` into the game's `UserSights` folder:

- **Windows:** `Documents\My Games\WarThunder\Saves\{user id}\production\UserSights\`
- **Linux:** `$HOME/.config/WarThunder/Saves/{user id}/production/UserSights/`
- **macOS:** `Users/{user name}/My Games/WarThunder/Saves/{user id}/production/UserSights/`

Your user ID can be found on your profile page at [store.gaijin.net](https://store.gaijin.net). If the `UserSights` folder doesn't exist, create it.

### Loading sights in-game

1. In the hangar, open **sight customization** for a ground vehicle.
2. You'll see a **Choose preset** dropdown at the top (default: `User sight`) — this controls the overall combination of sight file + in-game overrides. Leave it on `User sight`.
3. Below that is the **Reticle** dropdown (default: `Default grid sight`) — this is where custom sights appear. Select one of the generated sights from this menu.

## Sight types

| Sight | Rangefinder | Sensitivity setup required | Notes |
| --- | --- | --- | --- |
| **Tochka-SM2** | Box | Yes | Full-featured; supports two-shell overlay, laser, rocket, and howitzer variants |
| **Luch** | Box | Yes | Lightweight, simplified geometry |
| **Sector** | Box | Yes | Sector-based scale; SPAA-oriented |
| **Duga** | Standard | No | Alternative layout, no special setup needed |
| **Duga-2** | Standard | No | Updated Duga geometry |
| **Luch Lite** | Standard | No | Minimal reticle, no special setup needed |

## Sensitivity and scroll wheel setup

> [!IMPORTANT]
> This section only applies to **box rangefinder** sights (Tochka-SM2, Luch, Sector). The other sights (Duga, Duga-2, Luch Lite) work without any special control configuration.

The box rangefinder sights use the scroll wheel to control sight distance, and the distance tick spacing is calculated based on a specific mouse wheel sensitivity value. For the rangefinder markings to line up correctly, your in-game sensitivity must match what the generator used.

### In-game controls setup

1. Go to **Controls** → **Ground Vehicles**.
2. Set the **Mouse Wheel (ground vehicles)** dropdown to `Sight distance control`.
3. Set **Mouse Wheel Multiplier (ground vehicles)** to match the sensitivity value in the generator (default: **50%**).
4. Open the **Sight distance control** axis binding page and make sure:
   - `Increase value` and `Decrease value` are **unbound** (clear any keybinds here).
   - All sensitivity/multiplier sliders on this page are at their **default** values.

> [!NOTE]
> War Thunder treats binding a physical axis (mouse wheel) directly to a control very differently from binding hotkeys to increase/decrease that control. The box rangefinder mechanism requires the direct axis binding — binding `Increase`/`Decrease` keys instead will not work correctly.

### If sights feel "off"

If the rangefinder box doesn't line up with the distance markings, the most likely cause is a mismatch between the generator sensitivity and the in-game `Mouse Wheel Multiplier`. Make sure both values match, then regenerate sights.

## Important caveats

1. **Shell switching is manual.** The sights are pre-computed with real ballistic data rather than using the game's built-in rangefinder, so the sight for a given shell type won't automatically update when you switch ammo in-game. You'll need to select the correct sight for the shell you're using via the Reticle dropdown.

2. **Nation selection affects load times.** Generating sights for all nations produces a large number of files. Only select the nations you actually play to keep game load times reasonable.

## Options

- **Language** — localization for sight labels (English, Russian, Chinese, etc.)
- **Sensitivity** — must match your in-game `Mouse Wheel Multiplier` (only affects box rangefinder sights)
- **Nation checkboxes** — which nations to generate sights for
- **Sight subtypes** — toggle individual sight variants (double shell overlay, time of flight, armor penetration display, etc.)
- **Advanced Settings** — colors, sizes, and positioning for sight elements

## Support

Something broken? Have questions? Join the [Discord](https://discord.gg/sJJXeD82tF).

## Credits

- **[Assin127](https://live.warthunder.com/user/58909037/)** — original author of FCS Manager and all sight families
- **[tsvl](https://github.com/tsvl)** — current maintainer
