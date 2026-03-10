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
    /// Düşmanın Renderer bounds'unun ekran Rect'ini döndürür.
    /// Rage hit testi için CombatDirector tarafından kullanılır.
    /// </summary>
    public bool TryGetScreenRect(Camera cam, out Rect screenRect)
    {
        screenRect = Rect.zero;
        if (cam == null) return false;

        // Transform pozisyonunu ekrana çevir
        // Bounds/rotation karmaşasını önlemek için direkt transform kullan
        Vector3 worldPos = transform.position;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Kameranın önünde mi?
        // Z negatifse arkada - fliple
        if (screenPos.z < 0)
        {
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
        }

        // Düşmanın ekrandaki boyutunu scale'den tahmin et
        // EnemyPlaceHolder scale X=1.3, Y=1.8 - ekrana yansıyan boyutu mesafeye göre değişir
        float distToCamera = Mathf.Abs(worldPos.z - cam.transform.position.z);
        float screenHeightPx = Screen.height;
        float camFovRad = cam.fieldOfView * Mathf.Deg2Rad;
        float worldHeightVisible = 2f * distToCamera * Mathf.Tan(camFovRad * 0.5f);
        float scaleY = transform.lossyScale.y;
        float scaleX = transform.lossyScale.x;
        float halfH = (scaleY / worldHeightVisible) * screenHeightPx * 0.5f;
        float halfW = halfH * (scaleX / scaleY);

        // Biraz padding ekle - hit alanı görsel boyuttan biraz büyük olsun
        halfH *= 1.2f;
        halfW *= 1.2f;

        screenRect = new Rect(screenPos.x - halfW, screenPos.y - halfH, halfW * 2f, halfH * 2f);
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
