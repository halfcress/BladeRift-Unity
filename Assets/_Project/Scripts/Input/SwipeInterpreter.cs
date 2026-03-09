using UnityEngine;

public enum SwipeDirection
{
    None = 0,
    Left = 1,
    Right = 2,
    Up = 3,
    Down = 4
}

/// <summary>
/// Converts raw SwipeInput data into attack directions.
///
/// Current model:
/// - Multiple commits are allowed within a single press.
/// - After each successful commit, the local segment accumulator resets,
///   so the next chain step starts fresh.
/// - Finger / mouse lift resets the whole press state.
/// - Same-direction recommits are blocked to avoid jitter spam
///   (Right -> Right from tiny wobble, etc.).
///
/// Optional receiver integration:
/// - If combatReceiver is assigned, this component sends:
///   OnSwipeDirection(SwipeDirection dir)
///   OnSwipeDirectionCommitted(SwipeDirection dir)
/// </summary>
public class SwipeInterpreter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwipeInput swipe;
    [SerializeField] private MonoBehaviour combatReceiver;

    [Header("Direction Commit Tuning")]
    [Tooltip("Distance required for each chain segment to commit a direction.")]
    [SerializeField] private float commitDistancePx = 70f;

    [Tooltip("How much stronger the dominant axis should be. 1.0 = no bias, 1.2 = 20% stronger.")]
    [SerializeField] private float dominantAxisBias = 1.15f;

    [Tooltip("If false, diagonal / ambiguous swipes are ignored until direction becomes clear enough.")]
    [SerializeField] private bool allowDominantAxisFallback = true;

    [Header("Read-only (Debug)")]
    [SerializeField] private bool wasDownLastFrame;
    [SerializeField] private bool hasCommittedThisPress;
    [SerializeField] private Vector2 accumulatedDeltaPx;
    [SerializeField] private SwipeDirection currentDirection = SwipeDirection.None;
    [SerializeField] private SwipeDirection lastCommittedDirection = SwipeDirection.None;
    [SerializeField] private int committedCountThisPress;

    public bool HasCommittedThisPress => hasCommittedThisPress;
    public Vector2 AccumulatedDeltaPx => accumulatedDeltaPx;
    public SwipeDirection CurrentDirection => currentDirection;
    public SwipeDirection LastCommittedDirection => lastCommittedDirection;
    public int CommittedCountThisPress => committedCountThisPress;

    private void Reset()
    {
        swipe = GetComponent<SwipeInput>();
    }

    private void Awake()
    {
        if (swipe == null)
            swipe = GetComponent<SwipeInput>();
    }

    private void Update()
    {
        if (swipe == null)
            return;

        bool isDown = swipe.IsDown;

        if (isDown && !wasDownLastFrame)
            BeginPress();

        if (isDown)
            UpdateHeldPress();

        if (!isDown && wasDownLastFrame)
            EndPress();

        wasDownLastFrame = isDown;
    }

    private void BeginPress()
    {
        hasCommittedThisPress = false;
        accumulatedDeltaPx = Vector2.zero;
        currentDirection = SwipeDirection.None;
        lastCommittedDirection = SwipeDirection.None;
        committedCountThisPress = 0;
    }

    private void UpdateHeldPress()
    {
        accumulatedDeltaPx += swipe.DeltaPx;

        SwipeDirection preview = EvaluateDirection(accumulatedDeltaPx);
        currentDirection = preview;

        if (preview == SwipeDirection.None)
            return;

        if (accumulatedDeltaPx.magnitude < commitDistancePx)
            return;

        // Prevent jitter / wobble from spamming the same direction repeatedly.
        if (preview == lastCommittedDirection)
            return;

        CommitDirection(preview);
    }

    private void EndPress()
    {
        accumulatedDeltaPx = Vector2.zero;
        currentDirection = SwipeDirection.None;
        lastCommittedDirection = SwipeDirection.None;
        hasCommittedThisPress = false;
        committedCountThisPress = 0;
    }

    private void CommitDirection(SwipeDirection direction)
    {
        hasCommittedThisPress = true;
        currentDirection = direction;
        lastCommittedDirection = direction;
        committedCountThisPress++;

        if (combatReceiver != null)
        {
            combatReceiver.SendMessage("OnSwipeDirection", direction, SendMessageOptions.DontRequireReceiver);
            combatReceiver.SendMessage("OnSwipeDirectionCommitted", direction, SendMessageOptions.DontRequireReceiver);
        }

        // Start a fresh local segment for the next chain direction.
        accumulatedDeltaPx = Vector2.zero;
    }

    private SwipeDirection EvaluateDirection(Vector2 delta)
    {
        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);

        if (delta.magnitude < commitDistancePx)
            return SwipeDirection.None;

        bool xDominant = absX >= absY * dominantAxisBias;
        bool yDominant = absY >= absX * dominantAxisBias;

        if (xDominant)
            return delta.x >= 0f ? SwipeDirection.Right : SwipeDirection.Left;

        if (yDominant)
            return delta.y >= 0f ? SwipeDirection.Up : SwipeDirection.Down;

        if (!allowDominantAxisFallback)
            return SwipeDirection.None;

        return absX >= absY
            ? (delta.x >= 0f ? SwipeDirection.Right : SwipeDirection.Left)
            : (delta.y >= 0f ? SwipeDirection.Up : SwipeDirection.Down);
    }
}
