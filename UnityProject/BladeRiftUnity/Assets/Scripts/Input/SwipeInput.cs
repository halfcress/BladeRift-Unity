using System;
using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    [Header("Tuning")]
    [Tooltip("Deadzone in pixels. Smaller = more sensitive.")]
    [SerializeField] private float deadzonePx = 10f;

    [Tooltip("Max delta magnitude in pixels for normalization clamp.")]
    [SerializeField] private float maxDeltaPx = 300f;

    [Tooltip("If true, swipe is measured relative to initial press position (absolute). If false, per-frame delta.")]
    [SerializeField] private bool useAbsoluteFromStart = false;

    [Header("Read-only (Debug)")]
    [SerializeField] private bool isDown;
    [SerializeField] private Vector2 startPos;
    [SerializeField] private Vector2 currentPos;
    [SerializeField] private Vector2 deltaPx;
    [SerializeField] private Vector2 deltaNormalized;

    public bool IsDown => isDown;
    public Vector2 DeltaPx => deltaPx;
    public Vector2 DeltaNormalized => deltaNormalized;

    // Events
    public event Action OnPress;
    public event Action OnRelease;

    private int activeFingerId = -1;
    private Vector2 lastPos;

    private void Update()
    {
        // Prefer touch on mobile; fallback to mouse on editor/pc
        if (Input.touchCount > 0)
        {
            HandleTouch();
        }
        else
        {
            HandleMouse();
        }

        // Compute deltas
        if (isDown)
        {
            if (useAbsoluteFromStart)
                deltaPx = currentPos - startPos;
            else
                deltaPx = currentPos - lastPos;

            // Deadzone
            if (deltaPx.magnitude < deadzonePx)
                deltaPx = Vector2.zero;

            // Normalize (clamped)
            float mag = Mathf.Min(deltaPx.magnitude, maxDeltaPx);
            deltaNormalized = (mag <= 0.001f) ? Vector2.zero : (deltaPx.normalized * (mag / maxDeltaPx));
        }
        else
        {
            deltaPx = Vector2.zero;
            deltaNormalized = Vector2.zero;
        }

        lastPos = currentPos;
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDown = true;
            activeFingerId = -1;

            startPos = Input.mousePosition;
            currentPos = startPos;
            lastPos = startPos;

            OnPress?.Invoke();
        }

        if (isDown && Input.GetMouseButton(0))
        {
            currentPos = Input.mousePosition;
        }

        if (isDown && Input.GetMouseButtonUp(0))
        {
            isDown = false;
            activeFingerId = -1;

            OnRelease?.Invoke();
        }
    }

    private void HandleTouch()
    {
        // If we don't have an active finger, grab the first Began
        if (activeFingerId == -1)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began)
                {
                    activeFingerId = t.fingerId;
                    isDown = true;

                    startPos = t.position;
                    currentPos = startPos;
                    lastPos = startPos;

                    OnPress?.Invoke();
                    break;
                }
            }
        }

        // Track active finger
        if (activeFingerId != -1)
        {
            bool found = false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId != activeFingerId) continue;

                found = true;

                if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                {
                    currentPos = t.position;
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    isDown = false;
                    activeFingerId = -1;

                    OnRelease?.Invoke();
                }

                break;
            }

            // Safety: if active finger disappeared
            if (!found)
            {
                isDown = false;
                activeFingerId = -1;
                OnRelease?.Invoke();
            }
        }
    }
}