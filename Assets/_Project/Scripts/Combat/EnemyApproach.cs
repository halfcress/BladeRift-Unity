using UnityEngine;

/// <summary>
/// Düşmanın koridordan yaklaşmasını yönetir.
/// Belirli bir mesafeye gelince CombatDirector'a telegraph başlatması için sinyal verir.
/// 
/// Akış:
///   Idle -> Approaching -> TelegraphTriggered -> WaitingForResult -> Dead / Reset
/// </summary>
public class EnemyApproach : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatDirector combatDirector;

    [Header("Approach Settings")]
    [Tooltip("Düşmanın başlangıç pozisyonu (kameradan uzak nokta)")]
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, 30f);

    [Tooltip("Düşmanın durduğu pozisyon (telegraph tetiklenince durur)")]
    [SerializeField] private Vector3 stopPosition = new Vector3(0f, 0f, 6f);

    [Tooltip("Yaklaşma hızı (birim/saniye)")]
    [SerializeField] private float approachSpeed = 4f;

    [Tooltip("Bu mesafeye gelince telegraph başlar (kameradan Z mesafesi)")]
    [SerializeField] private float telegraphTriggerDistance = 8f;

    [Header("Chain Settings")]
    [SerializeField] private System.Collections.Generic.List<WeakpointDirection> chain = new()
    {
        WeakpointDirection.Right,
        WeakpointDirection.Up,
        WeakpointDirection.Left
    };

    [Header("Timing")]
    [Tooltip("Success sonrası bir sonraki düşman başlamadan önce bekleme (saniye)")]
    [SerializeField] private float deathPauseSeconds = 1.5f;

    [Tooltip("Spawn pozisyonuna dönmeden önce bekleme (saniye)")]
    [SerializeField] private float respawnDelaySeconds = 1.0f;

    [Tooltip("Ölüm flash süresi (saniye)")]
    [SerializeField] private float deathFlashDuration = 0.3f;

    [Header("State (Read-only)")]
    [SerializeField] private State currentState = State.Idle;

    public enum State { Idle, Approaching, TelegraphTriggered, WaitingForResult, Dead }

    private void Awake()
    {
        if (combatDirector == null)
            combatDirector = FindFirstObjectByType<CombatDirector>();
    }

    private void OnEnable()
    {
        if (combatDirector == null) return;
        combatDirector.OnCombatSuccess += HandleCombatSuccess;
        combatDirector.OnCombatFail    += HandleCombatFail;
    }

    private void OnDisable()
    {
        if (combatDirector == null) return;
        combatDirector.OnCombatSuccess -= HandleCombatSuccess;
        combatDirector.OnCombatFail    -= HandleCombatFail;
    }

    private void Start()
    {
        StartApproach();
    }

    private void Update()
    {
        if (currentState != State.Approaching) return;

        // Kameraya doğru ilerle
        transform.position = Vector3.MoveTowards(
            transform.position,
            stopPosition,
            approachSpeed * Time.deltaTime
        );

        // Telegraph mesafesine geldi mi?
        float distToCamera = transform.position.z;
        if (distToCamera <= telegraphTriggerDistance)
        {
            TriggerTelegraph();
        }
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
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Color original = rend.material.color;
            rend.material.color = Color.red;
            yield return new WaitForSeconds(deathFlashDuration);
            rend.material.color = original;
        }
        StartCoroutine(RespawnAfterDelay());
    }

    private void HandleCombatFail(string reason)
    {
        Debug.Log($"[EnemyApproach] Fail ({reason}), düşman bekliyor.");
        StartCoroutine(PunishFlash());
    }

    private System.Collections.IEnumerator PunishFlash()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Color original = rend.material.color;
            rend.material.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            rend.material.color = original;
        }
    }

    private System.Collections.IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSecondsRealtime(deathPauseSeconds);
        yield return new WaitForSecondsRealtime(respawnDelaySeconds);
        StartApproach();
    }

    // --- Editor helper ---
    private void OnDrawGizmosSelected()
    {
        // Spawn pozisyonu - mavi
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spawnPosition, 0.3f);

        // Stop pozisyonu - yeşil
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(stopPosition, 0.3f);

        // Telegraph mesafesi - sarı
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(stopPosition.x, stopPosition.y, telegraphTriggerDistance), 0.3f);
    }
}
