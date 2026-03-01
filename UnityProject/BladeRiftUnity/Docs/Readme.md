# BladeRift (Unity)

Mobil odaklı, 1. şahıs dungeon koridorunda geçen, **tek input (swipe)** ile oynanan aksiyon oyun prototipi.

## Unity
- Unity: **6000.3.10f1 LTS**
- Template: **Universal 3D (URP)**

## Core Gameplay (Kilitli Tasarım)
- Oyuncu sabit, dünya üzerimize akıyormuş gibi (koridor UV scroll).
- Tek input: **continuous swipe**.
- Düşman üzerinde **weak point zinciri** görünür (2D UI overlay).
- Weak point’ler sırayla yanar (tick sesi ile).
- Sonra **2 sn execution window** açılır (mini slow motion).
- Parmak kalkarsa: chain iptal/reset.
- Sadece sıradaki hedef daha parlak.
- Weak point’ler mikro hareket (jitter) yapar.
- Doğru zincir:
  - Basic: genelde ölür
  - Elite/Boss: **büyük hasar + interrupt + stagger** (her zaman tekte ölmez)
- Yanlış dokunuş: düşman saldırır, oyuncu hasar yer.
- Rage dolunca: weak point şartı kalkar, **anywhere execution**.
- 1 revive hakkı (chapter içinde).

## Hedef
- Çalışan, paylaşılabilir **APK v0.1** üretmek.

## Çalışma Kuralları
- Kod değişiklikleri küçük parçalara bölünür (dosya dosya).
- Her oturum sonunda `Docs/CHAT_STATE.md` güncellenir.

## Docs
- Docs/GAME_CONCEPT_TR.md
- Docs/GAME_RULES_TR.md
- Docs/ARCHITECTURE_TR.md
- Docs/TODO_TR.md
- Docs/CHAT_STATE.md