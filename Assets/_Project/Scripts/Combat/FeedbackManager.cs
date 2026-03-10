using System.Collections;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("Hit Stop")]
    [SerializeField] private float hitStopTimeScale = 0.05f;
    [SerializeField] private float hitStopDuration = 0.06f;

    [Header("Screen Shake — Normal Hit")]
    [SerializeField] private float hitShakeMagnitude = 0.05f;
    [SerializeField] private float hitShakeDuration = 0.08f;

    [Header("Screen Shake — Rage Hit")]
    [SerializeField] private float rageShakeMagnitude = 0.09f;
    [SerializeField] private float rageShakeDuration = 0.1f;

    [Header("Screen Shake — Fail")]
    [SerializeField] private float failShakeMagnitude = 0.15f;
    [SerializeField] private float failShakeDuration = 0.2f;

    [Header("Screen Shake — Chain Success")]
    [SerializeField] private float successShakeMagnitude = 0.07f;
    [SerializeField] private float successShakeDuration = 0.12f;

    [Header("References")]
    [SerializeField] private Camera targetCamera;

    private Vector3 cameraOrigin;
    private Coroutine shakeCoroutine;
    private bool isHitStopped = false;

    // SlashTrail bu property'yi okur — shake sırasında trail noktası eklenmez
    public bool IsShaking { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            cameraOrigin = targetCamera.transform.localPosition;
    }

    // ── Public API ──────────────────────────────────────────────

    public void PlayHitFeedback()
    {
        TriggerHitStop(hitStopDuration, hitStopTimeScale);
        TriggerShake(hitShakeMagnitude, hitShakeDuration);
    }

    public void PlayRageHitFeedback()
    {
        TriggerHitStop(hitStopDuration, hitStopTimeScale);
        TriggerShake(rageShakeMagnitude, rageShakeDuration);
    }

    public void PlayFailFeedback()
    {
        TriggerShake(failShakeMagnitude, failShakeDuration);
    }

    public void PlayChainSuccessFeedback()
    {
        TriggerShake(successShakeMagnitude, successShakeDuration);
    }

    // ── Internal ────────────────────────────────────────────────

    private void TriggerHitStop(float duration, float scale)
    {
        if (isHitStopped) return;
        StartCoroutine(HitStopRoutine(duration, scale));
    }

    private void TriggerShake(float magnitude, float duration)
    {
        if (targetCamera == null) return;
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(magnitude, duration));
    }

    private IEnumerator HitStopRoutine(float duration, float scale)
    {
        isHitStopped = true;
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        isHitStopped = false;
    }

    private IEnumerator ShakeRoutine(float magnitude, float duration)
    {
        IsShaking = true;  // ← shake başladı, SlashTrail bunu okur

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentMag = Mathf.Lerp(magnitude, 0f, t);
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * currentMag,
                Random.Range(-1f, 1f) * currentMag,
                0f
            );
            targetCamera.transform.localPosition = cameraOrigin + offset;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        targetCamera.transform.localPosition = cameraOrigin;
        shakeCoroutine = null;
        IsShaking = false;  // ← shake bitti
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        IsShaking = false;
        if (targetCamera != null)
            targetCamera.transform.localPosition = cameraOrigin;
    }
}
