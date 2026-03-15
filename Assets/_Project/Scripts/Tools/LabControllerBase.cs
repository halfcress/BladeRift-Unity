using UnityEngine;

public enum LabPlaybackState
{
    Idle = 0,
    Approach = 1,
    Preview = 2,
    Delay = 3,
    Execution = 4,
    Success = 5,
    Fail = 6
}

public abstract class LabControllerBase : MonoBehaviour
{
    [Header("State Roots (Enum sırasına göre)")]
    [SerializeField] private GameObject[] stateRoots = new GameObject[7];

    [Header("Preview Roots")]
    [SerializeField] private GameObject currentRoot;
    [SerializeField] private GameObject nextRoot;
    [SerializeField] private GameObject nextPlusOneRoot;

    [Header("Background Variants")]
    [SerializeField] private GameObject[] backgroundVariants;

    [Header("Runtime")]
    [SerializeField] private bool applyDefaultsOnEnable = true;
    [SerializeField] private LabPlaybackState currentState = LabPlaybackState.Idle;
    [SerializeField, Range(0.05f, 2f)] private float currentTimeScale = 1f;
    [SerializeField] private int backgroundIndex = 0;
    [SerializeField] private bool showCurrent = true;
    [SerializeField] private bool showNext = true;
    [SerializeField] private bool showNextPlusOne = true;

    public LabPlaybackState CurrentState => currentState;
    public float CurrentTimeScale => currentTimeScale;
    public int BackgroundIndex => backgroundIndex;

    protected virtual void OnEnable()
    {
        if (!applyDefaultsOnEnable)
        {
            return;
        }

        ApplyState(currentState);
        ApplyPreviewVisibility(showCurrent, showNext, showNextPlusOne);
        ApplyBackground(backgroundIndex);
        ApplyTimeScale(currentTimeScale);
    }

    protected virtual void OnDisable()
    {
        Time.timeScale = 1f;
    }

    [ContextMenu("Replay")]
    public virtual void Replay()
    {
        ApplyState(LabPlaybackState.Idle);
        ApplyPreviewVisibility(showCurrent, showNext, showNextPlusOne);
        ApplyBackground(backgroundIndex);
        ApplyTimeScale(currentTimeScale);
    }

    [ContextMenu("Reset Lab")]
    public virtual void ResetLab()
    {
        currentState = LabPlaybackState.Idle;
        currentTimeScale = 1f;
        backgroundIndex = 0;
        showCurrent = true;
        showNext = true;
        showNextPlusOne = true;
        Replay();
    }

    public virtual void SetState(LabPlaybackState newState)
    {
        currentState = newState;
        ApplyState(currentState);
    }

    public virtual void SetPreviewVisibility(bool current, bool next, bool nextPlusOne)
    {
        showCurrent = current;
        showNext = next;
        showNextPlusOne = nextPlusOne;
        ApplyPreviewVisibility(showCurrent, showNext, showNextPlusOne);
    }

    public virtual void SetBackgroundIndex(int index)
    {
        backgroundIndex = Mathf.Clamp(index, 0, Mathf.Max(0, backgroundVariants.Length - 1));
        ApplyBackground(backgroundIndex);
    }

    public virtual void SetTimeScale(float value)
    {
        currentTimeScale = Mathf.Clamp(value, 0.05f, 2f);
        ApplyTimeScale(currentTimeScale);
    }

    protected void ApplyState(LabPlaybackState state)
    {
        for (int i = 0; i < stateRoots.Length; i++)
        {
            if (stateRoots[i] != null)
            {
                stateRoots[i].SetActive(i == (int)state);
            }
        }
    }

    protected void ApplyPreviewVisibility(bool current, bool next, bool nextPlusOne)
    {
        if (currentRoot != null) currentRoot.SetActive(current);
        if (nextRoot != null) nextRoot.SetActive(next);
        if (nextPlusOneRoot != null) nextPlusOneRoot.SetActive(nextPlusOne);
    }

    protected void ApplyBackground(int index)
    {
        for (int i = 0; i < backgroundVariants.Length; i++)
        {
            if (backgroundVariants[i] != null)
            {
                backgroundVariants[i].SetActive(i == index);
            }
        }
    }

    protected void ApplyTimeScale(float value)
    {
        Time.timeScale = value;
    }
}
