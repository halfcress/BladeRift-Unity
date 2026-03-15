using System.Collections;
using UnityEngine;

public class CombatFlowLabController : LabControllerBase
{
    [Header("Flow Durations")]
    [SerializeField, Min(0f)] private float approachDuration = 0.5f;
    [SerializeField, Min(0f)] private float previewDuration = 0.8f;
    [SerializeField, Min(0f)] private float delayDuration = 0.2f;
    [SerializeField, Min(0f)] private float executionDuration = 1.2f;
    [SerializeField, Min(0f)] private float resolveDuration = 0.5f;

    private Coroutine flowRoutine;

    [ContextMenu("Play Standard Flow")]
    public void PlayStandardFlow()
    {
        StopCurrentFlow();
        flowRoutine = StartCoroutine(PlayStandardFlowRoutine());
    }

    [ContextMenu("Play Success Flow")]
    public void PlaySuccessFlow()
    {
        StopCurrentFlow();
        flowRoutine = StartCoroutine(PlayResolveRoutine(true));
    }

    [ContextMenu("Play Fail Flow")]
    public void PlayFailFlow()
    {
        StopCurrentFlow();
        flowRoutine = StartCoroutine(PlayResolveRoutine(false));
    }

    public override void Replay()
    {
        StopCurrentFlow();
        base.Replay();
    }

    private IEnumerator PlayStandardFlowRoutine()
    {
        SetState(LabPlaybackState.Approach);
        yield return new WaitForSecondsRealtime(approachDuration);

        SetState(LabPlaybackState.Preview);
        yield return new WaitForSecondsRealtime(previewDuration);

        SetState(LabPlaybackState.Delay);
        yield return new WaitForSecondsRealtime(delayDuration);

        SetState(LabPlaybackState.Execution);
        yield return new WaitForSecondsRealtime(executionDuration);

        SetState(LabPlaybackState.Success);
        yield return new WaitForSecondsRealtime(resolveDuration);

        SetState(LabPlaybackState.Idle);
        flowRoutine = null;
    }

    private IEnumerator PlayResolveRoutine(bool success)
    {
        SetState(LabPlaybackState.Execution);
        yield return new WaitForSecondsRealtime(executionDuration);

        SetState(success ? LabPlaybackState.Success : LabPlaybackState.Fail);
        yield return new WaitForSecondsRealtime(resolveDuration);

        SetState(LabPlaybackState.Idle);
        flowRoutine = null;
    }

    private void StopCurrentFlow()
    {
        if (flowRoutine != null)
        {
            StopCoroutine(flowRoutine);
            flowRoutine = null;
        }
    }
}
