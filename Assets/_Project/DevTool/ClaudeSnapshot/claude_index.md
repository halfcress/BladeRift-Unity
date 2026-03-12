# CLAUDE INDEX
commit: a342690 | message: states update
scene: Prototype_CombatCore | roots: 8 | objects: 64
unity: 6000.3.10f1 | export: 2026-03-12 13:26:12

compile: clean

## Files
  WorldScroller (317B) — Assets/_Project/Core/WorldScroller.cs
  CombatDirector (9K) — Assets/_Project/Scripts/Combat/CombatDirector.cs
  CombatTriggerTest (1K) — Assets/_Project/Scripts/Combat/CombatTriggerTest.cs
  ComboManager (1K) — Assets/_Project/Scripts/Combat/ComboManager.cs
  EnemyApproach (7K) — Assets/_Project/Scripts/Combat/EnemyApproach.cs
  FeedbackManager (4K) — Assets/_Project/Scripts/Combat/FeedbackManager.cs
  RageManager (1K) — Assets/_Project/Scripts/Combat/RageManager.cs
  SlashTrail (6K) — Assets/_Project/Scripts/Combat/SlashTrail.cs
  WeakpointDirection (232B) — Assets/_Project/Scripts/Combat/WeakpointDirection.cs
  WeakpointSequence (8K) — Assets/_Project/Scripts/Combat/WeakpointSequence.cs
  AudioManager (1K) — Assets/_Project/Scripts/Core/AudioManager.cs
  CorridorLoop (4K) — Assets/_Project/Scripts/Core/CorridorLoop.cs
  GameConfig (1K) — Assets/_Project/Scripts/Core/GameConfig.cs
  SwipeDebugHUD (1K) — Assets/_Project/Scripts/Input/SwipeDebugHUD.cs
  SwipeInput (4K) — Assets/_Project/Scripts/Input/SwipeInput.cs
  SwipeInterpreter (5K) — Assets/_Project/Scripts/Input/SwipeInterpreter.cs
  BillboardFacing (617B) — Assets/_Project/Scripts/Tools/BillboardFacing.cs
  ClaudeSnapshotTool (13K) — Assets/_Project/Scripts/Tools/ClaudeSnapshotTool.cs
  BackgroundScroller (945B) — Assets/_Project/Scripts/UI/BackgroundScroller.cs
  ButtonLabelPressEffect (2K) — Assets/_Project/Scripts/UI/ButtonLabelPressEffect.cs
  MainMenuManager (2K) — Assets/_Project/Scripts/UI/MainMenuManager.cs
  WeakpointDirectionView (3K) — Assets/_Project/Scripts/UI/WeakpointDirectionView.cs

## Components
GameRoot [CombatDirector | CorridorLoop | CombatTriggerTest | WeakpointSequence | ComboManager | RageManager]
  Corridor_01
  Corridor_02
  Corridor_03
  InputRoot [SwipeInput]
  EnemyRoot
    EnemySpawnPoint
      EnemyPlaceHolder [BillboardFacing | EnemyApproach]
  FeedbackManager [FeedbackManager]
EventSystem
Main Camera
  SlashTrail [SlashTrail]
Directional Light
Player_Reference
Global Volume
UIRoot
  WeakpointMarkerRoot [WeakpointDirectionView]
AudioManager [AudioManager]

