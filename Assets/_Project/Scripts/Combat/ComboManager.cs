using UnityEngine;
using TMPro;

/// <summary>
/// Fruit Ninja tarzi combo sistemi.
/// - Basarili her hit combo sayacini arttirir
/// - Sadece timeout combo'yu sifirlar
/// - Finger lift combo'yu bozmaz
/// </summary>
public class ComboManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI hitCountText;

    [Header("State (Read-only)")]
    [SerializeField] private int comboCount = 0;
    [SerializeField] private int totalHits = 0;

    // Events
    public event System.Action<int> OnComboChanged;

    public int ComboCount => comboCount;

    private void Awake()
    {
        UpdateUI();
    }

    /// <summary>Basarili hit - combo artar</summary>
    public void RegisterHit()
    {
        comboCount++;
        totalHits++;
        UpdateUI();
        OnComboChanged?.Invoke(comboCount);
        Debug.Log($"[Combo] Hit! Combo={comboCount} TotalHits={totalHits}");
    }

    /// <summary>Sadece timeout'ta cagrilir - combo sifirlanir</summary>
    public void RegisterTimeout()
    {
        if (comboCount > 0)
            Debug.Log($"[Combo] Timeout! Combo {comboCount} -> 0");
        comboCount = 0;
        UpdateUI();
        OnComboChanged?.Invoke(comboCount);
    }

    /// <summary>Zincir basarili tamamlandi - combo devam eder (sifirlanmaz)</summary>
    public void RegisterChainSuccess()
    {
        // Fruit Ninja gibi: basari combo'yu bozmaz, devam eder
        Debug.Log($"[Combo] Chain success! Combo={comboCount} devam ediyor.");
    }

    private void UpdateUI()
    {
        if (comboText != null)
        {
            if (comboCount <= 0)
                comboText.text = "0";
            else if (comboCount == 1)
                comboText.text = "HIT!";
            else
                comboText.text = $"x{comboCount} COMBO!";
        }

        if (hitCountText != null)
            hitCountText.text = $"Hits: {totalHits}";
    }
}
