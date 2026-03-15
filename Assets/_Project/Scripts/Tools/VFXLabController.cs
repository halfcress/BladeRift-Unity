using UnityEngine;

public class VFXLabController : LabControllerBase
{
    [Header("Readability Roots")]
    [SerializeField] private GameObject currentMarkerVisual;
    [SerializeField] private GameObject nextMarkerVisual;
    [SerializeField] private GameObject nextPlusOneMarkerVisual;
    [SerializeField] private GameObject rageMarkerVisual;

    [ContextMenu("Show Current Only")]
    public void ShowCurrentOnly()
    {
        SetPreviewVisibility(true, false, false);
    }

    [ContextMenu("Show Current + Next")]
    public void ShowCurrentAndNext()
    {
        SetPreviewVisibility(true, true, false);
    }

    [ContextMenu("Show Current + Next + Next+1")]
    public void ShowFullReadabilityStack()
    {
        SetPreviewVisibility(true, true, true);
    }

    public void SetRageMarkerVisible(bool visible)
    {
        if (rageMarkerVisual != null)
        {
            rageMarkerVisual.SetActive(visible);
        }
    }

    public void SetCurrentMarkerVisible(bool visible)
    {
        if (currentMarkerVisual != null)
        {
            currentMarkerVisual.SetActive(visible);
        }
    }

    public void SetNextMarkerVisible(bool visible)
    {
        if (nextMarkerVisual != null)
        {
            nextMarkerVisual.SetActive(visible);
        }
    }

    public void SetNextPlusOneMarkerVisible(bool visible)
    {
        if (nextPlusOneMarkerVisual != null)
        {
            nextPlusOneMarkerVisual.SetActive(visible);
        }
    }
}
