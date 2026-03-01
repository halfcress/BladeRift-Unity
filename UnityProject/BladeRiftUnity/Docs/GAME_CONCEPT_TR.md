# BladeRift – GAME CONCEPT (TR)

---

# 1. Elevator Pitch

BladeRift, mobil odaklı, 1. şahıs perspektifli, tek input (continuous swipe) ile oynanan
yüksek tempolu bir dungeon combat oyunudur.

Oyuncu sabittir. Dünya ve düşmanlar üzerine akıyormuş gibi ilerler.
Amaç; zayıf noktaları doğru sırayla ve tek akıcı hamlede keserek düşmanları
en verimli şekilde yok etmek, rage moduna girerek tempo yakalamak
ve chapter sonunda loot kazanarak karakterini kalıcı olarak güçlendirmektir.

Oyun iki temel eksen üzerine kuruludur:

1) Anlık tatmin (combat hissi)
2) Uzun vadeli bağımlılık (meta upgrade sistemi)

---

# 2. Oyun Kimliği ve Tasarım Felsefesi

## 2.1 Platform & Perspektif

- Platform: Mobil (Android öncelikli)
- Tür: Action / Reflex / Memory-based Combat
- Perspektif: 1. Şahıs
- Kontrol: Tek input (Swipe)

## 2.2 Tasarım Sütunları (Core Pillars)

1. Tek Input Derinliği  
   Tüm combat sistemi yalnızca swipe ile çalışır. Öğrenmesi basit, ustalaşması zordur.

2. Weakpoint Chain Mekaniği  
   Düşman üzerinde sırayla yanan zayıf noktalar bulunur.  
   Oyuncu continuous şekilde doğru sırayı takip ederek execution yapar.

3. Interrupt Önceliği  
   Doğru execution düşmanın saldırısını iptal eder.  
   Oyuncu “haksız” hasar yemez.

4. Rage Momentum Sistemi  
   Başarılı zincirler rage doldurur.  
   Rage modunda weakpoint şartı kalkar.

5. Yüksek Tatminli Feedback  
   - Mini slow motion  
   - 0.2 sn hit stop  
   - Beyaz ekran flash  
   - Zincir tamamlanınca güçlü görsel geri bildirim

6. Uzun Vadeli Meta  
   Upgrade sistemi derin olmalı. Oyuncu hem skill hem build ile ilerlemeli.

---

# 3. Core Gameplay (Moment-to-Moment)

## 3.1 Combat Akışı

1. Düşman yaklaşır.
2. Weakpoint zinciri sırayla yanar (Telegraph Phase).
3. Execution window açılır (2 saniye).
4. Mini slow motion başlar.
5. Oyuncu:
   - Parmağını kaldırmadan
   - Sıradaki weakpoint’e dokunarak zinciri ilerletir.

### Başarı Durumu

- Zincir tamamlanır.
- Basic düşman genelde ölür.
- Elite/Boss:
  - Büyük hasar alır
  - Interrupt olur
  - Stagger olur

### Hata Durumu

- Yanlış hedefe dokunma
- Parmağı kaldırma
- Sürenin bitmesi

Sonuç:
- Düşman saldırır
- Oyuncu hasar alır
- Düşman az hasar görür (chip damage)

Önemli tasarım ilkesi:
Oyuncu mükemmel execution yaptıysa saldırı yememeli.

---

# 4. Düşman Türleri

## 4.1 Basic Enemy

- 1-3 weakpoint
- Tek execution ile genelde ölür
- Basit saldırı paterni

## 4.2 Elite Enemy

- 3-5 weakpoint
- Execution:
  - Büyük hasar
  - Interrupt
  - Stagger
- Birden fazla execution gerekebilir

## 4.3 Boss (v0.1)

- Tek tip saldırı
- Interrupt edilebilir
- İleride:
  - Guard fazı
  - Multi-chain pattern
  - Sahne mekaniği

---

# 5. Rage Sistemi

Rage dolumu:
- Başarılı zincir
- Perfect streak

Rage aktifken:
- Weakpoint zorunluluğu kalkar
- Tek swipe = execution
- Tempo artar

Rage:
- Süre bazlıdır
- Upgrade ile geliştirilebilir

---

# 6. Chapter Yapısı

- Oyun chapter bazlı ilerler.
- Wave sistemi vardır.
- Aynı anda 2-3 düşman kombinasyonu olabilir.
- 5 bölümde 1 boss olabilir (esnek).

Chapter sonunda:
- Loot ekranı
- 3 seçenek
- 1 free reroll
- Loot x2 (reklam opsiyonu)

Oyuncu eski chapterlara dönebilir (farm).

---

# 7. Meta Progression

## 7.1 Upgrade Alanları

- Execution Damage %
- Instant Kill Chance %
- Execution Window +
- Weakpoint Hit Radius +
- Rage Gain +
- Rage Duration +
- Multiplier Cap +
- Multiplier Decay Slow

Amaç:
Başta hızlı tatmin → Orta oyunda optimizasyon → Geç oyunda hızlandırma isteği.

## 7.2 Uzun Vadeli Hedef

Upgrade sistemi 1 yıllık içerik derinliği taşımalı.  
Oyuncu kolay kolay bırakmamalı.

---

# 8. Monetizasyon Felsefesi

- Kozmetik satış
- Loot x2 (reklam)
- Ekstra reroll
- Upgrade hızlandırma
- Pay-to-skip var, pay-to-win yok

Skill her zaman önemli kalmalı.

---

# 9. Ürün Öncelikleri

Combat hissi: ULTRA+  
Görsel kalite: HIGH  
Ses & polish: Daha sonra

İlk hedef:
Çalışan, paylaşılan APK v0.1

---

# 10. Bilerek Ertelenenler

- Kompleks boss pattern
- Çoklu guard zincirleri
- Gelişmiş VFX
- Multiplayer
- Hikaye

Önce combat çekirdeği kusursuz olacak.

---

# 11. Bu Dokümanın Amacı

- Yeni bir ekip üyesi oyunu 10 dakikada anlayabilmeli.
- 3 ay sonra geri dönüldüğünde yön kaybolmamalı.
- Kod kararları vizyona göre alınmalı.
- Scope creep kontrol edilmeli.