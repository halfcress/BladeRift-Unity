# ARCHITECTURE (TR) - BladeRift

> Bu dosya teknik mimarinin kaynağıdır.
> Burada oyun kuralı değil; sistem sınırları, sorumluluklar ve veri akışı tanımlanır.
> Script isimleri zorunlu değildir. Önemli olan modül rolüdür.

---

## 1) Amaç

BladeRift'in combat sistemi zaman içinde değişse bile proje çöpe gitmeden ilerleyebilmelidir.

Bu mimarinin hedefleri:
- modüler yapı
- düşük bağımlılık
- mümkün olduğunca event-driven haberleşme
- veri ve kuralların tek yerde toplanması
- AI ile kod yazılırken kapsam kaymasını azaltmak

---

## 2) Bu Dokümanın Kapsamı

Bu dosya şunları tanımlar:
- hangi modül neyin sahibidir
- hangi modül neyi yapmaz
- sistemler arası sınırlar
- state akışı
- veri sahipliği

Bu dosya şunları tanımlamaz:
- tam gameplay kural metni
- sürelerin kesin sayısal değerleri
- inspector değerleri
- debug durumu
- mevcut script dosyalarının isim listesi

Gameplay kuralları için `GAME_RULES_TR.md` esas alınır.
Güncel ilerleme durumu için `CHAT_STATE.md` esas alınır.

---

## 3) Yüksek Seviye Sistem Görünümü

### 3.1 Ana Katmanlar
- Input
- Sequence / State
- Combat Validation
- Presentation (UI)
- Feedback
- Enemy
- Spawn / Flow (ileriki faz)

### 3.2 Ana İlke
- **Input** parmak verisini sağlar
- **Sequence** combat akışını ve fazları yönetir
- **Combat** hit geçerliliğini değerlendirir
- **Presentation** sadece gösterir
- **Feedback** hissi üretir
- **Enemy** düşmanın yaşam döngüsünü yönetir

---

## 4) Modüller

## 4.1 Input Modülü
**Sorumluluk:**
- Parmağın basılı olup olmadığını bilmek
- Parmağın anlık ekran pozisyonunu sağlamak
- Parmağın ilk temasını ve bırakılmasını tespit etmek
- Mouse ve touch girdisini ortak bir modelde sunmak

**Yapmaz:**
- weakpoint bilmez
- hit kararı vermez
- combo/rage yönetmez
- fail kararı vermez

**Not:**
Input katmanı yalnızca ham oyuncu etkileşimini sağlar.

**Uygulayan modül notu:** _Current input provider buraya bağlanır._

---

## 4.2 Sequence / State Modülü
**Sorumluluk:**
- combat akışını fazlar halinde yürütmek
- pattern öğretme fazını yönetmek
- execution'ı başlatmak
- aktif weakpoint sırasını yönetmek
- success / fail sonucuna geçmek

**Yapmaz:**
- hit hissi üretmez
- düşman ölümü efektini oynatmaz
- UI kararları vermez
- parmak verisini doğrudan üretmez

**Uygulayan modül notu:** _Current combat sequence / state owner buraya bağlanır._

---

## 4.3 Combat Validation Modülü
**Sorumluluk:**
- execution sırasında yalnızca **aktif weakpoint** için geçerli hit kontrolü yapmak
- aktif weakpoint'in içinden geçildi mi sorusunu cevaplamak
- success / advance / fail sinyalini ilgili sisteme vermek
- timeout veya execution içi finger lift gibi fail sebeplerini result olarak üretmek

**Yapmaz:**
- yön kontrolü yapmaz
- yanlış input diye cezalandırma yapmaz
- boş alanda gezinmeyi fail saymaz
- UI gösterimi yapmaz

**Not:**
Bu katman hedef bazlı çalışır.
Hit'in geçerli olması için aktif weakpoint'in içinden geçilmesi gerekir.
Swipe yönü önemli değildir.

**Uygulayan modül notu:** _Current combat validator buraya bağlanır._

---

## 4.4 Presentation (UI) Modülü
**Sorumluluk:**
- weakpoint marker'ları göstermek
- aktif marker'ı görünür kılmak
- combo, rage, debug ve HUD bilgisini göstermek
- fail/success ile ilgili gerekli görsel yazıları göstermek gerekiyorsa göstermek

**Yapmaz:**
- gameplay kuralı vermez
- hit kararı vermez
- sıra kararı vermez
- fail koşulu belirlemez

**Not:**
UI yalnızca oyuncunun görmesi gereken bilgiyi sunar.

**Uygulayan modül notu:** _Current presentation / HUD layer buraya bağlanır._

---

## 4.5 Feedback Modülü
**Sorumluluk:**
- hit stop
- screen shake
- slash / hit sesleri
- combo pulse
- fail feedback
- death feedback
- genel vuruş tatmini

**Yapmaz:**
- combo sayısını hesaplamaz
- rage miktarını hesaplamaz
- gameplay sonucu belirlemez

**Not:**
Feedback ayrı katmandır. Prototype içinde de ayrı tutulur.

**Uygulayan modül notu:** _Current feedback layer buraya bağlanır._

---

## 4.6 Enemy Modülü
**Sorumluluk:**
- düşmanın uzaktan yaklaşması
- yaklaştıkça perspektif olarak büyümesi
- telegraph başlama eşiğine gelmesi
- düşmanın execution, punish, death ve recover akışını yönetmesi
- düşman tipine göre data kullanması

**Yapmaz:**
- ham input işlemez
- weakpoint hit test yapmaz
- UI çizmez

**Not:**
Prototype için aynı anda yalnızca **tek aktif execution enemy** vardır.
Bir düşman çözülmeden diğeri başlamaz.

**Uygulayan modül notu:** _Current enemy owner buraya bağlanır._

---

## 4.7 Spawn / Flow Modülü
**Durum:** İleri faz

**Sorumluluk:**
- düşman sırası
- wave akışı
- chapter pacing
- ileride çoklu düşman grupları

**Prototype kuralı:**
Bu modül henüz tam kapsamda aktif olmak zorunda değildir.
v0.1 için tek aktif düşman akışı yeterlidir.

**Uygulayan modül notu:** _İleri faz._

---

## 5) Combat State Akışı

Prototype için önerilen net akış:

1. **Approach**
   - Düşman uzaktan gelir
   - Yaklaştıkça büyür

2. **TelegraphBuild**
   - Pattern birikmeli görünür:
   - önce `1`
   - sonra `1+2`
   - sonra `1+2+3`

3. **TelegraphFullHold**
   - Tam pattern kısa süre görünür
   - Amaç: oyuncuya sırayı okutmak

4. **ExecutionStart**
   - Telegraph kapanır
   - Sadece `1` görünür kalır
   - Sayaç çalışmaya başlar

5. **ExecutionActive**
   - Oyuncu istediği an ilk teması yapabilir
   - İlk temastan sonra parmak kaldırmak faildir
   - `1` vurulunca `2`, `2` vurulunca `3` görünür

6. **SuccessResolve**
   - Son weakpoint tamamlanır
   - Kısa ölüm animasyonu / feedback oynar
   - Mini ara verilir
   - Sonraki düşmana geçilir

7. **FailResolve**
   - Punish oynar
   - Combo reset, rage reset gibi sonuçlar ilgili sistemlere iletilir
   - Kısa bekleme sonrası aynı düşman için tekrar `1` noktasından başlanır
   - Pattern tekrar baştan gösterilmez

---

## 6) Input ve Execution İlişkisi

### 6.1 Execution Öncesi
- Oyuncu ekrana basmak zorunda değildir
- Sadece sayaç akmaktadır

### 6.2 Execution İçinde
- Oyuncu istediği an ilk teması yapabilir
- İlk temas sonrası parmak execution bitene kadar kalkmamalıdır

### 6.3 Ignore Davranışları
Aşağıdakiler doğrudan fail üretmez:
- boş alanda gezinmek
- aktif olmayan weakpoint'e değmek
- yanlış sıradaki weakpoint'e temas etmek
- gösterişli / serbest hareketler yapmak

Bunlar yalnızca süre kaybettirir.

---

## 7) Sonuç Üretimi

### 7.1 Success
Combat tarafı yalnızca sonucu üretir:
- chain tamamlandı
- execution başarılı

Bunun sonrası:
- feedback katmanı hissi oynatır
- enemy katmanı death / interrupt / stagger davranışını işler
- skor/comb/rage katmanı kendi sonuçlarını uygular

### 7.2 Fail
Combat tarafı yalnızca sonucu üretir:
- fail reason: timeout / execution içi finger lift

Bunun sonrası:
- punish feedback oynar
- combo sistemi reset uygular
- rage sistemi reset uygular
- enemy tarafı fail sonrası akışı sürdürür

---

## 8) Event Yaklaşımı

Ana ilke:
- sistemler mümkün olduğunca event-driven ilerler
- ancak prototype içinde gereksiz karmaşa yaratmamak adına gerektiğinde doğrudan referans kullanımı serbesttir

Bu yüzden yaklaşım:
- **öncelik:** event / signal
- **izinli istisna:** pratik doğrudan bağlantı

Katı teorik saflık yerine sürdürülebilirlik tercih edilir.

---

## 9) Veri Sahipliği

### 9.1 Enemy Data
Aşağıdaki bilgiler düşman verisinin parçasıdır:
- düşman tipi (`Common / Elite / Boss`)
- weakpoint sayısı
- pattern karmaşıklığı
- yaklaşma / punish / death davranışları
- ileride HP, stagger, interrupt dayanımı

### 9.2 Global Config
Aşağıdaki bilgiler global tuning/config tarafında yaşayabilir:
- telegraph adım süreleri
- execution süreleri
- feedback yoğunlukları
- kolaylık / zorluk tuning değerleri

---

## 10) Prototype Kısıtları

v0.1 / prototype için sabit kurallar:
- tek aktif düşman
- pattern öğretme fazı düşman başına bir kez gösterilir
- fail sonrası pattern tekrar oynatılmaz
- tekrar doğrudan ilk weakpoint'ten başlanır
- yön tabanlı kontrol yoktur
- aktif hedef dışındaki temaslar ignore edilir

---

## 11) Esneklik Sınırı

AI veya insan tarafından yeni kod yazılırken:
- mevcut modül rolü değiştirilmez
- gameplay kuralı architecture içinde uydurulmaz
- her yeni modül ilgili architecture maddesine bağlanır
- architecture dışına taşan esneklik kabul edilmez

Her yeni teknik eklemede mümkünse şu format kullanılır:

`Uygulayan modül:`  
`Bu maddeyi sahiplenen katman:`  
`Bağlı olduğu veri:`  

---

## 12) Kapanış Kuralı

Bu dosya şu sorunun cevabıdır:

**"Bu sistemi teknik olarak nasıl böldük ve her parça neyin sahibi?"**

Bu dosya gameplay rulebook değildir.
