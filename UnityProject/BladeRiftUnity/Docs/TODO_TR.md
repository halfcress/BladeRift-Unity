# TODO (MASTER) - BladeRift

## PHASE 0 — Foundation
- [x] Repo oluştur (BladeRift-Unity)
- [x] Unity 6000.3.10f1 LTS proje (Universal 3D/URP)
- [x] .gitignore ile Library/Temp/Logs vb. ignore
- [x] İlk commit + push
- [ ] Docs dosyaları (README + CHAT_STATE + TODO) repoya ekle

## PHASE 1 — Combat Core (Enemy yok, koridor yok)
Amaç: sadece UI + input ile “weak point zinciri” prototipini çalıştırmak.
- [ ] Canvas + WeakpointOverlay
- [ ] Touch+Mouse unified input (continuous swipe session)
- [ ] Weakpoint marker pool (UI)
- [ ] Zincir: rastgele noktalar, sırayla yanma + tick
- [ ] Execution window: 2 sn + mini slow motion
- [ ] Kural: finger up = reset
- [ ] Kural: sadece sıradaki hedef highlight
- [ ] Micro jitter
- [ ] Success/Fail feedback (UI text + flash placeholder)

## PHASE 2 — Enemy Placeholder
- [ ] Enemy placeholder (capsule)
- [ ] Yaklaşma
- [ ] Telegraph -> zinciri başlatır
- [ ] Doğru chain -> interrupt + stagger + hasar
- [ ] Yanlış -> enemy attack + oyuncu hasar
- [ ] Elite mantığı (tek chain her zaman öldürmez)

## PHASE 3 — Wave / Chapter
- [ ] Wave director (2–3 enemy aynı anda)
- [ ] Chapter 60–90 sn
- [ ] Chapter end ekranı
- [ ] 1 revive

## PHASE 4 — Rage
- [ ] Rage bar dolumu
- [ ] Rage aktif: anywhere execution
- [ ] Rage süre yönetimi

## PHASE 5 — Loot & Meta (Chapter sonunda)
- [ ] Loot: 3 seçenek
- [ ] 1 free reroll
- [ ] Loot x2 (buton placeholder)
- [ ] Meta upgrade v0 + save/load

## PHASE 6+ (Sonra)
- Boss template
- Monetizasyon (timer speed-up + reroll)
- Polish