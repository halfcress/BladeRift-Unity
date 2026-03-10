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
- Unity Version: 6000.3.10f1 LTS
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

## 4) Şu An Doğrulanan Çalışan Kısımlar

### Environment
- Infinite corridor sistemi çalışıyor ve stabil görünüyor
- Ceiling, fog ve torch light yapısı sahnede mevcut

### Core Combat Loop (Kısmi doğrulandı)
Console çıktısına göre aşağıdakiler çalışıyor:
- combat test tetikleyicisi zinciri başlatıyor
- telegraph fazı açılıyor
- execution window açılıyor
- ilk hit kayıt oluyor
- ikinci hit kayıt oluyor
- combo artışı log'a düşüyor
- timeout sonucu fail akışı çalışıyor

### UI / Marker Temeli
- marker gösterimi mevcut
- combo / hit sayaçlarının debug amaçlı kullanımı sahnede mantıklı

---

## 5) Mevcut Kod ile Yeni Tasarım Arasındaki Farklar

Şu anki implementasyon ile kilitlenen yeni tasarım arasında açık farklar var:

- mevcut kodda yön filtresi kalıntıları bulunuyor
- execution fail mantığı yeni kuralla tamamen hizalı değil
- finger lift kuralı yeni tasarıma göre yeniden uygulanmalı
- active weakpoint progression yeni kurala göre sadeleştirilmeli
- punish sonrası pattern öğretme tekrar etmeyecek şekilde akış kurulmalı
- rage davranışı yeni kurala göre ayrıştırılmalı

Kısacası:
**mevcut sahne tamamen bozuk değil, ancak yeni locked combat tasarımıyla tam hizalı değil.**

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

Scope dışında / ileri faz:
- çoklu aktif düşman
- full spawn director
- chapter pacing sistemi
- tam elite/boss davranış seti
- gelişmiş VFX / polish
- final mobile optimization

---

## 7) Sıradaki İşler

En yakın işler:

1. Combat sistemini yeni locked rules'a hizalamak
2. Yön bazlı kalıntıları temizlemek
3. First-touch lock + finger lift fail mantığını uygulamak
4. Telegraph -> execution -> punish -> retry akışını yeni kurala göre düzeltmek
5. Enemy yaklaşma + death / punish / restart loop'unu oturtmak
6. Feedback katmanını ayrı ve net kurmak
7. Rage davranışını yeni kurala göre uygulamak

---

## 8) Ana Aktif Riskler

- Eski combat mantığının kodda kalıntı bırakması
- Yeni kuralların birden fazla dosyada tekrar yazılıp tekrar çelişki üretmesi
- AI ile yeni kod yazılırken architecture dışına taşılması
- Debug için eklenen geçici logic'in kalıcı hale gelmesi

---

## 9) Workflow Rules

- Kod yazarken önce `GAME_RULES_TR.md` ve `ARCHITECTURE_TR.md` kontrol edilir
- `CHAT_STATE.md` tasarım kuralı üretmez; yalnızca mevcut ilerlemeyi özetler
- Snapshot / console / sahne durumu gerektiğinde doğrulama kaynağı olarak kullanılır
- Uzun vadeli tasarım kararları `GAME_RULES` veya `GAME_CONCEPT` içine işlenir
- Teknik rol/sınır değişiklikleri `ARCHITECTURE` içinde tutulur

---

## 10) Kısa Durum Özeti

Şu an proje:
- sahne ve görsel prototip olarak ayakta
- combat temel akışı kısmen çalışır halde
- ama yeni locked combat tasarımına göre refactor / hizalama gerektiriyor

Bu dosyanın amacı:
**"Bugün fiilen nerede kaldık?"** sorusuna kısa ve net cevap vermektir.
