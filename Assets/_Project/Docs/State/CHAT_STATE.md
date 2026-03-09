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
- Current Scene Path: `Assets/_Project/Scenes/Prototype/Prototype_CombatCore.unity`
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

## 3) Mandatory Design / Architecture Context (Critical)

### Architecture Must Be Read
ChatState okunurken mutlaka aşağıdaki mimari doküman okunmalıdır:

- `Assets/_Project/Docs/Architecture/ARCHITECTURE_TR.md`

Bu doküman sistem mimarisinin ana doğruluk kaynağıdır.  
Script sorumlulukları, controller ayrımları ve teknik sınırlar burada tanımlıdır.

### Design Docs Must Be Read
ChatState okunurken mutlaka aşağıdaki tasarım dokümanları da okunmalıdır:

- `Assets/_Project/Docs/Design/GAME_CONCEPT_TR.md`
- `Assets/_Project/Docs/Design/GAME_RULES_TR.md`

Bu dokümanlar oyunun mekaniklerini, combat felsefesini ve feature sınırlarını tanımlar.

### No Drift Rule
Proje bu bağlamlardan sapmamalıdır.

- Yeni sistemler Architecture ile çelişmemelidir.
- Yeni mekanikler GAME_RULES ile çelişmemelidir.
- Yeni yorumlar GAME_CONCEPT’in hedef yönünü bozmaz.
- CHAT_STATE, bu dokümanların üstüne çıkmaz; onları yorumlayan pratik state dokümanıdır.

### Conflict Priority Order
Bir çelişki oluşursa öncelik sırası:

1. `ARCHITECTURE_TR.md`
2. `GAME_RULES_TR.md`
3. `GAME_CONCEPT_TR.md`
4. `CHAT_STATE.md`

---

## 4) Current Working State

### Environment
- Infinite corridor sistemi çalışıyor.
- Corridor loop stabilize edildi.
- Gap / pop / corridor stretching sorunları çözüldü.
- Ceiling eklendi.
- Fog eklendi.
- Torch lights eklendi.
- Işık loop problemi çözüldü.
- Koridor akışı ve ışık akışı stabil.
- Torch light shadow warning’i temizlemek için torch gölgeleri kapatıldı.

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
- Aynı press içinde çoklu yön commit çalışıyor.
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
- HUD artık raw input ve interpreter verisini gösterebiliyor.
- Debug text üzerinde görülen başlıca alanlar:
  - `CurrentDir`
  - `LastCommitted`
  - `AccumPx`
  - `CommittedThisPress`

### Combat
- `Assets/_Project/Scripts/Combat/CombatDirector.cs` oluşturuldu.
- `CombatDirector` component’i `GameRoot` üzerine eklendi.
- Swipe → CombatDirector bağlantısı çalışıyor.
- CombatDirector swipe log alıyor.
- Şu an minimal receiver iskeleti mevcut.
- Combat flow henüz nihai sisteme bağlanmadı.

### Enemy / Weakpoint Placeholder
- `EnemyRoot` oluşturuldu.
- `EnemyPlaceHolder` world-space quad olarak kuruldu.
- `BillboardFacing` ile kameraya dönük placeholder enemy çalışıyor.
- `WeakPoint` geçici placeholder sphere olarak eklendi.
- Enemy ve weakpoint materyalleri görünür/readable hale getirildi:
  - enemy = gri
  - weakpoint = kırmızı
- Mevcut weakpoint sphere, **nihai sistem değildir**; debug placeholder’dır.
- Doküman yönü gereği nihai weakpoint sistemi **UI overlay** tarafına taşınacaktır.

---

## 5) Current Technical Architecture

### Working Gameplay Stack
- 3D corridor environment
- SwipeInput
- SwipeInterpreter (segment-based chain)
- SwipeDebugHUD
- CombatDirector
- 2D billboard enemy placeholder
- Weakpoint placeholder
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

### Placeholder Rule
Debug amaçlı yapılan geçici çözümler:
- placeholder mesh
- temporary weakpoint sphere
- debug HUD
- testing scripts

nihai sistem olarak kabul edilmez.

Architecture / Design ile çelişen placeholder’lar kalıcılaştırılmaz.

---

## 6) Folder / Project Structure (Current Standard)

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
- `Assets/_Project/Scripts/UI`

### Script Placement Rule (Critical)
- Yeni scriptler **mevcut klasör mimarisine uygun** eklenmelidir.
- Klasör yapısı **tahmin edilmez**.
- Doğru klasör yolu gerektiğinde **latest snapshot’tan doğrulanır**.
- `Scripts` altına gelişi güzel dosya atılmaz.

### Unity Version Rule (Critical)

Unity yönlendirmeleri **Unity 6.3 LTS (6000.3.10f1)** menü yapısına göre yapılmalıdır.

- Eski Unity menü yolları kullanılmamalıdır.
- UI oluşturma yolu Unity 6.x standardına göre verilmelidir.
- Yanlış veya eski menü yönlendirmeleri debug süresini ciddi artırdığı için bu kural zorunludur.

---

## 7) Source of Truth Rules (Critical)

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
- Ancak yorumlama yapılırken mutlaka şu dokümanlarla birlikte okunur:
  - `ARCHITECTURE_TR.md`
  - `GAME_CONCEPT_TR.md`
  - `GAME_RULES_TR.md`
  - gerekirse `TODO_TR.md`
  - gerekirse `DEBUG_JOURNAL.md`
  - gerekirse `README.md`

### Architecture Authority Rule

CHAT_STATE mevcut çalışan prototipi anlatır.

Ancak sistem yönü belirlenirken aşağıdaki dokümanlar **nihai referans** kabul edilir:

- `ARCHITECTURE_TR.md`
- `GAME_RULES_TR.md`
- `GAME_CONCEPT_TR.md`

Eğer CHAT_STATE ile bu dokümanlar arasında çelişki oluşursa:

Architecture / Design docs önceliklidir.

---

## 8) BladeRift Workflow (Locked)

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

### Scene Safety Rule
- Eğer bir feature scene/component/reference wiring içeriyorsa:
  - scene kaydedilmelidir
  - ilgili `.unity` dosyası commit/push edilmelidir
- Kod + scene + snapshot hizalanmadan durum “güvende” sayılmaz.
- Scene-side değişiklikler yalnızca `.cs` push ile güvenli kabul edilmez.

### Pre-Implementation Doc Check Rule
Yeni bir sistem implement edilmeden önce mutlaka şu dokümanlar kontrol edilmelidir:

- `ARCHITECTURE_TR.md`
- `GAME_RULES_TR.md`
- `GAME_CONCEPT_TR.md`

Eğer dokümanlarda tanımlı bir sistem varsa, onunla çelişen yeni sistem doğrudan uydurulmaz.

### Scope Drift Prevention Rule
Yeni bir feature eklenmeden önce şu kontrol yapılır:

- combat loop’u destekliyor mu?
- corridor runner pacing’i bozuyor mu?
- swipe mechanic ile uyumlu mu?

Eğer cevap hayır ise feature reddedilir ya da ertelenir.

---

## 9) ProjectState DevTool (Current)

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

## 10) Main Pain Points Learned So Far

Projenin en çok vakit kaybettiren tarafları:

1. Kod gerçeği ile Unity scene gerçeğinin farklı olması
2. Aynı bug için tekrar tekrar kör denemeler yapılması
3. Yeni sohbette aynı bağlamda olup olmadığımızdan emin olamama
4. Eski debug denemelerinin yorumlanmış bilgiye dönüşmemesi
5. Snapshot’ların birikip gürültü oluşturması
6. Mevcut klasör yapısını bozan hızlı ama yanlış script yerleşimleri
7. Kaynaklardan doğrulanabilecek şeylerde gereksiz varsayım yapılması
8. Placeholder debug sistemlerinin nihai sistem sanılması
9. Scene save edilmeden ilerlenmesi
10. Architecture / Design docs okunmadan feature yönünün kayması

Bu yüzden mevcut sistem:
- GitHub + Snapshot + ChatState + DebugJournal + SnapshotCompare + Design Docs + Architecture Docs
üzerine kurulmuştur.

---

## 11) Documents Reading Priority

Yeni sohbet / context refresh sırasında öncelik sırası:

1. `CHAT_STATE.md`
2. `ARCHITECTURE_TR.md` **(zorunlu)**
3. `GAME_RULES_TR.md` **(zorunlu)**
4. `GAME_CONCEPT_TR.md` **(zorunlu)**
5. GitHub’daki latest pushed code
6. latest FULL snapshot (varsa / gerekirse)
7. `TODO_TR.md`
8. `DEBUG_JOURNAL.md`
9. `SNAPSHOT_COMPARE.md` (özellikle debug akışında)
10. `README.md`

Not:
- Architecture ve Design docs okunmadan sistem yönü belirlenmez.
- CHAT_STATE tek başına yeterli referans sayılmaz; mutlaka bu docs ile birlikte yorumlanır.

---

## 12) Current Next Step

Şu anda sıradaki ana gameplay milestone:

### Weakpoint System Direction Alignment
Yani:
- mevcut çalışan swipe chain çıktısını
- combat rules ile uyumlu weakpoint sistemine bağlamak
- placeholder world-space weakpoint’ten
- mimariye uygun **UI overlay weakpoint** sistemine geçmek

Yakın sonraki hedef:
- `WeakpointOverlayController`
- common enemy için düşük adımlı sequence
- elite enemy için daha uzun sequence
- swipe direction → weakpoint match → hit/miss flow

### Current Prototype Layer (Important)

Sahnede şu anda bulunan bazı scriptler **ara test katmanıdır**:

- `WeakpointDirectionView`
- `WeakpointCombatTest`

Bu scriptler swipe → direction doğrulaması için yazılmıştır.

Bu sistemler **nihai mimari değildir** ve yalnızca prototip test katmanı olarak kabul edilir.

---

## 13) Near-Term Goals

- Mevcut placeholder enemy/weakpoint görselliğini debug için okunur tutmak
- World-space weakpoint’ten UI overlay weakpoint sistemine geçmek
- SwipeInterpreter ile CombatDirector bağlantısını rule-based hit kontrolüne çevirmek
- Common düşman için 1–2 step test akışı kurmak
- Elite düşman için 3–4 step test akışı kurmak
- İlk gerçek combat flow prototipini ayağa kaldırmak

### Target Combat Architecture (Docs-Aligned)

Nihai combat mimarisi aşağıdaki modüler akışa göre kurulacaktır:

SwipeInput  
↓  
WeakpointOverlayController  
↓  
WeakpointSequence  
↓  
CombatDirector  
↓  
FeedbackController

Açıklama:

WeakpointOverlayController  
→ düşman üzerinde UI overlay weakpoint'leri oluşturur

WeakpointSequence  
→ telegraph phase sırasında weakpoint zincirini sırayla aktive eder

CombatDirector  
→ execution window sırasında swipe inputlarını değerlendirir  
→ hit / fail kararını verir

FeedbackController  
→ hit VFX / slash / feedback üretir

---

## 14) Long-Term Goals

- güçlü combat hissi
- stylized dungeon presentation
- hızlı iteration
- paylaşılabilir APK
- debug süresini ciddi biçimde azaltan reusable dev toolset
- docs ile sürekli hizalı kalan sistem geliştirme akışı

---

## 15) Notes

- Kod tarafında kaynak: **GitHub pushed state**
- Scene/debug kaynak: **latest snapshot**
- Snapshot compare artık aktif bir debug aracı
- Aynı bug için aynı denemeleri tekrar etmemek amacıyla:
  - `DEBUG_JOURNAL.md`
  - `SNAPSHOT_COMPARE.md`
  - `CHAT_STATE.md`
  düzenli kullanılmalı
- Şu an combat input için hedef sistem:
  - **4 yön**
  - **press içinde chain**
  - **finger lift ile reset**

<!-- AUTOGENERATED - DO NOT EDIT BELOW THIS LINE -->

*Son guncelleme: 2026-03-09 16:34:04*

## AUTO: Last Snapshot
- Kind: DEBUG
- Date: 2026-03-09 16:34:04
- Scene: Prototype_CombatCore
- Commit: 5d7675f — "DevTool upgrade"
- Root objects: 7
- Total objects: 53

## AUTO: Script Files
- Assets\_Project\Core\WorldScroller.cs
- Assets\_Project\Scripts\Combat\CombatDirector.cs
- Assets\_Project\Scripts\Combat\WeakpointCombatTest.cs
- Assets\_Project\Scripts\Core\CorridorLoop.cs
- Assets\_Project\Scripts\Input\SwipeDebugHUD.cs
- Assets\_Project\Scripts\Input\SwipeInput.cs
- Assets\_Project\Scripts\Input\SwipeInterpreter.cs
- Assets\_Project\Scripts\Tools\BillboardFacing.cs
- Assets\_Project\Scripts\UI\WeakpointDirectionView.cs
- Assets\_Project\Scripts\UI\WeakpointMarkerController.cs

## AUTO: Scene Hierarchy (Summary)
- GameRoot [CombatDirector, CorridorLoop, WeakpointMarkerController, WeakpointCombatTest]
  - Corridor_01
    - Wall_Left [MeshFilter, MeshRenderer, BoxCollider]
    - Wall_Right [MeshFilter, MeshRenderer, BoxCollider]
    - Floor [MeshFilter, MeshRenderer, BoxCollider]
    - Ceiling [MeshFilter, MeshRenderer, BoxCollider]
    - TorchLight_Left01 [Light, UniversalAdditionalLightData]
    - TorchLight_Left02 [Light, UniversalAdditionalLightData]
    - TorchLight_Left03 [Light, UniversalAdditionalLightData]
    - TorchLight_Left04 [inactive] [Light, UniversalAdditionalLightData]
    - TorchLight_Right01 [Light, UniversalAdditionalLightData]
    - TorchLight_Right02 [Light, UniversalAdditionalLightData]
    - TorchLight_Right03 [Light, UniversalAdditionalLightData]
    - TorchLight_Right04 [inactive] [Light, UniversalAdditionalLightData]
  - Corridor_02
    - Wall_Left [MeshFilter, MeshRenderer, BoxCollider]
    - Wall_Right [MeshFilter, MeshRenderer, BoxCollider]
    - Floor [MeshFilter, MeshRenderer, BoxCollider]
    - Ceiling [MeshFilter, MeshRenderer, BoxCollider]
    - TorchLight_Left01 [Light, UniversalAdditionalLightData]
    - TorchLight_Left02 [Light, UniversalAdditionalLightData]
    - TorchLight_Left03 [Light, UniversalAdditionalLightData]
    - TorchLight_Left04 [inactive] [Light, UniversalAdditionalLightData]
    - TorchLight_Right01 [Light, UniversalAdditionalLightData]
    - TorchLight_Right02 [Light, UniversalAdditionalLightData]
    - TorchLight_Right03 [Light, UniversalAdditionalLightData]
    - TorchLight_Right04 [inactive] [Light, UniversalAdditionalLightData]
  - Corridor_03
    - Wall_Left [MeshFilter, MeshRenderer, BoxCollider]
    - Wall_Right [MeshFilter, MeshRenderer, BoxCollider]
    - Floor [MeshFilter, MeshRenderer, BoxCollider]
    - Ceiling [MeshFilter, MeshRenderer, BoxCollider]
    - TorchLight_Left01 [Light, UniversalAdditionalLightData]
    - TorchLight_Left02 [Light, UniversalAdditionalLightData]
    - TorchLight_Left03 [Light, UniversalAdditionalLightData]
    - TorchLight_Left04 [inactive] [Light, UniversalAdditionalLightData]
    - TorchLight_Right01 [Light, UniversalAdditionalLightData]
    - TorchLight_Right02 [Light, UniversalAdditionalLightData]
    - TorchLight_Right03 [Light, UniversalAdditionalLightData]
    - TorchLight_Right04 [inactive] [Light, UniversalAdditionalLightData]
  - InputRoot [SwipeInput, SwipeInterpreter, SwipeDebugHUD]
  - EnemyRoot
    - EnemyPlaceHolder [MeshFilter, MeshRenderer, BillboardFacing]
- EventSystem [EventSystem, InputSystemUIInputModule]
- Main Camera [Camera, AudioListener, UniversalAdditionalCameraData]
- Directional Light [Light, UniversalAdditionalLightData]
- Player_Reference [inactive] [MeshFilter, MeshRenderer, BoxCollider]
- Global Volume [Volume]
- UIRoot [Canvas, CanvasScaler, GraphicRaycaster]
  - DebugSwipeText [CanvasRenderer, TextMeshProUGUI]
  - WeakpointMarkerRoot [WeakpointDirectionView]
    - WeakpointMarker [CanvasRenderer, Image, CanvasGroup]

## AUTO: Scene Metrics
- Renderers: 13
- Lights: 19
- Triangles: 146
- Vertices: 292
- MeshFilters: 13
- ParticleSystems: 0
- Animators: 0
- Canvases: 1

## AUTO: Todo
Open: 59 | Done: 34

  **Input / Combat**
  - [ ] `SwipeInterpreter` çıkışını `CombatDirector` receiver’a bağla
  - [ ] Swipe direction geldiğinde `CombatDirector` log/receiver testini doğrula
  - [ ] Finger lift = chain reset davranışını yeni chain yapısında test et
  - [ ] Aynı yön spam engelini hissiyat açısından değerlendir
  - [ ] Debug text’e gerekirse combat receiver sonucu da yazdır
  **Combat Placeholder**
  - [ ] Basit enemy placeholder oluştur
  - [ ] 2D billboard enemy yaklaşımını sahnede test et
  - [ ] Enemy’nin kameraya dönük kalmasını sağla
  - [ ] Distance / scale büyüme mantığını test et
  **Weakpoint Placeholder**
  - [ ] Basit weakpoint placeholder sistemi tasarla
  - [ ] Zayıf nokta görseli için placeholder sprite / indicator ekle
  - [ ] Doğru swipe yönü ile weakpoint eşleşme testini yap
  - [ ] Zig-zag weakpoint pattern’lerini 4 yön sistemiyle dene
  **Weakpoint Overlay System (Next Major Step)**
  - [ ] `WeakpointOverlayController` oluştur
  - [ ] Weakpoint'leri UI overlay olarak üret
  ... (daha fazlasi var)

  Son tamamlananlar:
  - [x] Camera FOV değişimi compare raporunda doğru yakalandı
  - [x] Working vs Debug snapshot compare doğrulandı
  - [x] Readable snapshot diff raporu (v4) tamamlandı
  - [x] ProjectState DevTool v3 compare sistemi tamamlandı
  - [x] Snapshot archive / cleanup sistemi tamamlandı

## AUTO: Milestones
- (Henuz milestone yok)

<!-- END AUTOGENERATED -->

