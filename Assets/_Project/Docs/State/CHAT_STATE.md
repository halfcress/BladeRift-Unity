# CHAT_STATE (Current Progress Truth)

> Bu dosya yalnızca güncel çalışma durumunu tutar.
> Oyun kuralları için `GAME_RULES_TR.md`
> Teknik yapı için `ARCHITECTURE_TR.md`
> Ürün vizyonu için `GAME_CONCEPT_TR.md` okunur.

---

## 1) Project Info

- Project Name: BladeRift
- Repo: BladeRift-Unity
- Repo URL: `https://github.com/halfcress/BladeRift-Unity`
- Unity Version: `6000.3.10f1 LTS`
- Template: Universal 3D (URP)
- Target Platform: Android (mobile-first)
- Current Main Test Platform: PC
- Current Scene: `Prototype_CombatCore`
- Current Scene Path: `Assets/_Project/Scenes/Prototype/Prototype_CombatCore.unity`
- Screen Target: `9:16 (1080x1920)`

---

## 2) Doküman Hiyerarşisi

Çelişki halinde öncelik sırası:

1. `GAME_RULES_TR.md`
2. `ARCHITECTURE_TR.md`
3. `GAME_CONCEPT_TR.md`
4. `CHAT_STATE.md`

Rol dağılımı:
- **Gameplay truth** → `GAME_RULES_TR.md`
- **Technical truth** → `ARCHITECTURE_TR.md`
- **Product vision truth** → `GAME_CONCEPT_TR.md`
- **Current progress truth** → `CHAT_STATE.md`

---

## 3) Core Direction (Locked)

- Perspektif: **First-person**
- Hareket hissi: **Oyuncu sabit, dünya üstüne akar**
- Ana tema: **Stylized dungeon combat**
- Ortam: **3D corridor**
- Düşman yaklaşımı: **2D billboard / sprite enemy**
- v0.1 hedefi: **Paylaşılabilir combat prototype / APK**

Combat kural detayları bu dosyada tekrar edilmez.
Tam combat kuralı için `GAME_RULES_TR.md` esas alınır.

---

## 4) Şu An Doğrulanan / Bilinen Durum

### Dokümanlar
- Canonical doküman yapısı temizlendi
- `START_HERE`, `GAME_RULES`, `ARCHITECTURE`, `CHAT_STATE`, `TODO` rolleri ayrıştırıldı
- Yeni chat akışı için navigator mantığı kuruldu

### Combat Implementasyonu
- Mevcut sahne tamamen bozuk değil
- Eski combat akışının bazı parçaları çalışıyor
- Ama implementasyon hâlâ tam olarak yeni locked target-sequence tasarımına hizalanmış değil

### Scene / Prototype
- Infinite corridor temeli mevcut
- Enemy placeholder yaklaşımı mevcut
- Marker / combo / hit sayaçları debug açısından kullanılabilir durumda

### DevTool / Snapshot
- Snapshot workflow aktif kullanılıyor
- MINI / FULL snapshot yaklaşımı devtool tarafında geliştiriliyor
- Ama tooling tarafı ayrı iş kalemi olarak takip edilmeli

---

## 5) Mevcut Açık Farklar

Mevcut implementasyon ile locked design arasında açık farklar var:

- direction tabanlı kalıntılar tamamen temizlenmemiş olabilir
- execution akışı eski mantık parçaları taşıyor olabilir
- first-touch lock + finger lift fail tam hizalı olmayabilir
- punish sonrası retry loop yeni kurala göre yeniden kurulmalı
- rage davranışı yeni kuralla birebir kontrol edilmeli

Kısacası:
**design locked, implementation hâlâ hizalanıyor.**

---

## 6) Current Prototype Scope

Prototype için aktif scope:
- tek aktif düşman
- yaklaşan düşman hissi
- pattern öğretme
- execution
- combo
- rage
- punish akışı
- feedback temeli
- snapshot / debug tooling desteği

Scope dışında / ileri faz:
- çoklu aktif düşman
- full spawn director
- chapter pacing sistemi
- tam elite / boss davranış seti
- gelişmiş VFX / polish
- final mobile optimization

---

## 7) Şu Anki Aktif İşler

Ana aktif işler:
1. Combat sistemini yeni locked rules'a hizalamak
2. Eski yön bazlı kalıntıları temizlemek
3. First-touch lock + finger lift fail mantığını gerçek koda geçirmek
4. Telegraph -> execution -> punish -> retry akışını yeni kurala göre düzeltmek
5. Enemy approach / death / retry loop'u oturtmak
6. Feedback katmanını ayrı ve net kurmak
7. Rage davranışını yeni kurala göre uygulamak
8. DevTool snapshot akışını MINI / FULL yaklaşımıyla sadeleştirmek

---

## 8) Aktif Riskler

- Eski combat mantığının kodda kalıntı bırakması
- Yeni kuralların tekrar dağınık hale gelmesi
- AI ile yeni kod yazılırken architecture dışına taşılması
- Debug için eklenen geçici logic'in kalıcı hale gelmesi
- Dokümanların güncellenmeden unutulması
- Tooling değişikliklerinin TODO / CHAT_STATE'e işlenmemesi

---

## 9) Workflow Rules

- Kod yazarken önce `GAME_RULES_TR.md` ve `ARCHITECTURE_TR.md` kontrol edilir
- `CHAT_STATE.md` tasarım kuralı üretmez; yalnızca mevcut ilerlemeyi özetler
- Snapshot, console ve sahne durumu gerektiğinde doğrulama kaynağı olarak kullanılır
- Uzun vadeli tasarım kararları `GAME_RULES` veya `GAME_CONCEPT` içine işlenir
- Teknik rol/sınır değişiklikleri `ARCHITECTURE` içinde tutulur
- Doküman güncellemesi gerekiyorsa sessizce geçilmez, kullanıcıya söylenir

---

## 10) Kısa Durum Özeti

Şu an proje:
- sahne ve görsel prototip olarak ayakta
- combat temel akışı kısmen çalışır halde
- ama yeni locked combat tasarımına göre refactor / hizalama gerektiriyor
- snapshot / devtool tarafı da aktif geliştiriliyor

Bu dosyanın amacı:
**"Bugün fiilen nerede kaldık?"** sorusuna kısa ve net cevap vermektir.