# BladeRift – GAME CONCEPT (TR)

> Bu dosya BladeRift'in ürün vizyonunu anlatır.
> Detaylı gameplay kuralları burada tekrar edilmez.
> Combat'ın kesin davranışı için `GAME_RULES_TR.md` okunur.

---

## 1) Elevator Pitch

BladeRift, mobil odaklı, 1. şahıs perspektifli, yüksek tempolu bir dungeon combat oyunudur.

Oyuncu sabittir.
Dünya ve düşmanlar oyuncunun üstüne akıyormuş gibi ilerler.
Oyunun hedefi; düşman pattern'lerini okuyup doğru anda execute ederek yüksek tatminli bir combat hissi yakalamak, rage ile tempoyu yükseltmek ve uzun vadede karakterini güçlendirmektir.

BladeRift iki ana eksen üzerinde durur:
1. anlık tatmin
2. uzun vadeli ilerleme arzusu

---

## 2) Oyun Kimliği

- Platform: Mobil (Android öncelikli)
- Tür: Action / Reflex / Pattern-based Combat
- Perspektif: 1. şahıs
- Ana kontrol: Tek input / swipe
- Görsel yaklaşım: Stylized dungeon corridor
- Combat hissi: hızlı, keskin, tatminli, akıcı

---

## 3) Tasarım Sütunları

### 3.1 Tek Input Derinliği
Oyunun temel kontrol yapısı tek input üzerinden akar.
Öğrenmesi kolay, ustalaşması zaman isteyen bir combat hissi hedeflenir.

### 3.2 Pattern Okuma + Execution
Oyuncu önce düşmanın pattern'ini okur, sonra bu pattern'i zaman baskısı altında uygular.
Bu yapı oyuna hem refleks hem kısa süreli hafıza baskısı ekler.

### 3.3 Interrupt Önceliği
Başarılı execution düşmanın saldırı akışını bozar.
Oyuncu doğru oynadığında haksız hasar yememelidir.

### 3.4 Rage Momentum
Doğru hit'ler ve başarılı tempo oyuna hız katar.
Rage aktif olduğunda combat daha saldırgan, daha hızlı ve daha güç hissi veren bir yapıya dönüşür.

### 3.5 Yüksek Tatminli Feedback
BladeRift'in başarısı yalnızca kurala değil, hisse de bağlıdır.
Oyuncu gerçekten bir şeyi kestiğini hissetmelidir.

### 3.6 Uzun Vadeli Meta
Combat anlık zevki taşırken, meta sistem oyuncuyu geri getiren uzun vadeli bağımlılığı üretir.

---

## 4) Combat Fantasy

BladeRift'in combat fantezisi şudur:

- düşman yaklaşır
- oyuncu tehdidi okur
- kısa bir zihinsel hazırlık yaşar
- doğru anda zinciri uygular
- vuruşlar tatminli şekilde akar
- başarı tempo ve güç hissi üretir

Detaylı combat kuralı burada tutulmaz.
Tam rule set için `GAME_RULES_TR.md` referans alınır.

---

## 5) Düşman Vizyonu

### 5.1 Common
- daha kısa pattern
- daha okunabilir tehdit
- temel tempo taşıyıcısı

### 5.2 Elite
- daha zor pattern
- daha sert punish riski
- daha yüksek execution baskısı

### 5.3 Boss
- en kompleks tehdit
- daha ağır combat kimliği
- gelecekte çok fazlı ve daha özel davranışlar

Tam weakpoint sayıları ve gameplay kuralları concept seviyesinde değil, rules/data seviyesinde tutulur.

---

## 6) v0.1 / Current Product Direction

v0.1 için hedef:
- çalışan ve paylaşılabilir combat prototype
- yaklaşan düşman hissi
- pattern öğretme
- execution
- combo
- rage
- temel punish ve feedback
- mobil düşünülerek kurulmuş ama PC'de hızlı test edilebilir yapı

Prototype kapsamı:
- aynı anda tek aktif düşman
- tek koridor hissi
- combat çekirdeğini doğrulamaya odaklı akış

---

## 7) Long-Term Vision

Uzun vadede BladeRift şu alanlara genişleyebilir:
- wave tabanlı chapter yapısı
- farklı düşman kombinasyonları
- aynı anda birden fazla tehdit
- daha kompleks elite / boss pattern'leri
- daha güçlü rage varyasyonları
- meta build çeşitliliği
- loot ve progression derinliği
- daha güçlü ses / VFX / polish katmanı

Bu maddeler vizyonu anlatır; prototype scope'unu zorunlu kılmaz.

---

## 8) Rage Vizyonu

Rage, BladeRift'in tempo kırıcı değil tempo yükseltici mekanizmasıdır.

Amaç:
- oyuncuya zincir doğruluğunun ödülünü vermek
- combat'ı kısa süreliğine daha saldırgan hale getirmek
- ritmi yükseltmek
- "ben şu an güçlendim" hissi yaratmak

Kesin rage kuralı için `GAME_RULES_TR.md` referans alınır.

---

## 9) Meta Progression Vizyonu

Uzun vadeli progression alanları:
- execution rahatlığı
- tempo artırıcı yükseltmeler
- rage süresi / rage verimliliği
- hasar çeşitleri
- chain verimliliği
- chapter farming motivasyonu

Amaç:
Başta tatmin, orta oyunda optimizasyon, ileri oyunda ustalık hissi.

---

## 10) Monetizasyon Felsefesi

BladeRift'in monetizasyonu skill hissini öldürmemelidir.

Hedef yaklaşım:
- kozmetik satışlar
- loot / progression hızlandırıcı yardımcılar
- opsiyonel reklam temelli ödüller

Ana ilke:
**pay-to-skip olabilir, pay-to-win olmamalıdır.**

---

## 11) Bilerek Ertelenenler

Şimdilik ana odak dışında kalanlar:
- gelişmiş hikâye
- karmaşık boss fazları
- ağır VFX production
- multiplayer
- aşırı sistem genişliği

Önce combat çekirdeği kusursuz oturmalıdır.

---

## 12) Bu Dokümanın Rolü

Bu dosya şu sorunun cevabıdır:

**"BladeRift nasıl bir oyun olmak istiyor?"**

Bu dosya rulebook değildir.
Bu dosya product vision kaynağıdır.
