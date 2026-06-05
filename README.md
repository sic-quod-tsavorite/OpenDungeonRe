# OpenDungeonRe

> **A Dungeon Crawler Game** - Remake of VRDungeon for modern Unity

**OpenDungeonRe** is a complete rebuild of the original VRDungeon school project, updated for Unity 6.4 and converted from VR to a traditional first-person perspective. The game retains all the original dungeon crawling mechanics, asset packs, and level design, but with new controls suitable for keyboard/mouse, gamepad, and touch input.

---

## Project Overview

| **Aspect**           | **Details**                            |
| -------------------- | -------------------------------------- |
| **Original Project** | VRDungeon (VR-based, Unity 2019.4.9f1) |
| **Engine**           | Unity 6.4.9f1                          |
| **Perspective**      | First-Person (Non-VR)                  |
| **Render Pipeline**  | Universal Render Pipeline (URP)        |
| **Input System**     | Unity Input System Package             |
| **Target Platforms** | Linux, WebGL, Android                  |

---

## Project Goals

- **Modernize**: Update from Unity 2019.4.9f1 to Unity 6.4
- **Convert**: Remove all VR/XR dependencies and convert to first-person
- **Preserve**: Keep all original asset packs and content
- **Cross-Platform**: Build for Linux, Web, and Android
- **Gameplay**: Maintain original dungeon crawling experience with adapted controls

---

## Getting Started

### Prerequisites

- **Unity Hub** installed
- **Unity Editor 6.4.9f1** (or compatible 6.4.x version)
- **Required Packages** (auto-installed via manifest.json):
  - `com.unity.inputsystem` - New Input System
  - `com.unity.render-pipelines.universal` - URP

### Installation

1. **Clone or Open Project**

   ```bash
   # Clone the repository (if using Git)
   git clone <repository-url>
   cd OpenDungeonRe
   ```

   Or open directly in Unity Hub.

2. **Open in Unity**
   - Launch Unity Hub
   - Add project folder
   - Unity will automatically resolve and install required packages

3. **Initial Setup** (First Time Only)
   - Import required asset packs from the Unity Asset Store (see [Asset Packs](#asset-packs) section)

---

## Game Controls

| **Action**     | **Keyboard & Mouse** | **Gamepad** | **Touch**        |
| -------------- | -------------------- | ----------- | ---------------- |
| Move           | WASD                 | Left Stick  | Virtual Joystick |
| Look           | Mouse                | Right Stick | Swipe            |
| Jump           | Space                | A           | Jump Button      |
| Sprint         | Left Shift           | LT          | Sprint Button    |
| Crouch         | C                    | B           | Crouch Button    |
| Attack         | Left Mouse Button    | RT          | Attack Button    |
| Interact       | E                    | X           | Interact Button  |
| Inventory Prev | Scroll Wheel         | LB          | Prev Button      |
| Inventory Next | Scroll Wheel         | RB          | Next Button      |

---

## Player Setup

### Creating the Player

For a fresh scene or if setting up manually:

1. **Create Player Hierarchy**

   ```
   Player (GameObject)
   └── CameraPivot (GameObject)
       └── Main Camera (Camera)
   ```

2. **Add Components to Player**
   - `CharacterController` - Collision and movement
   - `FirstPersonController` - Main FPS controller script
   - `PlayerInput` - Input System component

3. **Configure CharacterController**
   | Property | Value |
   |----------|-------|
   | Radius | 0.5 |
   | Height | 1.8 |
   | Center | (0, 0, 0) |
   | Skin Width | 0.1 |
   | Step Offset | 0.3 |
   | Slope Limit | 45° |

4. **Configure Camera**
   - CameraPivot Position: (0, 0.3, 0) relative to Player
   - Field of View: 60-90° (recommended: 75°)
   - Tag: **MainCamera**

5. **Assign Input Actions**
   - In `PlayerInput` component:
     - **Actions**: `OpenDungeonRe_InputActions`
     - **Behavior**: Invoke Unity Events or Send Messages
     - **Default Action Map**: Player
     - **Default Control Scheme**: Keyboard&Mouse

6. **Configure FirstPersonController Script**
   - **Player Camera**: Assign Main Camera
   - **Camera Pivot**: Assign CameraPivot
   - **Walk Speed**: 5.0
   - **Run Speed**: 8.0
   - **Jump Height**: 1.0
   - **Ground Check Distance**: 0.1
   - **Mouse Sensitivity**: 50 (adjust to preference)

### Ground Detection

The FirstPersonController uses **CharacterController.isGrounded** combined with a **downward raycast** for reliable ground detection. This eliminates the need for a dedicated ground layer or layer mask configuration. The ground check distance can be adjusted in the script settings (default: 0.1).

---

## Scene Setup

### Using OpenDungeonRe.unity

The main scene has been updated from the original VRDungeon. VR-specific GameObjects have been removed and the scene is now configured for first-person non-VR gameplay. You can use this scene directly, or start fresh with StarterScene.

### Using StarterScene

The `StarterScene.unity` is a clean, empty scene ready for you to build upon:

1. Open `Assets/Scenes/StarterScene.unity`
2. Follow Player Setup instructions above
3. Add your dungeon prefabs and assets

---

## Asset Packs

The project requires the following asset packs. Download them from the Unity Asset Store and import into your project:

- [BrokenVector - Ultimate Dungeon Pack](https://assetstore.unity.com/packages/2d/textures-materials/ultimate-dungeon-pack-161947)
- [Low Poly Dungeons](https://assetstore.unity.com/packages/3d/environments/low-poly-dungeons-lite-167475)
- [RPG Monster Wave 02 PBR](https://assetstore.unity.com/packages/3d/characters/creatures/rpg-monster-wave-02-pbr-147798)

---

## Technical Details

### Render Pipeline

- **Universal Render Pipeline (URP)**
- Configured in `ProjectSettings/RendererDataAsset`
- Ensure all materials use URP-compatible shaders

### Input System

- Uses **Unity's New Input System**
- Configuration: `OpenDungeonRe_InputActions.inputactions`
- Supports multiple control schemes
- No VR/XR bindings (removed from original)

### Character Controller

- Uses **UnityEngine.CharacterController**
- Fully first-person optimized

---

## Troubleshooting

### Materials Appearing Purple

**Cause**: Shader references broken after Unity version upgrade or URP migration.

**Solutions**:

1. **Reimport All**: `Assets > Reimport All`
2. **Check Shader Assignments**: Select purple materials and verify shader is set to a URP shader (e.g., `Universal Render Pipeline/Lit`)
3. **URP Conversion**: If assets were made for Built-in RP, use Unity's URP Upgrader (Window > Rendering > Render Pipeline Converter)

### CharacterController Errors (CS1061)

**Error**: `'CharacterController' does not contain a definition for 'height'/'radius'/'center'/'Move'`

**Cause**: Missing or incorrect using directive, or wrong CharacterController type.

**Solution**:

- Ensure you're using `UnityEngine.CharacterController`
- Add at the top of your script:
  ```csharp
  using UnityEngine;
  ```

### Input Not Working

1. Verify `PlayerInput` component has **Actions** assigned
2. Check **Behavior** is set to "Invoke Unity Events" or "Send Messages"
3. Ensure **Default Action Map** is set to "Player"
4. Test with **Input Debugger** (Window > Analysis > Input Debugger)

### InvalidOperationException: Input System Conflict

**Error**: You are trying to read Input using the UnityEngine.Input class, but you have switched active Input handling to Input System package in Player Settings.

**Cause**: Asset pack demo scenes contain scripts using the old Legacy Input System (UnityEngine.Input) which conflicts with the project's Input System setting.

**Solution**: Remove the demo folder containing legacy scripts:

- Delete `Assets/BrokenVector/UltimateDungeonPack/Scenes/Demo/`

Do not use scripts from asset pack Demo folders. Use the custom player scripts in `Assets/Scripts/Player/` instead.

---

## Build Configuration

### Supported Platforms

| Platform    | Status   | Notes                          |
| ----------- | -------- | ------------------------------ |
| **Linux**   | Default  | Primary development platform   |
| **WebGL**   | Target   | Enable WebGL build support     |
| **Android** | Target   | Requires Android build support |
| **Windows** | Untested | Not a development priority     |

### Build Steps

1. **Configure Build Settings**
   - File > Build Settings
   - Add scenes: `OpenDungeonRe` and/or `StarterScene`
   - Set first scene in build order

2. **Platform-Specific Setup**
   - **WebGL**: Enable compression, set resolution scaling
   - **Android**: Configure player settings, add required permissions
   - **Linux**: Enable x86_64 architecture

3. **Build**
   - Select target platform
   - Click "Build" or "Build And Run"

---

## Development Notes

### Changes from VRDungeon

| **Original (VRDungeon)** | **Remake (OpenDungeonRe)**   |
| ------------------------ | ---------------------------- |
| VR First-Person          | Non-VR First-Person          |
| Unity 2019.4.9f1         | Unity 6.4.9f1                |
| Built-in Render Pipeline | Universal Render Pipeline    |
| VRTK / SteamVR           | Removed                      |
| VR Input Bindings        | Keyboard/Mouse/Gamepad/Touch |
| VR Character Controller  | Standard CharacterController |

---

## Getting Help

1. **Check Unity Console** for specific error messages
2. **Verify all file references** exist in Project window
3. **Ensure Input System package** is installed (com.unity.inputsystem)

---

## Credits

- **Original Project**: VRDungeon (School Project)
  - School Project Collaborators: Snufty, AdnanSB
- **Asset Packs**:
  - [BrokenVector - Ultimate Dungeon Pack](https://assetstore.unity.com/packages/2d/textures-materials/ultimate-dungeon-pack-161947)
  - [Low Poly Dungeons](https://assetstore.unity.com/packages/3d/environments/low-poly-dungeons-lite-167475)
  - [RPG Monster Wave 02 PBR](https://assetstore.unity.com/packages/3d/characters/creatures/rpg-monster-wave-02-pbr-147798)
- **Engine**: Unity Technologies
