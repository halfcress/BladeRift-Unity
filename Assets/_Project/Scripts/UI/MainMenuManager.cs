using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Ayarlar")]
    [SerializeField] private Button musicToggleButton;
    [SerializeField] private TextMeshProUGUI musicToggleText;
    [SerializeField] private Button languageToggleButton;
    [SerializeField] private TextMeshProUGUI languageToggleText;

    [Header("Kredi")]
    [SerializeField] private Image logoImage;
    [SerializeField] private Sprite logoSprite;

    [Header("Sahne")]
    [SerializeField] private string gameSceneName = "Prototype_CombatCore";

    private bool musicOn = true;
    private bool isTurkish = true;

    private void Start()
    {
        ShowMain();
        if (logoImage != null && logoSprite != null)
            logoImage.sprite = logoSprite;
        UpdateMusicText();
        UpdateLanguageText();
    }

    // --- Panel Geçiþleri ---

    public void ShowMain()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    // --- Buton Aksiyonlarý ---

    public void OnPlayPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnMusicToggle()
    {
        musicOn = !musicOn;
        UpdateMusicText();
        AudioListener.volume = musicOn ? 1f : 0f;
    }

    public void OnLanguageToggle()
    {
        isTurkish = !isTurkish;
        UpdateLanguageText();
        PlayerPrefs.SetString("Language", isTurkish ? "TR" : "EN");
        PlayerPrefs.Save();
    }

    // --- UI Güncelleme ---

    private void UpdateMusicText()
    {
        if (musicToggleText != null)
            musicToggleText.text = musicOn ? "Müzik: Açýk" : "Music: Off";
    }

    private void UpdateLanguageText()
    {
        if (languageToggleText != null)
            languageToggleText.text = isTurkish ? "Dil: TR" : "Language: EN";
    }
}