# TODO_TR

> Aktif iş listesi. Odak güncel ve sıradaki işlerde olmalıdır.

---

## Aktif Faz

**Faz:** Combat Prototype Foundation — Hit-Test Doğrulama

Ana amaç:
- Marker üstünden geçmek = HIT çalışıyor mu doğrulamak
- Combo sisteminin görsel olarak çalıştığını görmek
- İlk gerçek combat loop'u ayağa kaldırmak

---

## Tamamlananlar

- [x] Prototype_CombatCore sahnesi oluşturuldu
- [x] First-person karar kilitlendi
- [x] Infinite corridor sistemi kuruldu ve stabilize edildi
- [x] Ceiling, fog, torch lights eklendi
- [x] SwipeInput sistemi kuruldu (mouse + touch)
- [x] SwipeInterpreter segment-based chain mantığı
- [x] CombatDirector oluşturuldu
- [x] WeakpointSequence telegraph → execution akışı
- [x] WeakpointDirectionView marker sistemi
- [x] World-space WeakPoint takibinden sabit ekran pozisyonlarına geçildi
- [x] Finger lift = reset kaldırıldı (Fruit Ninja modeli)
- [x] ComboManager oluşturuldu
- [x] ComboText + HitCountText UI eklendi
- [x] SwipeDebugHUD kaldırıldı
- [x] ProjectState DevTool (snapshot, compare, journal) tamamlandı

---

## Şu An Yapılacaklar

### Doğrulama (Öncelik 1)
- [ ] Hit-test çalışıyor mu doğrula — marker üstünden geç, HIT logu + combo artıyor mu?
- [ ] ComboText ekranda görünüyor mu? ("HIT!" / "x2 COMBO!")
- [ ] Timeout = combo sıfırlıyor mu?
- [ ] Parmak kaldırma = combo **bozmuyor** mu?

### Temizlik
- [ ] `WeakpointCombatTest.cs` kaldır (eski prototip, kullanılmıyor)
- [ ] `WeakPoint_1/2/3` world objelerini sahneye kaldır (artık kullanılmıyor)

---

## Bir Sonraki Katman

### EnemyController (Sonraki Major Step)
- [ ] `EnemyController.cs` oluştur
- [ ] Düşman koridorun sonundan yaklaşır (scale büyür)
- [ ] Belirli mesafeye gelince telegraph başlar
- [ ] Execute = interrupt (düşman saldırısı kesilir)
- [ ] Düşman hasar alır, stagger olur

### Combat Flow Polish
- [ ] CombatTriggerTest'i EnemyController'a bağla (otomatik trigger kalkacak)
- [ ] Telegraph süresi + hold süresi GameConfig'ten okunuyor mu doğrula
- [ ] Execution window timeScale etkisini test et

### Feedback
- [ ] Basit slash trail effect
- [ ] Hit VFX (basit flash)
- [ ] Combo sayacına pulse animasyon

---

## İleri Düzey / Sonraya Bırakılanlar

- [ ] ScriptableObject tabanlı enemy data config
- [ ] Wave / spawn pacing sistemi
- [ ] Rage sistemi implementasyonu
- [ ] UI polish
- [ ] Audio placeholder
- [ ] Mobile optimization
- [ ] APK test build
- [ ] Diagonal / 8 yön input (4 yön oturduktan sonra değerlendir)

---

## Tooling / Workflow

- [ ] Auto DEBUG snapshot → auto journal entry bağlantısı
- [ ] CHAT_STATE düzenli güncelle (her milestone sonrası)
- [ ] DEBUG_JOURNAL aktif kullan

---

## Notlar

- Kod kaynağı: **GitHub pushed state**
- Scene/debug kaynağı: **Snapshot (sadece gerektiğinde)**
- MCP: Sadece kod yazma + sahne kaydetme
- Sahne değişiklikleri: Kullanıcı elle yapar
- 30+ satır kod: `.cs` dosyası olarak verilir
