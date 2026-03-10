# TODO_TR

> Aktif iş listesi.
> Bu dosya yalnızca yapılacak işleri ve öncelik sırasını tutar.
> Gameplay kuralları için `GAME_RULES_TR.md`
> Teknik sınırlar için `ARCHITECTURE_TR.md`
> Güncel durum özeti için `CHAT_STATE.md` referans alınır.

---

## 1) Aktif Faz

**Faz:** Locked Combat Refactor + Enemy Approach Loop

Ana amaç:
- Combat sistemini yeni locked rules ile birebir hizalamak
- Eski yön bazlı kalıntıları temizlemek
- Düşman yaklaşma -> telegraph -> execution -> punish / death loop'unu kurmak
- Feedback ve rage temelini oturtmak

---

## 2) Birinci Öncelik — Combat Refactor

### 2.1 Eski Yön Bazlı Kalıntıları Temizle
- [ ] Direction / dot threshold mantığını kaldır
- [ ] Yön bazlı hit kabulünü kaldır
- [ ] Combat validation'ı sadece **aktif weakpoint içinden geçme** kuralına çevir
- [ ] `Right / Up / Left` gibi yön odaklı test mantıklarını temizle
- [ ] Artık gereksiz olan 8 yön / diagonal düşüncesini tamamen bırak

### 2.2 Telegraph Akışını Yeni Tasarıma Çevir
- [ ] Telegraph fazını `1 -> 1+2 -> 1+2+3` şeklinde çalıştır
- [ ] Full pattern hold süresini ayarla
- [ ] Telegraph sırasında her yeni weakpoint görünümüne ses / feedback hook'u ekle
- [ ] Telegraph bitince execution'a geçişi temiz hale getir

### 2.3 Execution Akışını Yeni Tasarıma Çevir
- [ ] Execution başında sadece `1` aktif weakpoint görünür kalsın
- [ ] `1` tamamlanınca `2` açılsın
- [ ] `2` tamamlanınca `3` açılsın
- [ ] Aynı anda yalnızca tek aktif weakpoint görünsün
- [ ] Yanlış / aktif olmayan weakpoint teması **ignore** olsun
- [ ] Boş alanda gezinme **ignore** olsun
- [ ] Sadece süre ve aktif hedef ilerlemesi önemli olsun

### 2.4 Finger / Fail Kuralını Kilitlenen Tasarıma Uygula
- [ ] Execution sayaçları oyuncu dokunmadan da aksın
- [ ] Oyuncu execution içinde istediği an ilk teması yapabilsin
- [ ] İlk temastan sonra **first-touch lock** başlasın
- [ ] Execution tamamlanmadan finger lift olursa fail üret
- [ ] Timeout fail çalışsın
- [ ] Fail reason sistemini netleştir: `Timeout` / `FingerLift`

---

## 3) İkinci Öncelik — Punish / Retry Loop

### 3.1 Fail Sonrası Akış
- [ ] Fail olduğunda kısa punish feedback oynat
- [ ] Combo reset uygula
- [ ] Rage reset uygula
- [ ] İleride HP cezası bağlanabilecek alan bırak
- [ ] Kısa bekleme sonrası aynı düşmana yeniden dön

### 3.2 Retry Kuralı
- [ ] Fail sonrası pattern öğretme fazını tekrar oynatma
- [ ] Aynı düşman için doğrudan `1. weakpoint`ten yeniden başlat
- [ ] Retry loop'un temiz ve tutarlı çalıştığını test et

---

## 4) Üçüncü Öncelik — Enemy Approach Loop

### 4.1 Yaklaşma
- [ ] Düşman koridorun sonundan gelsin
- [ ] Yaklaştıkça perspektif olarak büyüsün
- [ ] Koridor hareketi ile görsel olarak uyumlu yaklaşma hissi ver

### 4.2 Telegraph Trigger
- [ ] Düşman belirli threshold noktasına gelince telegraph başlasın
- [ ] Telegraph tetikleme anı ile approach akışı uyumlu olsun

### 4.3 Success Sonrası
- [ ] Son weakpoint kesildiğinde kısa death feedback / animasyon oynat
- [ ] Mini bekleme ver
- [ ] Sonraki düşmanı başlat

### 4.4 Prototype Scope Kuralı
- [ ] Aynı anda yalnızca tek aktif düşman kuralını koru
- [ ] Bir düşman çözülmeden yenisi başlamasın

---

## 5) Dördüncü Öncelik — Rage Sistemi

### 5.1 Rage Dolumu
- [ ] Her doğru weakpoint hitinde rage ver
- [ ] Rage'i combodan bağımsız sayaç olarak işlet

### 5.2 Rage Active Davranışı
- [ ] Rage aktifken weakpoint zorunluluğunu kaldır
- [ ] Rage aktifken düşman silüetine slash yeterli olsun
- [ ] Rage aktifken tek slash execution davranışını test et

### 5.3 Rage Fail Davranışı
- [ ] Fail olduğunda rage'i sıfırla
- [ ] Rage reset feedback'ini gerekiyorsa bağla

---

## 6) Beşinci Öncelik — Feedback Katmanı

### 6.1 Hit Feedback
- [ ] Ufak ekran titremesi
- [ ] Hit stop
- [ ] Tatmin edici slash / hit sesleri
- [ ] Doğru hitte anlık görsel tepki

### 6.2 Combo / UI Feedback
- [ ] Combo popup / pulse
- [ ] Debug amaçlı combo ve rage takibini görünür tut
- [ ] Mekanik oturana kadar debug HUD yaklaşımını pratik tut

### 6.3 Fail / Death Feedback
- [ ] Fail anında kısa ve net punish feedback
- [ ] Success anında kısa death feedback
- [ ] İki durumun hissi birbirinden ayrışsın

---

## 7) Altıncı Öncelik — DevTool / Snapshot Tooling

### 7.1 Mini Snapshot Modeli
- [ ] `ProjectStateModels.cs` içine `MiniSnapshot` modeli ekle
- [ ] `MiniSnapshot` alanlarını netleştir: `meta + scene + code + consoleLogs + compileErrors`

### 7.2 Serializer
- [ ] `ProjectStateSerializer.cs` içine `FillMiniSnapshot()` ekle
- [ ] Son 50 console log yakalamayı ekle
- [ ] Log türlerini ayır: `Log / Warning / Error`
- [ ] Compile error yakalama ekle
- [ ] `DevTool` ve `TutorialInfo` klasörlerini `.cs` filtre dışında bırak

### 7.3 Exporter
- [ ] `ProjectStateExporter.cs` içine `Export MINI WORKING` menü item ekle
- [ ] `ProjectStateExporter.cs` içine `Export MINI DEBUG` menü item ekle
- [ ] Mevcut FULL export menülerine dokunma

### 7.4 Auto Snapshot
- [ ] `ProjectStateAutoSnapshot.cs` tarafında auto snapshot -> FULL yerine MINI DEBUG alsın

### 7.5 Compare
- [ ] `ProjectStateCompare.cs` içine `Compare Mini Debugs` menü item ekle
- [ ] `Mini Working vs Mini Debug` karşılaştırmasını destekle
- [ ] Ortak alan karşılaştırması: `meta + scene + oyun kodları`
- [ ] `DevTool / TutorialInfo` farkları gürültü üretmesin

### 7.6 Menü Hedefi
- [ ] Menü son halini doğrula:

    Tools > BladeRift > Project State >
    ├── Snapshots > Export WORKING
    ├── Snapshots > Export DEBUG
    ├── Snapshots > Export MINI WORKING
    ├── Snapshots > Export MINI DEBUG
    ├── Analysis > Compare Latest Working vs Debug
    ├── Analysis > Compare Mini Debugs
    └── ... (geri kalan mevcut menüler)

## 8) Yedinci Öncelik — Temizlik

### 8.1 Kod Temizliği

- [ ] Eski prototip / test kalıntılarını ayıkla
- [ ] Artık kullanılmayan yön bazlı kodları temizle
- [ ] Geçici logic ile kalıcı logic'i ayır

### 8.2 Sahne Temizliği

- [ ] Kullanılmayan world-space weakpoint objelerini kaldır veya devre dışı bırak
- [ ] Sahnede artık owner'ı olmayan referansları temizle

### 8.3 Test Temizliği

- [ ] Geçici test tetikleyicilerini yeni akışa göre güncelle
- [ ] Eski chain test mantığını yeni target-sequence mantığına çevir

## 9) Sonraki Katmanlar (Bu Fazdan Sonra)

### 9.1 Enemy Data

- [ ] Düşman tipine göre weakpoint sayısı / davranışı veri tarafına taşı
- [ ] Common / Elite / Boss farklarını data-driven hale getir

### 9.2 Wave / Flow

- [ ] Basit düşman akış yöneticisi
- [ ] Sonra wave / chapter pacing temeli

### 9.3 Mobile / Build

- [ ] Mobile test hazırlığı
- [ ] APK test build
- [ ] Performans / input doğrulaması

## 10) Bilerek Ertelenenler

Şimdilik odak dışı:

- [ ] Çoklu aktif düşman
- [ ] Gelişmiş boss fazları
- [ ] Ağır VFX polish
- [ ] Hikâye / narrative
- [ ] Multiplayer
- [ ] Çok geniş progression sistemi

11) Kısa Çalışma Notu

Bu dosyanın ana mesajı şu:

Eski yön bazlı combat mantığını bırak, locked target-sequence combat tasarımını ayağa kaldır.

Başarı ölçütü:

-Düşman yaklaşır
-Pattern bir kez öğretilir
-Execution doğru akar
-Finger lift ve timeout fail çalışır
-Punish / retry loop temizdir
-Rage ve feedback temel haliyle hissedilir
-Snapshot tooling MINI / FULL mantığıyla temiz çalışır