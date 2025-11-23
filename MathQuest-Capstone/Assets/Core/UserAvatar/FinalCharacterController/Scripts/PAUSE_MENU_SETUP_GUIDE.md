# Pause Menu Setup Guide

This guide explains how to set up the pause menu system in your game.

## Overview

The pause menu system consists of three main scripts:
1. **PauseMenuManager.cs** - Manages pause menu UI, Escape key input, and menu options
2. **SettingsPanel.cs** - In-game settings panel for sensitivity and SFX volume
3. **PlayerSpawnTracker.cs** - Tracks initial player spawn position for the unstuck feature

## Setup Steps

### Step 1: Add Scripts to Scene

1. In your game scene (e.g., `BlackSmithEnv.unity` or `GamePlay.unity`):
   - Create an empty GameObject named `PauseMenuManager`
   - Add the `PauseMenuManager` component to it
   - Create another empty GameObject named `SettingsPanel`
   - Add the `SettingsPanel` component to it
   - Create another empty GameObject named `PlayerSpawnTracker`
   - Add the `PlayerSpawnTracker` component to it

### Step 2: Create Pause Menu UI

1. **Create Canvas** (if not already exists):
   - Right-click in Hierarchy → UI → Canvas
   - Name it `PauseMenuCanvas`
   - Set Canvas component:
     - Render Mode: Screen Space - Overlay
     - Sort Order: 100 (higher than other UI to appear on top)

2. **Create Background Panel**:
   - Right-click `PauseMenuCanvas` → UI → Panel
   - Name it `PauseMenuPanel`
   - Set Image component:
     - Color: Black with Alpha ~200 (semi-transparent)
   - Set RectTransform:
     - Anchor: Stretch (all sides)
     - Left/Right/Top/Bottom: 0

3. **Create Menu Container**:
   - Right-click `PauseMenuPanel` → UI → Panel (or Empty GameObject)
   - Name it `MenuContainer`
   - Add Vertical Layout Group component:
     - Spacing: 20
     - Child Alignment: Middle Center
     - Child Control Width: true
     - Child Control Height: true
   - Add Content Size Fitter:
     - Horizontal Fit: Preferred Size
     - Vertical Fit: Preferred Size
   - Set RectTransform:
     - Anchor: Middle Center
     - Width: 400
     - Height: 300

4. **Create Title Text** (optional):
   - Right-click `MenuContainer` → UI → Text - TextMeshPro
   - Name it `TitleText`
   - Set text to "PAUSED"
   - Set font size to 48
   - Center align

5. **Create Main Menu Button**:
   - Right-click `MenuContainer` → UI → Button - TextMeshPro
   - Name it `MainMenuButton`
   - Set button text to "Return to Main Menu"
   - Set font size to 24
   - Set RectTransform height to 60

6. **Create Settings Button**:
   - Right-click `MenuContainer` → UI → Button - TextMeshPro
   - Name it `SettingsButton`
   - Set button text to "Settings"
   - Set font size to 24
   - Set RectTransform height to 60

7. **Create Unstuck Button**:
   - Right-click `MenuContainer` → UI → Button - TextMeshPro
   - Name it `UnstuckButton`
   - Set button text to "Unstuck"
   - Set font size to 24
   - Set RectTransform height to 60

8. **Create Resume Button**:
   - Right-click `MenuContainer` → UI → Button - TextMeshPro
   - Name it `ResumeButton`
   - Set button text to "Resume"
   - Set font size to 24
   - Set RectTransform height to 60

### Step 3: Configure PauseMenuManager

1. Select the `PauseMenuManager` GameObject
2. In the Inspector, assign references:
   - **Pause Menu Panel**: Drag `PauseMenuPanel` from Hierarchy
   - **Main Menu Button**: Drag `MainMenuButton` from Hierarchy
   - **Settings Button**: Drag `SettingsButton` from Hierarchy
   - **Unstuck Button**: Drag `UnstuckButton` from Hierarchy
   - **Resume Button**: Drag `ResumeButton` from Hierarchy
3. Configure settings:
   - **Pause Time On Open**: true (recommended)
   - **Lock Cursor On Resume**: true (recommended)

### Step 4: Create Settings Panel UI

1. **Create Settings Panel** (under PauseMenuCanvas):
   - Right-click `PauseMenuCanvas` → UI → Panel
   - Name it `SettingsPanel`
   - Set Image component:
     - Color: Black with Alpha ~220 (slightly more opaque than pause menu)
   - Set RectTransform:
     - Anchor: Stretch (all sides)
     - Left/Right/Top/Bottom: 0

2. **Create Settings Container**:
   - Right-click `SettingsPanel` → UI → Panel (or Empty GameObject)
   - Name it `SettingsContainer`
   - Add Vertical Layout Group component:
     - Spacing: 30
     - Child Alignment: Middle Center
     - Child Control Width: true
     - Child Control Height: false
   - Add Content Size Fitter:
     - Horizontal Fit: Preferred Size
     - Vertical Fit: Preferred Size
   - Set RectTransform:
     - Anchor: Middle Center
     - Width: 500
     - Height: 400

3. **Create Title Text**:
   - Right-click `SettingsContainer` → UI → Text - TextMeshPro
   - Name it `TitleText`
   - Set text to "SETTINGS"
   - Set font size to 42
   - Center align

4. **Create Sensitivity Horizontal Setting**:
   - Right-click `SettingsContainer` → UI → Panel
   - Name it `SensitivityHSection`
   - Set RectTransform height to 80
   - Add Horizontal Layout Group:
     - Spacing: 20
     - Child Control Width: false
     - Child Control Height: false
   - Add child: Text - TextMeshPro (Label)
     - Text: "Horizontal Sensitivity:"
     - Font size: 24
   - Add child: Slider
     - Name: `SensitivityHSlider`
     - Set RectTransform width to 200
   - Add child: Text - TextMeshPro (Value)
     - Name: `SensitivityHValueText`
     - Text: "1.0"
     - Font size: 20
     - Width: 50

5. **Create Sensitivity Vertical Setting**:
   - Right-click `SettingsContainer` → UI → Panel
   - Name it `SensitivityVSection`
   - Set RectTransform height to 80
   - Add Horizontal Layout Group (same as above)
   - Add same children structure as SensitivityH
     - Label: "Vertical Sensitivity:"
     - Slider: `SensitivityVSlider`
     - Value Text: `SensitivityVValueText`

6. **Create SFX Volume Setting**:
   - Right-click `SettingsContainer` → UI → Panel
   - Name it `SFXVolumeSection`
   - Set RectTransform height to 80
   - Add Horizontal Layout Group (same as above)
   - Add same children structure:
     - Label: "SFX Volume:"
     - Slider: `SFXVolumeSlider`
     - Value Text: `SFXVolumeValueText`

7. **Create Close Button**:
   - Right-click `SettingsContainer` → UI → Button - TextMeshPro
   - Name it `CloseButton`
   - Set button text to "Close"
   - Set font size to 24
   - Set RectTransform height to 60

### Step 5: Configure SettingsPanel

1. Select the `SettingsPanel` GameObject
2. In the Inspector, assign references:
   - **Settings Panel**: Drag `SettingsPanel` (the panel GameObject) from Hierarchy
   - **Sensitivity Horizontal Slider**: Drag `SensitivityHSlider` from Hierarchy
   - **Sensitivity Vertical Slider**: Drag `SensitivityVSlider` from Hierarchy
   - **SFX Volume Slider**: Drag `SFXVolumeSlider` from Hierarchy
   - **Sensitivity H Value Text**: Drag `SensitivityHValueText` from Hierarchy
   - **Sensitivity V Value Text**: Drag `SensitivityVValueText` from Hierarchy
   - **SFX Volume Value Text**: Drag `SFXVolumeValueText` from Hierarchy
   - **Close Button**: Drag `CloseButton` from Hierarchy
3. Configure default values (optional):
   - **Default Sensitivity H**: 0.1 (matches PlayerController default)
   - **Default Sensitivity V**: 0.1 (matches PlayerController default)
   - **Default SFX Volume**: 1.0 (100%)
   - **Min/Max Sensitivity**: 0.01 to 1.0 (can be adjusted)
   - **Sensitivity Multiplier**: 10 (for display - 0.1 sensitivity = 1.0 on slider)

### Step 6: Configure PauseMenuManager

1. Select the `PauseMenuManager` GameObject
2. In the Inspector, assign references:
   - **Pause Menu Panel**: Drag `PauseMenuPanel` from Hierarchy
   - **Main Menu Button**: Drag `MainMenuButton` from Hierarchy
   - **Settings Button**: Drag `SettingsButton` from Hierarchy
   - **Unstuck Button**: Drag `UnstuckButton` from Hierarchy
   - **Resume Button**: Drag `ResumeButton` from Hierarchy
   - **Settings Panel**: Drag the `SettingsPanel` GameObject (with SettingsPanel component) from Hierarchy
3. Configure settings:
   - **Pause Time On Open**: true (recommended)
   - **Lock Cursor On Resume**: true (recommended)

### Step 7: Configure PlayerSpawnTracker

1. Select the `PlayerSpawnTracker` GameObject
2. In the Inspector:
   - **Player Transform**: Drag your player GameObject from Hierarchy (or leave null to auto-detect)
   - **Use Current Position As Spawn**: true (automatically uses player's starting position)
   - **Custom Spawn Position**: Only used if Use Current Position is false
   - **Reset Rotation On Spawn**: true (recommended)
   - **Spawn Rotation**: Vector3.zero (or set custom rotation)

### Step 5: Verify Player Tag/Component

Make sure your player GameObject has one of:
- Tag: "Player"
- OR: `PlayerController` component attached

The PlayerSpawnTracker will automatically find the player using these.

## Usage

- **Pause Game**: Press `Escape` key
- **Resume Game**: Press `Escape` again OR click "Resume" button
- **Return to Main Menu**: Click "Return to Main Menu" button (loads scene 0)
- **Open Settings**: Click "Settings" button (opens in-game settings panel)
- **Close Settings**: Click "Close" button in settings panel OR press `Escape`
- **Adjust Sensitivity**: Use sliders in settings panel (saved automatically)
- **Adjust SFX Volume**: Use slider in settings panel (saved automatically)
- **Unstuck**: Click "Unstuck" button (resets player to spawn position and resumes game)

## Features

### Pause Behavior
- When paused, game time is set to 0 (everything freezes)
- Cursor is unlocked and visible for menu interaction
- Player input is disabled while paused
- Cannot pause while puzzle is active

### Settings Panel
- In-game settings panel (no scene loading needed)
- Adjust mouse sensitivity (horizontal and vertical separately)
- Adjust SFX volume with slider
- Settings saved automatically using PlayerPrefs
- Settings persist across game sessions
- Applied immediately when changed

### Unstuck Feature
- Resets player position to initial spawn location
- Resets player rotation to spawn rotation
- Automatically resumes game after unstuck
- Works with CharacterController to properly reset position

### Scene Management
- Properly cleans up singletons (e.g., QuestManager) when returning to main menu
- Restores time scale before scene changes

## Customization

### Changing Scene Indices

If your scene indices differ, update in `PauseMenuManager.cs`:
- Main Menu: `SceneManager.LoadSceneAsync(0);` (line ~139)
- Settings: `SceneManager.LoadSceneAsync(4);` (line ~148)

### Styling the Menu

You can style the pause menu buttons and background:
- Use your SteampunkUI prefabs for buttons
- Adjust colors, fonts, and sizes to match your game's aesthetic
- Add animations using DOTween (already in project)

### Disabling Time Pause

If you want menus/puzzles to continue animating while paused:
- Set `Pause Time On Open` to false in PauseMenuManager inspector
- Note: This means some animations will continue during pause

## Troubleshooting

### Menu doesn't appear on Escape
- Check that PauseMenuManager is in the scene
- Verify PauseMenuPanel is assigned
- Check Console for errors

### Settings panel doesn't open
- Verify SettingsPanel GameObject exists with SettingsPanel component
- Check that SettingsPanel reference is assigned in PauseMenuManager
- Verify settings panel GameObject is assigned in SettingsPanel component

### Settings don't save
- Check Console for PlayerPrefs errors
- Verify AudioManager exists in scene
- Verify PlayerController exists in scene
- Settings are saved immediately when slider values change

### Sensitivity doesn't apply
- Verify PlayerController exists in scene
- Check that lookSenseH and lookSenseV are public fields
- Settings are applied immediately when slider changes

### SFX Volume doesn't change
- Verify AudioManager exists in scene
- Check that AudioManager has audioMixer assigned
- Verify SFXVolume parameter exists in AudioMixer
- Settings are applied immediately when slider changes

### Unstuck doesn't work
- Verify PlayerSpawnTracker is in the scene
- Check that player has "Player" tag or PlayerController component
- Look in Console for error messages

### Can't click buttons in menu
- Make sure EventSystem exists in scene
- Check Canvas Sort Order (should be high, e.g., 100)
- Verify buttons are not blocked by other UI elements

### Game doesn't resume properly
- Check that Time.timeScale is being reset to 1
- Verify cursor lock state is restored
- Ensure player input components are re-enabled

## Testing

1. Enter play mode
2. Press Escape - menu should appear
3. Press Escape again - menu should disappear
4. Click each button to verify functionality
5. Test unstuck: move player away, pause, click unstuck, verify position reset

## Notes

- The pause menu prevents pausing when puzzles are active
- Settings panel is shown instead of pause menu when Settings button is clicked
- Escape key closes settings panel first, then pause menu
- Settings are saved to PlayerPrefs and persist across game sessions
- Sensitivity values are multiplied by 10 for display (0.1 = 1.0 on slider)
- Settings are applied immediately - no need to click "Apply" button
- PlayerSpawnTracker automatically detects spawn position on scene load
- Scripts use singleton pattern for easy access from other scripts
- Scene indices are hardcoded; adjust if your build settings differ

