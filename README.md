# Land Usurpation Deed

Template from https://github.com/MinusGix/ElinExampleMod  
Inspired by https://github.com/105gun/ElinMoongateInvasion  
Run `dotnet build` to build

## What it does

Adds the **Land Usurpation Deed** — a new item that lets you claim any player-published
base as your own territory.

Player bases (`Zone_User`) are only accessible via Moongate — they cannot be entered
directly from the world map. So unlike the vanilla Land Deed (used *on* the world map),
the Usurpation Deed is used **from inside** the target base after entering it via Moongate.

| Where you are | Result |
|---|---|
| Inside a Zone_User (enemy player base) | ✅ Can usurp |
| Inside your own faction zone | ❌ Blocked |
| Anywhere else (dungeon, town, world map…) | ❌ Blocked |

**Flow:**
1. Enter a player base via Moongate
2. Use the deed from your inventory while inside
3. Pick which of your home branches to relocate there
4. Confirm — you are returned to the world map, the foreign base is destroyed, your branch takes its place at those elomap coordinates

The original **Land Deed (Relocate)** is untouched. This is a separate item.

## Getting the item in-game

Use the debug console:
```
item deed_usurpation
```

## Build & Install

1. Copy `Directory.Build.props.template` → `Directory.Build.props` and set `<GamePath>` to your Elin folder.
2. `dotnet build`
3. Done — the DLL and `package/` contents are copied automatically to `Elin/Package/UsurpationDeed/`.
