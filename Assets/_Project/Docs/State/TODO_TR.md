# TODO_TR

> Bu dosya aktif iş listesidir.
> Biten işler burada tutulabilir ama odak güncel ve sıradaki işlerde olmalıdır.

---

## Aktif Faz

**Faz:** Combat Prototype Foundation

Ana amaç:
- Swipe input’u gerçek combat yönlerine çevirmek
- Combat flow’un ilk oynanabilir prototipini kurmak
- Düşman yaklaşımını 2D billboard sistemine oturtmak

---

## Tamamlananlar

- [x] Prototype_CombatCore sahnesi oluşturuldu
- [x] First-person karar kilitlendi
- [x] Infinite corridor sistemi kuruldu
- [x] Corridor loop stabilize edildi
- [x] Gap / pop / uzama görsel sorunları çözüldü
- [x] Ceiling eklendi
- [x] Fog eklendi
- [x] Torch lighting eklendi
- [x] Light loop problemi çözüldü
- [x] SwipeInput sistemi kuruldu
- [x] Mouse + touch input okunuyor
- [x] Debug text ile input değerleri görüntüleniyor
- [x] `SwipeInterpreter.cs` oluşturuldu
- [x] Swipe yönü `Left / Right / Up / Down` olarak yorumlanıyor
- [x] İlk single-commit-per-press sürümü test edildi
- [x] Whole-press accumulator bug’ı tespit edildi
- [x] `SwipeInterpreter.cs` segment-based chain mantığına revize edildi
- [x] Aynı press içinde chain commit çalışıyor
- [x] `SwipeDebugHUD.cs` interpreter bilgilerini gösterecek şekilde güncellendi
- [x] Debug text üzerinde `CurrentDir / LastCommitted / AccumPx / CommittedThisPress` doğrulandı
- [x] `Assets/_Project/Scripts/Combat` klasörü oluşturuldu
- [x] `CombatDirector.cs` oluşturuldu
- [x] `CombatDirector` GameRoot üzerine eklendi
- [x] Repo root temiz Unity yapısına getirildi
- [x] `_Project` merkezli klasör yapısı oluşturuldu
- [x] ProjectState DevTool v2 tamamlandı
- [x] Working / Debug snapshot ayrımı tamamlandı
- [x] Snapshot index sistemi tamamlandı
- [x] Debug journal sistemi tamamlandı
- [x] Snapshot archive / cleanup sistemi tamamlandı
- [x] ProjectState DevTool v3 compare sistemi tamamlandı
- [x] Readable snapshot diff raporu (v4) tamamlandı
- [x] Working vs Debug snapshot compare doğrulandı
- [x] Camera FOV değişimi compare raporunda doğru yakalandı

---

## Şu An Yapılacaklar (En Yakın)

### Input / Combat
- [ ] `SwipeInterpreter` çıkışını `CombatDirector` receiver’a bağla
- [ ] Swipe direction geldiğinde `CombatDirector` log/receiver testini doğrula
- [ ] Finger lift = chain reset davranışını yeni chain yapısında test et
- [ ] Aynı yön spam engelini hissiyat açısından değerlendir
- [ ] Debug text’e gerekirse combat receiver sonucu da yazdır

### Combat Placeholder
- [ ] Basit enemy placeholder oluştur
- [ ] 2D billboard enemy yaklaşımını sahnede test et
- [ ] Enemy’nin kameraya dönük kalmasını sağla
- [ ] Distance / scale büyüme mantığını test et

### Weakpoint Placeholder
- [ ] Basit weakpoint placeholder sistemi tasarla
- [ ] Zayıf nokta görseli için placeholder sprite / indicator ekle
- [ ] Doğru swipe yönü ile weakpoint eşleşme testini yap
- [ ] Zig-zag weakpoint pattern’lerini 4 yön sistemiyle dene

---

## Bir Sonraki Katman

### Combat Flow
- [ ] Telegraph → execution window akışını başlat
- [ ] Success / fail durumlarını CombatDirector içine oturt
- [ ] Chain sequence mantığını gerçek combat akışında test et
- [ ] Combat state geçişlerini netleştir

### Feedback
- [ ] Basit slash feedback ekle
- [ ] Basit hit VFX ekle
- [ ] Basit blood particle placeholder ekle
- [ ] Swipe hit sonrası görsel tepki ver

---

## Tooling / Workflow

### DevTool Next Ideas
- [ ] Auto DEBUG snapshot → auto journal entry bağlantısını aktif et
- [ ] Snapshot compare summary polish
- [ ] Compare raporunda `Camera.Camera.fieldOfView` gibi tekrarları temizle
- [ ] Snapshot compare için daha iyi section counters
- [ ] Snapshot diff viewer editor window fikrini değerlendir
- [ ] Oto-MD updater V1 tasarımı yap

### Docs
- [ ] `CHAT_STATE.md` düzenli güncelle
- [ ] `DEBUG_JOURNAL.md` aktif kullan
- [ ] `SNAPSHOT_COMPARE.md` debug akışında referans kullan
- [ ] `README.md` repo root için sade tut
- [ ] Design / Architecture / State ayrımını koru
- [ ] Klasör yapısı değişirse snapshot bazlı olarak state’e yansıt

### Workflow Rules
- [ ] Yeni script eklerken mevcut `Assets/_Project/Scripts` klasör mimarisini baz al
- [ ] Doğru script klasörünü latest snapshot’tan doğrula
- [ ] Kod için önce GitHub, scene/runtime için önce snapshot kontrol et
- [ ] Kullanıcıdan, kaynaklardan doğrulanabilecek bilgiyi manuel kontrol etmesini isteme
- [ ] 30+ satırlık kodları chat’e yapıştırma; `.cs` dosyası olarak ver

---

## İleri Düzey / Sonraya Bırakılanlar

- [ ] ScriptableObject tabanlı combat config
- [ ] Enemy data config
- [ ] Wave / spawn pacing sistemi
- [ ] UI polish
- [ ] Audio placeholder
- [ ] Mobile optimization
- [ ] APK test build
- [ ] Diagonal / 8 yön input ihtiyacını ancak 4 yön combat oturduktan sonra yeniden değerlendir

---

## Notlar

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