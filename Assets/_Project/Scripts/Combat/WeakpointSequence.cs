using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weakpoint zincirinin tum mantigini yonetir.
///
/// Telegraph akisi:
///   1) TelegraphReveal: marker'lar 1er saniye arayla sirayla belirir
///   2) TelegraphHold:   hepsi 1 saniye birlikte ekranda durur
///   3) ExecutionWindow: hepsi kaybolur, sadece siradaki kalir; oyuncu swipe yapar
/// </summary>
public class WeakpointSequence : MonoBehaviour
{
    // ─── Events ──────────────────────────────────────────────────────────

    public event Action<int>    OnTelegraphStep;
    public event Action<float>  OnExecutionWindowStart;
    public event Action         OnExecutionWindowEnd;
    public event Action<int>    OnChainAdvance;
    public event Action         OnChainSuccess;
    public event Action<string> OnChainFail;

    // ─── Inspector ───────────────────────────────────────────────────────

    [Header("Referanslar")]
    [SerializeField] private WeakpointDirectionView directionView;
    [SerializeField] private GameConfig config;

    [Header("Ayarlar")]
    [Tooltip("Her marker'in arasindaki sure (saniye)")]
    [SerializeField] private float telegraphRevealInterval = 1f;
    [Tooltip("Tum marker'lar gorunce bekleme suresi (saniye)")]
    [SerializeField] private float telegraphHoldSeconds    = 1f;

    [Header("Chain (Read-only)")]
    [SerializeField] private List<WeakpointDirection> chain = new();
    [SerializeField] private int currentIndex = 0;

    [Header("State (Read-only)")]
    [SerializeField] private Phase currentPhase = Phase.Idle;
    [SerializeField] private float phaseTimer   = 0f;
    [SerializeField] private int   revealedCount = 0;

    // ─── Enum ────────────────────────────────────────────────────────────

    public enum Phase { Idle, TelegraphReveal, TelegraphHold, ExecutionWindow, Done }

    // ─── Properties ──────────────────────────────────────────────────────

    public Phase CurrentPhase => currentPhase;
    public int   CurrentIndex => currentIndex;
    public int   ChainLength  => chain.Count;

    public WeakpointDirection CurrentTarget =>
        (currentIndex < chain.Count) ? chain[currentIndex] : WeakpointDirection.None;

    // ─── Public API ──────────────────────────────────────────────────────

    public void StartSequence(List<WeakpointDirection> directions)
    {
        if (directions == null || directions.Count == 0)
        {
            Debug.LogWarning("WeakpointSequence: Bos zincir baslatılamaz.");
            return;
        }

        chain        = new List<WeakpointDirection>(directions);
        currentIndex = 0;
        revealedCount = 0;
        phaseTimer   = 0f;
        currentPhase = Phase.TelegraphReveal;

        // Ilk marker'i hemen goster
        RevealNextMarker();

        Debug.Log($"WeakpointSequence: Zincir basladi. Adim={chain.Count}");
    }

    public void SubmitSwipe(SwipeDirection swipeDir)
    {
        if (currentPhase != Phase.ExecutionWindow) return;

        var dir = ConvertSwipe(swipeDir);
        if (dir == WeakpointDirection.None) return;

        if (dir == CurrentTarget)
        {
            currentIndex++;
            OnChainAdvance?.Invoke(currentIndex);
            Debug.Log($"WeakpointSequence: HIT! {currentIndex}/{chain.Count}");

            if (currentIndex >= chain.Count)
                CompleteChain();
            else
                ShowExecutionTarget();
        }
        else
        {
            FailChain("WrongDirection");
        }
    }

/// <summary>
    /// CombatDirector'dan dogrudan hit bildirimi alir.
    /// SubmitSwipe'in yerini alir - yon kontrolu CombatDirector'da yapiliyor.
    /// </summary>
    public void SubmitHit()
    {
        if (currentPhase != Phase.ExecutionWindow) return;

        currentIndex++;
        OnChainAdvance?.Invoke(currentIndex);
        Debug.Log($"WeakpointSequence: HIT! {currentIndex}/{chain.Count}");

        if (currentIndex >= chain.Count)
            CompleteChain();
        else
            ShowExecutionTarget();
    }


public void OnFingerLifted()
    {
        // Fruit Ninja tarzi: finger lift artik chain reset yapmaz
        // Sadece timeout chain'i bozar
    }

    public void ResetSequence()
    {
        currentPhase  = Phase.Idle;
        currentIndex  = 0;
        revealedCount = 0;
        chain.Clear();
        phaseTimer = 0f;
        directionView?.HideAll();
    }

    // ─── Update ──────────────────────────────────────────────────────────

    private void Update()
    {
        if (config == null) return;

        phaseTimer += Time.unscaledDeltaTime;

        switch (currentPhase)
        {
            case Phase.TelegraphReveal: UpdateTelegraphReveal(); break;
            case Phase.TelegraphHold:   UpdateTelegraphHold();   break;
            case Phase.ExecutionWindow: UpdateExecutionWindow(); break;
        }
    }

private void UpdateTelegraphReveal()
    {
        if (phaseTimer < telegraphRevealInterval) return;
        phaseTimer = 0f;

        if (revealedCount < chain.Count)
        {
            RevealNextMarker();
        }
        else
        {
            EnterTelegraphHold();
        }
    }

    private void UpdateTelegraphHold()
    {
        if (phaseTimer >= telegraphHoldSeconds)
            OpenExecutionWindow();
    }

    private void UpdateExecutionWindow()
    {
        if (phaseTimer >= config.executionWindowSeconds)
            FailChain("Timeout");
    }

    // ─── Internal ────────────────────────────────────────────────────────

private void RevealNextMarker()
    {
        if (directionView == null || revealedCount >= chain.Count) return;

        directionView.ShowTelegraphStep(revealedCount, chain[revealedCount]);
        OnTelegraphStep?.Invoke(revealedCount);
        Debug.Log($"WeakpointSequence: Telegraph goster index={revealedCount} dir={chain[revealedCount]}");

        revealedCount++;
        // Hold'a gecis UpdateTelegraphReveal'dan yapilir, burada degil
    }

    private void EnterTelegraphHold()
    {
        currentPhase = Phase.TelegraphHold;
        phaseTimer   = 0f;
        Debug.Log("WeakpointSequence: TelegraphHold basladi.");
    }

    private void OpenExecutionWindow()
    {
        currentIndex = 0;
        currentPhase = Phase.ExecutionWindow;
        phaseTimer   = 0f;

        Time.timeScale = config.timeScaleDuringExecution;

        ShowExecutionTarget();

        OnExecutionWindowStart?.Invoke(config.executionWindowSeconds);
        Debug.Log($"WeakpointSequence: ExecutionWindow acildi. Sure={config.executionWindowSeconds}s");
    }

private void ShowExecutionTarget()
    {
        if (directionView == null) return;
        // Execution'da sadece siradaki marker gorunur, diğerleri gizli
        directionView.HideAll();
        directionView.ShowTelegraphStep(currentIndex, CurrentTarget);
    }

    private void CompleteChain()
    {
        currentPhase   = Phase.Done;
        Time.timeScale = 1f;
        directionView?.HideAll();
        OnExecutionWindowEnd?.Invoke();
        OnChainSuccess?.Invoke();
        Debug.Log("WeakpointSequence: BASARILI!");
    }

    private void FailChain(string reason)
    {
        currentPhase   = Phase.Done;
        Time.timeScale = 1f;
        directionView?.HideAll();
        OnExecutionWindowEnd?.Invoke();
        OnChainFail?.Invoke(reason);
        Debug.Log($"WeakpointSequence: BASARISIZ. Sebep={reason}");
    }

    private WeakpointDirection ConvertSwipe(SwipeDirection dir)
    {
        switch (dir)
        {
            case SwipeDirection.Up:    return WeakpointDirection.Up;
            case SwipeDirection.Down:  return WeakpointDirection.Down;
            case SwipeDirection.Left:  return WeakpointDirection.Left;
            case SwipeDirection.Right: return WeakpointDirection.Right;
            default:                   return WeakpointDirection.None;
        }
    }
}
