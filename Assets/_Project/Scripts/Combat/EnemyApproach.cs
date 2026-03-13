using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Düşmanın koridordan yaklaşmasını yönetir.
/// Archetype içindeki approachBehaviorType burada gerçek harekete bağlanır.
/// </summary>
public class EnemyApproach : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatDirector combatDirector;
    [SerializeField] private EnemyArchetypeData archetypeData;
    [SerializeField] private EnemyWeakpointAnchors weakpointAnchors;
    [SerializeField] private WeakpointDirectionView directionView;
    private EnemySpawner spawner;

    [Header("Approach Settings")]
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, 30f);
    [SerializeField] private Vector3 stopPosition = new Vector3(0f, 0f, 6f);

    [Header("Approach Behavior Tuning")]
    [SerializeField] private float sideLeanAmplitude = 0.8f;
    [SerializeField] private float sideLeanFrequency = 4f;
    [SerializeField] private float dashSlowMultiplier = 0.9f;
    [SerializeField] private float dashBurstMultiplier = 1.9f;
    [SerializeField] private float dashBurstDistance = 3f;
    [SerializeField] private float heavyWalkBaseMultiplier = 0.72f;
    [SerializeField] private float heavyWalkPulseAmplitude = 0.28f;
    [SerializeField] private float heavyWalkPulseFrequency = 1.8f;

    [Header("Rage Hit")]
    [Tooltip("Silüet hit alanını genişletme çarpanı. 1.0 = tam bounds, 1.3 = %30 padding")]
    [SerializeField] private float rageHitPadding = 1.3f;

    [Header("State (Read-only)")]
    [SerializeField] private State currentState = State.Idle;

    public enum State { Idle, Approaching, TelegraphTriggered, WaitingForResult, Dead }

    public bool IsInDeathSequence => currentState == State.Dead;

    private Renderer cachedRenderer;
    private bool validated = false;
    private float approachElapsed = 0f;
    private bool punishRoutineRunning = false;

    private void Awake()
    {
        if (combatDirector == null)
            combatDirector = FindFirstObjectByType<CombatDirector>();

        if (directionView == null)
            directionView = FindFirstObjectByType<WeakpointDirectionView>();

        if (weakpointAnchors == null)
            weakpointAnchors = GetComponentInChildren<EnemyWeakpointAnchors>();

        spawner = FindFirstObjectByType<EnemySpawner>();
        cachedRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnEnable()
    {
        if (combatDirector == null) return;
        combatDirector.OnCombatSuccess += HandleCombatSuccess;
        combatDirector.OnCombatFail += HandleCombatFail;
    }

    private void OnDisable()
    {
        if (combatDirector == null) return;
        combatDirector.OnCombatSuccess -= HandleCombatSuccess;
        combatDirector.OnCombatFail -= HandleCombatFail;
    }

    private void Start()
    {
        ValidateWeakpointSetupOrBreak();
        if (!validated)
            return;

        StartApproach();
    }

    private void Update()
    {
        if (archetypeData == null)
        {
            Debug.LogError("[EnemyApproach] archetypeData yok!", this);
            return;
        }

        if (currentState != State.Approaching) return;

        approachElapsed += Time.deltaTime;
        ApplyApproachMovement();

        float distToCamera = transform.position.z;
        if (distToCamera <= archetypeData.telegraphTriggerDistance)
            TriggerTelegraph();
    }

    public bool TryGetScreenRect(Camera cam, out Rect screenRect)
    {
        screenRect = Rect.zero;
        if (cam == null || cachedRenderer == null) return false;

        Bounds bounds = cachedRenderer.bounds;
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        Vector3[] corners = new Vector3[8];
        corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
        corners[1] = center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[2] = center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[3] = center + new Vector3(-extents.x, extents.y, extents.z);
        corners[4] = center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[5] = center + new Vector3(extents.x, -extents.y, extents.z);
        corners[6] = center + new Vector3(extents.x, extents.y, -extents.z);
        corners[7] = center + new Vector3(extents.x, extents.y, extents.z);

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < 8; i++)
        {
            Vector3 sp = cam.WorldToScreenPoint(corners[i]);
            if (sp.z < 0f) return false;

            if (sp.x < minX) minX = sp.x;
            if (sp.x > maxX) maxX = sp.x;
            if (sp.y < minY) minY = sp.y;
            if (sp.y > maxY) maxY = sp.y;
        }

        float w = maxX - minX;
        float h = maxY - minY;
        float padW = w * (rageHitPadding - 1f) * 0.5f;
        float padH = h * (rageHitPadding - 1f) * 0.5f;

        screenRect = new Rect(minX - padW, minY - padH, w + padW * 2f, h + padH * 2f);
        return true;
    }

    private void StartApproach()
    {
        transform.position = spawnPosition;
        approachElapsed = 0f;
        currentState = State.Approaching;
        Debug.Log($"[EnemyApproach] Yaklaşma başladı. Behavior={archetypeData.approachBehaviorType}");
    }

    private void ApplyApproachMovement()
    {
        switch (archetypeData.approachBehaviorType)
        {
            case EnemyApproachBehaviorType.SideLean:
                MoveSideLean();
                break;

            case EnemyApproachBehaviorType.Dash:
                MoveDash();
                break;

            case EnemyApproachBehaviorType.HeavyWalk:
                MoveHeavyWalk();
                break;

            case EnemyApproachBehaviorType.Straight:
            default:
                MoveStraight(archetypeData.approachSpeed);
                break;
        }
    }

    private void MoveStraight(float speed)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            stopPosition,
            speed * Time.deltaTime
        );
    }

    private void MoveSideLean()
    {
        MoveStraight(archetypeData.approachSpeed);

        float progressToTrigger = Mathf.InverseLerp(spawnPosition.z, archetypeData.telegraphTriggerDistance, transform.position.z);
        float amplitude = Mathf.Lerp(sideLeanAmplitude, 0f, progressToTrigger);

        Vector3 p = transform.position;
        p.x = stopPosition.x + Mathf.Sin(approachElapsed * sideLeanFrequency) * amplitude;
        transform.position = p;
    }

    private void MoveDash()
    {
        float burstLineZ = archetypeData.telegraphTriggerDistance + dashBurstDistance;
        float speedMultiplier = transform.position.z > burstLineZ ? dashSlowMultiplier : dashBurstMultiplier;
        MoveStraight(archetypeData.approachSpeed * speedMultiplier);
    }

    private void MoveHeavyWalk()
    {
        float pulse01 = 0.5f + 0.5f * Mathf.Sin(approachElapsed * heavyWalkPulseFrequency * Mathf.PI * 2f);
        float speedMultiplier = heavyWalkBaseMultiplier + pulse01 * heavyWalkPulseAmplitude;
        MoveStraight(archetypeData.approachSpeed * speedMultiplier);
    }

    private void ValidateWeakpointSetupOrBreak()
    {
        validated = false;

        if (archetypeData == null)
        {
            BreakInvalidSetup("[EnemyApproach] archetypeData yok!");
            return;
        }

        if (!archetypeData.ValidateRuntimeConfig(out string archetypeError))
        {
            BreakInvalidSetup($"[EnemyApproach] INVALID ARCHETYPE CONFIG | Enemy={name} | Archetype={archetypeData.name} | {archetypeError}");
            return;
        }

        if (weakpointAnchors == null)
        {
            BreakInvalidSetup($"[EnemyApproach] {name}: EnemyWeakpointAnchors component yok!");
            return;
        }

        List<WeakpointZone> requiredZones = archetypeData.GetEnabledZones();
        if (requiredZones == null || requiredZones.Count == 0)
        {
            BreakInvalidSetup($"[EnemyApproach] {name}: archetype içinde aktif weakpoint zone yok!");
            return;
        }

        if (!weakpointAnchors.ValidateRequiredZones(requiredZones, out string anchorError))
        {
            BreakInvalidSetup($"[EnemyApproach] INVALID WEAKPOINT SETUP | Enemy={name} | Archetype={archetypeData.name} | {anchorError}");
            return;
        }

        validated = true;
    }

    private void BreakInvalidSetup(string message)
    {
        Debug.LogError(message, this);
        currentState = State.Idle;
        enabled = false;
        Debug.Break();
    }

    private void TriggerTelegraph()
    {
        currentState = State.TelegraphTriggered;
        Debug.Log("[EnemyApproach] Telegraph tetiklendi.");

        if (!validated)
        {
            BreakInvalidSetup($"[EnemyApproach] {name}: validate edilmeden combat başlatılmaya çalışıldı.");
            return;
        }

        WeakpointZone[] builtPattern = archetypeData.BuildPattern();
        if (builtPattern == null || builtPattern.Length == 0)
        {
            BreakInvalidSetup($"[EnemyApproach] {name}: BuildPattern boş döndü!");
            return;
        }

        directionView?.BindEnemyAnchors(weakpointAnchors);
        combatDirector.StartCombatSequence(new List<WeakpointZone>(builtPattern));
        currentState = State.WaitingForResult;
    }

    private void HandleCombatSuccess()
    {
        currentState = State.Dead;
        Debug.Log($"[EnemyApproach] Düşman öldü. DeathBehavior={archetypeData.deathBehaviorType}");
        StartCoroutine(DeathThenDespawn());
    }

    private IEnumerator DeathThenDespawn()
    {
        Color originalColor = cachedRenderer != null ? cachedRenderer.material.color : Color.white;
        Vector3 originalScale = transform.localScale;

        switch (archetypeData.deathBehaviorType)
        {
            case EnemyDeathBehaviorType.HeavySplit:
                yield return HeavySplitDeath(originalColor, originalScale);
                break;

            case EnemyDeathBehaviorType.Burst:
                yield return BurstDeath(originalColor, originalScale);
                break;

            case EnemyDeathBehaviorType.EliteFinisher:
                yield return EliteFinisherDeath(originalColor, originalScale);
                break;

            case EnemyDeathBehaviorType.StandardSplit:
            default:
                yield return StandardSplitDeath(originalColor, originalScale);
                break;
        }

        spawner?.OnEnemyKilledImmediate();
        Destroy(gameObject);
    }

    private IEnumerator StandardSplitDeath(Color originalColor, Vector3 originalScale)
    {
        yield return FlashColor(Color.red, archetypeData.deathFlashDuration, originalColor);
        yield return PulseScale(originalScale, 1.04f, archetypeData.deathFlashDuration * 0.75f);
        yield return new WaitForSecondsRealtime(archetypeData.deathPauseSeconds);
        yield return new WaitForSecondsRealtime(archetypeData.respawnDelaySeconds);
    }

    private IEnumerator HeavySplitDeath(Color originalColor, Vector3 originalScale)
    {
        yield return FlashColor(new Color(1f, 0.25f, 0.25f, 1f), archetypeData.deathFlashDuration * 1.15f, originalColor);
        yield return PulseScale(originalScale, 1.10f, archetypeData.deathFlashDuration);
        yield return FlashColor(Color.red, archetypeData.deathFlashDuration * 0.8f, originalColor);
        yield return new WaitForSecondsRealtime(archetypeData.deathPauseSeconds * 1.35f);
        yield return new WaitForSecondsRealtime(archetypeData.respawnDelaySeconds * 1.15f);
    }

    private IEnumerator BurstDeath(Color originalColor, Vector3 originalScale)
    {
        yield return FlashColor(Color.white, archetypeData.deathFlashDuration * 0.35f, originalColor);
        yield return FlashColor(Color.red, archetypeData.deathFlashDuration * 0.5f, originalColor);
        yield return PulseScale(originalScale, 1.14f, archetypeData.deathFlashDuration * 0.45f);
        yield return new WaitForSecondsRealtime(archetypeData.deathPauseSeconds * 0.45f);
        yield return new WaitForSecondsRealtime(archetypeData.respawnDelaySeconds * 0.6f);
    }

    private IEnumerator EliteFinisherDeath(Color originalColor, Vector3 originalScale)
    {
        yield return FlashColor(Color.white, archetypeData.deathFlashDuration * 0.45f, originalColor);
        yield return PulseScale(originalScale, 1.08f, archetypeData.deathFlashDuration * 0.5f);
        yield return FlashColor(new Color(1f, 0.15f, 0.15f, 1f), archetypeData.deathFlashDuration * 0.9f, originalColor);
        yield return PulseScale(originalScale, 1.16f, archetypeData.deathFlashDuration * 0.8f);
        yield return new WaitForSecondsRealtime(archetypeData.deathPauseSeconds * 1.6f);
        yield return new WaitForSecondsRealtime(archetypeData.respawnDelaySeconds * 1.25f);
    }

    private IEnumerator FlashColor(Color flashColor, float duration, Color originalColor)
    {
        if (cachedRenderer == null)
            yield break;

        cachedRenderer.material.color = flashColor;
        yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, duration));
        cachedRenderer.material.color = originalColor;
    }

    private IEnumerator PulseScale(Vector3 originalScale, float multiplier, float duration)
    {
        float clampedDuration = Mathf.Max(0.01f, duration);
        Vector3 targetScale = originalScale * multiplier;

        float half = clampedDuration * 0.5f;
        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / half);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, lerp);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / half);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, lerp);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    public void SetRageVisual(bool rageActive)
    {
        Transform outline = transform.Find("OutlineQuad");
        if (outline != null)
            outline.gameObject.SetActive(rageActive);
    }

    private void HandleCombatFail(string reason)
    {
        if (currentState == State.Dead)
            return;

        if (punishRoutineRunning)
            return;

        Debug.Log($"[EnemyApproach] Fail ({reason}), punish={archetypeData.punishBehaviorType}");
        StartCoroutine(PlayPunishBehavior());
    }

    private IEnumerator PlayPunishBehavior()
    {
        punishRoutineRunning = true;

        Color originalColor = cachedRenderer != null ? cachedRenderer.material.color : Color.white;
        Vector3 originalScale = transform.localScale;
        Vector3 originalPosition = transform.localPosition;

        switch (archetypeData.punishBehaviorType)
        {
            case EnemyPunishBehaviorType.HeavyPunish:
                yield return HeavyPunishBehavior(originalColor, originalScale, originalPosition);
                break;

            case EnemyPunishBehaviorType.FakeOut:
                yield return FakeOutPunishBehavior(originalColor, originalScale, originalPosition);
                break;

            case EnemyPunishBehaviorType.ArmorBreak:
                yield return ArmorBreakPunishBehavior(originalColor, originalScale, originalPosition);
                break;

            case EnemyPunishBehaviorType.StandardRetry:
            default:
                yield return StandardRetryPunishBehavior(originalColor, originalScale, originalPosition);
                break;
        }

        if (cachedRenderer != null)
            cachedRenderer.material.color = originalColor;

        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        punishRoutineRunning = false;
    }

    private IEnumerator StandardRetryPunishBehavior(Color originalColor, Vector3 originalScale, Vector3 originalPosition)
    {
        yield return FlashColor(Color.white, 0.12f, originalColor);
    }

    private IEnumerator HeavyPunishBehavior(Color originalColor, Vector3 originalScale, Vector3 originalPosition)
    {
        yield return FlashColor(new Color(1f, 0.2f, 0.2f, 1f), 0.10f, originalColor);
        yield return PulseScale(originalScale, 1.08f, 0.18f);
        yield return ShakeLocalPosition(originalPosition, 0.10f, 0.08f, 28f);
    }

    private IEnumerator FakeOutPunishBehavior(Color originalColor, Vector3 originalScale, Vector3 originalPosition)
    {
        yield return FlashColor(Color.white, 0.08f, originalColor);
        yield return NudgeSideToSide(originalPosition, 0.18f, 0.35f);
        yield return FlashColor(new Color(1f, 0.85f, 0.85f, 1f), 0.06f, originalColor);
    }

    private IEnumerator ArmorBreakPunishBehavior(Color originalColor, Vector3 originalScale, Vector3 originalPosition)
    {
        yield return FlashColor(new Color(1f, 1f, 0.55f, 1f), 0.07f, originalColor);
        yield return FlashColor(Color.white, 0.06f, originalColor);
        yield return PulseScale(originalScale, 1.12f, 0.20f);
        yield return ShakeLocalPosition(originalPosition, 0.12f, 0.10f, 34f);
    }

    private IEnumerator ShakeLocalPosition(Vector3 originalPosition, float duration, float amplitude, float frequency)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float offsetX = Mathf.Sin(elapsed * frequency * Mathf.PI * 2f) * amplitude;
            transform.localPosition = originalPosition + new Vector3(offsetX, 0f, 0f);
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    private IEnumerator NudgeSideToSide(Vector3 originalPosition, float totalDuration, float distance)
    {
        float half = Mathf.Max(0.01f, totalDuration * 0.5f);
        Vector3 left = originalPosition + new Vector3(-distance, 0f, 0f);
        Vector3 right = originalPosition + new Vector3(distance, 0f, 0f);

        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / half);
            transform.localPosition = Vector3.Lerp(originalPosition, left, lerp);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / half);
            transform.localPosition = Vector3.Lerp(left, right, lerp);
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spawnPosition, 0.3f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(stopPosition, 0.3f);

        float telegraphDistance = archetypeData != null ? archetypeData.telegraphTriggerDistance : 8f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(stopPosition.x, stopPosition.y, telegraphDistance), 0.3f);
    }
}
