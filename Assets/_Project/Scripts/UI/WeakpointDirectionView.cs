using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enemy prefab üzerindeki gerçek weakpoint anchor noktalarına göre katmanlı marker çizer.
/// Marker rolleri:
/// - marker 0 = current
/// - marker 1 = next
/// - marker 2 = next+1
/// </summary>
public class WeakpointDirectionView : MonoBehaviour
{
    private enum MarkerRole
    {
        Preview,
        Current,
        Next,
        NextPlusOne
    }

    private enum MarkerMode
    {
        Hidden,
        Preview,
        Execution
    }

    [System.Serializable]
    private class MarkerParts
    {
        public RectTransform root;
        public RectTransform coreRect;
        public RectTransform innerRingRect;
        public RectTransform outerRingRect;
        public RectTransform glowRect;
        public RectTransform shimmerRect;

        public Image coreImage;
        public Image innerRingImage;
        public Image outerRingImage;
        public Image glowImage;
        public Image shimmerImage;

        public Vector3 coreBaseScale = Vector3.one;
        public Vector3 innerBaseScale = Vector3.one;
        public Vector3 outerBaseScale = Vector3.one;
        public Vector3 glowBaseScale = Vector3.one;
        public Vector3 shimmerBaseScale = Vector3.one;

        public WeakpointZone zone = WeakpointZone.None;
        public MarkerRole role;
        public bool visible;
    }

    [Header("Marker Root Listesi (WeakpointMarker_1/2/3)")]
    [SerializeField] private List<RectTransform> markerRects = new();

    [Header("Referanslar")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private RageManager rageManager;

    [Header("Debug (read-only)")]
    [SerializeField] private WeakpointZone currentZone = WeakpointZone.None;
    [SerializeField] private EnemyWeakpointAnchors boundAnchors;

    private readonly List<MarkerParts> markers = new();
    private Canvas cachedCanvas;
    private Camera cachedCamera;
    private MarkerMode currentMode = MarkerMode.Hidden;

    private void Awake()
    {
        if (rageManager == null)
            rageManager = FindFirstObjectByType<RageManager>();

        if (markerRects.Count == 0)
            AutoFindMarkers();

        cachedCanvas = GetComponentInParent<Canvas>();
        cachedCamera = Camera.main;

        ApplyMarkerAnchors();
        CacheMarkerParts();
        HideAll();
    }

    private void OnValidate()
    {
        ApplyMarkerAnchors();
    }

    private void Update()
    {
        UpdateMarkerAnimations(Time.unscaledTime);
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

    private void CacheMarkerParts()
    {
        markers.Clear();

        for (int i = 0; i < markerRects.Count; i++)
        {
            RectTransform root = markerRects[i];
            if (root == null) continue;

            MarkerParts parts = new MarkerParts();
            parts.root = root;
            parts.coreRect = root.Find("Core") as RectTransform;
            parts.innerRingRect = root.Find("InnerRing") as RectTransform;
            parts.outerRingRect = root.Find("OuterRing") as RectTransform;
            parts.glowRect = root.Find("Glow") as RectTransform;
            parts.shimmerRect = root.Find("Shimmer") as RectTransform;

            parts.coreImage = parts.coreRect != null ? parts.coreRect.GetComponent<Image>() : null;
            parts.innerRingImage = parts.innerRingRect != null ? parts.innerRingRect.GetComponent<Image>() : null;
            parts.outerRingImage = parts.outerRingRect != null ? parts.outerRingRect.GetComponent<Image>() : null;
            parts.glowImage = parts.glowRect != null ? parts.glowRect.GetComponent<Image>() : null;
            parts.shimmerImage = parts.shimmerRect != null ? parts.shimmerRect.GetComponent<Image>() : null;

            parts.coreBaseScale = parts.coreRect != null ? parts.coreRect.localScale : Vector3.one;
            parts.innerBaseScale = parts.innerRingRect != null ? parts.innerRingRect.localScale : Vector3.one;
            parts.outerBaseScale = parts.outerRingRect != null ? parts.outerRingRect.localScale : Vector3.one;
            parts.glowBaseScale = parts.glowRect != null ? parts.glowRect.localScale : Vector3.one;
            parts.shimmerBaseScale = parts.shimmerRect != null ? parts.shimmerRect.localScale : Vector3.one;

            markers.Add(parts);
        }
    }

    public void BindEnemyAnchors(EnemyWeakpointAnchors anchors)
    {
        boundAnchors = anchors;
    }

    public void ShowPreviewZone(WeakpointZone zone)
    {
        HideAll();
        currentMode = MarkerMode.Preview;
        ShowMarker(0, zone, MarkerRole.Preview);
    }

    public void ShowExecutionNavigation(IReadOnlyList<WeakpointZone> chain, int currentIndex)
    {
        HideAll();
        currentMode = MarkerMode.Execution;

        if (chain == null || chain.Count == 0) return;
        if (currentIndex < 0 || currentIndex >= chain.Count) return;

        ShowMarker(0, chain[currentIndex], MarkerRole.Current);

        if (currentIndex + 1 < chain.Count)
            ShowMarker(1, chain[currentIndex + 1], MarkerRole.Next);

        if (currentIndex + 2 < chain.Count)
            ShowMarker(2, chain[currentIndex + 2], MarkerRole.NextPlusOne);
    }

    public bool TryGetActiveMarkerScreenPos(out Vector2 screenPos)
    {
        screenPos = Vector2.zero;
        if (markers.Count == 0) return false;
        if (markers[0].root == null || !markers[0].root.gameObject.activeInHierarchy) return false;

        screenPos = markers[0].root.position;
        return true;
    }

    public void HideAll()
    {
        for (int i = 0; i < markers.Count; i++)
        {
            MarkerParts parts = markers[i];
            if (parts.root != null)
                parts.root.gameObject.SetActive(false);

            parts.visible = false;
            parts.zone = WeakpointZone.None;
        }

        currentMode = MarkerMode.Hidden;
        currentZone = WeakpointZone.None;
    }

    public void Hide()
    {
        HideAll();
    }

    private void ShowMarker(int markerIndex, WeakpointZone zone, MarkerRole role)
    {
        if (markerIndex < 0 || markerIndex >= markers.Count) return;
        if (zone == WeakpointZone.None) return;
        if (boundAnchors == null)
        {
            Debug.LogError("[WeakpointDirectionView] EnemyWeakpointAnchors bağlanmamış. Marker gösterilemiyor.", this);
            Debug.Break();
            return;
        }

        if (!boundAnchors.TryGetAnchor(zone, out Transform anchor) || anchor == null)
        {
            Debug.LogError($"[WeakpointDirectionView] Missing anchor for zone {zone} on {boundAnchors.name}", boundAnchors);
            Debug.Break();
            return;
        }

        MarkerParts parts = markers[markerIndex];
        if (parts.root == null) return;

        if (!TryGetCanvasAnchoredPosition(anchor.position, parts.root, out Vector2 anchoredPosition))
        {
            Debug.LogError($"[WeakpointDirectionView] Anchor ekran pozisyonuna çevrilemedi. zone={zone}", anchor);
            Debug.Break();
            return;
        }

        parts.root.anchoredPosition = anchoredPosition;
        parts.root.gameObject.SetActive(true);
        parts.visible = true;
        parts.zone = zone;
        parts.role = role;

        ApplyStaticVisual(parts, GetProfile(role));

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

    private void ApplyStaticVisual(MarkerParts parts, GameConfig.WeakpointVisualProfile profile)
    {
        if (profile == null) return;

        SetImage(parts.coreImage, profile.baseColor, profile.baseAlpha);
        SetImage(parts.innerRingImage, profile.ringColor, profile.ringAlpha);
        SetImage(parts.outerRingImage, profile.ringColor, profile.outerRingAlpha);
        SetImage(parts.glowImage, profile.glowColor, profile.glowAlpha);
        SetImage(parts.shimmerImage, profile.shimmerColor, profile.shimmerAlpha);

        if (parts.coreRect != null) parts.coreRect.localScale = parts.coreBaseScale * profile.scale;
        if (parts.innerRingRect != null) parts.innerRingRect.localScale = parts.innerBaseScale * profile.scale;
        if (parts.outerRingRect != null) parts.outerRingRect.localScale = parts.outerBaseScale * profile.scale;
        if (parts.glowRect != null) parts.glowRect.localScale = parts.glowBaseScale * profile.glowScale * profile.scale;
        if (parts.shimmerRect != null) parts.shimmerRect.localScale = parts.shimmerBaseScale * profile.scale;

        if (parts.innerRingRect != null)
            parts.innerRingRect.localRotation = Quaternion.identity;
        if (parts.outerRingRect != null)
            parts.outerRingRect.localRotation = Quaternion.identity;
        if (parts.shimmerRect != null)
            parts.shimmerRect.localRotation = Quaternion.identity;
    }

    private void UpdateMarkerAnimations(float time)
    {
        for (int i = 0; i < markers.Count; i++)
        {
            MarkerParts parts = markers[i];
            if (!parts.visible || parts.root == null || !parts.root.gameObject.activeInHierarchy)
                continue;

            GameConfig.WeakpointVisualProfile profile = GetProfile(parts.role);
            if (profile == null)
                continue;

            float pulse = 1f;
            if (profile.pulseAmplitude > 0f && profile.pulseSpeed > 0f)
            {
                float t = (Mathf.Sin(time * profile.pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
                pulse += Mathf.Lerp(-profile.pulseAmplitude, profile.pulseAmplitude, t);
            }

            if (parts.coreRect != null) parts.coreRect.localScale = parts.coreBaseScale * (profile.scale * pulse);
            if (parts.innerRingRect != null) parts.innerRingRect.localScale = parts.innerBaseScale * (profile.scale * pulse);
            if (parts.outerRingRect != null) parts.outerRingRect.localScale = parts.outerBaseScale * (profile.scale * pulse);
            if (parts.glowRect != null) parts.glowRect.localScale = parts.glowBaseScale * (profile.glowScale * profile.scale * pulse);
            if (parts.shimmerRect != null) parts.shimmerRect.localScale = parts.shimmerBaseScale * (profile.scale * pulse);

            if (parts.innerRingRect != null && Mathf.Abs(profile.innerRotationSpeed) > 0.01f)
                parts.innerRingRect.localRotation = Quaternion.Euler(0f, 0f, time * profile.innerRotationSpeed);

            if (parts.outerRingRect != null && Mathf.Abs(profile.outerRotationSpeed) > 0.01f)
                parts.outerRingRect.localRotation = Quaternion.Euler(0f, 0f, time * profile.outerRotationSpeed);

            if (parts.shimmerRect != null && profile.shimmerSweepSpeed > 0f)
                parts.shimmerRect.localRotation = Quaternion.Euler(0f, 0f, time * profile.shimmerSweepSpeed);
        }
    }

    private GameConfig.WeakpointVisualProfile GetProfile(MarkerRole role)
    {
        bool rage = rageManager != null && rageManager.IsRageActive;

        if (gameConfig == null)
        {
            Debug.LogWarning("[WeakpointDirectionView] GameConfig atanmadı. Varsayılan görünüm kullanılıyor.", this);
            return GameConfig.WeakpointVisualProfile.CreateFallback(role, rage);
        }

        if (rage)
        {
            return role switch
            {
                MarkerRole.Preview => gameConfig.ragePreviewVisual,
                MarkerRole.Current => gameConfig.rageCurrentVisual,
                MarkerRole.Next => gameConfig.rageNextVisual,
                MarkerRole.NextPlusOne => gameConfig.rageNextPlusOneVisual,
                _ => gameConfig.rageCurrentVisual
            };
        }

        return role switch
        {
            MarkerRole.Preview => gameConfig.normalPreviewVisual,
            MarkerRole.Current => gameConfig.normalCurrentVisual,
            MarkerRole.Next => gameConfig.normalNextVisual,
            MarkerRole.NextPlusOne => gameConfig.normalNextPlusOneVisual,
            _ => gameConfig.normalCurrentVisual
        };
    }

    private static void SetImage(Image img, Color color, float alpha)
    {
        if (img == null) return;
        Color c = color;
        c.a = alpha;
        img.color = c;
    }
}
