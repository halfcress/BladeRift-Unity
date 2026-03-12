using UnityEngine;

/// <summary>
/// Düşmanın koridordan yaklaşmasını yönetir.
/// </summary>
public class EnemyApproach : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatDirector combatDirector;
    [SerializeField] private EnemyArchetypeData archetypeData;

    [Header("Approach Settings")]
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, 30f);
    [SerializeField] private Vector3 stopPosition = new Vector3(0f, 0f, 6f);
    [SerializeField] private float approachSpeed = 4f;
    [SerializeField] private float telegraphTriggerDistance = 8f;

   

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
            archetypeData.approachSpeed * Time.deltaTime
        );

        float distToCamera = transform.position.z;
        if (distToCamera <= archetypeData.telegraphTriggerDistance)
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

        if (archetypeData == null)
        {
            Debug.LogError("[EnemyApproach] archetypeData yok!");
            return;
        }

        if (archetypeData.pattern == null || archetypeData.pattern.Length == 0)
        {
            Debug.LogError("[EnemyApproach] archetypeData.pattern bos!");
            return;
        }

        combatDirector.StartCombatSequence(
            new System.Collections.Generic.List<WeakpointDirection>(archetypeData.pattern)
        );

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