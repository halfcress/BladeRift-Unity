# CHAT_STATE (Single Source of Truth)

> Bu dosya: “Bugün en son nerede kaldık?” sorusunun tek cevabıdır.
> Yeni sohbete başlarken referans alınır.
> Kural: Varsayım yok. Debug/iterasyon sırasında ihtiyaç olduğunda FULL snapshot istenir ve kullanıcı yükler.

---

## 1) Project Info

- Project Name: BladeRift
- Repo: BladeRift-Unity
- Unity Version: 6000.3.10f1 LTS
- Template: Universal 3D (URP)
- Target Platform: Android (mobile-first) *(şimdilik PC üzerinde test ediliyor)*
- Screen Target: 9:16 (1080x1920)
- Current Scene: `Prototype_CombatCore`

---

## 2) Core Decisions (Locked)

- Perspektif: **First-person**
- Hareket hissi: **Oyuncu sabit, dünya geriye akar (world comes to you)**
- Kontrol: **Continuous swipe** (touch + mouse unified)
- Weakpoint chain: telegraph → execution window (hedef ~2s)
- Finger lift = chain reset
- Execution = interrupt (doğru execution sonrası oyuncu “haksız” hit yememeli)
- Elite/Boss tekte ölmek zorunda değil (heavy damage + stagger)
- Rage: weakpoint kuralını geçici kaldırır
- v0.1 hedefi: “paylaşılabilir APK + combat feel ultra”

---

## 3) Scene Hierarchy (Baseline)

Scene root'ları:
- `GameRoot`
- `UIRoot` (Canvas)
- `EventSystem`
- `Main Camera`
- `Directional Light`

Koridor segmentleri (GameRoot altında):
- `Corridor_01`
- `Corridor_02`
- `Corridor_03` *(varsa / önerilir)*

Her segmentin içinde (hepsi segment root’un direct child’ı; Wall altına child yapılmaz):
- `Floor` (Cube)
- `Wall_Left` (Cube)
- `Wall_Right` (Cube)
- `Ceiling` (Cube)
- `TorchLight_*` (Point Lights) *(segment root altında)*

---

## 4) Geometry Standard (Prototip)

Amaç: Plane/Cube ölçek karmaşası bitsin; tek ölçü standardı olsun.

Segment uzunluğu: **100** (Z ekseni)

Önerilen standart ölçüler (local):
- Floor: Scale (6, 0.2, 100)
- Wall_Left:  Position (-2.9, 1.5, 0), Scale (0.2, 3, 100)
- Wall_Right: Position ( 2.9, 1.5, 0), Scale (0.2, 3, 100)
- Ceiling:    Position (0, 3, 0), Scale (6, 0.2, 100)

---

## 5) Camera (Critical)

- FOV: ~55 (40–60 bandında kal)
- Near: 0.1
- Far: **~140**  ✅ (pop / “koridor uzadı” hissini bitiren ana ayar)
- Not: FOV yanlışlıkla çok yükselirse (örn 146) ciddi distorsiyon ve “V gibi duvar” hissi yapar.

---

## 6) Infinite Corridor Loop (Current Working State)

Durum: ✅ Stabil / Confirmed

- Segment hizası bounds üzerinden düzgün.
- “Gap/boşluk” problemi çözüldü.
- Speed 50 testinde pop hissi yok (kamera Far ayarı sayesinde).

Loop script:
- `CorridorLoop.cs` (GameRoot üstünde)
- segments list: Corridor_01/02/(03)
- recycleBehind: ~20 (stabil test edildi)
- speed: normal test 6; stres testi 50

---

## 7) Atmosphere: Fog + Ceiling (DONE)

Fog (klasik RenderSettings/Lighting üzerinden):
- Mode: Linear
- Start: **5**
- End: **120**
- Color: #1B2430 (soğuk dungeon önerisi)

Ceiling:
- Her segmentte mevcut (Cube)
- Dungeon hissi + loop maskelenmesi sağlar.

URP Volume Override’da Fog görünmedi; klasik fog ile devam edildi (şimdilik yeterli).

---

## 8) Lighting: Torch Lights (DONE + NOTE)

Durum: ✅ İyi çalışıyor, loop sırasında “ışık kayboluyor” sorunu düzeltildi.

TorchLight’lar:
- Parent: Segment root (Corridor_01/02/03) ✅
- Tip: Point Light, Realtime
- Öneri değerler:
  - Intensity: ~1.6–2.0
  - Range: ~18
  - Color: #FFB36A
  - Shadows: performans için sınırlı sayıda açık tutulabilir

Önemli bug & çözüm:
- Segment sonuna çok yakın eklenen “4. ışıklar” (Z=95 civarı) kamera daha arkaya geçmeden recycle olunca “yakınlaşıp kayboluyor” gibi hissediyordu.
- Çözüm: 4. ışıkları devre dışı bırakıldı (veya ileride Z=85 gibi daha erkene çekilebilir).

Not:
- URP’de “Additional Lights Per Object Limit” ayarı denenmişti ancak kök sebep ışıkların recycle timing’i idi.

---

## 9) Snapshot Workflow (MANDATORY for Debug)

Kullanıcı Unity bilmiyor; tutorial yerine birlikte proje geliştirerek öğreniyor.
Bu nedenle “varsayım” yapılmaz; gerektiğinde snapshot alınır.

Snapshot tool:
- Script: `Assets/Scripts/Tools/SceneSnapshotExporter.cs`
- Menü: `Tools/BladeRift/Export FULL Snapshot (Scene+Project+Code)`
- Çıktı: `Assets/Docs/Snapshots/BladeRift_FULL_<SceneName>_<timestamp>.json`

Kural:
- Debug anında asistan “FULL snapshot at” der.
- Kullanıcı json’u sohbet’e yükler.
- Eğer exporter/.cs dosyası unutulursa kullanıcıdan yeniden istenir.

---

## 10) What Is Working (Confirmed)

- `Prototype_CombatCore` sahnesi var.
- 9:16 oran set.
- Infinite corridor akıyor ve stabil.
- Camera FOV/Far ayarıyla pop/jump hissi çözüldü.
- Ceiling + Fog ile dungeon atmosferi geldi.
- Torch lighting stabil (4. ışıklar devre dışı).

---

## 11) Known Issues (Open)

- Şu an “görsel stabilizasyon” tamamlandı.
- Açık kritik issue yok.

---

## 12) Next Targets (Next Session)

Combat’a giriş:

1) SwipeInput (touch + mouse unified) — continuous swipe
2) Weakpoint overlay UI (placeholder)
3) CombatDirector (flow: telegraph → window → success/fail)
4) Enemy placeholder prefab + health/damage
5) Hit feedback (hit stop / flash) + minimal VFX/SFX

---

## 13) Commit Hygiene Notes

- Değişikliklerden sonra:
  - Scene kaydet (Ctrl+S)
  - FULL snapshot export al (istersen commit öncesi arşiv)
- Tools klasörü ve exporter scripti projede kalıcı olmalı.