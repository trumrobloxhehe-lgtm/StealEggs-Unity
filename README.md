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

The lake asset package must be imported locally in Unity; its licensed model files are not included in this repository.

### Replace the water in the current scene
1. Pull the latest `main` branch and wait for Unity to compile the new editor scripts.
2. Open **Tools → Steal Eggs → Lake Asset Installer**.
3. Click **Tự tìm asset hồ, cây, đá và scene demo**. Confirm that the **Mô hình/Prefab hồ** field points to a Lake terrain tile from the imported Pure Poly pack. If it does not, drag the correct Lake tile from the Project window into that field.
4. Click **THAY HỒ CŨ BẰNG ASSET HỒ MỚI**, inspect the result in Scene view, and press Ctrl+S. The tool removes the known old water surface object only; it leaves the rest of the current scene in place. Use Ctrl+Z if the selected model is wrong.

### Rebuild the demo arena
1. With the asset package imported, choose **Steal Eggs → Build Demo Arena**.
2. The generator searches the imported assets for a Lake terrain tile and uses it instead of creating the old water surface when it detects one.
3. If the Console reports that the Lake tile was not detected, the generator uses a static fallback surface; open Lake Asset Installer and manually assign the right tile to the field.
4. Save a backup first: rebuilding the demo arena regenerates the demo scene and can replace edits made to that scene.

The tool fits the selected Lake model to the existing lakeside zone as a starting point. Check the model's position and scale in Scene view after placement. The terrain, meshes, and materials remain local to your Unity project; do not upload asset-store files to this repository unless the asset license explicitly allows redistribution.
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
