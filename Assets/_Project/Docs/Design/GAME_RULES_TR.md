# GAME RULES (TR) - BladeRift

## Amaç
Oyunun "kurallarını" ve ayarlanabilir parametrelerini tanımlar.
Kod yazarken sabit (hardcode) değerlerden kaçınmak için referans alınır.

---

## 1) Core Combat

- Oyuncu sabit, tek input: **swipe**
- Weakpoint zinciri **UI overlay** olarak görünür (sabit ekran pozisyonları)
- Weakpoint'ler sırayla yanar (telegraph) ve her adımda tick sinyali verir
- Sonrasında **Execution Window** açılır

### Execution Mekaniği (Tasarım Kararı — Fruit Ninja Modeli)

Oyuncu parmağını marker'ın üstünden geçirir → HIT.

- Yön bilgisi ikincil kontrol olarak kullanılır (dot threshold)
- Parmağı kaldırıp yeniden basmak serbesttir
- Marker'lar **sabit ekran pozisyonlarında** durur

### Execution Window Parametreleri
- Süre: **2 saniye** (`ExecutionWindowSeconds`)
- Mini slow motion: `timeScale ~ 0.8` (`TimeScaleDuringExecution`)

---

## 2) Başarı / Başarısızlık

### Başarı
- Oyuncu sırayla her marker'ın üstünden doğru yönde geçerse zincir ilerler
- Zincir tamamlanırsa **Execution Success**

### Başarısızlık (Tek koşul)
- **Sürenin bitmesi (Timeout)**

Sonuç:
- Düşman saldırır
- Oyuncu hasar alır
- Combo **sıfırlanır**

### Başarısızlık SAYILMAYAN durumlar
- Parmağı kaldırmak → serbest
- Marker dışına swipe atmak → sadece o hit sayılmaz, combo bozulmaz
- Yanlış yön → sadece o hit sayılmaz, combo bozulmaz

> Tasarım ilkesi: Oyuncu doğru execution yaptıysa ceza yememeli.
> Ceza yalnızca zamanında tepki verememekten kaynaklanır.

---

## 3) Execution Sonucu

**Execution = Interrupt**

- Basic düşman: çoğunlukla ölür
- Elite/Boss: her zaman tekte ölmez:
  - Büyük hasar
  - **Interrupt (saldırı iptali)**
  - Stagger

---

## 4) Combo Sistemi (Fruit Ninja Modeli)

- Her başarılı **hit** combo sayacını artırır
- Zincir başarıyla tamamlandığında combo **devam eder** (sıfırlanmaz)
- Düşmanlar arası geçişte combo **devam eder**
- **Sadece timeout** combo'yu sıfırlar

Ekran gösterimi:
- 1 hit: `"HIT!"`
- 2+ hit: `"x{n} COMBO!"`

---

## 5) Rage

- Rage dolumu: başarılı chain + streak
- Rage aktifken: weakpoint şartı kalkar, anywhere execution
- Rage süre bazlıdır, upgrade ile geliştirilebilir

---

## 6) Revive

- Chapter içinde **1 revive hakkı** (v0.1)

---

## 7) Pacing

- Wave tabanlı ilerleme
- Aynı anda **2–3 düşman kombinasyonu**
- Chapter süresi hedef: **60–90 saniye** (v0.1)

---

## 8) Ayarlanabilir Parametreler (GameConfig ScriptableObject)

| Parametre | Default | Açıklama |
|---|---|---|
| `ExecutionWindowSeconds` | 2.0 | Execution süresi |
| `TimeScaleDuringExecution` | 0.8 | Mini slow motion |
| `TelegraphStepSeconds` | 1.0 | Her adım arası süre |
| `TelegraphHoldSeconds` | 1.0 | Tüm adımlar göründükten sonra bekleme |
| `WeakpointHitRadiusPx` | 80 | Marker hit zone yarıçapı |
| `WeakpointMinDeltaPx` | 8 | Min swipe hareketi (px/frame) |
| `DirectionDotThreshold` | 0.3 | Yön toleransı (0=her yön, 1=tam eşleşme) |
| `FailPunishDamage` | — | Timeout → oyuncu hasar |
| `FailChipDamage` | — | Timeout → düşman chip hasar |
| `BasicExecutionDamage` | — | Basic düşman execution hasarı |
| `EliteExecutionDamage` | — | Elite düşman execution hasarı |
| `BossExecutionDamage` | — | Boss execution hasarı |
| `RageGainOnHit` | — | Her hit'te rage dolumu |
| `RageGainOnSuccess` | — | Zincir tamamında rage dolumu |
| `RageDurationSeconds` | — | Rage süresi |

---

## 9) Değişiklik Protokolü

- Önce **config değerleriyle** dene
- Yetmezse yeni modül ekle
- En son refactor
