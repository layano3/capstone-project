# Sound Effects Setup Guide

This guide will help you set up sound effects for XP gains and chest unlocking.

## 🎵 Setting Up XP Reward Sounds

There are **three different systems** that can award XP. You need to set up sounds for each one you're using:

### Option 1: XPRewardSystem (Main System)

**Step 1:** Find the XPRewardSystem
1. In Unity, search for `XPRewardSystem` in your scene hierarchy
2. Select the GameObject that has the `XPRewardSystem` component

**Step 2:** Assign Audio Clips
In the Inspector, you'll see a section called **"Audio & VFX (Optional)"** with these fields:
- **XP Gain Sound** - Plays when any XP is awarded
- **Quest Complete Sound** - Plays when a quest is completed
- **Puzzle Solved Sound** - Plays when a puzzle is solved
- **Level Up Sound** - (Future use) Plays when player levels up

**Step 3:** Enable/Disable Sounds
- **Play Sounds** checkbox - Toggle this to enable/disable all XP sounds
- **Spawn VFX** checkbox - Toggle this to enable/disable VFX (if you have VFX prefabs assigned)

---

### Option 2: PuzzleLockedInteractable (Built-in Reward System)

**Step 1:** Find Puzzle-Locked Objects
1. Select any GameObject that has the `PuzzleLockedInteractable` component
2. This is used for chests, doors, or other objects that require puzzle solving

**Step 2:** Enable XP Rewards
1. In the Inspector, find the **"Rewards"** section
2. Check **"Award XP On Solve"** to enable XP rewards
3. Set the **XP Reward Amount** and **XP Reward Reason**

**Step 3:** Assign Audio Clips
1. Scroll down to the **"Audio (Optional)"** section
2. Assign these sounds:
   - **XP Gain Sound** - Generic XP gain sound (fallback)
   - **Puzzle Solved Sound** - Plays when puzzle is solved and XP is awarded ⭐
3. Make sure **"Use Audio Manager"** is checked
4. Make sure **"Play Sounds"** is checked

---

### Option 3: PuzzleXPReward (Helper Component)

**Step 1:** Find PuzzleXPReward Components
1. Select any GameObject that has the `PuzzleXPReward` component
2. This is a helper component that can be triggered from UnityEvents

**Step 2:** Assign Audio Clips
1. In the Inspector, find the **"Audio (Optional)"** section
2. Assign these sounds:
   - **XP Gain Sound** - Generic XP gain sound (fallback)
   - **Puzzle Solved Sound** - Plays when XP is awarded ⭐
3. Make sure **"Use Audio Manager"** is checked
4. Make sure **"Play Sounds"** is checked

---

## 🎵 Setting Up Chest Sounds

### For Basic Chests (ChestAnimationInteractable)

1. Select your chest GameObject in the scene
2. Find the **Chest Animation Interactable** component in the Inspector
3. Look for the **"Audio (Optional)"** section
4. Assign these sounds:
   - **Open Sound** - Plays when chest opens
   - **Close Sound** - Plays when chest closes (optional)
   - **Use Audio Manager** - Check this to use the centralized audio system (recommended)

### For Puzzle-Locked Chests (PuzzleLockedChest)

1. Select your puzzle-locked chest GameObject
2. Find the **Puzzle Locked Chest** component
3. Look for the **"Audio (Optional)"** section
4. Assign these sounds:
   - **Open Sound** - Plays when chest opens
   - **Close Sound** - Plays when chest closes (optional)
   - **Unlock Sound** - Plays when puzzle is solved and chest unlocks ⭐
   - **Locked Sound** - Plays when player tries to open a locked chest
   - **Use Audio Manager** - Check this to use the centralized audio system (recommended)

---

## 📝 Recommended Sound Effects

### XP Gain Sounds
- **XP Gain**: Short, positive "ding" or "chime" sound
- **Quest Complete**: Triumphant fanfare or success sound
- **Puzzle Solved**: Satisfying "click" or "unlock" sound

### Chest Sounds
- **Open Sound**: Wooden creak, lid opening, or treasure chest opening
- **Unlock Sound**: Key turning, lock clicking, or magical unlock sound
- **Locked Sound**: Lock rattle, "locked" sound, or denial sound

---

## 🎮 Testing Your Sounds

### Test XP Sounds:
1. Play the game
2. Complete a quest or solve a puzzle
3. You should hear the assigned sound effect

### Test Chest Sounds:
1. Play the game
2. Approach a chest and press E to interact
3. You should hear:
   - **Locked sound** (if chest is locked)
   - **Unlock sound** (when puzzle is solved)
   - **Open sound** (when chest opens)

---

## 🔧 Troubleshooting

### No sounds playing?
1. **Check AudioManager exists**: Make sure there's an AudioManager in your scene
2. **Check volume**: Go to Settings and make sure SFX volume is not muted
3. **Check audio clips**: Make sure audio clips are assigned in the Inspector
4. **Check "Use Audio Manager"**: Make sure this checkbox is enabled

### Sounds too quiet/loud?
- Adjust the volume parameter in the code (currently set to 0.8f = 80% volume)
- Or adjust the SFX volume slider in your game settings

### Multiple sounds playing at once?
- This is normal! The AudioManager uses object pooling to handle multiple sounds
- If you want to limit concurrent sounds, adjust `maxConcurrentSFX` in AudioManager

---

## 💡 Tips

1. **Use AudioManager**: Always check "Use Audio Manager" for consistent volume control
2. **3D Sounds**: Chest sounds are 3D positioned, so they'll sound closer/farther based on player position
3. **Audio Format**: Use .wav or .ogg for best performance (avoid .mp3 for short sound effects)
4. **Volume Levels**: Keep sound effects at similar volume levels for consistency

---

## 📦 Where to Get Sound Effects

Free resources:
- **Freesound.org** - Free sound effects library
- **OpenGameArt.org** - Free game assets including sounds
- **Unity Asset Store** - Free and paid sound packs

Paid resources:
- **Unity Asset Store** - Professional sound effect packs
- **AudioJungle** - High-quality sound effects

---

## ✅ Quick Checklist

- [ ] XPRewardSystem has audio clips assigned
- [ ] Chest components have audio clips assigned
- [ ] "Use Audio Manager" is checked on all chests
- [ ] AudioManager exists in the scene
- [ ] SFX volume is not muted in settings
- [ ] Tested sounds in-game

---

**Need help?** Check the console for any error messages, or review the code comments in the scripts.

