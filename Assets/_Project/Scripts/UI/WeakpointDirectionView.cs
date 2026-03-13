using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enemy prefab üzerindeki gerçek weakpoint anchor noktalarına göre marker çizer.
/// Marker rolleri:
/// - marker 0 = current (çok net)
/// - marker 1 = next (sönük)
/// - marker 2 = next+1 (ultra sönük)
/// </summary>
public class WeakpointDirectionView : MonoBehaviour
{
    [Header("Marker Listesi (WeakpointMarker_1/2/3)")]
    [SerializeField] private List<RectTransform> markerRects = new();

    [Header("Gorunum")]
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color rageColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private Color nextColor = new Color(1f, 0.35f, 0.35f, 0.55f);
    [SerializeField] private Color ultraDimColor = new Color(1f, 0.35f, 0.35f, 0.18f);
    [SerializeField] private RageManager rageManager;

    [Header("Debug (read-only)")]
    [SerializeField] private WeakpointZone currentZone = WeakpointZone.None;
    [SerializeField] private EnemyWeakpointAnchors boundAnchors;

    private Canvas cachedCanvas;
    private Camera cachedCamera;

    private void Awake()
    {
        if (rageManager == null)
            rageManager = FindFirstObjectByType<RageManager>();

        if (markerRects.Count == 0)
            AutoFindMarkers();

        cachedCanvas = GetComponentInParent<Canvas>();
        cachedCamera = Camera.main;

        ApplyMarkerAnchors();
        HideAll();
        Debug.Log($"[WeakpointDirectionView] Awake OK. markers={markerRects.Count}");
    }

    private void OnValidate()
    {
        ApplyMarkerAnchors();
    }

    private void AutoFindMarkers()
    {
        markerRects.Clear();
        for (int i = 1; i <= 3; i++)
        {
            Transform t = transform.Find($"WeakpointMarker_{i}");
            if (t != null)
                markerRects.Add(t.GetComponent<RectTransform>());
        }
    }

    private void ApplyMarkerAnchors()
    {
        for (int i = 0; i < markerRects.Count; i++)
        {
            if (markerRects[i] == null) continue;

            markerRects[i].anchorMin = new Vector2(0.5f, 0.5f);
            markerRects[i].anchorMax = new Vector2(0.5f, 0.5f);
            markerRects[i].pivot = new Vector2(0.5f, 0.5f);
        }
    }

    public void BindEnemyAnchors(EnemyWeakpointAnchors anchors)
    {
        boundAnchors = anchors;
    }

    public void ShowPreviewZone(WeakpointZone zone)
    {
        HideAll();

        bool rage = rageManager != null && rageManager.IsRageActive;
        ShowMarker(0, zone, rage ? rageColor : activeColor);
    }

    public void ShowExecutionNavigation(IReadOnlyList<WeakpointZone> chain, int currentIndex)
    {
        HideAll();

        if (chain == null || chain.Count == 0) return;
        if (currentIndex < 0 || currentIndex >= chain.Count) return;

        bool rage = rageManager != null && rageManager.IsRageActive;

        ShowMarker(0, chain[currentIndex], rage ? rageColor : activeColor);

        if (currentIndex + 1 < chain.Count)
            ShowMarker(1, chain[currentIndex + 1], nextColor);

        if (currentIndex + 2 < chain.Count)
            ShowMarker(2, chain[currentIndex + 2], ultraDimColor);
    }

    public bool TryGetActiveMarkerScreenPos(out Vector2 screenPos)
    {
        screenPos = Vector2.zero;
        if (markerRects.Count == 0) return false;
        if (markerRects[0] == null || !markerRects[0].gameObject.activeInHierarchy) return false;

        screenPos = markerRects[0].position;
        return true;
    }

    public void HideAll()
    {
        for (int i = 0; i < markerRects.Count; i++)
        {
            if (markerRects[i] != null)
                markerRects[i].gameObject.SetActive(false);
        }

        currentZone = WeakpointZone.None;
    }

    public void Hide()
    {
        HideAll();
    }

    private void ShowMarker(int markerIndex, WeakpointZone zone, Color color)
    {
        if (markerIndex < 0 || markerIndex >= markerRects.Count) return;
        if (zone == WeakpointZone.None) return;
        if (boundAnchors == null)
        {
            Debug.LogError("[WeakpointDirectionView] EnemyWeakpointAnchors baglanmamis. Marker gosterilemiyor.", this);
            Debug.Break();
            return;
        }

        if (!boundAnchors.TryGetAnchor(zone, out Transform anchor) || anchor == null)
        {
            Debug.LogError($"[WeakpointDirectionView] Missing anchor for zone {zone} on {boundAnchors.name}", boundAnchors);
            Debug.Break();
            return;
        }

        RectTransform rect = markerRects[markerIndex];
        if (rect == null) return;

        if (!TryGetCanvasAnchoredPosition(anchor.position, rect, out Vector2 anchoredPosition))
        {
            Debug.LogError($"[WeakpointDirectionView] Anchor ekran pozisyonuna çevrilemedi. zone={zone}", anchor);
            Debug.Break();
            return;
        }

        rect.anchoredPosition = anchoredPosition;
        rect.gameObject.SetActive(true);
        SetColor(rect, color);

        if (markerIndex == 0)
            currentZone = zone;
    }

    private bool TryGetCanvasAnchoredPosition(Vector3 worldPos, RectTransform markerRect, out Vector2 anchoredPosition)
    {
        anchoredPosition = Vector2.zero;
        if (markerRect == null) return false;

        if (cachedCanvas == null)
            cachedCanvas = GetComponentInParent<Canvas>();

        if (cachedCamera == null)
            cachedCamera = Camera.main;

        RectTransform parentRect = markerRect.parent as RectTransform;
        if (parentRect == null) return false;

        Camera eventCamera = null;
        if (cachedCanvas != null && cachedCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            eventCamera = cachedCanvas.worldCamera != null ? cachedCanvas.worldCamera : cachedCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cachedCamera, worldPos);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, eventCamera, out anchoredPosition);
    }

    private void SetColor(RectTransform rect, Color color)
    {
        Image img = rect.GetComponent<Image>();
        if (img != null)
            img.color = color;
    }
}
