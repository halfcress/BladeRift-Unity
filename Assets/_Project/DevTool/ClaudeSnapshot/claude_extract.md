# CLAUDE EXTRACT
snapshot: a342690 | 2026-03-12 13:26:12
compile: clean
command: EnemyApproach, CombatTriggerTest --scene --logs

## EnemyApproach.cs
path: Assets\_Project\Scripts\Combat\EnemyApproach.cs
```csharp
using UnityEngine;

/// <summary>
/// Düşmanın koridordan yaklaşmasını yönetir.
/// </summary>
public class EnemyApproach : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatDirector combatDirector;

    [Header("Approach Settings")]
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, 30f);
    [SerializeField] private Vector3 stopPosition = new Vector3(0f, 0f, 6f);
    [SerializeField] private float approachSpeed = 4f;
    [SerializeField] private float telegraphTriggerDistance = 8f;

    [Header("Chain Settings")]
    [SerializeField]
    private System.Collections.Generic.List<WeakpointDirection> chain = new()
    {
        WeakpointDirection.Right,
        WeakpointDirection.Up,
        WeakpointDirection.Left
    };

    [Header("Rage Hit")]
    [Tooltip("Silüet hit alanını genişletme çarpanı. 1.0 = tam bounds, 1.3 = %30 padding")]
    [SerializeField] private float rageHitPadding = 1.3f;

    [Header("Timing")]
    [SerializeField] private float deathPauseSeconds = 1.5f;
    [SerializeField] private float respawnDelaySeconds = 1.0f;
    [SerializeField] private float deathFlashDuration = 0.3f;

    [Header("State (Read-only)")]
    [SerializeField] private State currentState = State.Idle;

    public enum State { Idle, Approaching, TelegraphTriggered, WaitingForResult, Dead }

    private Renderer cachedRenderer;

    private void Awake()
    {
        if (combatDirector == null)
            combatDirector = FindFirstObjectByType<CombatDirector>();
        cachedRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnEnable()
    {
        if (combatDirector == null) return;
        combatDirector.OnCombatSuccess += HandleCombatSuccess;
        combatDirector.OnCombatFail += HandleCombatFail;
    }

    private void OnDisable()
    {
        if (combatDirector == null) return;
        combatDirector.OnCombatSuccess -= HandleCombatSuccess;
        combatDirector.OnCombatFail -= HandleCombatFail;
    }

    private void Start()
    {
        StartApproach();
    }

    private void Update()
    {
        if (currentState != State.Approaching) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            stopPosition,
            approachSpeed * Time.deltaTime
        );

        float distToCamera = transform.position.z;
        if (distToCamera <= telegraphTriggerDistance)
            TriggerTelegraph();
    }

    // --- Public: Rage hit testi için ekran sınırları ---

    /// <summary>
    /// Düşmanın Renderer bounds'unun 8 köşesini ekrana projekte ederek
    /// gerçek ekran Rect'ini döndürür.
    /// Rage hit testi için CombatDirector tarafından kullanılır.
    /// </summary>
    public bool TryGetScreenRect(Camera cam, out Rect screenRect)
    {
        screenRect = Rect.zero;
        if (cam == null || cachedRenderer == null) return false;

        Bounds bounds = cachedRenderer.bounds;

        // Bounds'un 8 köşesini hesapla
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3[] corners = new Vector3[8];
        corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        corners[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[3] = center + new Vector3(-extents.x, extents.y, extents.z);
        corners[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[5] = center + new Vector3(extents.x, -extents.y, extents.z);
        corners[6] = center + new Vector3(extents.x, extents.y, -extents.z);
        corners[7] = center + new Vector3(extents.x, extents.y, extents.z);

        // Her köşeyi ekran koordinatına çevir
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < 8; i++)
        {
            Vector3 sp = cam.WorldToScreenPoint(corners[i]);

            // Kameranın arkasındaysa geçersiz
            if (sp.z < 0f) return false;

            if (sp.x < minX) minX = sp.x;
            if (sp.x > maxX) maxX = sp.x;
            if (sp.y < minY) minY = sp.y;
            if (sp.y > maxY) maxY = sp.y;
        }

        // Padding uygula — hit alanını biraz genişlet
        float w = maxX - minX;
        float h = maxY - minY;
        float padW = w * (rageHitPadding - 1f) * 0.5f;
        float padH = h * (rageHitPadding - 1f) * 0.5f;

        screenRect = new Rect(minX - padW, minY - padH, w + padW * 2f, h + padH * 2f);

        return true;
    }

    // --- Internal ---

    private void StartApproach()
    {
        transform.position = spawnPosition;
        currentState = State.Approaching;
        Debug.Log("[EnemyApproach] Yaklaşma başladı.");
    }

    private void TriggerTelegraph()
    {
        currentState = State.TelegraphTriggered;
        Debug.Log("[EnemyApproach] Telegraph tetiklendi.");
        combatDirector.StartCombatSequence(chain);
        currentState = State.WaitingForResult;
    }

    private void HandleCombatSuccess()
    {
        currentState = State.Dead;
        Debug.Log("[EnemyApproach] Düşman öldü. Sonraki başlıyor...");
        StartCoroutine(DeathFlashThenRespawn());
    }

    private System.Collections.IEnumerator DeathFlashThenRespawn()
    {
        if (cachedRenderer != null)
        {
            Color original = cachedRenderer.material.color;
            cachedRenderer.material.color = Color.red;
            yield return new WaitForSeconds(deathFlashDuration);
            cachedRenderer.material.color = original;
        }
        StartCoroutine(RespawnAfterDelay());
    }

    public void SetRageVisual(bool rageActive)
    {
        Transform outline = transform.Find("OutlineQuad");
        if (outline != null)
            outline.gameObject.SetActive(rageActive);
    }

    private void HandleCombatFail(string reason)
    {
        Debug.Log($"[EnemyApproach] Fail ({reason}), düşman bekliyor.");
        StartCoroutine(PunishFlash());
    }

    private System.Collections.IEnumerator PunishFlash()
    {
        if (cachedRenderer != null)
        {
            Color original = cachedRenderer.material.color;
            cachedRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            cachedRenderer.material.color = original;
        }
    }

    private System.Collections.IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSecondsRealtime(deathPauseSeconds);
        yield return new WaitForSecondsRealtime(respawnDelaySeconds);
        StartApproach();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spawnPosition, 0.3f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(stopPosition, 0.3f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(stopPosition.x, stopPosition.y, telegraphTriggerDistance), 0.3f);
    }
}
```

## CombatTriggerTest.cs
path: Assets\_Project\Scripts\Combat\CombatTriggerTest.cs
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SADECE TEST AMACLI. Nihai sistem değildir.
/// Play'e basınca 1 saniye bekler, sonra otomatik bir zincir başlatır.
/// Konsol loglarını izleyerek hit/miss test edebilirsin.
/// </summary>
public class CombatTriggerTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatDirector combatDirector;

    [Header("Test Chain")]
    [SerializeField] private List<WeakpointDirection> testChain = new List<WeakpointDirection>
    {
        WeakpointDirection.Right,
        WeakpointDirection.Up,
        WeakpointDirection.Left
    };

    [Header("Settings")]
    [Tooltip("Play'den kaç saniye sonra zincir baslasin.")]
    [SerializeField] private float delaySeconds = 1.5f;

    [Tooltip("Zincir bitince kaç saniye sonra yeniden baslasin. 0 = tekrar baslatma.")]
    [SerializeField] private float repeatAfterSeconds = 3f;

    private void Awake()
    {
        if (combatDirector == null)
            combatDirector = FindFirstObjectByType<CombatDirector>();
    }

    private void Start()
    {
        StartCoroutine(TriggerLoop());
    }

private IEnumerator TriggerLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(delaySeconds);

            Debug.Log("CombatTriggerTest: Zincir baslatiliyor...");
            combatDirector.StartCombatSequence(new List<WeakpointDirection>(testChain));

            // Zincir bitene kadar bekle (Done veya Idle olana kadar)
            yield return new WaitUntil(() =>
            {
                var seq = combatDirector.GetSequence();
                if (seq == null) return true;
                var phase = seq.CurrentPhase;
                return !combatDirector.IsCombatActive && !combatDirector.IsWaitingForRetry;
            });

            if (repeatAfterSeconds <= 0f)
                yield break;

            yield return new WaitForSecondsRealtime(repeatAfterSeconds);
        }
    }
}

```

## Scene Hierarchy
GameRoot [CombatDirector | CorridorLoop | CombatTriggerTest | WeakpointSequence | ComboManager | RageManager]
  Corridor_01
    Wall_Left [MeshFilter | MeshRenderer | BoxCollider]
    Wall_Right [MeshFilter | MeshRenderer | BoxCollider]
    Floor [MeshFilter | MeshRenderer | BoxCollider]
    Ceiling [MeshFilter | MeshRenderer | BoxCollider]
    TorchLight_Left01 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Left02 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Left03 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Left04 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right01 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right02 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right03 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right04 [Light | Rendering.Universal.UniversalAdditionalLightData]
  Corridor_02
    Wall_Left [MeshFilter | MeshRenderer | BoxCollider]
    Wall_Right [MeshFilter | MeshRenderer | BoxCollider]
    Floor [MeshFilter | MeshRenderer | BoxCollider]
    Ceiling [MeshFilter | MeshRenderer | BoxCollider]
    TorchLight_Left01 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Left02 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Left03 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Left04 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right01 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right02 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right03 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right04 [Light | Rendering.Universal.UniversalAdditionalLightData]
  Corridor_03
    Wall_Left [MeshFilter | MeshRenderer | BoxCollider]
    Wall_Right [MeshFilter | MeshRenderer | BoxCollider]
    Floor [MeshFilter | MeshRenderer | BoxCollider]
    Ceiling [MeshFilter | MeshRenderer | BoxCollider]
    TorchLight_Left01 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Left02 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Left03 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Left04 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right01 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right02 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right03 [Light | Rendering.Universal.UniversalAdditionalLightData]
    TorchLight_Right04 [Light | Rendering.Universal.UniversalAdditionalLightData]
  InputRoot [SwipeInput]
  EnemyRoot
    EnemySpawnPoint
      EnemyPlaceHolder [MeshFilter | MeshRenderer | BillboardFacing | EnemyApproach]
        WeakPoint_1 [MeshFilter | MeshRenderer | SphereCollider]
        WeakPoint_2 [MeshFilter | MeshRenderer | SphereCollider]
        WeakPoint_3 [MeshFilter | MeshRenderer | SphereCollider]
        OutlineQuad [MeshFilter | MeshRenderer | MeshCollider]
  FeedbackManager [FeedbackManager]
EventSystem [EventSystems.EventSystem | InputSystem.UI.InputSystemUIInputModule]
Main Camera [Camera | AudioListener | Rendering.Universal.UniversalAdditionalCameraData]
  SlashTrail [MeshFilter | MeshRenderer | SlashTrail]
Directional Light [Light | Rendering.Universal.UniversalAdditionalLightData]
Player_Reference [MeshFilter | MeshRenderer | BoxCollider]
Global Volume [Rendering.Volume]
UIRoot [Canvas | UI.CanvasScaler | UI.GraphicRaycaster]
  WeakpointMarkerRoot [WeakpointDirectionView]
    WeakpointMarker_1 [CanvasRenderer | UI.Image | CanvasGroup]
    WeakpointMarker_2 [CanvasRenderer | UI.Image | CanvasGroup]
    WeakpointMarker_3 [CanvasRenderer | UI.Image | CanvasGroup]
  ComboText [CanvasRenderer | TMPro.TextMeshProUGUI]
  HitCountText [CanvasRenderer | TMPro.TextMeshProUGUI]
  RageText [CanvasRenderer | TMPro.TextMeshProUGUI]
AudioManager [AudioManager]

## Console Logs (son 20)
(Console log yakalama için MINI snapshot kullanılabilir)

