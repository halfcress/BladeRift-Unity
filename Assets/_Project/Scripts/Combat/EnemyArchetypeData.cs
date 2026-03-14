using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum EnemyArchetypeType
{
    Basic,
    Fast,
    Tank,
    Trick,
    Elite,
    Boss
}

public enum EnemyPatternComplexity
{
    Simple,
    Standard,
    Dense,
    
}

public enum EnemyApproachBehaviorType
{
    Straight,
    SideLean,
    Dash,
    HeavyWalk
}

public enum EnemyPunishBehaviorType
{
    StandardRetry,
    HeavyPunish,
    FakeOut,
    ArmorBreak
}

public enum EnemyDeathBehaviorType
{
    StandardSplit,
    HeavySplit,
    Burst,
    EliteFinisher
}

[Serializable]
public class ZoneWeightEntry
{
    public WeakpointZone zone = WeakpointZone.Chest;
    public bool enabled = true;
    [Min(0f)] public float weight = 1f;
}

[CreateAssetMenu(fileName = "EnemyArchetype_", menuName = "BladeRift/Combat/Enemy Archetype Data")]
public class EnemyArchetypeData : ScriptableObject
{
    [Header("Core")]
    public EnemyArchetypeType archetypeType = EnemyArchetypeType.Basic;
    public EnemyPatternComplexity patternComplexity = EnemyPatternComplexity.Standard;
    public EnemyApproachBehaviorType approachBehaviorType = EnemyApproachBehaviorType.Straight;
    public EnemyPunishBehaviorType punishBehaviorType = EnemyPunishBehaviorType.StandardRetry;
    public EnemyDeathBehaviorType deathBehaviorType = EnemyDeathBehaviorType.StandardSplit;

    [Header("Approach")]
    [Min(0.1f)] public float approachSpeed = 4f;
    [Min(0.1f)] public float telegraphTriggerDistance = 8f;

    [Header("Death Timing")]
    [Min(0f)] public float deathPauseSeconds = 1.5f;
    [Min(0f)] public float respawnDelaySeconds = 1f;
    [Min(0f)] public float deathFlashDuration = 0.3f;

    [Header("Weakpoint Pattern (Legacy / Manual)")]
    [Tooltip("Controlled-random kapalıysa bu dizi doğrudan kullanılır.")]
    public WeakpointZone[] fixedZonePattern;

    [Header("Controlled Random")]
    [Tooltip("Açıksa pattern fixedZonePattern yerine zone havuzundan üretilir.")]
    public bool useControlledRandom = true;

    [Min(2)]
    [Tooltip("Üretilecek pattern uzunluğu. 2-6 arası.")]
    public int controlledPatternLength = 3;

    [Tooltip("Body-zone havuzu ve ağırlıkları. Aynı zone art arda veya 1 boşlukla tekrar etmez.")]
    public List<ZoneWeightEntry> zonePool = new();

    [Header("Debug")]
    [Tooltip("Üretilen pattern'i Console'a yazar.")]
    public bool debugLogGeneratedPattern = true;

    public List<WeakpointZone> GetEnabledZones()
    {
        EnsureDefaultZonePool();

        List<WeakpointZone> result = new();

        if (useControlledRandom)
        {
            List<ZoneWeightEntry> enabledPool = CollectEnabledPool();
            for (int i = 0; i < enabledPool.Count; i++)
            {
                WeakpointZone zone = enabledPool[i].zone;
                if (!result.Contains(zone))
                    result.Add(zone);
            }

            return result;
        }

        if (fixedZonePattern != null)
        {
            for (int i = 0; i < fixedZonePattern.Length; i++)
            {
                WeakpointZone zone = fixedZonePattern[i];
                if (zone == WeakpointZone.None)
                    continue;
                if (!result.Contains(zone))
                    result.Add(zone);
            }
        }

        return result;
    }

    public bool ValidateRuntimeConfig(out string error)
    {
        EnsureDefaultZonePool();

        if (!useControlledRandom)
        {
            if (fixedZonePattern == null || fixedZonePattern.Length == 0)
            {
                error = "Controlled-random kapalı ama fixedZonePattern boş.";
                return false;
            }

            int validCount = 0;
            for (int i = 0; i < fixedZonePattern.Length; i++)
            {
                if (fixedZonePattern[i] != WeakpointZone.None)
                    validCount++;
            }

            if (validCount == 0)
            {
                error = "fixedZonePattern içinde geçerli zone yok.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        if (controlledPatternLength < 2 || controlledPatternLength > 6)
        {
            error = $"controlledPatternLength geçersiz: {controlledPatternLength}. Beklenen aralık 2-6.";
            return false;
        }

        List<ZoneWeightEntry> enabledPool = CollectEnabledPool();
        if (enabledPool.Count == 0)
        {
            error = "Controlled-random açık ama aktif/weighted zone yok.";
            return false;
        }

        HashSet<WeakpointZone> uniqueZones = new();
        for (int i = 0; i < enabledPool.Count; i++)
        {
            WeakpointZone zone = enabledPool[i].zone;
            if (!uniqueZones.Add(zone))
            {
                error = $"ZonePool içinde duplicate aktif zone var: {zone}. Her zone yalnızca 1 kez tanımlanmalı.";
                return false;
            }
        }

        int effectiveLength = GetEffectivePatternLength();
        int requiredUniqueZones = effectiveLength <= 2 ? 2 : 3;
        if (uniqueZones.Count < requiredUniqueZones)
        {
            error = $"EffectivePatternLength={effectiveLength} için en az {requiredUniqueZones} benzersiz aktif zone gerekli. Şu an {uniqueZones.Count} var.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public WeakpointZone[] BuildPattern()
    {
        if (!ValidateRuntimeConfig(out string configError))
        {
            Debug.LogError($"[EnemyArchetypeData] {name}: {configError}", this);
            return Array.Empty<WeakpointZone>();
        }

        if (!useControlledRandom)
            return CloneFixedPattern();

        List<ZoneWeightEntry> enabledPool = CollectEnabledPool();
        int effectiveLength = GetEffectivePatternLength();
        List<WeakpointZone> result = new(effectiveLength);

        for (int i = 0; i < effectiveLength; i++)
        {
            List<ZoneWeightEntry> candidates = CollectValidCandidates(enabledPool, result, i, effectiveLength);
            if (candidates.Count == 0)
            {
                Debug.LogError($"[EnemyArchetypeData] {name}: Index {i} için geçerli complexity-aware candidate kalmadı. Mevcut pattern={FormatPattern(result)}", this);
                return Array.Empty<WeakpointZone>();
            }

            result.Add(PickWeightedWithComplexity(candidates, i, effectiveLength));
        }

        if (debugLogGeneratedPattern)
            Debug.Log($"[EnemyArchetypeData] {name}: Generated pattern = {FormatPattern(result)} | Complexity={patternComplexity} | Length={effectiveLength}", this);

        return result.ToArray();
    }

    private void OnValidate()
    {
        EnsureDefaultZonePool();
        controlledPatternLength = Mathf.Clamp(controlledPatternLength, 2, 6);
        approachSpeed = Mathf.Max(0.1f, approachSpeed);
        telegraphTriggerDistance = Mathf.Max(0.1f, telegraphTriggerDistance);
        deathPauseSeconds = Mathf.Max(0f, deathPauseSeconds);
        respawnDelaySeconds = Mathf.Max(0f, respawnDelaySeconds);
        deathFlashDuration = Mathf.Max(0f, deathFlashDuration);

        if (zonePool == null)
            zonePool = new List<ZoneWeightEntry>();

        for (int i = 0; i < zonePool.Count; i++)
        {
            if (zonePool[i] == null)
                zonePool[i] = new ZoneWeightEntry();

            zonePool[i].weight = Mathf.Max(0f, zonePool[i].weight);
        }
    }

    private List<ZoneWeightEntry> CollectEnabledPool()
    {
        List<ZoneWeightEntry> enabledPool = new();

        if (zonePool == null)
            return enabledPool;

        for (int i = 0; i < zonePool.Count; i++)
        {
            ZoneWeightEntry entry = zonePool[i];
            if (entry == null) continue;
            if (!entry.enabled) continue;
            if (entry.zone == WeakpointZone.None) continue;
            if (entry.weight <= 0f) continue;

            enabledPool.Add(entry);
        }

        return enabledPool;
    }

    private int GetEffectivePatternLength()
    {
        if (!useControlledRandom)
            return fixedZonePattern != null ? fixedZonePattern.Length : 0;

        return Mathf.Clamp(controlledPatternLength, 2, 6);
    }

    private List<ZoneWeightEntry> CollectValidCandidates(List<ZoneWeightEntry> enabledPool, List<WeakpointZone> currentPattern, int index, int totalLength)
    {
        List<ZoneWeightEntry> candidates = new();

        for (int i = 0; i < enabledPool.Count; i++)
        {
            ZoneWeightEntry entry = enabledPool[i];
            WeakpointZone candidate = entry.zone;

            if (currentPattern.Count >= 1 && candidate == currentPattern[^1])
                continue;

            if (currentPattern.Count >= 2 && candidate == currentPattern[^2])
                continue;

            if (!IsZoneAllowedByComplexity(candidate, index, totalLength))
                continue;

            candidates.Add(entry);
        }

        return candidates;
    }

    private bool IsZoneAllowedByComplexity(WeakpointZone zone, int index, int totalLength)
    {
        bool isTorso = zone == WeakpointZone.LeftTorso || zone == WeakpointZone.RightTorso;
        bool isHead = zone == WeakpointZone.Head;

        switch (patternComplexity)
        {
            case EnemyPatternComplexity.Simple:
                if (index == 0 && (isHead || isTorso))
                    return false;
                if (index <= 1 && isTorso)
                    return false;
                return true;

            case EnemyPatternComplexity.Standard:
                if (index == 0 && isTorso)
                    return false;
                return true;

            case EnemyPatternComplexity.Dense:
                return true;

            default:
                return true;
        }
    }

    private WeakpointZone PickWeightedWithComplexity(List<ZoneWeightEntry> candidates, int index, int totalLength)
    {
        float totalWeight = 0f;
        for (int i = 0; i < candidates.Count; i++)
            totalWeight += GetComplexityAdjustedWeight(candidates[i], index, totalLength);

        if (totalWeight <= 0f)
            return candidates[UnityEngine.Random.Range(0, candidates.Count)].zone;

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += GetComplexityAdjustedWeight(candidates[i], index, totalLength);
            if (roll <= cumulative)
                return candidates[i].zone;
        }

        return candidates[^1].zone;
    }

    private float GetComplexityAdjustedWeight(ZoneWeightEntry entry, int index, int totalLength)
    {
        float weight = Mathf.Max(0f, entry.weight);
        bool isTorso = entry.zone == WeakpointZone.LeftTorso || entry.zone == WeakpointZone.RightTorso;
        bool isHead = entry.zone == WeakpointZone.Head;
        bool isShoulder = entry.zone == WeakpointZone.LeftShoulder || entry.zone == WeakpointZone.RightShoulder;
        bool isChest = entry.zone == WeakpointZone.Chest;

        switch (patternComplexity)
        {
            case EnemyPatternComplexity.Simple:
                if (isChest) weight *= 1.35f;
                if (isShoulder) weight *= 1.2f;
                if (isHead) weight *= 0.7f;
                if (isTorso) weight *= 0.45f;
                break;

            case EnemyPatternComplexity.Standard:
                if (index == totalLength - 1 && isHead)
                    weight *= 1.15f;
                break;

            case EnemyPatternComplexity.Dense:
                if (isHead) weight *= 1.35f;
                if (isTorso) weight *= 1.25f;
                if (isChest) weight *= 0.9f;
                break;

        }

        return Mathf.Max(0f, weight);
    }

    private WeakpointZone[] CloneFixedPattern()
    {
        if (fixedZonePattern == null || fixedZonePattern.Length == 0)
            return Array.Empty<WeakpointZone>();

        WeakpointZone[] clone = new WeakpointZone[fixedZonePattern.Length];
        Array.Copy(fixedZonePattern, clone, fixedZonePattern.Length);
        return clone;
    }

    private string FormatPattern(IReadOnlyList<WeakpointZone> pattern)
    {
        if (pattern == null || pattern.Count == 0)
            return "[]";

        StringBuilder sb = new StringBuilder();
        sb.Append('[');
        for (int i = 0; i < pattern.Count; i++)
        {
            if (i > 0)
                sb.Append(" -> ");
            sb.Append(pattern[i]);
        }
        sb.Append(']');
        return sb.ToString();
    }

    private void EnsureDefaultZonePool()
    {
        if (zonePool == null)
            zonePool = new List<ZoneWeightEntry>();

        if (zonePool.Count > 0)
            return;

        zonePool.Add(new ZoneWeightEntry { zone = WeakpointZone.Head, enabled = true, weight = 1f });
        zonePool.Add(new ZoneWeightEntry { zone = WeakpointZone.LeftShoulder, enabled = true, weight = 1f });
        zonePool.Add(new ZoneWeightEntry { zone = WeakpointZone.RightShoulder, enabled = true, weight = 1f });
        zonePool.Add(new ZoneWeightEntry { zone = WeakpointZone.Chest, enabled = true, weight = 1f });
        zonePool.Add(new ZoneWeightEntry { zone = WeakpointZone.LeftTorso, enabled = false, weight = 1f });
        zonePool.Add(new ZoneWeightEntry { zone = WeakpointZone.RightTorso, enabled = false, weight = 1f });
    }
}
