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
/// Converts raw SwipeInput data into a single committed attack direction
/// (Left / Right / Up / Down) per press.
///
/// Why this works with the current SwipeInput:
/// - SwipeInput currently uses per-frame delta by default.
/// - This script accumulates those small deltas over the whole press,
///   so direction commit is stable and readable.
/// - Finger lift always resets the chain state for this input layer.
///
/// Optional receiver integration:
/// - If you assign a CombatDirector (or any MonoBehaviour) to combatReceiver,
///   this script will SendMessage one of these methods when direction commits:
///     OnSwipeDirection(SwipeDirection dir)
///     OnSwipeDirectionCommitted(SwipeDirection dir)
/// - SendMessage is used intentionally so this file compiles even if the
///   current CombatDirector API is not finalized yet.
/// </summary>
public class SwipeInterpreter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwipeInput swipe;
    [SerializeField] private MonoBehaviour combatReceiver;

    [Header("Direction Commit Tuning")]
    [Tooltip("Total accumulated swipe distance required before a direction is committed.")]
    [SerializeField] private float commitDistancePx = 70f;

    [Tooltip("How much stronger the dominant axis should be. 1.0 = no bias, 1.2 = 20% stronger.")]
    [SerializeField] private float dominantAxisBias = 1.15f;

    [Tooltip("If false, diagonal/ambiguous swipes are ignored until direction becomes clear enough.")]
    [SerializeField] private bool allowDominantAxisFallback = true;

    [Header("Read-only (Debug)")]
    [SerializeField] private bool wasDownLastFrame;
    [SerializeField] private bool hasCommittedThisPress;
    [SerializeField] private Vector2 accumulatedDeltaPx;
    [SerializeField] private SwipeDirection currentDirection = SwipeDirection.None;
    [SerializeField] private SwipeDirection lastCommittedDirection = SwipeDirection.None;

    public bool HasCommittedThisPress => hasCommittedThisPress;
    public Vector2 AccumulatedDeltaPx => accumulatedDeltaPx;
    public SwipeDirection CurrentDirection => currentDirection;
    public SwipeDirection LastCommittedDirection => lastCommittedDirection;

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

        // New press started
        if (isDown && !wasDownLastFrame)
        {
            BeginPress();
        }

        // While finger/mouse is down, accumulate movement and try to commit once.
        if (isDown)
        {
            accumulatedDeltaPx += swipe.DeltaPx;

            if (!hasCommittedThisPress)
            {
                SwipeDirection preview = EvaluateDirection(accumulatedDeltaPx);
                currentDirection = preview;

                if (preview != SwipeDirection.None && accumulatedDeltaPx.magnitude >= commitDistancePx)
                {
                    CommitDirection(preview);
                }
            }
        }

        // Finger lifted -> reset chain/input state for the next press.
        if (!isDown && wasDownLastFrame)
        {
            EndPress();
        }

        wasDownLastFrame = isDown;
    }

    private void BeginPress()
    {
        hasCommittedThisPress = false;
        accumulatedDeltaPx = Vector2.zero;
        currentDirection = SwipeDirection.None;
    }

    private void EndPress()
    {
        hasCommittedThisPress = false;
        accumulatedDeltaPx = Vector2.zero;
        currentDirection = SwipeDirection.None;
    }

    private void CommitDirection(SwipeDirection direction)
    {
        hasCommittedThisPress = true;
        currentDirection = direction;
        lastCommittedDirection = direction;

        if (combatReceiver != null)
        {
            combatReceiver.SendMessage("OnSwipeDirection", direction, SendMessageOptions.DontRequireReceiver);
            combatReceiver.SendMessage("OnSwipeDirectionCommitted", direction, SendMessageOptions.DontRequireReceiver);
        }
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

        // Fallback for near-diagonal movement: still pick the stronger axis.
        if (absX >= absY)
            return delta.x >= 0f ? SwipeDirection.Right : SwipeDirection.Left;

        return delta.y >= 0f ? SwipeDirection.Up : SwipeDirection.Down;
    }
}
