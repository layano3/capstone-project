# Reusable Puzzle System Guide

## 🎯 Overview
The puzzle system is now **fully reusable**! You can add puzzle-locking to **ANY interactable object** in your game using the `PuzzleLockedInteractable` component.

---

## 🔧 How It Works

### The Wrapper Pattern
`PuzzleLockedInteractable` acts as a **wrapper** around any existing `IInteractable`:

```
Player presses E
      ↓
PuzzleLockedInteractable.Interact()
      ↓
Is locked? → NO → Call wrapped interactable
      ↓ YES
Show puzzle
      ↓
Puzzle solved? → YES → Call wrapped interactable
```

---

## 🚀 Quick Setup (Any Object)

### Step 1: Create Your Puzzle UI (One Time)
```
1. Create empty GameObject
2. Add "PuzzleUISetup" component
3. Right-click → "Create Math Puzzle UI"
4. Delete the setup GameObject
```
**This creates a shared puzzle UI that ALL locked objects can use!**

### Step 2: Add to Your Chest
```
1. Select your chest GameObject
2. Keep your existing ChestAnimationInteractable
3. Add "PuzzleLockedInteractable" component
4. Drag "MathPuzzlePanel" to "Puzzle Component" field
5. Check "Is Locked"
```

### Step 3: Add to a Door
```
1. Select your door GameObject
2. Add "DoorInteractable" component (or your own door script)
3. Add "PuzzleLockedInteractable" component
4. Drag "MathPuzzlePanel" to "Puzzle Component" field
5. Check "Is Locked"
```

### Step 4: Add to ANYTHING
```
1. Select ANY GameObject with IInteractable
2. Add "PuzzleLockedInteractable" component
3. Drag "MathPuzzlePanel" to "Puzzle Component" field
4. Check "Is Locked"
```

**Done!** The same puzzle UI works for all objects!

---

## 📦 Component Structure

### PuzzleLockedInteractable (The Wrapper)
**Purpose:** Adds puzzle-locking to any interactable  
**Location:** Add to the same GameObject as your interactable  
**Reusable:** ✅ Yes! Add to unlimited objects  

**Settings:**
- **Is Locked** - Enable/disable puzzle requirement
- **Puzzle Component** - Reference to your puzzle (shared across all objects)
- **Solve Once** - Puzzle only needs to be solved once
- **Can Be Manually Unlocked** - Allow code/triggers to unlock
- **Locked Message** - Custom message when locked
- **Unlocked Message** - Custom message when unlocked

### SimpleMathPuzzle (The Puzzle)
**Purpose:** The actual puzzle logic and UI  
**Location:** On the MathPuzzlePanel GameObject  
**Reusable:** ✅ Yes! One puzzle UI for all locked objects  

**Settings:**
- **Min/Max Number** - Difficulty range
- **Allow Subtraction** - Include subtraction
- **Allow Multiplication** - Include multiplication

---

## 💡 Usage Examples

### Example 1: Puzzle-Locked Chest
```
GameObject: Treasure Chest
├─ ChestAnimationInteractable (your existing script)
└─ PuzzleLockedInteractable (new wrapper)
   └─ Puzzle Component: MathPuzzlePanel
   └─ Is Locked: ✓
   └─ Solve Once: ✓
```

### Example 2: Puzzle-Locked Door
```
GameObject: Secret Door
├─ DoorInteractable (opens/closes door)
└─ PuzzleLockedInteractable (new wrapper)
   └─ Puzzle Component: MathPuzzlePanel
   └─ Is Locked: ✓
   └─ Solve Once: ✓
```

### Example 3: Puzzle-Locked NPC
```
GameObject: Wise Elder
├─ NPCInteractable (dialogue system)
└─ PuzzleLockedInteractable (new wrapper)
   └─ Puzzle Component: MathPuzzlePanel
   └─ Is Locked: ✓
   └─ Solve Once: ✗ (puzzle resets each conversation)
```

### Example 4: Multiple Chests, One Puzzle
```
GameObject: Chest 1
├─ ChestAnimationInteractable
└─ PuzzleLockedInteractable → MathPuzzlePanel

GameObject: Chest 2
├─ ChestAnimationInteractable
└─ PuzzleLockedInteractable → MathPuzzlePanel (same puzzle!)

GameObject: Chest 3
├─ ChestAnimationInteractable
└─ PuzzleLockedInteractable → MathPuzzlePanel (same puzzle!)
```
**All three chests share the same puzzle UI!**

---

## 🎨 Configuration Patterns

### Pattern 1: Solve Once (Tutorial Chest)
```
Solve Once: ✓
Can Be Manually Unlocked: ✓

Result: Player solves puzzle once, chest stays unlocked forever
```

### Pattern 2: Repeating Puzzle (Training Dummy)
```
Solve Once: ✗
Can Be Manually Unlocked: ✗

Result: Player must solve puzzle every time they interact
```

### Pattern 3: Quest-Based Unlock
```
Solve Once: ✓
Can Be Manually Unlocked: ✓

Result: Player solves puzzle OR quest system unlocks it via code
```

### Pattern 4: Optional Puzzle
```
Is Locked: ✗ (initially)

Result: Object works normally, but can be locked via code later
```

---

## 🔌 Unity Events Integration

`PuzzleLockedInteractable` has built-in Unity Events:

### On Puzzle Completed
Triggered when player successfully solves the puzzle
```
Use cases:
- Play success sound
- Spawn reward particles
- Update quest progress
- Unlock other objects
```

### On Puzzle Cancelled
Triggered when player cancels the puzzle
```
Use cases:
- Play cancel sound
- Show hint message
- Log attempt count
```

### On Manually Unlocked
Triggered when unlocked via code/trigger
```
Use cases:
- Quest completion unlocks
- Key item unlocks
- Time-based unlocks
```

---

## 💻 Code Examples

### Unlock from Code
```csharp
// Get the puzzle lock component
PuzzleLockedInteractable puzzleLock = chest.GetComponent<PuzzleLockedInteractable>();

// Unlock without puzzle
puzzleLock.UnlockManually();
```

### Lock from Code
```csharp
// Lock it again
puzzleLock.Lock();
```

### Toggle Lock
```csharp
// Toggle between locked/unlocked
puzzleLock.ToggleLock();
```

### Enable/Disable Puzzle Requirement
```csharp
// Disable puzzle requirement entirely
puzzleLock.SetLockEnabled(false);

// Enable it again
puzzleLock.SetLockEnabled(true);
```

### Check Status
```csharp
// Check if currently locked
bool isLocked = puzzleLock.IsLocked;

// Check if has been unlocked before
bool wasUnlocked = puzzleLock.HasBeenUnlocked;
```

### Reset Everything
```csharp
// Reset lock and puzzle state
puzzleLock.ResetLock();
```

---

## 🎮 Example Interactables Included

I've created example scripts you can use:

### DoorInteractable.cs
- Opens/closes doors with rotation animation
- Add PuzzleLockedInteractable to require puzzle before opening

### LeverInteractable.cs
- Activates/deactivates levers
- Has Unity Events for activated/deactivated
- Add PuzzleLockedInteractable to require puzzle before pulling

### NPCInteractable.cs
- Cycles through dialogue lines
- Has Unity Events for dialogue start/end
- Add PuzzleLockedInteractable to require puzzle before talking

**All examples work with PuzzleLockedInteractable!**

---

## 🏗️ Creating Your Own Interactables

### Step 1: Implement IInteractable
```csharp
public class MyCustomInteractable : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        // Your interaction logic here
        Debug.Log("Interacted!");
    }
}
```

### Step 2: Add PuzzleLockedInteractable
```
1. Add your script to GameObject
2. Add PuzzleLockedInteractable to same GameObject
3. Assign puzzle reference
4. Done! Your custom interactable now supports puzzles!
```

**That's it!** No need to modify your script at all.

---

## 🔄 Component Execution Order

Unity automatically handles the order:

1. **Player presses E**
2. **Interactor** finds IInteractable components
3. **PuzzleLockedInteractable.Interact()** is called (if it's first)
4. **If locked:** Shows puzzle
5. **If unlocked:** Calls your actual interactable

**Note:** If you have multiple IInteractable components, Unity calls them in the order they appear in the Inspector. PuzzleLockedInteractable should be first (add it last, it appears at the bottom).

---

## 🎯 Best Practices

### ✅ DO:
- Use one shared puzzle UI for all locked objects
- Set "Solve Once" to true for permanent unlocks
- Use Unity Events for quest/reward integration
- Test with "Show Debug Messages" enabled
- Use the context menu "Test Unlock/Lock" buttons

### ❌ DON'T:
- Create multiple puzzle UIs (one is enough!)
- Add multiple PuzzleLockedInteractable to same object
- Forget to assign the puzzle component
- Disable "Can Be Manually Unlocked" if you need quest unlocks

---

## 🐛 Troubleshooting

### Puzzle doesn't show
- Check "Is Locked" is enabled
- Verify puzzle component is assigned
- Check MathPuzzlePanel exists in scene
- Look for warnings in Console

### Puzzle shows but object doesn't interact after solving
- Make sure you have another IInteractable component
- Check that component is working (test without puzzle lock)
- Verify "Show Debug Messages" logs show success

### Multiple objects share unlock state
- This is by design if "Solve Once" is enabled on the puzzle
- Each object tracks its own unlock state
- Puzzle completion is shared, but unlock state is per-object

### Can't manually unlock
- Check "Can Be Manually Unlocked" is enabled
- Use the context menu "Test Unlock" button
- Verify you're calling UnlockManually() not Unlock()

---

## 📊 Comparison: Old vs New System

### Old System (PuzzleLockedChest)
```
❌ Only works with chests
❌ Duplicates chest code
❌ Hard to extend to other objects
❌ Each object type needs custom script
```

### New System (PuzzleLockedInteractable)
```
✅ Works with ANY interactable
✅ Wraps existing scripts (no duplication)
✅ Easy to extend to new objects
✅ One component for everything
✅ Fully reusable puzzle UI
✅ Unity Events integration
✅ Manual unlock support
```

---

## 🚀 Advanced: Multiple Puzzle Types

You can have different puzzles for different objects:

```
Scene:
├─ MathPuzzlePanel (SimpleMathPuzzle)
├─ RiddlePuzzlePanel (YourRiddlePuzzle)
└─ PatternPuzzlePanel (YourPatternPuzzle)

Easy Chest → PuzzleLockedInteractable → MathPuzzlePanel
Hard Chest → PuzzleLockedInteractable → RiddlePuzzlePanel
Boss Door → PuzzleLockedInteractable → PatternPuzzlePanel
```

Each object can use a different puzzle!

---

## 📝 Summary

### The Magic Formula:
```
ANY Interactable + PuzzleLockedInteractable = Puzzle-Locked Object
```

### One Puzzle UI = Unlimited Locked Objects

### Zero Code Changes to Your Existing Scripts

**That's the power of the wrapper pattern!** 🎉

---

## 🎓 Next Steps

1. ✅ Create puzzle UI (one time setup)
2. ✅ Add PuzzleLockedInteractable to your objects
3. ✅ Assign the shared puzzle reference
4. ✅ Configure settings per object
5. ✅ Test and enjoy!

**Your puzzle system is now fully modular and reusable!**

