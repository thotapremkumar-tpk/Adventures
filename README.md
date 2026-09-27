# Adventures — Treasure Hunter (GDIT Group Project)

A third-person treasure-hunting adventure made in **Unity 6 (6000.6.2f1, URP)** with Blender assets.

An archaeologist finds an ancient map and the **Sun Key**, travels to the Old Cave, falls deep underground and must survive five trials to reach the Treasure of the Old Kings.

## Levels (current demo)
| Scene | What happens |
|---|---|
| `00_Intro` | The study: examine the map on the desk (E), get the Sun Key, open the north door |
| `01_OldCave` | Dark cave lit by your torch; the floor collapses and you fall underground |
| `02_Level1_FanTower` | Open the Sun Door, ride 6 spinning/moving fans over an acid lake, take the Moon Key, open the Eclipse Door |
| `03_Level2_Maze` | Proverb labyrinth: follow the rising sun, 3-coil serpent, turn away from the owl, find the golden chest |
| Levels 3–5 | Coming soon |

## How to open
1. Install **Unity 6000.6.2f1** via Unity Hub.
2. Clone this repo, then in Unity Hub: **Add → Add project from disk** → select the cloned folder.
3. Open `Assets/Adventures/Scenes/00_Intro.unity` and press **Play**.

## Controls
WASD move · Shift sprint · Space jump · Mouse look (click Game view) · Q/R turn camera · E interact · F torch · M map · Tab re-read proverb · H help · **F5 skip to next scene (dev)**

## Project layout
- `Assets/Adventures/Scripts` – gameplay (player, camera, doors, keys, fans, traps, UI, sound)
- `Assets/Adventures/Editor` – scene builder (**menu: Adventures → Build All Scenes** regenerates all 4 scenes — this overwrites manual edits in them!)
- `Assets/Adventures/Scenes` – the game scenes
- `Assets/ThirdParty` – CC0 assets (KayKit Dungeon Remastered, KayKit Adventurers by Kay Lousberg)

## Team workflow
- Pull before you start: `git pull`
- **One person per scene at a time** (Unity scenes merge badly). Say in the group chat which scene you're editing.
- Commit small, clear messages; push when done: `git add -A && git commit -m "Level 3: add lava bridge" && git push`
- Use GitHub **Issues** for tasks/bugs and **Projects** board for To Do → Doing → Done.

## Credits
- KayKit Dungeon Remastered & Adventurers — Kay Lousberg (CC0)
- Procedural textures, meshes and sound effects generated in-project
