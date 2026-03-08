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
- [x] CombatDirector temel iskeleti oluşturuldu
- [x] ProjectStateExporter v2 kuruldu
- [x] Working / Debug / Archive snapshot ayrımı oluşturuldu
- [x] DEBUG_JOURNAL sistemi başlatıldı
- [x] Repo root temiz Unity yapısına getirildi
- [x] `_Project` merkezli klasör yapısı oluşturuldu

---

## Şu An Yapılacaklar (En Yakın)

### Input / Combat
- [ ] `SwipeInterpreter.cs` oluştur
- [ ] Swipe yönünü `Left / Right / Up / Down` olarak yorumla
- [ ] Swipe yönünü CombatDirector’a aktar
- [ ] Debug text’e attack direction yazdır

### Combat Placeholder
- [ ] Basit enemy placeholder oluştur
- [ ] 2D billboard enemy yaklaşımını sahnede test et
- [ ] Enemy’nin kameraya dönük kalmasını sağla
- [ ] Distance / scale büyüme mantığını test et

### Weakpoint Placeholder
- [ ] Basit weakpoint placeholder sistemi tasarla
- [ ] Zayıf nokta görseli için placeholder sprite/indicator ekle
- [ ] Doğru swipe yönü ile weakpoint eşleşme testini yap

---

## Bir Sonraki Katman

### Combat Flow
- [ ] Telegraph → execution window akışını başlat
- [ ] Success / fail durumlarını CombatDirector içine oturt
- [ ] Finger lift = chain reset davranışını gerçek akışta test et
- [ ] Combat state geçişlerini netleştir

### Feedback
- [ ] Basit slash feedback ekle
- [ ] Basit hit VFX ekle
- [ ] Basit blood particle placeholder ekle
- [ ] Swipe hit sonrası görsel tepki ver

---

## Tooling / Workflow

### Debug Tool Improvements
- [ ] Snapshot cleanup davranışını test et
- [ ] Working / Debug / Archive politikasını finalize et
- [ ] `DEBUG_JOURNAL.md` kullanım standardını belirle
- [ ] Git commit hash bilgisini journal entry’ye ekleme opsiyonunu değerlendir
- [ ] Oto-MD updater V1 tasarımı yap

### Docs
- [ ] `CHAT_STATE.md` düzenli güncelle
- [ ] `DEBUG_JOURNAL.md` aktif kullan
- [ ] `README.md` repo root için sadeleştir / güncelle
- [ ] Design / Architecture / State ayrımını koru

---

## İleri Düzey / Sonraya Bırakılanlar

- [ ] ScriptableObject tabanlı combat config
- [ ] Enemy data config
- [ ] Wave / spawn pacing sistemi
- [ ] UI polish
- [ ] Audio placeholder
- [ ] Mobile optimization
- [ ] APK test build

---

## Notlar

- Kod tarafında kaynak: **GitHub pushed state**
- Scene/debug kaynak: **latest snapshot**
- Aynı bug için aynı denemeleri tekrar etmemek amacıyla:
  - `DEBUG_JOURNAL.md`
  - `CHAT_STATE.md`
düzenli kullanılmalı