using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Combat akisinin tek kapisi.
///
/// Hit kurali: yön önemli degil.
/// Parmak aktif weakpoint marker'inin içinden geçerse HIT sayilir.
///
/// Finger-lift fail:
/// - Execution açilmadan parmak kalkmasi: sorun yok
/// - Execution açildiktan sonra ilk temas yapilmissa ve parmak kalkarsa: FAIL
///
/// Fail sonrasi retry:
/// - Telegraph tekrar oynatilmaz
/// - Ayni dusmanin zinciri 1. weakpoint'ten yeniden baslar
/// </summary>
public class CombatDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameConfig config;
    [SerializeField] private WeakpointSequence weakpointSequence;
    [SerializeField] private SwipeInput swipeInput;
    [SerializeField] private WeakpointDirectionView directionView;
    [SerializeField] private ComboManager comboManager;

    [Header("Hit Test")]
    [SerializeField] private float hitRadiusPx = 80f;
    [SerializeField] private float minDeltaPx = 8f;

    [Header("Retry")]
    [SerializeField] private float retryDelaySeconds = 1.0f;

    [Header("State (Read-only)")]
    [SerializeField] private bool combatActive = false;
    [SerializeField] private bool executionOpen = false;
    [SerializeField] private bool firstTouchMade = false;
    [SerializeField] private int successCount = 0;
    [SerializeField] private int failCount = 0;

    public event System.Action OnCombatSuccess;
    public event System.Action<string> OnCombatFail;

    public bool IsCombatActive => combatActive;
    public bool IsWaitingForRetry => waitingForRetry;

    // Retry icin aktif zinciri hatirla
    private List<WeakpointDirection> activeChain = new List<WeakpointDirection>();

    private bool hitRegisteredThisTarget = false;
    private bool waitingForRetry = false;

    private void Awake()
    {
        if (swipeInput == null)
            swipeInput = FindFirstObjectByType<SwipeInput>();
        if (weakpointSequence == null)
            weakpointSequence = FindFirstObjectByType<WeakpointSequence>();
        if (directionView == null)
            directionView = FindFirstObjectByType<WeakpointDirectionView>();
        if (comboManager == null)
            comboManager = FindFirstObjectByType<ComboManager>();

        Debug.Log("[CombatDirector] Awake OK.");
    }

    private void OnEnable()
    {
        if (weakpointSequence == null) return;
        weakpointSequence.OnExecutionWindowStart += HandleExecutionWindowStart;
        weakpointSequence.OnChainAdvance += HandleChainAdvance;
        weakpointSequence.OnChainSuccess += HandleChainSuccess;
        weakpointSequence.OnChainFail += HandleChainFail;
    }

    private void OnDisable()
    {
        if (weakpointSequence == null) return;
        weakpointSequence.OnExecutionWindowStart -= HandleExecutionWindowStart;
        weakpointSequence.OnChainAdvance -= HandleChainAdvance;
        weakpointSequence.OnChainSuccess -= HandleChainSuccess;
        weakpointSequence.OnChainFail -= HandleChainFail;
    }

    private void Update()
    {
        if (!combatActive) return;
        if (waitingForRetry) return;
        if (!executionOpen) return;
        if (weakpointSequence == null || swipeInput == null || directionView == null) return;
        if (weakpointSequence.CurrentPhase != WeakpointSequence.Phase.ExecutionWindow) return;

        // --- Finger-lift fail ---
        // Ilk temas yapildiysa ve parmak kalktiysa: FAIL
        if (firstTouchMade && !swipeInput.IsDown && weakpointSequence.CurrentPhase == WeakpointSequence.Phase.ExecutionWindow)
        {
            weakpointSequence.ForceFailExternal("FingerLift");
            return;
        }

        // Parmak basili degil: hit test yapma ama fail da uretme
        if (!swipeInput.IsDown) return;

      

        // --- Hit testi ---
        // Kural: yön önemli degil, sadece parmak aktif marker'in içinden geçmeli
        Vector2 delta = swipeInput.RawDeltaPx;
        if (delta.magnitude < minDeltaPx) return;

        Vector2 markerScreenPos;
        if (!directionView.TryGetActiveMarkerScreenPos(out markerScreenPos)) return;

        Vector2 fingerPos = swipeInput.FingerPosition;
        float dist = Vector2.Distance(fingerPos, markerScreenPos);
        if (dist > hitRadiusPx) return;

        // Ayni hedefe birden fazla hit kaydetme
        if (hitRegisteredThisTarget) return;
        hitRegisteredThisTarget = true;

        firstTouchMade = true;
        comboManager?.RegisterHit();
        Debug.Log($"[CombatDirector] HIT! dist={dist:F0}px");
        weakpointSequence.SubmitHit();
    }

    // --- Event handlers ---

    private void HandleExecutionWindowStart(float duration)
    {
        executionOpen = true;
        firstTouchMade = false;
        hitRegisteredThisTarget = false;
        Debug.Log("[CombatDirector] Execution açildi.");
    }

    private void HandleChainAdvance(int newIndex)
    {
        hitRegisteredThisTarget = false;
        Debug.Log($"[CombatDirector] Chain advance -> {newIndex}");
    }

    private void HandleChainSuccess()
    {
        successCount++;
        combatActive = false;
        executionOpen = false;
        firstTouchMade = false;
        hitRegisteredThisTarget = false;
        comboManager?.RegisterChainSuccess();
        OnCombatSuccess?.Invoke();
        Debug.Log($"[CombatDirector] BASARI! Toplam={successCount}");
    }

    private void HandleChainFail(string reason)
    {
        failCount++;
        executionOpen = false;
        firstTouchMade = false;
        hitRegisteredThisTarget = false;

        // Combo ve rage sifirla
        comboManager?.RegisterTimeout(); // hem timeout hem finger-lift icin combo reset
        OnCombatFail?.Invoke(reason);
        Debug.Log($"[CombatDirector] FAIL. Sebep={reason} Toplam={failCount}");

        // Retry: telegraph tekrar oynatilmadan 1. weakpoint'ten basla
        if (combatActive && activeChain != null && activeChain.Count > 0)
            StartCoroutine(RetryAfterDelay());
        else
            combatActive = false;
    }

    private IEnumerator RetryAfterDelay()
    {
        waitingForRetry = true;
        yield return new WaitForSecondsRealtime(retryDelaySeconds);
        waitingForRetry = false;

        if (!combatActive) yield break;

        Debug.Log("[CombatDirector] Retry: ayni dusmanin zinciri yeniden basliyor.");
        // Telegraph tekrar oynatilmaz: dogrudan execution'a gec
        weakpointSequence.StartExecutionDirectly(activeChain);
    }

    // --- Public API ---

    /// <summary>
    /// Yeni bir dusmanin zincirini baslatir. Telegraph dahil tam akis.
    /// </summary>
    public void StartCombatSequence(List<WeakpointDirection> chain)
    {
        if (weakpointSequence == null)
        {
            Debug.LogError("[CombatDirector] WeakpointSequence yok!");
            return;
        }

        activeChain = new List<WeakpointDirection>(chain);
        combatActive = true;
        executionOpen = false;
        firstTouchMade = false;
        hitRegisteredThisTarget = false;
        waitingForRetry = false;

        weakpointSequence.StartSequence(chain);
    }

    public WeakpointSequence GetSequence() => weakpointSequence;
}