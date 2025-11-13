# Puzzle-Locked Chest System Guide

## Overview
This system allows you to lock chests (or any interactable) behind puzzles that the player must solve before unlocking them.

---

## 📦 What's Included

### 1. **IPuzzle.cs** - Interface
- Base interface that all puzzles must implement
- Allows you to create different puzzle types (math, riddles, pattern matching, etc.)

### 2. **SimpleMathPuzzle.cs** - Example Puzzle
- A working math puzzle with random questions
- Supports addition, subtraction, and multiplication
- Fully functional UI integration

### 3. **PuzzleLockedChest.cs** - Locked Chest
- Replaces your `ChestAnimationInteractable`
- Requires puzzle completion before opening
- Can be toggled on/off

### 4. **PuzzleUISetup.cs** - UI Generator
- Automatically creates puzzle UI in your scene
- No manual UI setup needed!

---

## 🚀 Quick Setup (5 Steps)

### Step 1: Create the Puzzle UI
1. In Unity, create an empty GameObject in your scene
2. Add the `PuzzleUISetup` component to it
3. Right-click on the component → **"Create Math Puzzle UI"**
4. A complete puzzle UI will be created automatically!
5. Delete the PuzzleUISetup GameObject (you don't need it anymore)

### Step 2: Replace Your Chest Script
1. Select your chest GameObject
2. **Remove** the old `ChestAnimationInteractable` component
3. **Add** the new `PuzzleLockedChest` component
4. Assign your animation clips (same as before)

### Step 3: Connect the Puzzle
1. Find the `MathPuzzlePanel` GameObject in your Canvas
2. It should have a `SimpleMathPuzzle` component
3. In your chest's `PuzzleLockedChest` component:
   - Check ☑ **Requires Puzzle**
   - Drag the `MathPuzzlePanel` into the **Puzzle Component** field

### Step 4: Test It!
1. Play your game
2. Interact with the chest (press E)
3. The puzzle UI should appear
4. Solve the math problem
5. The chest opens automatically!

---

## ⚙️ Configuration Options

### PuzzleLockedChest Settings

| Setting | Description |
|---------|-------------|
| **Requires Puzzle** | Enable/disable puzzle requirement |
| **Puzzle Component** | Reference to your puzzle (SimpleMathPuzzle) |
| **Locked Message** | Message shown when chest is locked |
| **Toggle** | Allow closing the chest after opening |
| **Only Once** | Chest can only be opened once |

### SimpleMathPuzzle Settings

| Setting | Description |
|---------|-------------|
| **Min Number** | Minimum number in equations (default: 1) |
| **Max Number** | Maximum number in equations (default: 20) |
| **Allow Subtraction** | Include subtraction problems |
| **Allow Multiplication** | Include multiplication problems |

---

## 🎨 Customizing the Puzzle

### Change Difficulty
```csharp
// In SimpleMathPuzzle component:
Min Number: 1
Max Number: 10        // Easy
Max Number: 50        // Medium
Max Number: 100       // Hard

Allow Multiplication: ☑  // Makes it harder
```

### Change Appearance
Edit the UI elements in the `MathPuzzlePanel`:
- Change colors of buttons and backgrounds
- Resize the window
- Change fonts and text sizes
- Add decorative images

---

## 🔧 Advanced Usage

### Create Your Own Puzzle Type

1. Create a new script that implements `IPuzzle`:

```csharp
using UnityEngine;
using System;

public class MyCustomPuzzle : MonoBehaviour, IPuzzle
{
    public bool IsActive { get; private set; }
    public bool IsCompleted { get; private set; }
    
    public void ShowPuzzle(Action onComplete, Action onCancel)
    {
        // Show your puzzle UI
        IsActive = true;
        // When solved, call: onComplete?.Invoke();
    }
    
    public void HidePuzzle()
    {
        IsActive = false;
    }
    
    public void ResetPuzzle()
    {
        IsCompleted = false;
    }
}
```

2. Assign your custom puzzle to the chest instead of SimpleMathPuzzle

### Manually Unlock/Lock Chests

```csharp
// Get reference to chest
PuzzleLockedChest chest = GetComponent<PuzzleLockedChest>();

// Unlock without puzzle
chest.UnlockChest();

// Lock again
chest.LockChest();

// Check status
bool isLocked = chest.IsLocked;
bool isOpen = chest.IsOpened;
```

---

## 🐛 Troubleshooting

### Puzzle UI doesn't appear
- Check that `MathPuzzlePanel` exists in your Canvas
- Verify the puzzle component is assigned in PuzzleLockedChest
- Make sure "Requires Puzzle" is checked

### Puzzle appears but input doesn't work
- Check that an EventSystem exists in your scene
- Verify the cursor is unlocked (should happen automatically)
- Check Console for errors

### Chest opens without puzzle
- Make sure "Requires Puzzle" is checked
- Verify puzzle component is assigned and not null
- Check if puzzle was already completed (reset it)

### Text doesn't show in puzzle
- Assign a TextMeshPro font asset to the text components
- Check that TextMeshPro is imported in your project

---

## 💡 Ideas for Other Puzzle Types

You can create different puzzles by implementing `IPuzzle`:

1. **Pattern Matching** - Remember and repeat a sequence
2. **Riddles** - Answer text-based riddles
3. **Simon Says** - Click buttons in the right order
4. **Slider Puzzle** - Arrange tiles correctly
5. **Color Matching** - Match colors to a pattern
6. **Timed Challenges** - Complete task before time runs out

---

## 📝 Notes

- The puzzle system is **modular** - you can use it for doors, gates, or any IInteractable
- Multiple chests can share the same puzzle UI
- Puzzles can be reset and reused
- The system works with your existing Interactor setup
- Cursor automatically locks/unlocks when puzzle shows/hides

---

## 🎮 Player Experience Flow

1. Player approaches locked chest
2. Press E to interact
3. Puzzle UI appears (cursor unlocks)
4. Player solves puzzle
5. Success message shows
6. Chest automatically opens
7. Cursor locks again
8. Player continues playing

---

## Need Help?

Check the Console (Ctrl+Shift+C) for debug messages:
- "Chest is locked! Showing puzzle..."
- "Puzzle completed! Chest is now unlocked."
- "Generated puzzle: X + Y = ? (Answer: Z)"

These messages help you understand what's happening!

