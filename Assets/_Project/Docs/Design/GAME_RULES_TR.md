# GAME RULES (TR) - BladeRift

> Bu dosya BladeRift'in gameplay kural kaynağıdır.
> Combat, combo, rage ve punish davranışları burada kilitlenir.
> Teknik uygulama detayları burada değil, `ARCHITECTURE_TR.md` içinde anlatılır.

---

## 1) Amaç

BladeRift combat sistemi oyuncuya iki şeyi aynı anda vermelidir:
- pattern okuma
- zaman baskısı altında düzgün execution

Sistemin özü:

**Önce gör → sonra sırayla kes → süre içinde bitir.**

---

## 2) Core Combat Özeti

- Oyuncu sabittir
- Dünya ve düşmanlar oyuncunun üstüne akıyormuş gibi hissedilir
- Ana input **swipe / slash** davranışıdır
- Düşman üstünde weakpoint pattern'i gösterilir
- Oyuncu execution sırasında aktif weakpoint'leri sırayla tamamlar
- Combat yön bazlı değil, **hedef bazlıdır**

---

## 3) Düşman Türleri

### 3.1 Common
- Daha kısa pattern
- Daha düşük execution karmaşıklığı
- Prototype'ta temel test düşmanıdır

### 3.2 Elite
- Daha uzun pattern
- Daha zor execution
- Tamamlanınca çoğu zaman ölmez; ağır hasar + interrupt + stagger alabilir

### 3.3 Boss
- En kompleks pattern yapısı
- Çok fazlı veya daha ağır execution yapıları taşıyabilir
- Tek execution ile ölmek zorunda değildir

---

## 4) Telegraph (Pattern Öğretme)

Bu fazın amacı oyuncuya sırayı öğretmektir.

### 4.1 Görünme Mantığı
Weakpoint'ler **birikmeli** görünür:
- önce `1`
- sonra `1 + 2`
- sonra `1 + 2 + 3`

### 4.2 Full Pattern Hold
Tüm weakpoint'ler kısa süre birlikte görünür.
Bu, oyuncunun sırayı okuması içindir.

### 4.3 Ses / Feedback
Her yeni weakpoint belirdiğinde kısa ve net bir pattern sesi kullanılabilir.

### 4.4 Önemli Kural
Bir düşman için pattern öğretme fazı normal şartta **bir kez** gösterilir.

---

## 5) Execution Başlangıcı

Pattern öğretme bittikten sonra execution başlar.

### 5.1 Başlangıç Görünümü
Execution başladığı anda:
- `1` görünür kalır
- diğer future weakpoint'ler görünmez

### 5.2 Active Progression
- `1` tamamlanınca `2` görünür
- `2` tamamlanınca `3` görünür
- Son weakpoint tamamlanınca execution success olur

### 5.3 Tek Aktif Weakpoint Kuralı
Aynı anda yalnızca **tek aktif weakpoint** vardır.
Sıradaki weakpoint tamamlanmadan sonraki açılmaz.

---

## 6) Hit Kuralı

Bir hit'in geçerli sayılması için gerekli tek koşul:

- oyuncunun slash / swipe izi **aktif weakpoint'in içinden geçmelidir**

### 6.1 Yön Kuralı
- Swipe yönü önemli değildir
- Sağdan sola, soldan sağa, yukarıdan aşağı, çapraz veya serbest yönler kabul edilir
- Sistem yön bazlı değil, hedef bazlıdır

### 6.2 Yanlış / Boş Temaslar
Aşağıdakiler doğrudan fail sayılmaz:
- boş alanda dolaşmak
- aktif olmayan weakpoint'e temas etmek
- yanlış sıradaki noktaya gitmek
- gösterişli serbest hareketler yapmak

Bunlar yalnızca süreyi harcar.

---

## 7) Finger / Touch Kuralları

### 7.1 Execution Başladığında
Execution sayacı arkada çalışır.
Oyuncu execution açıldığı anda dokunmak zorunda değildir.

### 7.2 İlk Temas
Oyuncu execution süresi içinde istediği an ekrana ilk kez basabilir.

### 7.3 First-Touch Lock
Oyuncu ilk teması yaptıktan sonra:
- execution bitene kadar parmağını kaldıramaz

### 7.4 Finger Lift Fail
İlk temas yapıldıktan sonra parmak execution tamamlanmadan kalkarsa **fail** olur.

### 7.5 Düşmanlar Arası
Bir düşman bittikten sonra parmak kaldırmak serbesttir.
Bu, combo'yu tek başına bozmaz.

---

## 8) Başarı / Başarısızlık

## 8.1 Success
Execution başarıdır eğer:
- aktif weakpoint'ler doğru sırayla tamamlanmışsa
- süre dolmadan zincir bitmişse

### Success Sonucu
- son weakpoint kesilir
- düşman kısa death / execute feedback alır
- mini ara olur
- sonraki düşman başlar

## 8.2 Fail
Fail şu durumlarda oluşur:
1. **Timeout**
2. **Execution içi finger lift**

### Fail Sonucu
- punish olur
- combo sıfırlanır
- rage sıfırlanır
- ileride HP cezası uygulanabilir
- kısa bekleme olur
- aynı düşman için tekrar doğrudan `1` noktasından başlanır

### Kritik Kural
Fail sonrası pattern yeniden öğretilmez.
Telegraph tekrar baştan oynatılmaz.
Oyuncu doğrudan ilk weakpoint'ten yeniden dener.

---

## 9) Punish

Punish şu an bir sonuç ailesidir.
Prototype için punish şu etkileri içerebilir:
- kısa fail animasyonu
- combo reset
- rage reset
- ileride HP kaybı

Punish oyunu otomatik bitirmek zorunda değildir.

---

## 10) Combo

### 10.1 Temel Kural
Her doğru weakpoint:
- **+1 combo**
- **+1 hit**

### 10.2 Örnek
3 adımlı bir execution:
- 1. weakpoint = +1
- 2. weakpoint = +1
- 3. weakpoint = +1

Toplam:
- **+3 hit**
- düşman ölürse kill de alınmış olur

### 10.3 Reset Kuralı
Combo fail olduğunda sıfırlanır.
Yani:
- timeout → combo reset
- finger lift fail → combo reset

### 10.4 Görsel Gösterim
- 1 hit: `HIT!`
- 2+ hit: `xN COMBO!`

---

## 11) Rage

### 11.1 Rage Dolumu
Her doğru hit rage verir.

### 11.2 Rage ve Combo Ayrımı
Rage, combo ile aynı sistem değildir.
Birbirine bağlı hissedebilirler ama aynı sayaç değildirler.

### 11.3 Rage Aktifken
Rage aktif olduğunda:
- weakpoint zorunluluğu kalkar
- düşmanın silüetine slash atmak yeterlidir
- düşman tek slash ile execute olabilir
- tempo yükselir

### 11.4 Rage Reset
Fail olduğunda rage sıfırlanır.

---

## 12) Prototype Scope Kuralları

v0.1 / debug / prototype için:
- aynı anda tek aktif düşman vardır
- bir düşman ölmeden diğeri başlamaz
- execution süresi debug için geniş tutulabilir
- tuning değerleri zorluğa göre daha sonra sıkılaştırılabilir

---

## 13) Ayarlanabilir Parametreler

Aşağıdaki değerler tuning / config tarafında yaşar:

| Parametre | Önerilen Debug Default | Açıklama |
|---|---:|---|
| `ExecutionWindowSeconds` | 5.0 | Execution için toplam süre |
| `TelegraphStepSeconds` | 1.0 | Pattern build sırasında her adım arası süre |
| `TelegraphFullHoldSeconds` | 1.0 | Full pattern görünme süresi |
| `ExecutionStartDelaySeconds` | 0.0 | Telegraph sonrası opsiyonel ek bekleme |
| `TimeScaleDuringExecution` | 0.8 | İsteğe bağlı tempo hissi / mini slow motion |
| `PunishDelaySeconds` | tuning | Fail sonrası mini bekleme |
| `HitStopSeconds` | tuning | Hit hissi için kısa duraksama |
| `ScreenShakeIntensity` | tuning | Vuruş hissi için ekran sarsıntısı |

### Not
`ExecutionStartDelaySeconds` şu an zorunlu kural değildir.
Zorluk ayarına göre eklenebilir.

---

## 14) Uzun Vadeli Genişleme Notları

İleride eklenebilir:
- daha uzun chain'ler
- düşman tipine göre farklı pattern yapıları
- group enemy combat
- multi-wave chapter pacing
- rage çeşitleri
- punish çeşitleri
- daha sert elite/boss interrupt mantığı

Bunlar geleceğe dönük genişlemelerdir; mevcut prototype kuralını override etmez.

---

## 15) Kapanış Kuralı

Bu dosya şu sorunun cevabıdır:

**"Oyunda combat gerçekten nasıl çalışır?"**

Bu dosya mevcut gameplay truth kaynağıdır.
