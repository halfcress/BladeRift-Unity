# ARCHITECTURE (TR) - BladeRift

## Hedef
Combat tasarımı değişse bile sistemi çöpe atmadan ilerlemek:
- Modüler yapı
- Data-driven kurallar
- Event tabanlı haberleşme
- Küçük, bağımsız script dosyaları

## Scene / High-level
- UIRoot (Canvas)
  - WeakpointOverlay
  - HUD (HP/Rage/Score)
  - DebugText (prototipte)
- GameRoot
  - GameStateMachine
  - Input (SwipeInput)
  - CombatDirector
  - (sonra) SpawnDirector, EnemyRoot, CorridorRoot

## Modüller

### 1) Input
**Sorumluluk:** Parmağın basılı tutulması + pozisyon akışı
- `SwipeInput`
  - Mouse + Touch unify
  - Olaylar:
    - FingerDown(pos)
    - FingerMove(pos)
    - FingerUp()

> Input sistemi "weakpoint" bilmez. Sadece pozisyon yayınlar.

### 2) Weakpoint Overlay (UI)
**Sorumluluk:** Weakpoint marker üretme/gösterme, highlight, jitter
- `WeakpointOverlayController`
  - Marker pool
  - Marker yerleşimi (2D overlay)
  - Sıradaki hedef highlight
  - Micro jitter
  - Tick tetikleyici (event)

### 3) Sequence / Rules
**Sorumluluk:** Zincir mantığı (sırayla yanma, execution window vs)
- `WeakpointSequence`
  - Zincir listesi
  - Index takibi
  - Telegraph phase
  - Execution window phase

### 4) Combat Orchestrator
**Sorumluluk:** Kuralların tek kapısı (doğru/yanlış, success/fail)
- `CombatDirector`
  - Input eventlerini dinler
  - "Sıradaki hedefe dokundu mu?" hit test yapar
  - Success => Advance / Execute
  - Fail => Punish tetikler

### 5) Feedback
**Sorumluluk:** Hit stop / flash / shake / UI feedback
- `FeedbackController`
  - Combat eventlerini dinler
  - Prototipte: sadece UI text + basit flash

### 6) Enemy (sonraki faz)
**Sorumluluk:** Yaklaşma, saldırı, interrupt, stagger, HP
- `EnemyController`
  - Telegraph başlatır -> weakpoint sequence başlar
  - Execute -> interrupt + stagger + damage

### 7) Spawn / Chapter (sonraki faz)
- `SpawnDirector` (wave tabanlı)
- `ChapterController` (60-90sn)
- `ReviveController` (1 revive)

## Event Sözleşmesi (kural)
Sistemler birbirini referansla çağırmak yerine event dinler.

Önerilen eventler:
- Input
  - OnFingerDown(Vector2)
  - OnFingerMove(Vector2)
  - OnFingerUp()
- Weakpoint
  - OnTelegraphStep(int index)
  - OnExecutionWindowStart(float duration)
  - OnExecutionWindowEnd()
- Combat
  - OnChainAdvance(int newIndex)
  - OnChainSuccess()
  - OnChainFail(string reason)
- Feedback
  - OnFlash(float intensity)
  - OnHitStop(float seconds)

## Esneklik Kuralları
- Süreler/şiddetler kodda hardcode olmayacak -> `GameConfig` üzerinden.
- Prototip sahnesi ayrı tutulacak.
- Kod değişiklikleri dosya dosya küçük tutulacak.