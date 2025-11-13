# Puzzle System Architecture

## 🏗️ System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    PUZZLE SYSTEM LAYERS                      │
└─────────────────────────────────────────────────────────────┘

Layer 1: PLAYER INTERACTION
┌──────────────────────┐
│   Player presses E   │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│     Interactor       │  (Your existing system)
│  - Raycasts          │
│  - Finds IInteractable│
│  - Calls .Interact() │
└──────────┬───────────┘
           │
           ▼

Layer 2: PUZZLE WRAPPER (NEW!)
┌──────────────────────────────────────────┐
│      PuzzleLockedInteractable            │
│  ┌────────────────────────────────────┐  │
│  │ Is Locked?                         │  │
│  │  ├─ NO  → Call wrapped interactable│  │
│  │  └─ YES → Show puzzle              │  │
│  └────────────────────────────────────┘  │
└──────────┬───────────────────────────────┘
           │
           ├─────────────────┬─────────────────┐
           │                 │                 │
           ▼                 ▼                 ▼
    [If Unlocked]      [If Locked]      [After Solve]
           │                 │                 │
           │                 │                 │
           │            ┌────▼─────┐           │
           │            │  Puzzle  │           │
           │            │   UI     │           │
           │            └────┬─────┘           │
           │                 │                 │
           │            [Solved?]              │
           │                 │                 │
           │                 └─────────────────┘
           │                                   │
           └───────────────┬───────────────────┘
                           │
                           ▼

Layer 3: ACTUAL INTERACTABLE
┌──────────────────────────────────────────┐
│     Your Interactable Component          │
│  ┌────────────────────────────────────┐  │
│  │ ChestAnimationInteractable         │  │
│  │ DoorInteractable                   │  │
│  │ LeverInteractable                  │  │
│  │ NPCInteractable                    │  │
│  │ YourCustomInteractable             │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
```

---

## 🔄 Component Relationships

### Single Object Setup
```
GameObject: "Treasure Chest"
│
├─ Component: ChestAnimationInteractable (IInteractable)
│  └─ Opens/closes chest with animation
│
└─ Component: PuzzleLockedInteractable (IInteractable)
   ├─ Wraps ChestAnimationInteractable
   ├─ References: MathPuzzlePanel
   └─ Intercepts Interact() calls
```

### Shared Puzzle UI
```
Scene Hierarchy:
│
├─ Canvas
│  └─ MathPuzzlePanel (SimpleMathPuzzle)
│     └─ [Shared by all locked objects]
│
├─ Chest 1
│  ├─ ChestAnimationInteractable
│  └─ PuzzleLockedInteractable ──┐
│                                 │
├─ Chest 2                        │
│  ├─ ChestAnimationInteractable  │
│  └─ PuzzleLockedInteractable ──┼─→ All reference
│                                 │   same puzzle!
├─ Door                           │
│  ├─ DoorInteractable            │
│  └─ PuzzleLockedInteractable ──┤
│                                 │
└─ Lever                          │
   ├─ LeverInteractable           │
   └─ PuzzleLockedInteractable ──┘
```

---

## 🎯 Interaction Flow

### Flow 1: Unlocked Object
```
Player → Interactor → PuzzleLockedInteractable
                      ↓
                   [Check: Locked?]
                      ↓ NO
                   [Call wrapped.Interact()]
                      ↓
                   ChestAnimationInteractable
                      ↓
                   Chest Opens! ✓
```

### Flow 2: Locked Object (First Time)
```
Player → Interactor → PuzzleLockedInteractable
                      ↓
                   [Check: Locked?]
                      ↓ YES
                   [Show Puzzle]
                      ↓
                   SimpleMathPuzzle.ShowPuzzle()
                      ↓
                   [Player sees UI]
                      ↓
                   [Player solves: 5 + 3 = 8]
                      ↓
                   [Correct!]
                      ↓
                   PuzzleLockedInteractable.OnPuzzleCompleted()
                      ↓
                   [Set: hasBeenUnlocked = true]
                      ↓
                   [Call wrapped.Interact()]
                      ↓
                   ChestAnimationInteractable
                      ↓
                   Chest Opens! ✓
```

### Flow 3: Locked Object (Already Solved)
```
Player → Interactor → PuzzleLockedInteractable
                      ↓
                   [Check: Locked?]
                      ↓ YES
                   [Check: hasBeenUnlocked?]
                      ↓ YES
                   [Call wrapped.Interact()]
                      ↓
                   ChestAnimationInteractable
                      ↓
                   Chest Opens! ✓
```

---

## 🧩 Interface Hierarchy

```
IInteractable (Interface)
├─ void Interact()
│
├─ Implemented by:
│  ├─ PuzzleLockedInteractable ◄─── WRAPPER
│  ├─ ChestAnimationInteractable
│  ├─ DoorInteractable
│  ├─ LeverInteractable
│  ├─ NPCInteractable
│  └─ [Your custom scripts]
│
└─ All can be wrapped by PuzzleLockedInteractable!

IPuzzle (Interface)
├─ void ShowPuzzle(onComplete, onCancel)
├─ void HidePuzzle()
├─ void ResetPuzzle()
├─ bool IsActive
└─ bool IsCompleted
   │
   ├─ Implemented by:
   │  ├─ SimpleMathPuzzle ◄─── INCLUDED
   │  ├─ [Your riddle puzzle]
   │  ├─ [Your pattern puzzle]
   │  └─ [Your custom puzzles]
   │
   └─ All can be used by PuzzleLockedInteractable!
```

---

## 📦 Data Flow

### Puzzle State Management
```
┌─────────────────────────────────────────┐
│         SimpleMathPuzzle                │
│  ┌───────────────────────────────────┐  │
│  │ IsActive: bool                    │  │
│  │ IsCompleted: bool                 │  │
│  │ correctAnswer: int                │  │
│  └───────────────────────────────────┘  │
│         ▲                        │       │
│         │                        │       │
│    [ShowPuzzle]            [OnSubmit]   │
│         │                        │       │
│         │                        ▼       │
│    ┌────┴────────────────────────────┐  │
│    │  Callbacks:                     │  │
│    │  - onComplete: Action           │  │
│    │  - onCancel: Action             │  │
│    └─────────────────────────────────┘  │
└─────────────────────────────────────────┘
                    │
                    │ Invokes
                    ▼
┌─────────────────────────────────────────┐
│    PuzzleLockedInteractable             │
│  ┌───────────────────────────────────┐  │
│  │ isLocked: bool                    │  │
│  │ hasBeenUnlocked: bool             │  │
│  │ puzzle: IPuzzle                   │  │
│  │ wrappedInteractable: IInteractable│  │
│  └───────────────────────────────────┘  │
│         │                               │
│         │ Calls                         │
│         ▼                               │
│  ┌───────────────────────────────────┐  │
│  │ wrappedInteractable.Interact()    │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

---

## 🎨 Unity Inspector Layout

### GameObject with Puzzle Lock
```
Inspector: "Treasure Chest"
┌──────────────────────────────────────┐
│ Transform                            │
├──────────────────────────────────────┤
│ Chest Animation Interactable         │
│  ├─ Animation: [chest_anim]          │
│  ├─ Open Clip: [chest_open]          │
│  ├─ Close Clip: [chest_close]        │
│  └─ Toggle: ☑                        │
├──────────────────────────────────────┤
│ Puzzle Locked Interactable           │
│  Puzzle Lock Settings                │
│  ├─ Is Locked: ☑                     │
│  ├─ Puzzle Component: [MathPuzzle]   │
│  ├─ Solve Once: ☑                    │
│  └─ Can Be Manually Unlocked: ☑      │
│                                      │
│  Messages                            │
│  ├─ Locked Message: "Solve puzzle!"  │
│  └─ Unlocked Message: "Unlocked!"    │
│                                      │
│  Events                              │
│  ├─ On Puzzle Completed ()           │
│  ├─ On Puzzle Cancelled ()           │
│  └─ On Manually Unlocked ()          │
└──────────────────────────────────────┘
```

---

## 🔌 Extension Points

### Add Custom Puzzle Type
```
1. Create class implementing IPuzzle
   ↓
2. Implement required methods
   ↓
3. Create UI for your puzzle
   ↓
4. Assign to PuzzleLockedInteractable
   ↓
5. Works automatically! ✓
```

### Add Custom Interactable
```
1. Create class implementing IInteractable
   ↓
2. Implement Interact() method
   ↓
3. Add PuzzleLockedInteractable component
   ↓
4. Assign puzzle reference
   ↓
5. Works automatically! ✓
```

---

## 🎯 Key Design Principles

### 1. Separation of Concerns
```
PuzzleLockedInteractable → Handles locking logic
SimpleMathPuzzle        → Handles puzzle logic
ChestAnimationInteractable → Handles chest logic

Each component has ONE job!
```

### 2. Composition over Inheritance
```
Instead of:
  PuzzleLockedChest extends ChestAnimationInteractable

We use:
  PuzzleLockedInteractable wraps ChestAnimationInteractable

Benefits:
  ✓ Works with ANY interactable
  ✓ No code duplication
  ✓ Easy to add/remove
  ✓ Highly reusable
```

### 3. Interface-Based Design
```
IPuzzle interface → Any puzzle type works
IInteractable interface → Any interactable works

Result: Maximum flexibility!
```

### 4. Single Responsibility
```
Each component does ONE thing well:
  - Interactor: Finds interactables
  - PuzzleLockedInteractable: Manages lock state
  - SimpleMathPuzzle: Shows math puzzles
  - ChestAnimationInteractable: Animates chests
```

---

## 📊 Performance Considerations

### Memory
```
✓ One puzzle UI shared by all objects (efficient!)
✓ Each locked object has minimal state (2 bools)
✓ No runtime allocations during interaction
```

### CPU
```
✓ No Update() loops in PuzzleLockedInteractable
✓ Puzzle UI only active when shown
✓ Callbacks used instead of polling
```

### Scalability
```
✓ 1 puzzle UI can handle 1000+ locked objects
✓ No performance impact when not interacting
✓ Modular design allows easy optimization
```

---

## 🎓 Summary

The system uses a **wrapper pattern** to add puzzle-locking to any interactable without modifying the original code. This makes it:

- ✅ **Reusable** - Works with any IInteractable
- ✅ **Modular** - Each component has one job
- ✅ **Extensible** - Easy to add new puzzles/interactables
- ✅ **Efficient** - Shared resources, minimal overhead
- ✅ **Maintainable** - Clean separation of concerns

**One component to lock them all!** 🔒

