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
    [SerializeField] private Camera mainCamera;

    [Header("Hit Test")]
    [SerializeField] private float hitRadiusPx = 80f;
    [SerializeField] private float minDeltaPx = 8f;
    [SerializeField] private float rageMinDeltaPx = 2f;

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
    public bool IsWorldBlocked => combatActive || (enemyApproach != null && enemyApproach.IsInDeathSequence);

    private List<WeakpointZone> activeChain = new();
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
        if (mainCamera == null)
            mainCamera = Camera.main;

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
        if (weakpointSequence == null || swipeInput == null) return;
        if (weakpointSequence.CurrentPhase != WeakpointSequence.Phase.ExecutionWindow) return;

        bool isRage = rageManager != null && rageManager.IsRageActive;

        if (firstTouchMade && !swipeInput.IsDown)
        {
            weakpointSequence.ForceFailExternal("FingerLift");
            return;
        }

        if (!swipeInput.IsDown) return;

        Vector2 delta = swipeInput.RawDeltaPx;
        float requiredDelta = isRage ? rageMinDeltaPx : minDeltaPx;
        if (delta.magnitude < requiredDelta) return;

        if (directionView == null) return;

        Vector2 markerScreenPos;
        if (!directionView.TryGetActiveMarkerScreenPos(out markerScreenPos)) return;

        Vector2 fp = swipeInput.FingerPosition;
        float dist = Vector2.Distance(fp, markerScreenPos);
        if (dist > hitRadiusPx) return;

        if (hitRegisteredThisTarget) return;
        hitRegisteredThisTarget = true;

        firstTouchMade = true;
        comboManager?.RegisterHit();

        if (!isRage)
            rageManager?.RegisterHit();

        if (isRage)
        {
            FeedbackManager.Instance?.PlayRageHitFeedback();
            AudioManager.Instance?.PlayHitRage();
            Debug.Log($"[CombatDirector] RAGE HIT! dist={dist:F0}px");
        }
        else
        {
            FeedbackManager.Instance?.PlayHitFeedback();
            AudioManager.Instance?.PlayHitNormal();
            Debug.Log($"[CombatDirector] HIT! dist={dist:F0}px");
        }

        weakpointSequence.SubmitHit();
    }

    private void HandleExecutionWindowStart(float duration)
    {
        executionOpen = true;
        firstTouchMade = false;
        hitRegisteredThisTarget = false;
        Debug.Log("[CombatDirector] Execution acildi.");
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
        AudioManager.Instance?.PlayChainSuccess();
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
        AudioManager.Instance?.PlayFailPunish();
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

    public void StartCombatSequence(List<WeakpointZone> chain)
    {
        if (weakpointSequence == null)
        {
            Debug.LogError("[CombatDirector] WeakpointSequence yok!");
            return;
        }

        bool isRage = rageManager != null && rageManager.IsRageActive;

        if (isRage)
        {
            activeChain = new List<WeakpointZone> { WeakpointZone.Chest };
            Debug.Log("[CombatDirector] RAGE: Tek rage weakpoint kullanılacak.");
        }
        else
        {
            activeChain = new List<WeakpointZone>(chain);
        }

        combatActive = true;
        executionOpen = false;
        firstTouchMade = false;
        hitRegisteredThisTarget = false;
        waitingForRetry = false;

        if (isRage)
        {
            Debug.Log("[CombatDirector] SetRageVisual(true) cagriliyor.");
            enemyApproach?.SetRageVisual(true);
            weakpointSequence.StartExecutionDirectly(activeChain);
        }
        else
        {
            weakpointSequence.StartSequence(activeChain);
        }
    }

    public WeakpointSequence GetSequence() => weakpointSequence;

    public void SetCurrentEnemy(EnemyApproach newEnemyApproach)
    {
        enemyApproach = newEnemyApproach;
    }
}
