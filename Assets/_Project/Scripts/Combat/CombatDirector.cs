using UnityEngine;

/// <summary>
/// Combat akisinin tek kapisi.
/// Fruit Ninja tarzi: sadece timeout = fail, finger lift combo'yu bozmaz.
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
    [SerializeField] private float directionDotThreshold = 0.3f;

    [Header("State (Read-only)")]
    [SerializeField] private bool combatActive = false;
    [SerializeField] private int successCount = 0;
    [SerializeField] private int failCount = 0;

    private bool hitRegisteredThisTarget = false;

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

        Debug.Log($"[CombatDirector] Awake OK.");
    }

    private void OnEnable()
    {
        if (weakpointSequence != null)
        {
            weakpointSequence.OnChainSuccess += HandleChainSuccess;
            weakpointSequence.OnChainFail    += HandleChainFail;
            weakpointSequence.OnChainAdvance += HandleChainAdvance;
        }
    }

    private void OnDisable()
    {
        if (weakpointSequence != null)
        {
            weakpointSequence.OnChainSuccess -= HandleChainSuccess;
            weakpointSequence.OnChainFail    -= HandleChainFail;
            weakpointSequence.OnChainAdvance -= HandleChainAdvance;
        }
    }

private void Update()
    {
        if (!combatActive) return;
        if (weakpointSequence == null || swipeInput == null || directionView == null) return;
        if (weakpointSequence.CurrentPhase != WeakpointSequence.Phase.ExecutionWindow) return;

        // RawDeltaPx: deadzone yok, kucuk hareketleri de yakalar
        Vector2 delta = swipeInput.RawDeltaPx;
        if (delta.magnitude < minDeltaPx) return;

        Vector2 markerScreenPos;
        if (!directionView.TryGetActiveMarkerScreenPos(out markerScreenPos)) return;

        Vector2 fingerPos = swipeInput.FingerPosition;
        float dist = Vector2.Distance(fingerPos, markerScreenPos);
        if (dist > hitRadiusPx) return;

        WeakpointDirection target = weakpointSequence.CurrentTarget;
        Vector2 expectedDir = GetExpectedVector(target);
        float dot = Vector2.Dot(delta.normalized, expectedDir);
        if (dot < directionDotThreshold) return;

        if (hitRegisteredThisTarget) return;
        hitRegisteredThisTarget = true;

        comboManager?.RegisterHit();
        Debug.Log($"[CombatDirector] HIT! target={target} dist={dist:F0}px dot={dot:F2}");
        weakpointSequence.SubmitHit();
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
        hitRegisteredThisTarget = false;
        comboManager?.RegisterChainSuccess();
        Debug.Log($"[CombatDirector] BASARI! Toplam={successCount}");
    }

    private void HandleChainFail(string reason)
    {
        failCount++;
        combatActive = false;
        hitRegisteredThisTarget = false;
        if (reason == "Timeout")
            comboManager?.RegisterTimeout();
        Debug.Log($"[CombatDirector] BASARISIZ. Sebep={reason}");
    }

    public void StartCombatSequence(System.Collections.Generic.List<WeakpointDirection> chain)
    {
        if (weakpointSequence == null) { Debug.LogError("[CombatDirector] WeakpointSequence yok!"); return; }
        combatActive = true;
        hitRegisteredThisTarget = false;
        weakpointSequence.StartSequence(chain);
    }

    public WeakpointSequence GetSequence() => weakpointSequence;

    private Vector2 GetExpectedVector(WeakpointDirection dir)
    {
        switch (dir)
        {
            case WeakpointDirection.Up:    return Vector2.up;
            case WeakpointDirection.Down:  return Vector2.down;
            case WeakpointDirection.Left:  return Vector2.left;
            case WeakpointDirection.Right: return Vector2.right;
            default:                       return Vector2.zero;
        }
    }
}
