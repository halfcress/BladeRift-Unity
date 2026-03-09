using UnityEngine;

public enum WeakpointDirection
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4
}

public class WeakpointDirectionView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform markerRect;

    [Header("Layout")]
    [SerializeField] private float offsetDistance = 140f;

    [Header("Debug")]
    [SerializeField] private WeakpointDirection currentDirection = WeakpointDirection.Up;

    public WeakpointDirection CurrentDirection => currentDirection;

    private void Reset()
    {
        if (markerRect == null)
            markerRect = GetComponentInChildren<RectTransform>();
    }

    private void Awake()
    {
        if (markerRect == null)
            markerRect = GetComponentInChildren<RectTransform>();
    }

    private void OnEnable()
    {
        ApplyDirection(currentDirection);
    }

    public void SetDirection(WeakpointDirection direction)
    {
        currentDirection = direction;
        ApplyDirection(currentDirection);
    }

    private void ApplyDirection(WeakpointDirection direction)
    {
        if (markerRect == null)
            return;

        markerRect.anchoredPosition = GetAnchoredPosition(direction);
    }

    private Vector2 GetAnchoredPosition(WeakpointDirection direction)
    {
        switch (direction)
        {
            case WeakpointDirection.Up:
                return new Vector2(0f, offsetDistance);

            case WeakpointDirection.Down:
                return new Vector2(0f, -offsetDistance);

            case WeakpointDirection.Left:
                return new Vector2(-offsetDistance, 0f);

            case WeakpointDirection.Right:
                return new Vector2(offsetDistance, 0f);

            default:
                return Vector2.zero;
        }
    }
}
