# CHAT_STATE (Single Source of Truth)

> Bu dosya: "Bugün en son nerede kaldık?" sorusunun tek cevabıdır.
> Yeni sohbete başlarken ilk referans bu dosyadır.
> Varsayım yapılmaz.

---

## 1) Project Info

- Project Name: BladeRift
- Repo: BladeRift-Unity
- Repo URL: `https://github.com/halfcress/BladeRift-Unity`
- Unity Version: 6000.3.10f1 LTS
- Template: Universal 3D (URP)
- Target Platform: Android (mobile-first)
- Current Main Test Platform: PC
- Current Scene: `Prototype_CombatCore`
- Current Scene Path: `Assets/_Project/Scenes/Prototype/Prototype_CombatCore.unity`
- Screen Target: 9:16 (1080x1920)

---

## 2) Core Direction (Locked)

- Perspektif: **First-person**
- Hareket hissi: **Oyuncu sabit, dünya üstüne akar**
- Ana tema: **Stylized dungeon combat**
- Ortam: **3D corridor**
- Düşman yaklaşımı: **2D billboard / sprite enemy**
- Combat yaklaşımı: **Swipe ile marker'ın üstünden geç → execution**
- Fail koşulu: **Sadece Timeout = chain reset + combo sıfır**
- Finger lift: **Combo bozmaz (Fruit Ninja modeli)**
- v0.1 hedefi: **Paylaşılabilir combat prototype / APK**

---

## 3) Mandatory Design / Architecture Context (Critical)

Yeni sohbet açıldığında mutlaka birlikte okunmalı:

- `ARCHITECTURE_TR.md` — sistem mimarisinin ana doğruluk kaynağı
- `GAME_CONCEPT_TR.md` — oyun mekaniği ve hedef vizyon
- `GAME_RULES_TR.md` — kurallar ve config parametreleri

### Conflict Priority Order
1. `ARCHITECTURE_TR.md`
2. `GAME_RULES_TR.md`
3. `GAME_CONCEPT_TR.md`
4. `CHAT_STATE.md`

---

## 4) Current Working State

### Environment
- Infinite corridor sistemi çalışıyor, stabil.
- Ceiling, fog, torch lights aktif.

### Input
- `SwipeInput` — mouse + touch, `IsDown`, `DeltaPx`, `FingerPosition`
- `SwipeInterpreter` — segment-based, projede duruyor ama combat için kullanılmıyor
- `SwipeDebugHUD` — **kaldırıldı**

### Combat System (Aktif)

| Script | Konum | Görev |
|---|---|---|
| `CombatDirector` | Scripts/Combat | Hit-test yöneticisi, combat akışının kapısı |
| `WeakpointSequence` | Scripts/Combat | Telegraph → ExecutionWindow → Done akışı |
| `WeakpointDirectionView` | Scripts/UI | Sabit ekran marker yönetimi |
| `ComboManager` | Scripts/Combat | Fruit Ninja combo sistemi |
| `CombatTriggerTest` | Scripts/Combat | Test döngüsü (geçici) |
| `GameConfig` | Scripts/Core | ScriptableObject, tüm parametreler |

### Hit-Test Sistemi (Kritik — Tasarım Kararı)

**Marker'ın üstünden geçmek = HIT.**

- Her frame: `SwipeInput.FingerPosition` + `DeltaPx` kontrol edilir
- Parmak marker'a `hitRadiusPx` içindeyse VE doğru yönde gidiyorsa → HIT
- `hitRadiusPx = 80`, `minDeltaPx = 8`, `directionDotThreshold = 0.3`
- Marker pozisyonu: `RectTransform.position` (güvenilir, world-space takip YOK)
- Finger lift = combo bozmaz, sadece timeout = fail

> NOT: Eski "4 yön swipe commit" sistemi (SwipeInterpreter) devre dışı.
> Yön bilgisi hâlâ kullanılıyor ama ikincil — asıl tetikleyici marker üstünden geçmek.

### Weakpoint Marker Sistemi

- Marker'lar **sabit ekran pozisyonlarında** (world-space WeakPoint takibi kaldırıldı)
- 3 marker varsayılan pozisyon: sol-alt `(-150, -100)`, orta-üst `(0, 80)`, sağ-alt `(150, -100)`
- Telegraph: marker'lar sırayla belirir (kırmızı)
- Execution: sıradaki marker parlak kırmızı, diğerleri soluk
- `WeakPoint_1/2/3` world objeleri sahneye var ama artık kullanılmıyor (temizlenebilir)

### UI

- `ComboText` — ekran ortası üstü, sarı, `"HIT!" / "x2 COMBO!"`
- `HitCountText` — sol üst köşe, `"Hits: 0"`
- `DebugSwipeText` — **kaldırıldı**

### Enemy

- `EnemyPlaceHolder` — 2D billboard quad, kameraya dönük
- `WeakPoint_1/2/3` — EnemyPlaceHolder altında, artık kullanılmıyor

### GameConfig Asset

- Path: `Assets/_Project/ScriptableObjects/GameConfig.asset`
- `executionWindowSeconds = 2.0`
- `timeScaleDuringExecution = 0.8`

---

## 5) Scene Hierarchy (Özet)

```
GameRoot [CombatDirector, WeakpointSequence, ComboManager, CorridorLoop, CombatTriggerTest]
  - Corridor_01 / 02 / 03
  - InputRoot [SwipeInput, SwipeInterpreter]
  - EnemyRoot
    - EnemyPlaceHolder [BillboardFacing]
      - WeakPoint_1/2/3 (kullanılmıyor)
UIRoot [Canvas]
  - WeakpointMarkerRoot [WeakpointDirectionView]
    - WeakpointMarker_1/2/3 [Image, CanvasGroup]
  - ComboText [TextMeshProUGUI]
  - HitCountText [TextMeshProUGUI]
EventSystem
Main Camera
Directional Light
Global Volume
```

---

## 6) Script Files (Current)

```
Assets/_Project/Scripts/Combat/
  CombatDirector.cs
  WeakpointSequence.cs
  ComboManager.cs
  CombatTriggerTest.cs
  WeakpointCombatTest.cs   ← eski prototip, temizlenebilir

Assets/_Project/Scripts/Input/
  SwipeInput.cs
  SwipeInterpreter.cs      ← devre dışı, projede duruyor

Assets/_Project/Scripts/UI/
  WeakpointDirectionView.cs

Assets/_Project/Scripts/Core/
  CorridorLoop.cs
  GameConfig.cs

Assets/_Project/Scripts/Tools/
  BillboardFacing.cs
  ProjectState/ (DevTool)

Assets/_Project/Core/
  WorldScroller.cs
```

---

## 7) Çalışma Modu (Token Optimizasyonu — Aktif Kural)

- **MCP:** Sadece kod yazma + sahne kaydetme. Başka hiçbir şey için kullanılmaz.
- **Sahne değişiklikleri:** AI adım adım söyler, kullanıcı elle yapar.
- **Konsol:** Kullanıcı kopyalayıp yapıştırır.
- **Snapshot:** Sadece kullanıcı inisiyatifiyle, gerçekten gerektiğinde.
- **Kod:** `.cs` dosyası olarak verilir, kullanıcı kopyalar.
- **Yeni sohbet açılışı:** "hey zibidi, bladerift önbelleğini güncelle" + kısa durum özeti yeterli.

---

## 8) Source of Truth Rules

- **Kod:** GitHub pushed state
- **Scene/runtime:** Snapshot (sadece gerektiğinde)
- **Bağlam:** CHAT_STATE + Architecture/Design docs birlikte okunur

---

## 9) Folder Structure

```
Assets/_Project/
  Art/
  Audio/
  Docs/
    Architecture/
    Design/
    State/
    Snapshots/Working|Debug|Archive
  Prefabs/
  Scenes/Prototype/
  ScriptableObjects/
  Scripts/
    Combat/
    Core/
    Input/
    Tools/ProjectState/
    UI/
  Settings/
  UI/
  VFX/
```

---

## 10) Current Next Step

### Sıradaki Milestone: Hit-Test Doğrulama + İlk Gerçek Combat Loop

1. **Hit-test doğrulaması** — marker üstünden geç → HIT + combo artıyor mu?
2. **Combo görseli** — "x2 COMBO!" ekranda görünüyor mu?
3. **EnemyController** — koridorun sonundan yaklaşan düşman
4. **Execute = Interrupt** — düşman saldırısı kesilmeli

---

## 11) Main Pain Points (Öğrenilenler)

1. World-space WeakPoint → screen projeksiyon güvensiz → sabit UI pozisyona geçildi
2. Inspector'daki SerializeField değeri kodun default'unu override eder
3. Finger lift = reset tasarımı execution flow'u bozuyordu → Fruit Ninja modeline geçildi
4. SwipeInput.DeltaPx per-frame delta, deadzone var — çok yavaş swipe'ta 0 döner
5. hitRadiusPx Inspector'dan 200 set edilmişti, kod 60f yazıyordu ama Inspector kazandı
6. Snapshot token yediği için sadece gerçekten gerektiğinde istenmeli
7. MCP sahne manipülasyonu çağrıları pahalı — kullanıcı elle yapmalı

---

## 12) ProjectState DevTool

Menu: `Tools > BladeRift > Project State > ...`

Outputs:
- `Assets/_Project/Docs/State/CHAT_STATE.md`
- `Assets/_Project/Docs/State/DEBUG_JOURNAL.md`
- `Assets/_Project/Docs/State/SNAPSHOT_INDEX.md`
- `Assets/_Project/Docs/State/SNAPSHOT_COMPARE.md`
