using UnityEngine;

public class InputLabController : LabControllerBase
{
    [Header("Input Debug Roots")]
    [SerializeField] private GameObject swipeTraceRoot;
    [SerializeField] private GameObject toleranceZoneRoot;
    [SerializeField] private GameObject actionableWeakpointRoot;

    public void SetSwipeTraceVisible(bool visible)
    {
        if (swipeTraceRoot != null)
        {
            swipeTraceRoot.SetActive(visible);
        }
    }

    public void SetToleranceZoneVisible(bool visible)
    {
        if (toleranceZoneRoot != null)
        {
            toleranceZoneRoot.SetActive(visible);
        }
    }

    public void SetActionableWeakpointVisible(bool visible)
    {
        if (actionableWeakpointRoot != null)
        {
            actionableWeakpointRoot.SetActive(visible);
        }
    }

    [ContextMenu("Execution State")]
    public void JumpToExecution()
    {
        SetState(LabPlaybackState.Execution);
    }
}
