using UnityEngine;

public class WeakpointCombatTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatDirector combatDirector;
    [SerializeField] private WeakpointDirectionView directionView;

    [Header("Debug")]
    [SerializeField] private WeakpointDirection currentTargetDirection = WeakpointDirection.Right;

    private void Reset()
    {
        if (combatDirector == null)
            combatDirector = FindFirstObjectByType<CombatDirector>();

        if (directionView == null)
            directionView = FindFirstObjectByType<WeakpointDirectionView>();
    }

    private void Awake()
    {
        if (combatDirector == null)
            combatDirector = FindFirstObjectByType<CombatDirector>();

        if (directionView == null)
            directionView = FindFirstObjectByType<WeakpointDirectionView>();

        ApplyCurrentDirection();
    }

    private void OnEnable()
    {
        ApplyCurrentDirection();
    }

    public void SetTargetDirection(WeakpointDirection direction)
    {
        currentTargetDirection = direction;
        ApplyCurrentDirection();
    }

    public bool EvaluateSwipe(SwipeDirection swipeDirection)
    {
        WeakpointDirection swipeAsWeakpoint = ConvertSwipeDirection(swipeDirection);
        bool success = swipeAsWeakpoint == currentTargetDirection;

        Debug.Log(success
            ? $"WeakpointCombatTest HIT: swipe={swipeAsWeakpoint}, target={currentTargetDirection}"
            : $"WeakpointCombatTest MISS: swipe={swipeAsWeakpoint}, target={currentTargetDirection}");

        return success;
    }

    private void ApplyCurrentDirection()
    {
        if (directionView != null)
        {
            directionView.SetDirection(currentTargetDirection);
            directionView.gameObject.SetActive(false);
            directionView.gameObject.SetActive(true);  // Force UI refresh
        }
    }

    private void Update()
    {
        ApplyCurrentDirection();  // Her frame yön deðiþikliði yapýlýr
    }

    private WeakpointDirection ConvertSwipeDirection(SwipeDirection swipeDirection)
    {
        switch (swipeDirection)
        {
            case SwipeDirection.Up:
                return WeakpointDirection.Up;
            case SwipeDirection.Down:
                return WeakpointDirection.Down;
            case SwipeDirection.Left:
                return WeakpointDirection.Left;
            case SwipeDirection.Right:
                return WeakpointDirection.Right;
            default:
                return WeakpointDirection.None;
        }
    }
}
