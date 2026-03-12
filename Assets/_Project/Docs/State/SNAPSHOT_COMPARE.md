# SNAPSHOT_COMPARE

Generated: 2026-03-12 15:24:16

Working Snapshot:
- BladeRift_WORKING_Prototype_CombatCore_20260312_110225.json

Debug Snapshot:
- BladeRift_DEBUG_Prototype_CombatCore_20260310_225904.json

---

## META CHANGES
- head commit: [a342690] -> [2295573]
- commit message: [states update] -> [RageHit Bug Fix (CameraTag Problem)]

---

## OBJECTS ADDED
None.

---

## OBJECTS REMOVED
None.

---

## OBJECT STATE CHANGES
No changes.

---

## TRANSFORM CHANGES
No changes.

---

## COMPONENT FIELD CHANGES
Object: GameRoot
  Component: CombatDirector
    weakpointSequence
    WeakpointSequence on "GameRoot" [id:81520] -> WeakpointSequence on "GameRoot" [id:65064]
    swipeInput
    SwipeInput on "GameRoot/InputRoot" [id:81554] -> SwipeInput on "GameRoot/InputRoot" [id:65084]
    directionView
    WeakpointDirectionView on "UIRoot/WeakpointMarkerRoot" [id:81448] -> WeakpointDirectionView on "UIRoot/WeakpointMarkerRoot" [id:65002]

Object: GameRoot
  Component: CorridorLoop
    reference
    Transform on "Main Camera" [id:81578] -> Transform on "Main Camera" [id:65110]

Object: GameRoot
  Component: CombatTriggerTest
    combatDirector
    CombatDirector on "GameRoot" [id:81516] -> CombatDirector on "GameRoot" [id:65060]

Object: GameRoot
  Component: WeakpointSequence
    directionView
    WeakpointDirectionView on "UIRoot/WeakpointMarkerRoot" [id:81448] -> WeakpointDirectionView on "UIRoot/WeakpointMarkerRoot" [id:65002]
    telegraphRevealInterval
    0,1 -> 1
    telegraphHoldSeconds
    0,2 -> 1

Object: GameRoot
  Component: ComboManager
    comboText
    TextMeshProUGUI on "UIRoot/ComboText" [id:81584] -> TextMeshProUGUI on "UIRoot/ComboText" [id:-49940]
    hitCountText
    TextMeshProUGUI on "UIRoot/HitCountText" [id:81440] -> TextMeshProUGUI on "UIRoot/HitCountText" [id:-49974]

Object: GameRoot
  Component: RageManager
    rageText
    TextMeshProUGUI on "UIRoot/RageText" [id:81432] -> TextMeshProUGUI on "UIRoot/RageText" [id:-103544]

Object: GameRoot/InputRoot
  Component: SwipeInterpreter
    swipe
    SwipeInput on "GameRoot/InputRoot" [id:81554] -> SwipeInput on "GameRoot/InputRoot" [id:65084]
    combatReceiver
    CombatDirector on "GameRoot" [id:81516] -> CombatDirector on "GameRoot" [id:65060]

Object: GameRoot/EnemyRoot/EnemyPlaceHolder
  Component: EnemyApproach
    combatDirector
    CombatDirector on "GameRoot" [id:81516] -> CombatDirector on "GameRoot" [id:65060]
    approachSpeed
    40 -> 4

Object: GameRoot/FeedbackManager
  Component: FeedbackManager
    targetCamera
    Camera on "Main Camera" [id:81576] -> Camera on "Main Camera" [id:65108]

Object: Main Camera/SlashTrail
  Component: SlashTrail
    swipeInput
    SwipeInput on "GameRoot/InputRoot" [id:81554] -> SwipeInput on "GameRoot/InputRoot" [id:65084]
    targetCamera
    Camera on "Main Camera" [id:81576] -> Camera on "Main Camera" [id:65108]


---

## CODE CHANGES

### Added
None.

### Removed
- Assets\_Project\Scripts\UI\BackgroundScroller.cs
- Assets\_Project\Scripts\UI\ButtonLabelPressEffect.cs
- Assets\_Project\Scripts\UI\MainMenuManager.cs

### Modified

#### Assets\_Project\Scripts\Combat\CombatDirector.cs
```diff
  ...
              Vector2 fingerPos = swipeInput.FingerPosition;
              bool contains = cachedRectValid && cachedEnemyRect.Contains(fingerPos);
+ 
+             Debug.Log($"[RageHitTest] rectOk={cachedRectValid} fp={fingerPos} rect={cachedEnemyRect} contains={contains} delta={delta.magnitude:F1}");
  
              if (!contains) return;
  ...
          comboManager?.RegisterTimeout();
          rageManager?.ResetRage();
- 
          enemyApproach?.SetRageVisual(false);
          FeedbackManager.Instance?.PlayFailFeedback();
```

#### Assets\_Project\Scripts\Combat\EnemyApproach.cs
```diff
  ...
  
          screenRect = new Rect(minX - padW, minY - padH, w + padW * 2f, h + padH * 2f);
+ 
+         Debug.Log($"[RageHitTest] bounds.center={center} bounds.size={bounds.size} screenRect={screenRect} screen={Screen.width}x{Screen.height}");
  
          return true;
```

#### Assets\_Project\Scripts\Combat\WeakpointSequence.cs
```diff
  ...
          directionView.ShowTelegraphStep(revealedCount, chain[revealedCount]);
          OnTelegraphStep?.Invoke(revealedCount);
-         AudioManager.Instance?.PlayTelegraphStep();
          Debug.Log($"WeakpointSequence: Telegraph goster index={revealedCount} dir={chain[revealedCount]}");
  
```

#### Assets\_Project\Scripts\Core\AudioManager.cs
```diff
  ...
      [SerializeField] private AudioClip rageActivate;
      [SerializeField] private AudioClip chainSuccess;
-     [SerializeField] private AudioClip telegraphStep;
  
      [Header("Volume")]
-     [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
+     [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
  
      private AudioSource audioSource;
  ...
      }
  
-     public void PlayHitNormal() => Play(hitNormal);
-     public void PlayHitRage() => Play(hitRage, 1.2f);
-     public void PlayFailPunish() => Play(failPunish);
+     public void PlayHitNormal()    => Play(hitNormal);
+     public void PlayHitRage()      => Play(hitRage, 1.2f);
+     public void PlayFailPunish()   => Play(failPunish);
      public void PlayRageActivate() => Play(rageActivate);
      public void PlayChainSuccess() => Play(chainSuccess);
-     public void PlayTelegraphStep() => Play(telegraphStep);
  }
+ 
```

---

## SUMMARY
Objects added:           0
Objects removed:         0
Object state changes:    0
Transform changes:       0
Component field changes: 18
Code files added:        0
Code files removed:      3
Code files modified:     4

