using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Combat akisinin tek kapisi.
/// </summary>
public class CombatDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameConfig config;
    [SerializeField] private WeakpointSequence weakpointSequence;
    [SerializeField] private SwipeInput swipeInput;
    [SerializeField] private WeakpointDirectionView directionView;
    [SerializeField] private ComboManager comboManager;
    [SerializeField] private RageManager rageManager;
    [SerializeField] private EnemyApproach enemyApproach;

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
        if (rageManager == null)
            rageManager = FindFirstObjectByType<RageManager>();
        if (enemyApproach == null)
            enemyApproach = FindFirstObjectByType<EnemyApproach>();
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
        if (firstTouchMade && !swipeInput.IsDown && weakpointSequence.CurrentPhase == WeakpointSequence.Phase.ExecutionWindow)
        {
            weakpointSequence.ForceFailExternal("FingerLift");
            return;
        }

        if (!swipeInput.IsDown) return;

        // --- Hit testi ---
        Vector2 delta = swipeInput.RawDeltaPx;
        if (delta.magnitude < minDeltaPx) return;

        // RAGE HIT
        if (rageManager != null && rageManager.IsRageActive)
        {
            if (hitRegisteredThisTarget) return;
            hitRegisteredThisTarget = true;
            firstTouchMade = true;
            comboManager?.RegisterHit();
            FeedbackManager.Instance?.PlayRageHitFeedback();
            Debug.Log("[CombatDirector] RAGE HIT!");
            weakpointSequence.SubmitHit();
            return;
        }

        // NORMAL HIT
        Vector2 markerScreenPos;
        if (!directionView.TryGetActiveMarkerScreenPos(out markerScreenPos)) return;

        Vector2 fingerPos = swipeInput.FingerPosition;
        float dist = Vector2.Distance(fingerPos, markerScreenPos);
        if (dist > hitRadiusPx) return;

        if (hitRegisteredThisTarget) return;
        hitRegisteredThisTarget = true;

        firstTouchMade = true;
        comboManager?.RegisterHit();
        rageManager?.RegisterHit();
        FeedbackManager.Instance?.PlayHitFeedback();
        Debug.Log($"[CombatDirector] HIT! dist={dist:F0}px");
        weakpointSequence.SubmitHit();
    }

    // --- Event handlers ---

    private void HandleExecutionWindowStart(float duration)
    {
        executionOpen = true;
        firstTouchMade = false;
        hitRegisteredThisTarget = false;
        Debug.Log("[CombatDirector] Execution acildi.");

        if (rageManager != null && rageManager.IsRageActive)
            directionView.HideAll();
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
        enemyApproach?.SetRageVisual(false);
        FeedbackManager.Instance?.PlayChainSuccessFeedback();
        OnCombatSuccess?.Invoke();
        Debug.Log($"[CombatDirector] BASARI! Toplam={successCount}");
    }

    private void HandleChainFail(string reason)
    {
        failCount++;
        executionOpen = false;
        firstTouchMade = false;
        hitRegisteredThisTarget = false;

        comboManager?.RegisterTimeout();
        rageManager?.ResetRage();
        enemyApproach?.SetRageVisual(false);
        FeedbackManager.Instance?.PlayFailFeedback();
        OnCombatFail?.Invoke(reason);
        Debug.Log($"[CombatDirector] FAIL. Sebep={reason} Toplam={failCount}");

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
        weakpointSequence.StartExecutionDirectly(activeChain);
    }

    // --- Public API ---

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

        if (rageManager != null && rageManager.IsRageActive)
        {
            enemyApproach?.SetRageVisual(true);
            weakpointSequence.StartExecutionDirectly(chain);
        }
        else
            weakpointSequence.StartSequence(chain);
    }

    public WeakpointSequence GetSequence() => weakpointSequence;
}
