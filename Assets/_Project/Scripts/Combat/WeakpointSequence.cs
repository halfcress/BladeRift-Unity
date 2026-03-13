using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weakpoint zincirinin tum mantigini yonetir.
///
/// Telegraph akisi:
///   1) TelegraphReveal: marker'lar sirayla belirir (1 -> 1+2 -> 1+2+3)
///   2) TelegraphHold:   hepsi birlikte kisa sure durur
///   3) ExecutionWindow: sadece aktif weakpoint kalir, oyuncu swipe yapar
///
/// Retry akisi (telegraph olmadan):
///   - StartExecutionDirectly() ile dogrudan ExecutionWindow'a geçilir
///
/// Fail kaynaklari:
///   - Timeout (UpdateExecutionWindow)
///   - FingerLift (CombatDirector -> ForceFailExternal)
/// </summary>
public class WeakpointSequence : MonoBehaviour
{
    public event Action<int> OnTelegraphStep;
    public event Action<float> OnExecutionWindowStart;
    public event Action OnExecutionWindowEnd;
    public event Action<int> OnChainAdvance;
    public event Action OnChainSuccess;
    public event Action<string> OnChainFail;

    [Header("Referanslar")]
    [SerializeField] private WeakpointDirectionView directionView;
    [SerializeField] private GameConfig config;

    [Header("Ayarlar")]
    [Tooltip("Her marker'in arasindaki sure (saniye)")]
    [SerializeField] private float telegraphRevealInterval = 1f;
    [Tooltip("Tum marker'lar gorunce bekleme suresi (saniye)")]
    [SerializeField] private float telegraphHoldSeconds = 1f;

    [Header("Chain (Read-only)")]
    [SerializeField] private List<WeakpointZone> chain = new();
    [SerializeField] private int currentIndex = 0;

    [Header("State (Read-only)")]
    [SerializeField] private Phase currentPhase = Phase.Idle;
    [SerializeField] private float phaseTimer = 0f;
    [SerializeField] private int revealedCount = 0;

    public enum Phase { Idle, TelegraphReveal, TelegraphHold, ExecutionWindow, Done }

    public Phase CurrentPhase => currentPhase;
    public int CurrentIndex => currentIndex;
    public int ChainLength => chain.Count;

    public WeakpointZone CurrentTarget =>
        (currentIndex < chain.Count) ? chain[currentIndex] : WeakpointZone.None;

    public void StartSequence(List<WeakpointZone> zones)
    {
        if (zones == null || zones.Count == 0)
        {
            Debug.LogWarning("WeakpointSequence: Bos zincir baslatılamaz.");
            return;
        }

        chain = new List<WeakpointZone>(zones);
        currentIndex = 0;
        revealedCount = 0;
        phaseTimer = 0f;
        currentPhase = Phase.TelegraphReveal;

        directionView?.HideAll();
        RevealNextMarker();

        Debug.Log($"WeakpointSequence: Zincir basladi (telegraph). Adim={chain.Count}");
    }

    public void StartExecutionDirectly(List<WeakpointZone> zones)
    {
        if (zones == null || zones.Count == 0)
        {
            Debug.LogWarning("WeakpointSequence: Bos zincir.");
            return;
        }

        chain = new List<WeakpointZone>(zones);
        currentIndex = 0;
        revealedCount = chain.Count;
        phaseTimer = 0f;

        directionView?.HideAll();
        OpenExecutionWindow();

        Debug.Log($"WeakpointSequence: Dogrudan execution basladi (retry). Adim={chain.Count}");
    }

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

    public void ForceCompleteChain()
    {
        if (currentPhase != Phase.ExecutionWindow) return;

        currentIndex = chain.Count;
        Debug.Log("WeakpointSequence: RAGE — ForceCompleteChain!");
        CompleteChain();
    }

    public void ForceFailExternal(string reason)
    {
        if (currentPhase != Phase.ExecutionWindow) return;
        FailChain(reason);
    }

    public void ResetSequence()
    {
        currentPhase = Phase.Idle;
        currentIndex = 0;
        revealedCount = 0;
        chain.Clear();
        phaseTimer = 0f;
        Time.timeScale = 1f;
        directionView?.HideAll();
    }

    private void Update()
    {
        if (config == null) return;

        phaseTimer += Time.unscaledDeltaTime;

        switch (currentPhase)
        {
            case Phase.TelegraphReveal:
                UpdateTelegraphReveal();
                break;
            case Phase.TelegraphHold:
                UpdateTelegraphHold();
                break;
            case Phase.ExecutionWindow:
                UpdateExecutionWindow();
                break;
        }
    }

    private void UpdateTelegraphReveal()
    {
        if (phaseTimer < telegraphRevealInterval) return;
        phaseTimer = 0f;

        if (revealedCount < chain.Count)
            RevealNextMarker();
        else
            EnterTelegraphHold();
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

    private void RevealNextMarker()
    {
        if (directionView == null || revealedCount >= chain.Count) return;

        directionView.ShowTelegraphStep(revealedCount, chain[revealedCount]);
        OnTelegraphStep?.Invoke(revealedCount);
        AudioManager.Instance?.PlayTelegraphStep();
        Debug.Log($"WeakpointSequence: Telegraph goster index={revealedCount} zone={chain[revealedCount]}");

        revealedCount++;
    }

    private void EnterTelegraphHold()
    {
        currentPhase = Phase.TelegraphHold;
        phaseTimer = 0f;
        Debug.Log("WeakpointSequence: TelegraphHold basladi.");
    }

    private void OpenExecutionWindow()
    {
        currentIndex = 0;
        currentPhase = Phase.ExecutionWindow;
        phaseTimer = 0f;

        Time.timeScale = config.timeScaleDuringExecution;

        ShowExecutionTarget();
        OnExecutionWindowStart?.Invoke(config.executionWindowSeconds);
        Debug.Log($"WeakpointSequence: ExecutionWindow acildi. Sure={config.executionWindowSeconds}s");
    }

    private void ShowExecutionTarget()
    {
        if (directionView == null) return;
        directionView.HideAll();
        directionView.ShowTelegraphStep(currentIndex, CurrentTarget);
    }

    private void CompleteChain()
    {
        currentPhase = Phase.Done;
        Time.timeScale = 1f;
        directionView?.HideAll();
        OnExecutionWindowEnd?.Invoke();
        OnChainSuccess?.Invoke();
        Debug.Log("WeakpointSequence: BASARILI!");
    }

    private void FailChain(string reason)
    {
        currentPhase = Phase.Done;
        Time.timeScale = 1f;
        directionView?.HideAll();
        OnExecutionWindowEnd?.Invoke();
        OnChainFail?.Invoke(reason);
        Debug.Log($"WeakpointSequence: BASARISIZ. Sebep={reason}");
    }
}
