using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weakpoint zincirinin tum mantigini yonetir.
///
/// Akis:
///   1) PreviewScan: pattern zone'lari tek marker ile hizli tur atar
///   2) ExecutionWindow: current / next / next+1 rehber katmani acilir
///
/// Retry akisi:
///   - StartExecutionDirectly() preview olmadan dogrudan ExecutionWindow'a geçer
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

    [Header("Preview")]
    [Tooltip("Preview bittikten sonra execution'a geçmeden once kisa bekleme.")]
    [SerializeField] private float previewPostDelaySeconds = 0.05f;

    [Header("Chain (Read-only)")]
    [SerializeField] private List<WeakpointZone> chain = new();
    [SerializeField] private int currentIndex = 0;
    [SerializeField] private int previewIndex = 0;

    [Header("State (Read-only)")]
    [SerializeField] private Phase currentPhase = Phase.Idle;
    [SerializeField] private float phaseTimer = 0f;

    public enum Phase { Idle, PreviewScan, PreviewToExecutionDelay, ExecutionWindow, Done }

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
        previewIndex = 0;
        phaseTimer = 0f;
        currentPhase = Phase.PreviewScan;

        directionView?.HideAll();
        ShowPreviewCurrentZone();

        Debug.Log($"WeakpointSequence: Zincir basladi (preview scan). Adim={chain.Count}");
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
        previewIndex = 0;
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
            ShowExecutionNavigation();
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
        previewIndex = 0;
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
            case Phase.PreviewScan:
                UpdatePreviewScan();
                break;
            case Phase.PreviewToExecutionDelay:
                UpdatePreviewToExecutionDelay();
                break;
            case Phase.ExecutionWindow:
                UpdateExecutionWindow();
                break;
        }
    }

    private void UpdatePreviewScan()
    {
        if (phaseTimer < config.telegraphStepSeconds) return;
        phaseTimer = 0f;

        previewIndex++;
        if (previewIndex >= chain.Count)
        {
            EnterPreviewToExecutionDelay();
            return;
        }

        ShowPreviewCurrentZone();
    }

    private void UpdatePreviewToExecutionDelay()
    {
        if (phaseTimer >= previewPostDelaySeconds)
            OpenExecutionWindow();
    }

    private void UpdateExecutionWindow()
    {
        if (phaseTimer >= config.executionWindowSeconds)
            FailChain("Timeout");
    }

    private void ShowPreviewCurrentZone()
    {
        if (directionView == null) return;
        if (previewIndex < 0 || previewIndex >= chain.Count) return;

        directionView.ShowPreviewZone(chain[previewIndex]);
        OnTelegraphStep?.Invoke(previewIndex);
        AudioManager.Instance?.PlayTelegraphStep();
        Debug.Log($"WeakpointSequence: Preview zone index={previewIndex} zone={chain[previewIndex]}");
    }

    private void EnterPreviewToExecutionDelay()
    {
        currentPhase = Phase.PreviewToExecutionDelay;
        phaseTimer = 0f;
        Debug.Log("WeakpointSequence: Preview bitti, execution'a geçiliyor.");
    }

    private void OpenExecutionWindow()
    {
        currentIndex = 0;
        currentPhase = Phase.ExecutionWindow;
        phaseTimer = 0f;

        Time.timeScale = config.timeScaleDuringExecution;

        ShowExecutionNavigation();
        OnExecutionWindowStart?.Invoke(config.executionWindowSeconds);
        Debug.Log($"WeakpointSequence: ExecutionWindow acildi. Sure={config.executionWindowSeconds}s");
    }

    private void ShowExecutionNavigation()
    {
        if (directionView == null) return;
        directionView.ShowExecutionNavigation(chain, currentIndex);
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
