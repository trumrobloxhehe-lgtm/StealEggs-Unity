# StealEggs3D — Unity Gameplay Prototype

This project is an original, Roblox-inspired egg-stealing simulator prototype. It is **not an exact copy** of another game's protected character models, maps, UI, or art, and it is not yet a finished online multiplayer game.

## Current prototype features
- Third-person orbit camera: hold right mouse button and drag to rotate; wheel to zoom.
- Block-style avatar with procedural idle/walk/run/jump animations.
- WASD/arrows to move, Space to jump, Shift to sprint, Q to dash, E to collect eggs.
- Eggs with Common, Rare, Golden and Diamond rarities and different coin values.
- Eggs respawn after a short delay with pickup/delivery particle effects.
- Bring eggs to the blue base for coins and unhatched-egg inventory.
- Press the **Hatch Egg** HUD button to create original pets with randomized income.
- Pets appear in a showcase near base and generate passive coins per second.
- Click shop buttons for movement speed, pickup range, bag capacity and dash cooldown upgrades.
- Two repeatable-progress missions with one-time coin rewards.
- Explore a much larger 160 × 110 world with separate grassland, woodland, lakeside and ancient-ruins zones, perimeter walls, tree lines, rocks and flower patches.
- Expanded blue/orange bases, more egg spawn points, larger speed-training treadmill, and three wide progression gates.
- Higher available Unity quality settings: 4× anti-aliasing, high shadow resolution/distance, anisotropic filtering, HDR camera, richer directional lighting and smoother metallic materials.
- Stand on the speed-training treadmill to raise Speed and unlock gates to later regions.
- Four original nest guardians chase after nearby eggs are stolen; getting caught can cost one carried egg.
- Progress (coins, upgrades, trained speed, quests and hatched pet records) saves locally using Unity PlayerPrefs.

## Open and generate the demo scene
1. In GitHub, choose **Code → Download ZIP**, extract the repository.
2. Use Unity Hub → **Projects → Add/Open** and select the extracted repository root (it contains `Assets`, `Packages`, and `ProjectSettings`).
3. Open with Unity 6.6 and wait for compilation.
4. On the Unity top menu choose **Steal Eggs → Build Demo Arena**.
5. Press Play.

If you keep using an older local project, copy the repository's complete `Assets/StealEggs` folder into your project's `Assets` folder and replace files with the same names; then let Unity compile and run the menu command.

## Add the Free Low Poly Nature Forest lake
1. Import the **Free Low Poly Nature Forest** asset into the same Unity project. The asset itself is not included in this repository.
2. Pull the latest `main` branch so `Assets/StealEggs/Editor/StealEggsLakeInstaller.cs` is added to your project.
3. Wait for Unity to finish compiling, then open **Tools → Steal Eggs → Lake Asset Installer**.
4. Click **Tự tìm asset hồ, cây, đá và scene demo**. If the automatic search picks the wrong item, drag the correct lake tile/model, tree prefab, rock prefab, or demo scene from the Project window into the matching fields.
5. **Save a backup of your scene first.** To try a single lake tile, use **TẠO KHU HỒ**. To copy the demo environment, first use a duplicate scene and set an X offset so it will not cover the existing map.
6. Inspect the result in Scene view and save with Ctrl+S. If the wrong object appears or the layout is misplaced, use Undo (Ctrl+Z).

The installer is a Unity Editor utility; it does not automatically modify a scene when the repository is pulled. Automatic asset matching is best-effort, so check the selected asset before building the lake area. The package and its meshes/materials remain local to your Unity project and must be imported separately.

## Controls
- **W/A/S/D** or arrow keys: move relative to camera.
- **Hold right mouse + drag:** rotate camera.
- **Mouse wheel:** zoom.
- **Space:** jump; **Shift:** sprint; **Q:** dash; **E:** collect.
- Stand on the blue treadmill to train speed.
- Return eggs to the blue base, then click **Hatch Egg** to hatch pets.
- Click the four shop buttons to upgrade.

## Known limitations
This is an early offline prototype assembled through source updates; it has **not been run and verified in the user's local Unity Editor yet**. The rival/guardians are simple AI, the pets are procedural placeholder visuals, and the region barriers are a prototype progression mechanic. Online multiplayer, server synchronization, mobile controls, full original high-quality art, extensive pet catalog, balanced economy, anti-cheat, and a verified CrazyGames Web build still need implementation and testing. Local PlayerPrefs saves on that device/browser build only; it is not a cloud account save.
