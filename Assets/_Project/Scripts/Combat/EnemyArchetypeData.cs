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

[CreateAssetMenu(fileName = "EnemyArchetype_", menuName = "BladeRift/Combat/Enemy Archetype Data")]
public class EnemyArchetypeData : ScriptableObject
{
    public EnemyArchetypeType archetypeType = EnemyArchetypeType.Basic;

    public float approachSpeed = 4f;
    public float telegraphTriggerDistance = 8f;

    public float deathPauseSeconds = 1.5f;
    public float respawnDelaySeconds = 1f;
    public float deathFlashDuration = 0.3f;

    [Header("Weakpoint Pattern (Bridge)")]
    [Tooltip("Geçici bridge veri. Controlled-random gelene kadar sabit zone dizisi kullanılır.")]
    public WeakpointZone[] fixedZonePattern;
}
