using UnityEngine;
using TMPro;

/// <summary>
/// Rage sistemi.
/// - Her doğru hit rage doldurur (combo'dan bağımsız)
/// - Rage dolunca aktif olur: weakpoint zorunluluğu kalkar
/// - Fail → rage sıfırlanır
/// </summary>
public class RageManager : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] private int hitsToActivate = 6;

    [Header("Debug UI (opsiyonel)")]
    [SerializeField] private TextMeshProUGUI rageText;

    [Header("State (Read-only)")]
    [SerializeField] private int rageHits = 0;
    [SerializeField] private bool rageActive = false;

    public bool IsRageActive => rageActive;

    public void RegisterHit()
    {
        if (rageActive) return;
        rageHits++;
        if (rageHits >= hitsToActivate)
        {
            rageActive = true;
            Debug.Log("[Rage] RAGE AKTIF!");
        }
        UpdateUI();
    }

    public void ResetRage()
    {
        rageHits = 0;
        rageActive = false;
        Debug.Log("[Rage] Reset.");
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (rageText == null) return;
        if (rageActive)
            rageText.text = "RAGE!";
        else
            rageText.text = $"Rage: {rageHits}/{hitsToActivate}";
    }
}