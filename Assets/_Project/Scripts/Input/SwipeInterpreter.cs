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
/// Interprets SwipeInput into cardinal directions and supports multi-commit chains
/// within a single press.
///
/// Core behavior:
/// - While the finger/mouse is held, we accumulate a SEGMENT delta.
/// - When that segment clearly resolves into a direction and passes commitDistancePx,
///   we commit one chain step.
/// - After commit, the segment resets so the next turn can be read as a new step.
/// - Releasing the finger resets the whole chain state for this input layer.
///
/// This fits zig-zag weakpoint paths better than whole-press accumulation.
/// </summary>
public class SwipeInterpreter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwipeInput swipe;
    [SerializeField] private MonoBehaviour combatReceiver;

    [Header("Direction Commit Tuning")]
    [Tooltip("Segment distance required before a direction is committed.")]
    [SerializeField] private float commitDistancePx = 70f;

    [Tooltip("How much stronger the dominant axis should be. 1.0 = no bias, 1.2 = 20% stronger.")]
    [SerializeField] private float dominantAxisBias = 1.15f;

    [Tooltip("If false, diagonal/ambiguous swipes are ignored until direction becomes clear enough.")]
    [SerializeField] private bool allowDominantAxisFallback = true;

    [Tooltip("Prevents same-direction spam within the same press. Recommended true for chain combat.")]
    [SerializeField] private bool blockSameDirectionRepeatInSamePress = true;

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
        committedCountThisPress = 0;
        accumulatedDeltaPx = Vector2.zero;
        currentDirection = SwipeDirection.None;
        lastCommittedDirection = SwipeDirection.None;
    }

    private void EndPress()
    {
        hasCommittedThisPress = false;
        committedCountThisPress = 0;
        accumulatedDeltaPx = Vector2.zero;
        currentDirection = SwipeDirection.None;
        lastCommittedDirection = SwipeDirection.None;
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

        if (blockSameDirectionRepeatInSamePress && committedCountThisPress > 0 && preview == lastCommittedDirection)
        {
            // Prevent repeated firing from a long drag in the same direction.
            // Reset the current segment and wait for a meaningful turn.
            accumulatedDeltaPx = Vector2.zero;
            currentDirection = SwipeDirection.None;
            return;
        }

        CommitDirection(preview);
    }

    private void CommitDirection(SwipeDirection direction)
    {
        hasCommittedThisPress = true;
        committedCountThisPress++;
        currentDirection = direction;
        lastCommittedDirection = direction;

        if (combatReceiver != null)
        {
            combatReceiver.SendMessage("OnSwipeDirection", direction, SendMessageOptions.DontRequireReceiver);
            combatReceiver.SendMessage("OnSwipeDirectionCommitted", direction, SendMessageOptions.DontRequireReceiver);
        }

        // Start a fresh segment immediately so the same press can continue chaining.
        accumulatedDeltaPx = Vector2.zero;
        currentDirection = SwipeDirection.None;
    }

    private SwipeDirection EvaluateDirection(Vector2 delta)
    {
        if (delta.magnitude < commitDistancePx)
            return SwipeDirection.None;

        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);

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
