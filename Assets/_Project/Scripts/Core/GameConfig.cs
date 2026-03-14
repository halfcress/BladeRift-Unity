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
        [Header("Backplate")]
        public Color backplateColor = new Color(0.08f, 0.02f, 0.02f, 1f);
        [Range(0f, 1f)] public float backplateAlpha = 0f;
        [Min(0.1f)] public float backplateScale = 1.35f;

        [Header("Core")]
        public Color baseColor = new Color(1f, 0.985f, 0.94f, 1f);
        [Range(0f, 1f)] public float baseAlpha = 1f;
        [Min(0.1f)] public float scale = 1f;

        [Header("Ring")]
        public Color ringColor = new Color(1f, 0.97f, 0.9f, 1f);
        [Range(0f, 1f)] public float ringAlpha = 0.9f;
        [Range(0f, 1f)] public float outerRingAlpha = 0.7f;

        [Header("Glow")]
        public Color glowColor = new Color(1f, 0.22f, 0.08f, 1f);
        [Range(0f, 1f)] public float glowAlpha = 0.25f;
        [Min(0.1f)] public float glowScale = 1.08f;

        [Header("Aggressive Corona")]
        public Color coronaAColor = new Color(1f, 0.34f, 0.1f, 1f);
        [Range(0f, 1f)] public float coronaAAlpha = 0f;
        [Min(0.1f)] public float coronaAScale = 1.55f;
        public float coronaARotationSpeed = 24f;

        public Color coronaBColor = new Color(1f, 0.83f, 0.34f, 1f);
        [Range(0f, 1f)] public float coronaBAlpha = 0f;
        [Min(0.1f)] public float coronaBScale = 1.9f;
        public float coronaBRotationSpeed = -18f;

        [Range(0f, 0.25f)] public float coronaPulseAmplitude = 0.08f;
        public float coronaPulseSpeed = 1.9f;
        [Range(0f, 0.5f)] public float coronaFlickerStrength = 0.16f;
        public float coronaFlickerSpeed = 7.5f;

        [Header("Shimmer")]
        public Color shimmerColor = Color.white;
        [Range(0f, 1f)] public float shimmerAlpha = 0.15f;
        public float shimmerSweepSpeed = 26f;

        [Header("Animation")]
        [Range(0f, 0.25f)] public float pulseAmplitude = 0.035f;
        public float pulseSpeed = 1.4f;
        public float innerRotationSpeed = 18f;
        public float outerRotationSpeed = -10f;

        public static WeakpointVisualProfile CreateFallback(object role, bool rage)
        {
            string roleName = role != null ? role.ToString() : string.Empty;
            bool isPreview = roleName == "Preview";
            bool isCurrent = roleName == "Current";
            bool isNext = roleName == "Next";
            bool isNextPlusOne = roleName == "NextPlusOne";

            WeakpointVisualProfile p = new WeakpointVisualProfile();

            if (rage)
            {
                p.baseColor = new Color(1f, 0.92f, 0.72f, 1f);
                p.ringColor = new Color(1f, 0.93f, 0.82f, 1f);
                p.glowColor = new Color(1f, 0.5f, 0.12f, 1f);
                p.coronaAColor = new Color(1f, 0.62f, 0.15f, 1f);
                p.coronaBColor = new Color(1f, 0.9f, 0.45f, 1f);
                p.backplateColor = new Color(0.1f, 0.04f, 0.01f, 1f);
            }

            if (isPreview)
            {
                p.backplateAlpha = 0f;
                p.baseAlpha = 0.88f;
                p.scale = 1.0f;
                p.ringAlpha = 0.6f;
                p.outerRingAlpha = 0.3f;
                p.glowAlpha = 0.15f;
                p.coronaAAlpha = 0f;
                p.coronaBAlpha = 0f;
                p.shimmerAlpha = 0f;
                p.pulseAmplitude = 0f;
                p.pulseSpeed = 0f;
                p.shimmerSweepSpeed = 0f;
                p.innerRotationSpeed = 8f;
                p.outerRotationSpeed = -4f;
                p.coronaPulseAmplitude = 0f;
                p.coronaPulseSpeed = 0f;
                p.coronaFlickerStrength = 0f;
                p.coronaFlickerSpeed = 0f;
                return p;
            }

            if (isCurrent)
            {
                p.backplateAlpha = rage ? 0.42f : 0.34f;
                p.backplateScale = 1.18f;

                p.baseAlpha = 1f;
                p.scale = 1.18f;

                p.ringAlpha = 1f;
                p.outerRingAlpha = 0.92f;

                p.glowAlpha = rage ? 0.28f : 0.18f;
                p.glowScale = 1.05f;

                p.coronaAAlpha = 0.96f;
                p.coronaAScale = 1.54f;
                p.coronaARotationSpeed = 26f;

                p.coronaBAlpha = 0.62f;
                p.coronaBScale = 1.9f;
                p.coronaBRotationSpeed = -20f;

                p.coronaPulseAmplitude = 0.09f;
                p.coronaPulseSpeed = 2.05f;
                p.coronaFlickerStrength = 0.18f;
                p.coronaFlickerSpeed = 8.5f;

                p.shimmerAlpha = 0.18f;
                p.shimmerSweepSpeed = 22f;

                p.pulseAmplitude = 0.05f;
                p.pulseSpeed = 1.55f;
                p.innerRotationSpeed = 16f;
                p.outerRotationSpeed = -9f;
                return p;
            }

            if (isNext)
            {
                p.backplateAlpha = rage ? 0.08f : 0.04f;
                p.backplateScale = 1.08f;

                p.baseAlpha = 0.64f;
                p.scale = 0.9f;

                p.ringAlpha = 0.72f;
                p.outerRingAlpha = 0.38f;

                p.glowAlpha = 0.08f;
                p.glowScale = 1.04f;

                p.coronaAAlpha = 0f;
                p.coronaBAlpha = 0f;

                p.shimmerAlpha = 0.04f;
                p.shimmerSweepSpeed = 12f;

                p.pulseAmplitude = 0.012f;
                p.pulseSpeed = 0.9f;
                p.innerRotationSpeed = 11f;
                p.outerRotationSpeed = -6f;
                p.coronaPulseAmplitude = 0f;
                p.coronaPulseSpeed = 0f;
                p.coronaFlickerStrength = 0f;
                p.coronaFlickerSpeed = 0f;
                return p;
            }

            if (isNextPlusOne)
            {
                p.backplateAlpha = 0f;
                p.baseAlpha = 0.34f;
                p.scale = 0.72f;
                p.ringAlpha = 0.42f;
                p.outerRingAlpha = 0.18f;
                p.glowAlpha = 0.03f;
                p.glowScale = 1.02f;
                p.coronaAAlpha = 0f;
                p.coronaBAlpha = 0f;
                p.shimmerAlpha = 0f;
                p.shimmerSweepSpeed = 0f;
                p.pulseAmplitude = 0f;
                p.pulseSpeed = 0f;
                p.innerRotationSpeed = 7f;
                p.outerRotationSpeed = -4f;
                p.coronaPulseAmplitude = 0f;
                p.coronaPulseSpeed = 0f;
                p.coronaFlickerStrength = 0f;
                p.coronaFlickerSpeed = 0f;
                return p;
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
        backplateAlpha = 0f,
        baseAlpha = 0.88f,
        scale = 1.0f,
        ringAlpha = 0.6f,
        outerRingAlpha = 0.3f,
        glowAlpha = 0.15f,
        glowScale = 1.04f,
        coronaAAlpha = 0f,
        coronaBAlpha = 0f,
        shimmerAlpha = 0f,
        pulseAmplitude = 0f,
        pulseSpeed = 0f,
        innerRotationSpeed = 8f,
        outerRotationSpeed = -4f,
        shimmerSweepSpeed = 0f,
        coronaPulseAmplitude = 0f,
        coronaPulseSpeed = 0f,
        coronaFlickerStrength = 0f,
        coronaFlickerSpeed = 0f
    };

    public WeakpointVisualProfile normalCurrentVisual = new WeakpointVisualProfile
    {
        backplateColor = new Color(0.12f, 0.03f, 0.02f, 1f),
        backplateAlpha = 0.34f,
        backplateScale = 1.18f,

        baseColor = new Color(1f, 0.99f, 0.95f, 1f),
        baseAlpha = 1f,
        scale = 1.18f,

        ringColor = new Color(1f, 0.97f, 0.9f, 1f),
        ringAlpha = 1f,
        outerRingAlpha = 0.92f,

        glowColor = new Color(1f, 0.22f, 0.08f, 1f),
        glowAlpha = 0.18f,
        glowScale = 1.05f,

        coronaAColor = new Color(1f, 0.34f, 0.1f, 1f),
        coronaAAlpha = 0.96f,
        coronaAScale = 1.54f,
        coronaARotationSpeed = 26f,

        coronaBColor = new Color(1f, 0.83f, 0.34f, 1f),
        coronaBAlpha = 0.62f,
        coronaBScale = 1.90f,
        coronaBRotationSpeed = -20f,

        coronaPulseAmplitude = 0.09f,
        coronaPulseSpeed = 2.05f,
        coronaFlickerStrength = 0.18f,
        coronaFlickerSpeed = 8.5f,

        shimmerColor = Color.white,
        shimmerAlpha = 0.18f,
        shimmerSweepSpeed = 22f,

        pulseAmplitude = 0.05f,
        pulseSpeed = 1.55f,
        innerRotationSpeed = 16f,
        outerRotationSpeed = -9f
    };

    public WeakpointVisualProfile normalNextVisual = new WeakpointVisualProfile
    {
        backplateColor = new Color(0.08f, 0.02f, 0.02f, 1f),
        backplateAlpha = 0.04f,
        backplateScale = 1.08f,

        baseColor = new Color(1f, 0.98f, 0.94f, 1f),
        baseAlpha = 0.64f,
        scale = 0.90f,

        ringColor = new Color(1f, 0.96f, 0.9f, 1f),
        ringAlpha = 0.72f,
        outerRingAlpha = 0.38f,

        glowColor = new Color(1f, 0.22f, 0.08f, 1f),
        glowAlpha = 0.08f,
        glowScale = 1.04f,

        coronaAAlpha = 0f,
        coronaBAlpha = 0f,

        shimmerColor = Color.white,
        shimmerAlpha = 0.04f,
        shimmerSweepSpeed = 12f,

        pulseAmplitude = 0.012f,
        pulseSpeed = 0.90f,
        innerRotationSpeed = 11f,
        outerRotationSpeed = -6f,

        coronaPulseAmplitude = 0f,
        coronaPulseSpeed = 0f,
        coronaFlickerStrength = 0f,
        coronaFlickerSpeed = 0f
    };

    public WeakpointVisualProfile normalNextPlusOneVisual = new WeakpointVisualProfile
    {
        backplateAlpha = 0f,
        baseColor = new Color(1f, 0.98f, 0.94f, 1f),
        baseAlpha = 0.34f,
        scale = 0.72f,

        ringColor = new Color(1f, 0.96f, 0.9f, 1f),
        ringAlpha = 0.42f,
        outerRingAlpha = 0.18f,

        glowColor = new Color(1f, 0.22f, 0.08f, 1f),
        glowAlpha = 0.03f,
        glowScale = 1.02f,

        coronaAAlpha = 0f,
        coronaBAlpha = 0f,

        shimmerAlpha = 0f,
        shimmerSweepSpeed = 0f,

        pulseAmplitude = 0f,
        pulseSpeed = 0f,
        innerRotationSpeed = 7f,
        outerRotationSpeed = -4f,

        coronaPulseAmplitude = 0f,
        coronaPulseSpeed = 0f,
        coronaFlickerStrength = 0f,
        coronaFlickerSpeed = 0f
    };

    public WeakpointVisualProfile ragePreviewVisual = new WeakpointVisualProfile
    {
        baseColor = new Color(1f, 0.94f, 0.78f, 1f),
        ringColor = new Color(1f, 0.94f, 0.84f, 1f),
        glowColor = new Color(1f, 0.48f, 0.12f, 1f),
        coronaAColor = new Color(1f, 0.6f, 0.12f, 1f),
        coronaBColor = new Color(1f, 0.92f, 0.42f, 1f),
        backplateColor = new Color(0.1f, 0.04f, 0.01f, 1f),

        backplateAlpha = 0f,
        baseAlpha = 0.90f,
        scale = 1.02f,
        ringAlpha = 0.66f,
        outerRingAlpha = 0.36f,
        glowAlpha = 0.18f,
        glowScale = 1.05f,
        coronaAAlpha = 0f,
        coronaBAlpha = 0f,
        shimmerAlpha = 0f,
        pulseAmplitude = 0f,
        pulseSpeed = 0f,
        innerRotationSpeed = 10f,
        outerRotationSpeed = -6f,
        shimmerSweepSpeed = 0f,
        coronaPulseAmplitude = 0f,
        coronaPulseSpeed = 0f,
        coronaFlickerStrength = 0f,
        coronaFlickerSpeed = 0f
    };

    public WeakpointVisualProfile rageCurrentVisual = new WeakpointVisualProfile
    {
        backplateColor = new Color(0.1f, 0.04f, 0.01f, 1f),
        backplateAlpha = 0.42f,
        backplateScale = 1.18f,

        baseColor = new Color(1f, 0.95f, 0.8f, 1f),
        baseAlpha = 1f,
        scale = 1.2f,

        ringColor = new Color(1f, 0.94f, 0.84f, 1f),
        ringAlpha = 1f,
        outerRingAlpha = 0.94f,

        glowColor = new Color(1f, 0.48f, 0.12f, 1f),
        glowAlpha = 0.22f,
        glowScale = 1.06f,

        coronaAColor = new Color(1f, 0.62f, 0.15f, 1f),
        coronaAAlpha = 1f,
        coronaAScale = 1.56f,
        coronaARotationSpeed = 28f,

        coronaBColor = new Color(1f, 0.9f, 0.45f, 1f),
        coronaBAlpha = 0.68f,
        coronaBScale = 1.96f,
        coronaBRotationSpeed = -22f,

        coronaPulseAmplitude = 0.1f,
        coronaPulseSpeed = 2.2f,
        coronaFlickerStrength = 0.2f,
        coronaFlickerSpeed = 9.5f,

        shimmerColor = Color.white,
        shimmerAlpha = 0.2f,
        shimmerSweepSpeed = 24f,

        pulseAmplitude = 0.052f,
        pulseSpeed = 1.7f,
        innerRotationSpeed = 18f,
        outerRotationSpeed = -10f
    };

    public WeakpointVisualProfile rageNextVisual = new WeakpointVisualProfile
    {
        backplateColor = new Color(0.1f, 0.04f, 0.01f, 1f),
        backplateAlpha = 0.06f,
        backplateScale = 1.1f,

        baseColor = new Color(1f, 0.95f, 0.8f, 1f),
        baseAlpha = 0.68f,
        scale = 0.92f,

        ringColor = new Color(1f, 0.94f, 0.84f, 1f),
        ringAlpha = 0.74f,
        outerRingAlpha = 0.4f,

        glowColor = new Color(1f, 0.48f, 0.12f, 1f),
        glowAlpha = 0.1f,
        glowScale = 1.04f,

        coronaAAlpha = 0f,
        coronaBAlpha = 0f,

        shimmerColor = Color.white,
        shimmerAlpha = 0.05f,
        shimmerSweepSpeed = 12f,

        pulseAmplitude = 0.012f,
        pulseSpeed = 1f,
        innerRotationSpeed = 12f,
        outerRotationSpeed = -7f,

        coronaPulseAmplitude = 0f,
        coronaPulseSpeed = 0f,
        coronaFlickerStrength = 0f,
        coronaFlickerSpeed = 0f
    };

    public WeakpointVisualProfile rageNextPlusOneVisual = new WeakpointVisualProfile
    {
        baseColor = new Color(1f, 0.95f, 0.8f, 1f),
        baseAlpha = 0.38f,
        scale = 0.74f,

        ringColor = new Color(1f, 0.94f, 0.84f, 1f),
        ringAlpha = 0.44f,
        outerRingAlpha = 0.2f,

        glowColor = new Color(1f, 0.48f, 0.12f, 1f),
        glowAlpha = 0.04f,
        glowScale = 1.02f,

        coronaAAlpha = 0f,
        coronaBAlpha = 0f,

        shimmerAlpha = 0f,
        shimmerSweepSpeed = 0f,

        pulseAmplitude = 0f,
        pulseSpeed = 0f,
        innerRotationSpeed = 7f,
        outerRotationSpeed = -4f,

        coronaPulseAmplitude = 0f,
        coronaPulseSpeed = 0f,
        coronaFlickerStrength = 0f,
        coronaFlickerSpeed = 0f
    };
}
