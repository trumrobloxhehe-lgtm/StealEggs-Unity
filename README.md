# StealEggs3D — Unity Starter Prototype

This is a **first playable prototype**, not a finished multiplayer game. It builds a small 3D arena so you can test moving, collecting eggs, taking eggs from the rival side, and returning to your own base.

## Included
- WASD / arrow-key movement
- Press **E** near an egg to pick it up
- Return to the blue base on the left to bank carried eggs
- Rival bot on the orange base that competes for eggs
- UI showing carried eggs and banked scores
- Unity Editor menu command that creates the demo scene and materials

## Import into an existing Unity project
1. Open your Unity project (`StealEggs3D`).
2. In File Explorer, open the downloaded ZIP and copy the `Assets/StealEggs` folder into your project's `Assets` folder. The final path should be `StealEggs3D/Assets/StealEggs/`.
3. Return to Unity and wait for scripts to compile.
4. In Unity's top menu, choose **Steal Eggs → Build Demo Arena**.
5. The tool creates and saves `Assets/StealEggs/Scenes/StealEggsDemo.unity` and opens it.
6. Press the **Play** triangle at the top of Unity to test.

If Unity reports an input API error, go to **Edit → Project Settings → Player → Other Settings → Active Input Handling** and choose **Both** (or **Input System Package (New)**), then allow Unity to restart if it asks.

## Controls
- **W/A/S/D** or arrow keys: move
- **E**: pick up the nearest egg within reach
- Walk back to the blue base to bank the eggs automatically

## GitHub
This source code is stored in the `trumrobloxhehe-lgtm/StealEggs-Unity` repository. Unity does **not** need to be linked to GitHub for the prototype to run. GitHub is for storing/versioning the source; Unity is where you open and run the game.

## Important limits of this first prototype
- The rival is a local computer-controlled bot, not a network player.
- Multiplayer, shop/upgrades, final character/egg art, saved progression, mobile controls, and the CrazyGames web build still need to be implemented and tested.
