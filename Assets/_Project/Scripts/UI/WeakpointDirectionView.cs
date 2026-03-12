using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Weakpoint marker UI sistemi.
/// - Marker'lar sabit ekran pozisyonlarinda durur (world-space takip YOK)
/// - Telegraph: marker'lar sirayla belirir
/// - Execution: sadece siraradaki marker aktif, diğerleri soluk
/// - Hit-test: marker'in RectTransform.position'i kullanilir (guvenilir)
/// </summary>
public class WeakpointDirectionView : MonoBehaviour
{
    [Header("Marker Listesi (WeakpointMarker_1/2/3)")]
    [SerializeField] private List<RectTransform> markerRects = new();

    [Header("Sabit Ekran Pozisyonlari (Ekran merkezinden offset, px)")]
    [SerializeField] private List<Vector2> markerPositions = new()
    {
        new Vector2(-150f, -100f),
        new Vector2(0f,     80f),
        new Vector2(150f,  -100f),
    };

    [Header("Gorunum")]
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color rageColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private Color dimColor    = new Color(1f, 0.3f, 0.3f, 0.3f);
    [SerializeField] private RageManager rageManager;

    [Header("Debug (read-only)")]
    [SerializeField] private int currentTargetIndex = 0;

    private void Awake()
    {
        if (rageManager == null)
            rageManager = FindFirstObjectByType<RageManager>();

        if (markerRects.Count == 0) AutoFindMarkers();
        ApplyPositions();
        HideAll();
        Debug.Log($"[WeakpointDirectionView] Awake OK. markers={markerRects.Count}");
    }

    private void AutoFindMarkers()
    {
        markerRects.Clear();
        for (int i = 1; i <= 3; i++)
        {
            var t = transform.Find($"WeakpointMarker_{i}");
            if (t != null) markerRects.Add(t.GetComponent<RectTransform>());
        }
    }

    private void ApplyPositions()
    {
        for (int i = 0; i < markerRects.Count; i++)
        {
            if (markerRects[i] == null) continue;
            markerRects[i].anchorMin        = new Vector2(0.5f, 0.5f);
            markerRects[i].anchorMax        = new Vector2(0.5f, 0.5f);
            markerRects[i].pivot            = new Vector2(0.5f, 0.5f);
            if (i < markerPositions.Count)
                markerRects[i].anchoredPosition = markerPositions[i];
        }
    }

    // ── Public API ───────────────────────────────────────────────────────

public void ShowTelegraphStep(int index, WeakpointDirection direction)
    {
        if (index < 0 || index >= markerRects.Count) return;
        currentTargetIndex = index;
        markerRects[index].gameObject.SetActive(true);
        bool rage = rageManager != null && rageManager.IsRageActive;
        SetColor(index, rage ? rageColor : activeColor);
    }

    public void ShowExecutionTarget(int index, WeakpointDirection direction)
    {
        currentTargetIndex = index;

        bool rage = rageManager != null && rageManager.IsRageActive;

        for (int i = 0; i < markerRects.Count; i++)
        {
            if (markerRects[i] == null) continue;

            markerRects[i].gameObject.SetActive(true);

            if (i == index)
            {
                SetColor(i, rage ? rageColor : activeColor);
            }
            else
            {
                SetColor(i, dimColor);
            }
        }
    }

    public bool TryGetActiveMarkerScreenPos(out Vector2 screenPos)
    {
        screenPos = Vector2.zero;
        if (currentTargetIndex < 0 || currentTargetIndex >= markerRects.Count) return false;
        var rect = markerRects[currentTargetIndex];
        if (rect == null || !rect.gameObject.activeInHierarchy) return false;
        screenPos = rect.position;
        return true;
    }

    public void HideAll()
    {
        foreach (var m in markerRects)
            if (m != null) m.gameObject.SetActive(false);
        currentTargetIndex = 0;
    }

    public void SetDirection(WeakpointDirection direction) => ShowExecutionTarget(0, direction);
    public void Hide() => HideAll();

    private void SetColor(int index, Color color)
    {
        if (index < 0 || index >= markerRects.Count) return;
        var img = markerRects[index].GetComponent<Image>();
        if (img != null) img.color = color;
    }
}
