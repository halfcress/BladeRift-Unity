using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Düşmanın koridordan yaklaşmasını yönetir.
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

    [Header("Rage Hit")]
    [Tooltip("Silüet hit alanını genişletme çarpanı. 1.0 = tam bounds, 1.3 = %30 padding")]
    [SerializeField] private float rageHitPadding = 1.3f;

    [Header("State (Read-only)")]
    [SerializeField] private State currentState = State.Idle;

    public enum State { Idle, Approaching, TelegraphTriggered, WaitingForResult, Dead }

    public bool IsInDeathSequence => currentState == State.Dead;

    private Renderer cachedRenderer;
    private bool validated = false;

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

        transform.position = Vector3.MoveTowards(
            transform.position,
            stopPosition,
            archetypeData.approachSpeed * Time.deltaTime
        );

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
        currentState = State.Approaching;
        Debug.Log("[EnemyApproach] Yaklaşma başladı.");
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
        Debug.Log("[EnemyApproach] Düşman öldü. Spawner yeni düşman üretecek...");
        StartCoroutine(DeathThenDespawn());
    }

    private IEnumerator DeathThenDespawn()
    {
        if (cachedRenderer != null)
        {
            Color original = cachedRenderer.material.color;
            cachedRenderer.material.color = Color.red;
            yield return new WaitForSeconds(archetypeData.deathFlashDuration);
            cachedRenderer.material.color = original;
        }

        yield return new WaitForSecondsRealtime(archetypeData.deathPauseSeconds);
        yield return new WaitForSecondsRealtime(archetypeData.respawnDelaySeconds);

        spawner?.OnEnemyKilledImmediate();
        Destroy(gameObject);
    }

    public void SetRageVisual(bool rageActive)
    {
        Transform outline = transform.Find("OutlineQuad");
        if (outline != null)
            outline.gameObject.SetActive(rageActive);
    }

    private void HandleCombatFail(string reason)
    {
        Debug.Log($"[EnemyApproach] Fail ({reason}), düşman bekliyor.");
        StartCoroutine(PunishFlash());
    }

    private IEnumerator PunishFlash()
    {
        if (cachedRenderer != null)
        {
            Color original = cachedRenderer.material.color;
            cachedRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            cachedRenderer.material.color = original;
        }
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
