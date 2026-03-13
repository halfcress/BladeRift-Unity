using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeakpointAnchorEntry
{
    public WeakpointZone zone = WeakpointZone.Chest;
    public Transform anchor;
}

/// <summary>
/// Prefab / varyant bazlı gerçek weakpoint anchor noktaları.
/// Pozisyon bilgisi archetype'ta değil, düşman prefab'ında tutulur.
/// </summary>
public class EnemyWeakpointAnchors : MonoBehaviour
{
    [SerializeField] private List<WeakpointAnchorEntry> anchors = new();

    private readonly Dictionary<WeakpointZone, Transform> lookup = new();

    private void Awake()
    {
        RebuildLookup();
    }

    private void OnValidate()
    {
        RebuildLookup();
    }

    public bool TryGetAnchor(WeakpointZone zone, out Transform anchor)
    {
        RebuildLookup();
        return lookup.TryGetValue(zone, out anchor) && anchor != null;
    }

    public bool ValidateRequiredZones(IReadOnlyList<WeakpointZone> requiredZones, out string error)
    {
        RebuildLookup();

        if (requiredZones == null || requiredZones.Count == 0)
        {
            error = "Required zone list boş.";
            return false;
        }

        List<string> missing = new();
        for (int i = 0; i < requiredZones.Count; i++)
        {
            WeakpointZone zone = requiredZones[i];
            if (zone == WeakpointZone.None)
                continue;

            if (!lookup.TryGetValue(zone, out Transform t) || t == null)
                missing.Add(zone.ToString());
        }

        if (missing.Count > 0)
        {
            error = $"Missing weakpoint anchors: {string.Join(", ", missing)}";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private void RebuildLookup()
    {
        lookup.Clear();
        if (anchors == null)
            return;

        for (int i = 0; i < anchors.Count; i++)
        {
            WeakpointAnchorEntry entry = anchors[i];
            if (entry == null) continue;
            if (entry.zone == WeakpointZone.None) continue;
            if (entry.anchor == null) continue;
            lookup[entry.zone] = entry.anchor;
        }
    }
}
