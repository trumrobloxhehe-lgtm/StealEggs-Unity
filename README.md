# StealEggs3D — Feature-Rich Unity Prototype

This is an **offline prototype** with a Roblox-inspired block avatar and third-person controls. It is not a finished multiplayer game or an exact Roblox clone.

## Included
- Third-person orbit camera: hold right mouse and drag to look around, scroll to zoom
- Block-style character with procedural idle, walk, run and jump animation
- WASD / arrows to move, Space to jump, Shift to sprint, Q to dash, E to collect eggs
- Common, rare, golden and diamond eggs with different values
- Eggs respawn after a short delay; pickup and delivery particle effects
- Bring eggs to the blue base to score and earn coins
- Rival computer-controlled block character competes for eggs
- Shop buttons upgrade speed, pickup range, bag size and dash cooldown
- Two simple missions with one-time coin rewards
- Local progress saved with Unity PlayerPrefs
- Generated arena with trees, fences, HUD, shop and mission panel

## Update an existing Unity project
1. Exit Play mode if it is running.
2. Extract the ZIP patch and open its `Assets` folder.
3. Copy the `StealEggs` folder into your project's `Assets` folder, e.g. `C:\Users\HUONG\StealEggs3D\Assets`.
4. If asked about duplicates, choose **Replace the files in the destination**.
5. Return to Unity and wait for compilation.
6. Select **Steal Eggs → Build Demo Arena** in the top menu, then press Play.

## Controls
- WASD / arrows: move relative to the camera
- Hold right mouse button + drag: rotate camera
- Mouse wheel: zoom
- Space: jump; Shift: sprint; Q: dash; E: collect an egg
- Return to the blue base to deposit eggs and earn coins
- Click the four shop buttons to purchase upgrades

## Still not implemented
Online multiplayer, network synchronization, mobile touch controls, final art/animation assets, anti-cheat, and a verified CrazyGames web build remain future work. The source has not been run in the user's local Unity Editor yet; a first local test is still needed.
