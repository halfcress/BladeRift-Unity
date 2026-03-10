using UnityEngine;

/// <summary>
/// Tüm combat parametrelerinin tek yeri.
/// Assets/_Project/ScriptableObjects/ altında bir asset olarak oluşturulur.
/// Kodda hardcode değer kullanma; buradan oku.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "BladeRift/GameConfig")]
public class GameConfig : ScriptableObject
{
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
}