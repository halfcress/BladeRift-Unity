# CHAT_STATE (Single Source of Truth)

> Bu dosya: “Bugün en son nerede kaldık?” sorusunun tek cevabıdır.
> Yeni sohbete başlarken ilk referans bu dosyadır.
> Varsayım yapılmaz. Kod, sahne ve debug durumu aşağıdaki kaynaklardan birlikte okunur.

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
- Screen Target: 9:16 (1080x1920)

---

## 2) Core Direction (Locked)

- Perspektif: **First-person**
- Hareket hissi: **Oyuncu sabit, dünya üstüne akar**
- Ana tema: **Stylized dungeon combat**
- Ortam: **3D corridor**
- Düşman yaklaşımı: **2D billboard / sprite enemy**
- Combat yaklaşımı: **Swipe-driven weakpoint combat**
- Ana kural: **Finger lift = chain reset**
- Combat input yönleri: **4 yön (Left / Right / Up / Down)**
- Çapraz yönler: **şimdilik yok**
- Chain mantığı: **tek press içinde multi-commit chain var**
- v0.1 hedefi: **Paylaşılabilir combat prototype / APK**

---

## 3) Current Working State

### Environment
- Infinite corridor sistemi çalışıyor.
- Corridor loop stabilize edildi.
- Gap / pop / corridor stretching sorunları çözüldü.
- Ceiling eklendi.
- Fog eklendi.
- Torch lights eklendi.
- Işık loop problemi çözüldü.
- Koridor akışı ve ışık akışı stabil.

### Camera
- Kamera değerleri ve gerçek sahne durumu gerektiğinde latest snapshot’tan okunmalı.
- Kamera/FOV konusunda markdown yerine snapshot gerçeği esas alınmalı.
- Aşırı FOV değerlerinden kaçınılacak.

### Input
- `SwipeInput` sistemi kuruldu.
- Mouse + touch input okunuyor.
- Debug text ile şu veriler ekranda görülüyor:
  - `IsDown`
  - `DeltaPx`
  - `DeltaNormalized`

### Swipe Interpretation
- `SwipeInterpreter.cs` oluşturuldu.
- İlk tek-commit-per-press yaklaşımı denendi.
- Daha sonra bu yapı **segment-based chain** mantığına revize edildi.
- Aynı press içinde çoklu yön commit artık çalışıyor.
- Yönler:
  - `Left`
  - `Right`
  - `Up`
  - `Down`
- Şu an diagonal / 8 yön desteği yok.
- Aynı press içinde zig-zag chain mantığı hedefleniyor.
- İlk whole-press accumulator bug’ı tespit edildi ve segment-based yaklaşıma geçildi.

### Debug HUD
- `SwipeDebugHUD.cs` güncellendi.
- HUD artık yalnızca raw input değil, interpreter verisini de gösterebiliyor.
- Debug text üzerinde görülen başlıca alanlar:
  - `CurrentDir`
  - `LastCommitted`
  - `AccumPx`
  - `CommittedThisPress`

### Combat
- `Assets/_Project/Scripts/Combat/CombatDirector.cs` oluşturuldu.
- `CombatDirector` component’i `GameRoot` üzerine eklendi.
- Şu an minimal receiver iskeleti mevcut.
- Combat flow henüz tam bağlanmadı.
- Swipe → CombatDirector bağlantısının sahnede gerçek testine bir sonraki adımda devam edilecek.

---

## 4) Current Technical Architecture

### Working Gameplay Stack
- 3D corridor environment
- SwipeInput
- SwipeInterpreter (segment-based chain)
- SwipeDebugHUD
- CombatDirector
- 2D billboard enemy yaklaşımı
- Weakpoint system (henüz placeholder aşamasında)
- GitHub source-of-truth workflow
- Snapshot / compare / debug workflow

### Selected Enemy Direction
- Düşmanlar gerçek 3D karakter olmak zorunda değil.
- Seçilen yaklaşım:
  - 3D corridor
  - 2D sprite / billboard enemy
  - yaklaşırken scale büyümesi
  - stylized okunabilir combat

Bu karar özellikle üretim hızını, okunabilirliği ve solo dev sürdürülebilirliğini artırmak için alındı.

---

## 5) Folder / Project Structure (Current Standard)

Unity project root repo root’tadır.

Ana oyun içeriği şu yapı altında tutulur:

- `Assets/_Project/Art`
- `Assets/_Project/Audio`
- `Assets/_Project/Docs`
- `Assets/_Project/Prefabs`
- `Assets/_Project/Scenes`
- `Assets/_Project/ScriptableObjects`
- `Assets/_Project/Scripts`
- `Assets/_Project/Settings`
- `Assets/_Project/UI`
- `Assets/_Project/VFX`

Docs yapısı:

- `Assets/_Project/Docs/State`
- `Assets/_Project/Docs/Design`
- `Assets/_Project/Docs/Architecture`
- `Assets/_Project/Docs/Snapshots/Working`
- `Assets/_Project/Docs/Snapshots/Debug`
- `Assets/_Project/Docs/Snapshots/Archive`

### Scripts Folder (Verified Standard)
Mevcut script klasör yapısı latest snapshot’tan doğrulanarak kullanılmalıdır.

- `Assets/_Project/Scripts/Core`
- `Assets/_Project/Scripts/Input`
- `Assets/_Project/Scripts/Combat`
- `Assets/_Project/Scripts/Tools`
- `Assets/_Project/Scripts/Tools/ProjectState`

### Script Placement Rule (Critical)
- Yeni scriptler **mevcut klasör mimarisine uygun** eklenmelidir.
- Klasör yapısı **tahmin edilmez**.
- Doğru klasör yolu gerektiğinde **latest snapshot’tan doğrulanır**.
- `Scripts` altına gelişi güzel dosya atılmaz.

---

## 6) Source of Truth Rules (Critical)

### Code
- **GitHub repo is source of truth for pushed code**
- Default repo URL:
  - `https://github.com/halfcress/BladeRift-Unity`
- Kullanıcı her anlamlı kod değişikliğinden sonra commit + push yapar.
- Aynı repo linki yeniden istenmez.
- Kod inceleme gerektiğinde önce GitHub’daki güncel repo kullanılır.
- Kod pushlandıysa ayrıca `.cs` dosyası istemek default davranış olmamalı.

### Scene / Runtime / Debug State
- **FULL snapshot is source of truth for scene/runtime state**
- Snapshot yalnızca gerektiğinde istenir.
- Snapshot özellikle şu durumlarda gerekir:
  - scene hierarchy
  - inspector değerleri
  - transform / prefab / runtime state
  - Unity tarafında kod dışı problemler
- Snapshot ile doğrulanabilecek bilgi kullanıcıya tekrar kontrol ettirilmez.

### Human-readable State
- **CHAT_STATE is interpreted project truth**
- `TODO_TR.md`, `DEBUG_JOURNAL.md`, `GAME_CONCEPT_TR.md`, `GAME_RULES_TR.md`, `ARCHITECTURE_TR.md`, `README.md`
  gerektiğinde birlikte değerlendirilir.

---

## 7) BladeRift Workflow (Locked)

Kullanıcı şu cümleyi söylediğinde:

**“hey zibidi, bladerift önbelleğini güncelle”**

şu varsayılır:

1. Son kod GitHub’a pushlandı
2. Kullanıcı son dump’ı paylaşacaktır
3. Dump’ın çalışma durumu kullanıcı tarafından belirtilecektir:
   - working state
   - debug state
4. CHAT_STATE ve gerekirse TODO / diğer markdownlar birlikte değerlendirilir
5. Proje hafızası yeni duruma göre hizalanır

### Default Rule
- Kod için önce GitHub’a bak
- Scene/debug için gerekirse snapshot iste
- Aynı şeyi tekrar tekrar deneme
- Başarısız debug yollarını `DEBUG_JOURNAL.md` veya state içinde kaydet

### No-Assumption Rule
- Proje durumu kaynaklardan doğrulanabiliyorsa varsayım yapılmaz.
- Snapshot veya GitHub’dan edinilebilecek bilgi için kullanıcıya “kontrol et” denmez.
- Önce kaynaklar okunur, sonra yönlendirme yapılır.

### Large Code Delivery Rule
- 30 satırı geçen kodlar chat içine düz metin olarak yapıştırılmaz.
- Bu tür kodlar doğrudan `.cs` dosyası olarak verilir.

---

## 8) ProjectState DevTool (Current)

Tool folder:
- `Assets/_Project/Scripts/Tools/ProjectState/`

Main files:
- `ProjectStateExporter.cs`
- `ProjectStatePaths.cs`
- `ProjectStateModels.cs`
- `ProjectStateGit.cs`
- `ProjectStateIndex.cs`
- `ProjectStateJournal.cs`
- `ProjectStateSerializer.cs`
- `ProjectStateCompare.cs`

Menu:
- `Tools > BladeRift > Project State > Export WORKING Snapshot`
- `Tools > BladeRift > Project State > Export DEBUG Snapshot`
- `Tools > BladeRift > Project State > Cleanup Snapshots`
- `Tools > BladeRift > Project State > Append DEBUG_JOURNAL Entry`
- `Tools > BladeRift > Project State > Update Snapshot Index`
- `Tools > BladeRift > Project State > Open Docs Folder`
- `Tools > BladeRift > Project State > Compare Latest Working vs Debug`

State outputs:
- `Assets/_Project/Docs/State/CHAT_STATE.md`
- `Assets/_Project/Docs/State/DEBUG_JOURNAL.md`
- `Assets/_Project/Docs/State/SNAPSHOT_INDEX.md`
- `Assets/_Project/Docs/State/SNAPSHOT_COMPARE.md`

Snapshot outputs:
- `Assets/_Project/Docs/Snapshots/Working`
- `Assets/_Project/Docs/Snapshots/Debug`
- `Assets/_Project/Docs/Snapshots/Archive`

### Current DevTool Capabilities
- Working snapshot export
- Debug snapshot export
- Snapshot archive / cleanup
- Snapshot index generation
- Manual debug journal append
- Snapshot compare report
- Working vs Debug diff reporting
- Git metadata capture inside snapshot

---

## 9) Main Pain Points Learned So Far

Projenin en çok vakit kaybettiren tarafları:

1. Kod gerçeği ile Unity scene gerçeğinin farklı olması
2. Aynı bug için tekrar tekrar kör denemeler yapılması
3. Yeni sohbette aynı bağlamda olup olmadığımızdan emin olamama
4. Eski debug denemelerinin yorumlanmış bilgiye dönüşmemesi
5. Snapshot’ların birikip gürültü oluşturması
6. Mevcut klasör yapısını bozan hızlı ama yanlış script yerleşimleri
7. Kaynaklardan doğrulanabilecek şeylerde gereksiz varsayım yapılması

Bu yüzden mevcut sistem:
- GitHub + Snapshot + ChatState + DebugJournal + SnapshotCompare
üzerine kurulmuştur.

---

## 10) Documents Reading Priority

Yeni sohbet / context refresh sırasında öncelik sırası:

1. `CHAT_STATE.md`
2. GitHub’daki latest pushed code
3. latest FULL snapshot (varsa / gerekirse)
4. `TODO_TR.md`
5. `DEBUG_JOURNAL.md`
6. `SNAPSHOT_COMPARE.md` (özellikle debug akışında)

Tasarım kararı gerekiyorsa:
7. `GAME_CONCEPT_TR.md`
8. `GAME_RULES_TR.md`
9. `ARCHITECTURE_TR.md`
10. `README.md`

---

## 11) Current Next Step

Şu anda sıradaki ana gameplay milestone:

### Swipe Chain → CombatDirector Connection
Yani:
- çalışan `SwipeInterpreter` chain çıktısını
- `CombatDirector` içine gerçek receiver ile bağlamak
- swipe yönü geldiğinde combat akışını başlatmak

Yakın sonraki hedef:
- `SwipeInterpreter` → `CombatDirector`
- ardından weakpoint placeholder katmanı

---

## 12) Near-Term Goals

- SwipeInterpreter ile CombatDirector bağlantısını gerçek test etmek
- Swipe direction’i combat event’e dönüştürmek
- Weakpoint placeholder sistemi kurmak
- Enemy placeholder (2D billboard) oluşturmak
- İlk combat flow prototipini ayağa kaldırmak

---

## 13) Long-Term Goals

- güçlü combat hissi
- stylized dungeon presentation
- hızlı iteration
- paylaşılabilir APK
- debug süresini ciddi biçimde azaltan reusable dev toolset

---

## 14) Notes

- Kullanıcı Unity’yi sıfırdan öğreniyor; her şey adım adım anlatılmalı
- Tek seferde çok adım verilmemeli
- Tutorial yerine proje geliştirerek öğrenme yaklaşımı kullanılıyor
- Gereksiz debug döngülerinden kaçınılmalı
- Varsayım yerine mümkün olduğunca:
  - GitHub code
  - latest snapshot
  - CHAT_STATE
üçlüsüne dayanılmalı
- Kod pushlandıysa ayrıca `.cs` dosyası istemek default davranış olmamalı
- 30+ satırlık kod gerekiyorsa chat’e yapıştırmak yerine dosya verilmelidir