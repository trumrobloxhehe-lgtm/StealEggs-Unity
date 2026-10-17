# StealEggs3D — Unity Prototype Starter

A starter prototype for a 3D egg-stealing game with a blocky, Roblox-inspired avatar and third-person controls. It is not an exact Roblox clone and is not a finished online game.

## Included
- Third-person follow/orbit camera
- Hold right mouse button and drag to rotate the camera; scroll wheel to zoom
- WASD / arrow-key camera-relative movement, Space to jump, Shift to sprint
- Procedural walk/run arm and leg animation, idle breathing/bob and raised arms while jumping
- Press E near an egg to pick it up; carry eggs back to the blue base to score
- Rival AI bot with matching blocky character and basic locomotion animation
- Unity Editor menu command to automatically generate the demo arena, characters, eggs, bases, HUD, camera and materials

## Use in an existing Unity project
1. Download this repository as a ZIP and extract it, or use GitHub Desktop to clone it.
2. Copy `Assets/StealEggs` into your Unity project's `Assets` folder, merging folders and replacing files with the same names.
3. Return to Unity and wait for scripts to compile.
4. Choose **Steal Eggs → Build Demo Arena** from the top menu.
5. Press the Play triangle. Click inside the Game view if keyboard input is not active.

## Controls
- **W/A/S/D** or arrow keys: move relative to the camera
- **Hold right mouse button + drag:** orbit camera
- **Mouse wheel:** zoom
- **Space:** jump
- **Shift:** run
- **E:** collect a nearby egg
- Return to the blue base to bank collected eggs

## Current limitations
This is an offline prototype, not a finished game. The rival is computer-controlled, and its animation is also procedural. Online multiplayer, shop/upgrades, saved progression, mobile touch controls, final art and a verified CrazyGames Web build still need work and testing.
