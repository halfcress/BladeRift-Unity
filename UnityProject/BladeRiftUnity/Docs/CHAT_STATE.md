# CHAT_STATE (Single Source of Truth)

---

## Project Info

- Project Name: BladeRift
- Unity Version: 6000.3.10f1 LTS
- Template: Universal 3D (URP)
- Platform Target: Android (mobile-first)

---

## Current Situation

- GitHub repository created and cleaned (.gitignore working correctly).
- Unity project initialized successfully.
- Documentation completed:
  - GAME_CONCEPT_TR.md
  - GAME_RULES_TR.md
  - ARCHITECTURE_TR.md
  - TODO_TR.md
- No gameplay code written yet.
- Prototype scene not created yet.

---

## Locked Design Decisions

These decisions are considered stable unless explicitly revised:

- Single input system (continuous swipe).
- Weakpoint chain mechanic (2D UI overlay).
- Sequential telegraph phase before execution.
- 2-second execution window with mini slow motion (~0.8 timeScale).
- Finger lift = chain reset.
- Only the next weakpoint is strongly highlighted.
- Execution = interrupt (player should not get hit after correct execution).
- Elite/Boss do not always die in one execution (large damage + stagger).
- Rage mode removes weakpoint requirement temporarily.
- 1 revive per chapter (v0.1).
- Wave-based progression.
- First goal: playable APK v0.1 with strong combat feel.

---

## What Is Done

- Core concept defined.
- Combat rules defined.
- Architecture plan defined.
- Upgrade philosophy defined.
- Monetization direction defined.
- Scope intentionally controlled (no over-expansion yet).

---

## What Is NOT Done

- Prototype_CombatCore scene.
- Assets/_Project folder structure.
- GameConfig ScriptableObject.
- SwipeInput system.
- Weakpoint overlay prototype.
- CombatDirector.
- Enemy placeholder.
- Wave system.
- Rage implementation.

---

## Immediate Next Step (Single Focus)

1. Create `Prototype_CombatCore` scene.
2. Create `Assets/_Project` folder structure.
3. Implement:
   - GameConfig (ScriptableObject)
   - SwipeInput (touch + mouse unified skeleton)
4. Print debug output on swipe (no gameplay yet).

---

## Important Reminder

Combat feel > visuals.

Prototype first.
Polish later.
Refactor only if necessary.