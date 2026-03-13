using UnityEngine;

/// <summary>
/// Tüm combat parametrelerinin tek yeri.
/// Assets/_Project/ScriptableObjects/ altında bir asset olarak oluşturulur.
/// Kodda hardcode değer kullanma; buradan oku.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "BladeRift/GameConfig")]
public class GameConfig : ScriptableObject
{
    [System.Serializable]
    public class WeakpointVisualProfile
    {
        [Header("Core")]
        public Color baseColor = new Color(1f, 0.96f, 0.92f, 1f);
        [Range(0f, 1f)] public float baseAlpha = 1f;
        [Min(0.1f)] public float scale = 1f;

        [Header("Ring")]
        public Color ringColor = new Color(1f, 0.2f, 0.08f, 1f);
        [Range(0f, 1f)] public float ringAlpha = 0.8f;
        [Range(0f, 1f)] public float outerRingAlpha = 0.55f;

        [Header("Glow")]
        public Color glowColor = new Color(1f, 0.18f, 0.06f, 1f);
        [Range(0f, 1f)] public float glowAlpha = 0.65f;
        [Min(0.1f)] public float glowScale = 1.4f;

        [Header("Shimmer")]
        public Color shimmerColor = Color.white;
        [Range(0f, 1f)] public float shimmerAlpha = 0.2f;
        public float shimmerSweepSpeed = 30f;

        [Header("Animation")]
        [Range(0f, 0.25f)] public float pulseAmplitude = 0.035f;
        public float pulseSpeed = 1.4f;
        public float innerRotationSpeed = 18f;
        public float outerRotationSpeed = -10f;

        public static WeakpointVisualProfile CreateFallback(object role, bool rage)
        {
            WeakpointVisualProfile p = new WeakpointVisualProfile();

            if (rage)
            {
                p.baseColor = new Color(1f, 0.82f, 0.5f, 1f);
                p.ringColor = new Color(1f, 0.55f, 0.15f, 1f);
                p.glowColor = new Color(1f, 0.45f, 0.1f, 1f);
            }

            string roleName = role != null ? role.ToString() : string.Empty;
            switch (roleName)
            {
                case "Preview":
                    p.baseAlpha = 0.75f;
                    p.scale = 0.95f;
                    p.ringAlpha = 0.45f;
                    p.outerRingAlpha = 0.2f;
                    p.glowAlpha = 0.18f;
                    p.shimmerAlpha = 0f;
                    p.pulseAmplitude = 0f;
                    p.pulseSpeed = 0f;
                    p.innerRotationSpeed = 10f;
                    p.outerRotationSpeed = -6f;
                    break;
                case "Current":
                    p.baseAlpha = 1f;
                    p.scale = 1.05f;
                    p.ringAlpha = 0.92f;
                    p.outerRingAlpha = 0.72f;
                    p.glowAlpha = 0.72f;
                    p.shimmerAlpha = 0.28f;
                    p.pulseAmplitude = 0.04f;
                    p.pulseSpeed = 1.5f;
                    p.innerRotationSpeed = 20f;
                    p.outerRotationSpeed = -12f;
                    break;
                case "Next":
                    p.baseAlpha = 0.45f;
                    p.scale = 0.82f;
                    p.ringAlpha = 0.4f;
                    p.outerRingAlpha = 0.18f;
                    p.glowAlpha = 0.14f;
                    p.shimmerAlpha = 0.05f;
                    p.pulseAmplitude = 0.01f;
                    p.pulseSpeed = 0.9f;
                    p.innerRotationSpeed = 14f;
                    p.outerRotationSpeed = -8f;
                    break;
                case "NextPlusOne":
                    p.baseAlpha = 0.16f;
                    p.scale = 0.62f;
                    p.ringAlpha = 0.15f;
                    p.outerRingAlpha = 0.05f;
                    p.glowAlpha = 0.03f;
                    p.shimmerAlpha = 0f;
                    p.pulseAmplitude = 0f;
                    p.pulseSpeed = 0f;
                    p.innerRotationSpeed = 8f;
                    p.outerRotationSpeed = -5f;
                    break;
            }

            return p;
        }
    }

    [Header("Execution Window")]
    [Tooltip("Execution penceresi kaç saniye açık kalır.")]
    public float executionWindowSeconds = 2.0f;

    [Tooltip("Execution sırasında Time.timeScale değeri. (0.8 = hafif yavaşlama)")]
    public float timeScaleDuringExecution = 0.8f;

    [Header("Telegraph")]
    [Tooltip("Her weakpoint adımının kaç saniyede yanacağı.")]
    public float telegraphStepSeconds = 0.4f;

    [Header("Damage")]
    [Tooltip("Basic düşmana execution başına verilen hasar.")]
    public float basicExecutionDamage = 100f;

    [Tooltip("Elite düşmana execution başına verilen hasar.")]
    public float eliteExecutionDamage = 60f;

    [Tooltip("Boss'a execution başına verilen hasar.")]
    public float bossExecutionDamage = 40f;

    [Tooltip("Başarısız execution'da düşmanın oyuncuya verdiği hasar.")]
    public float failPunishDamage = 20f;

    [Tooltip("Başarısız execution'da oyuncunun düşmana verdiği chip hasarı.")]
    public float failChipDamage = 5f;

    [Header("Rage")]
    [Tooltip("Her başarılı chain adımında kazanılan rage miktarı.")]
    public float rageGainOnHit = 10f;

    [Tooltip("Zincir tamamen tamamlandığında kazanılan rage miktarı.")]
    public float rageGainOnSuccess = 25f;

    [Tooltip("Rage modunun kaç saniye sürdüğü.")]
    public float rageDurationSeconds = 5f;

    [Header("Weakpoint Visuals")]
    [Tooltip("Weakpoint jitter miktarı (piksel). Küçük titreme efekti.")]
    public float weakpointJitterPx = 7f;

    [Tooltip("Sıradaki (aktif) weakpoint için jitter çarpanı.")]
    public float activeWeakpointJitterMultiplier = 2.0f;

    [Space(8)]
    public WeakpointVisualProfile normalPreviewVisual = new WeakpointVisualProfile
    {
        baseAlpha = 0.75f,
        scale = 0.95f,
        ringAlpha = 0.45f,
        outerRingAlpha = 0.18f,
        glowAlpha = 0.15f,
        shimmerAlpha = 0f,
        pulseAmplitude = 0f,
        pulseSpeed = 0f,
        innerRotationSpeed = 10f,
        outerRotationSpeed = -6f,
        shimmerSweepSpeed = 0f
    };

    public WeakpointVisualProfile normalCurrentVisual = new WeakpointVisualProfile
    {
        baseAlpha = 1f,
        scale = 1.05f,
        ringAlpha = 0.92f,
        outerRingAlpha = 0.72f,
        glowAlpha = 0.72f,
        shimmerAlpha = 0.28f,
        pulseAmplitude = 0.04f,
        pulseSpeed = 1.55f,
        innerRotationSpeed = 22f,
        outerRotationSpeed = -12f,
        shimmerSweepSpeed = 32f
    };

    public WeakpointVisualProfile normalNextVisual = new WeakpointVisualProfile
    {
        baseAlpha = 0.42f,
        scale = 0.82f,
        ringAlpha = 0.38f,
        outerRingAlpha = 0.16f,
        glowAlpha = 0.12f,
        shimmerAlpha = 0.05f,
        pulseAmplitude = 0.012f,
        pulseSpeed = 0.95f,
        innerRotationSpeed = 14f,
        outerRotationSpeed = -8f,
        shimmerSweepSpeed = 18f
    };

    public WeakpointVisualProfile normalNextPlusOneVisual = new WeakpointVisualProfile
    {
        baseAlpha = 0.16f,
        scale = 0.62f,
        ringAlpha = 0.14f,
        outerRingAlpha = 0.05f,
        glowAlpha = 0.03f,
        shimmerAlpha = 0f,
        pulseAmplitude = 0f,
        pulseSpeed = 0f,
        innerRotationSpeed = 8f,
        outerRotationSpeed = -5f,
        shimmerSweepSpeed = 0f
    };

    public WeakpointVisualProfile ragePreviewVisual = new WeakpointVisualProfile
    {
        baseColor = new Color(1f, 0.86f, 0.62f, 1f),
        ringColor = new Color(1f, 0.52f, 0.14f, 1f),
        glowColor = new Color(1f, 0.48f, 0.12f, 1f),
        baseAlpha = 0.8f,
        scale = 1f,
        ringAlpha = 0.55f,
        outerRingAlpha = 0.25f,
        glowAlpha = 0.2f,
        shimmerAlpha = 0f,
        pulseAmplitude = 0f,
        pulseSpeed = 0f,
        innerRotationSpeed = 12f,
        outerRotationSpeed = -8f,
        shimmerSweepSpeed = 0f
    };

    public WeakpointVisualProfile rageCurrentVisual = new WeakpointVisualProfile
    {
        baseColor = new Color(1f, 0.86f, 0.62f, 1f),
        ringColor = new Color(1f, 0.52f, 0.14f, 1f),
        glowColor = new Color(1f, 0.48f, 0.12f, 1f),
        baseAlpha = 1f,
        scale = 1.08f,
        ringAlpha = 0.96f,
        outerRingAlpha = 0.78f,
        glowAlpha = 0.82f,
        shimmerAlpha = 0.32f,
        pulseAmplitude = 0.045f,
        pulseSpeed = 1.8f,
        innerRotationSpeed = 24f,
        outerRotationSpeed = -14f,
        shimmerSweepSpeed = 36f
    };

    public WeakpointVisualProfile rageNextVisual = new WeakpointVisualProfile
    {
        baseColor = new Color(1f, 0.86f, 0.62f, 1f),
        ringColor = new Color(1f, 0.52f, 0.14f, 1f),
        glowColor = new Color(1f, 0.48f, 0.12f, 1f),
        baseAlpha = 0.44f,
        scale = 0.84f,
        ringAlpha = 0.42f,
        outerRingAlpha = 0.18f,
        glowAlpha = 0.16f,
        shimmerAlpha = 0.05f,
        pulseAmplitude = 0.012f,
        pulseSpeed = 1.05f,
        innerRotationSpeed = 15f,
        outerRotationSpeed = -9f,
        shimmerSweepSpeed = 18f
    };

    public WeakpointVisualProfile rageNextPlusOneVisual = new WeakpointVisualProfile
    {
        baseColor = new Color(1f, 0.86f, 0.62f, 1f),
        ringColor = new Color(1f, 0.52f, 0.14f, 1f),
        glowColor = new Color(1f, 0.48f, 0.12f, 1f),
        baseAlpha = 0.18f,
        scale = 0.64f,
        ringAlpha = 0.16f,
        outerRingAlpha = 0.06f,
        glowAlpha = 0.04f,
        shimmerAlpha = 0f,
        pulseAmplitude = 0f,
        pulseSpeed = 0f,
        innerRotationSpeed = 9f,
        outerRotationSpeed = -5f,
        shimmerSweepSpeed = 0f
    };
}
