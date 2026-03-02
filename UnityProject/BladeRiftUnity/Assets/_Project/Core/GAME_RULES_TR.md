# GAME RULES (TR) - BladeRift

## Amaç
Bu doküman oyunun “kurallarını” ve ayarlanabilir parametrelerini tanımlar.
Kod yazarken sabit (hardcode) değerlerden kaçınmak için referans alınır.

---

## 1) Core Combat (Şu anki tasarım)

- Oyuncu sabit, tek input: **continuous swipe**.
- Düşmanın weakpoint zinciri **2D UI overlay** olarak görünür.
- Weakpoint’ler sırayla yanar (telegraph) ve her adımda **tick** sinyali verir.
- Sonrasında **Execution Window** açılır:
  - Süre: **2 saniye**
  - Mini slow motion: `timeScale ~ 0.8`
- Parmak kalkarsa: chain **iptal/reset**.
- Execution window sırasında:
  - Tüm weakpoint’ler görünür
  - **Sadece sıradaki hedef** güçlü highlight olur
  - Weakpoint’ler **micro jitter** yapar (çok küçük)

---

## 2) Başarı / Başarısızlık

### Başarı
- Oyuncu parmağını kaldırmadan, sırayla doğru weakpoint’e temas ederse zincir ilerler.
- Zincir tamamlanırsa **Execution Success** oluşur.

### Başarısızlık
Aşağıdakilerden biri olursa:
- Yanlış hedefe temas
- Parmak kaldırma
- Sürenin bitmesi

Sonuç:
- Düşman saldırır
- Oyuncu hasar alır
- Düşman **az hasar** yer (chip damage)

---

## 3) Execution Sonucu (Çok önemli ilke)

**Execution = Interrupt**

- Basic düşman: çoğunlukla ölür.
- Elite/Boss: her zaman tekte ölmez:
  - Büyük hasar
  - **Interrupt (saldırı iptali)**
  - Stagger

> Oyuncu doğru execution yaptıysa “aynı anda” saldırı yememeli.
> Bu kural kontrol hissini korur.

---

## 4) Rage

- Rage dolumu: başarılı chain + streak
- Rage aktifken:
  - weakpoint şartı kalkar
  - **anywhere execution**
- Rage süre bazlıdır ve upgrade ile geliştirilebilir.

---

## 5) Revive

- Chapter içinde **1 revive hakkı** (v0.1).

---

## 6) Pacing

- Wave tabanlı ilerleme.
- Aynı anda **2–3 düşman kombinasyonu** (adil telegraph şartıyla).
- Chapter süresi hedef: **60–90 saniye** (v0.1)

---

## 7) Ayarlanabilir Parametreler (Config’te yaşar)

Bu değerler kodda değil `GameConfig` (ScriptableObject) üzerinden ayarlanmalı:

- `ExecutionWindowSeconds` (default 2.0)
- `TimeScaleDuringExecution` (default 0.8)
- `TelegraphStepSeconds` (default 0.35–0.50)
- `WeakpointHitRadiusPx` (default 40–70)
- `WeakpointJitterPx` (default 5–10)
- `FailPunishDamage` (enemy -> player)
- `FailChipDamage` (player -> enemy)
- `BasicExecutionDamage`
- `EliteExecutionDamage`
- `BossExecutionDamage`
- `RageGainOnHit`
- `RageGainOnSuccess`
- `RageDurationSeconds`

---

## 8) Değişiklik Protokolü (Esneklik)

- Önce **config değerleriyle** dene.
- Yetmezse yeni modül ekle (ör: guard phase).
- En son refactor.