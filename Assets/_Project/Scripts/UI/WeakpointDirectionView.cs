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
        public RectTransform backplateRect;
        public RectTransform glowRect;
        public RectTransform coronaARect;
        public RectTransform coronaBRect;
        public RectTransform outerRingRect;
        public RectTransform innerRingRect;
        public RectTransform coreRect;
        public RectTransform shimmerRect;

        public Image backplateImage;
        public Image glowImage;
        public Image coronaAImage;
        public Image coronaBImage;
        public Image outerRingImage;
        public Image innerRingImage;
        public Image coreImage;
        public Image shimmerImage;

        public Vector3 backplateBaseScale = Vector3.one;
        public Vector3 glowBaseScale = Vector3.one;
        public Vector3 coronaABaseScale = Vector3.one;
        public Vector3 coronaBBaseScale = Vector3.one;
        public Vector3 outerBaseScale = Vector3.one;
        public Vector3 innerBaseScale = Vector3.one;
        public Vector3 coreBaseScale = Vector3.one;
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
            parts.backplateRect = root.Find("Backplate") as RectTransform;
            parts.glowRect = root.Find("Glow") as RectTransform;
            parts.coronaARect = FindOptionalRect(root, "CoronaA", "FlameA");
            parts.coronaBRect = FindOptionalRect(root, "CoronaB", "FlameB");
            parts.outerRingRect = root.Find("OuterRing") as RectTransform;
            parts.innerRingRect = root.Find("InnerRing") as RectTransform;
            parts.coreRect = root.Find("Core") as RectTransform;
            parts.shimmerRect = root.Find("Shimmer") as RectTransform;

            parts.backplateImage = GetImage(parts.backplateRect);
            parts.glowImage = GetImage(parts.glowRect);
            parts.coronaAImage = GetImage(parts.coronaARect);
            parts.coronaBImage = GetImage(parts.coronaBRect);
            parts.outerRingImage = GetImage(parts.outerRingRect);
            parts.innerRingImage = GetImage(parts.innerRingRect);
            parts.coreImage = GetImage(parts.coreRect);
            parts.shimmerImage = GetImage(parts.shimmerRect);

            parts.backplateBaseScale = GetBaseScale(parts.backplateRect);
            parts.glowBaseScale = GetBaseScale(parts.glowRect);
            parts.coronaABaseScale = GetBaseScale(parts.coronaARect);
            parts.coronaBBaseScale = GetBaseScale(parts.coronaBRect);
            parts.outerBaseScale = GetBaseScale(parts.outerRingRect);
            parts.innerBaseScale = GetBaseScale(parts.innerRingRect);
            parts.coreBaseScale = GetBaseScale(parts.coreRect);
            parts.shimmerBaseScale = GetBaseScale(parts.shimmerRect);

            markers.Add(parts);
        }
    }

    private static RectTransform FindOptionalRect(RectTransform root, string primary, string fallback)
    {
        RectTransform rect = root.Find(primary) as RectTransform;
        if (rect != null)
            return rect;
        return root.Find(fallback) as RectTransform;
    }

    private static Image GetImage(RectTransform rect)
    {
        return rect != null ? rect.GetComponent<Image>() : null;
    }

    private static Vector3 GetBaseScale(RectTransform rect)
    {
        return rect != null ? rect.localScale : Vector3.one;
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

        SetImage(parts.backplateImage, profile.backplateColor, profile.backplateAlpha);
        SetImage(parts.glowImage, profile.glowColor, profile.glowAlpha);
        SetImage(parts.coronaAImage, profile.coronaAColor, profile.coronaAAlpha);
        SetImage(parts.coronaBImage, profile.coronaBColor, profile.coronaBAlpha);
        SetImage(parts.outerRingImage, profile.ringColor, profile.outerRingAlpha);
        SetImage(parts.innerRingImage, profile.ringColor, profile.ringAlpha);
        SetImage(parts.coreImage, profile.baseColor, profile.baseAlpha);
        SetImage(parts.shimmerImage, profile.shimmerColor, profile.shimmerAlpha);

        if (parts.backplateRect != null) parts.backplateRect.localScale = parts.backplateBaseScale * (profile.backplateScale * profile.scale);
        if (parts.glowRect != null) parts.glowRect.localScale = parts.glowBaseScale * (profile.glowScale * profile.scale);
        if (parts.coronaARect != null) parts.coronaARect.localScale = parts.coronaABaseScale * (profile.coronaAScale * profile.scale);
        if (parts.coronaBRect != null) parts.coronaBRect.localScale = parts.coronaBBaseScale * (profile.coronaBScale * profile.scale);
        if (parts.outerRingRect != null) parts.outerRingRect.localScale = parts.outerBaseScale * profile.scale;
        if (parts.innerRingRect != null) parts.innerRingRect.localScale = parts.innerBaseScale * profile.scale;
        if (parts.coreRect != null) parts.coreRect.localScale = parts.coreBaseScale * profile.scale;
        if (parts.shimmerRect != null) parts.shimmerRect.localScale = parts.shimmerBaseScale * profile.scale;

        if (parts.coronaARect != null) parts.coronaARect.localRotation = Quaternion.identity;
        if (parts.coronaBRect != null) parts.coronaBRect.localRotation = Quaternion.identity;
        if (parts.outerRingRect != null) parts.outerRingRect.localRotation = Quaternion.identity;
        if (parts.innerRingRect != null) parts.innerRingRect.localRotation = Quaternion.identity;
        if (parts.shimmerRect != null) parts.shimmerRect.localRotation = Quaternion.identity;
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

            float coronaPulse = 1f;
            if (profile.coronaPulseAmplitude > 0f && profile.coronaPulseSpeed > 0f)
            {
                float t = (Mathf.Sin(time * profile.coronaPulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
                coronaPulse += Mathf.Lerp(-profile.coronaPulseAmplitude, profile.coronaPulseAmplitude, t);
            }

            float flickerA = 1f;
            float flickerB = 1f;
            if (profile.coronaFlickerStrength > 0f && profile.coronaFlickerSpeed > 0f)
            {
                flickerA = 1f - profile.coronaFlickerStrength + profile.coronaFlickerStrength * (0.5f + 0.5f * Mathf.Sin(time * profile.coronaFlickerSpeed * 6.2831855f));
                flickerB = 1f - profile.coronaFlickerStrength + profile.coronaFlickerStrength * (0.5f + 0.5f * Mathf.Sin((time * profile.coronaFlickerSpeed * 1.173f + 0.37f) * 6.2831855f));
            }

            if (parts.backplateRect != null) parts.backplateRect.localScale = parts.backplateBaseScale * (profile.backplateScale * profile.scale * pulse);
            if (parts.glowRect != null) parts.glowRect.localScale = parts.glowBaseScale * (profile.glowScale * profile.scale * pulse);

            if (parts.coronaARect != null)
                parts.coronaARect.localScale = parts.coronaABaseScale * (profile.coronaAScale * profile.scale * pulse * coronaPulse);
            if (parts.coronaBRect != null)
                parts.coronaBRect.localScale = parts.coronaBBaseScale * (profile.coronaBScale * profile.scale * pulse * coronaPulse);

            if (parts.outerRingRect != null) parts.outerRingRect.localScale = parts.outerBaseScale * (profile.scale * pulse);
            if (parts.innerRingRect != null) parts.innerRingRect.localScale = parts.innerBaseScale * (profile.scale * pulse);
            if (parts.coreRect != null) parts.coreRect.localScale = parts.coreBaseScale * (profile.scale * pulse);
            if (parts.shimmerRect != null) parts.shimmerRect.localScale = parts.shimmerBaseScale * (profile.scale * pulse);

            if (parts.coronaAImage != null)
                SetImage(parts.coronaAImage, profile.coronaAColor, profile.coronaAAlpha * flickerA);
            if (parts.coronaBImage != null)
                SetImage(parts.coronaBImage, profile.coronaBColor, profile.coronaBAlpha * flickerB);

            if (parts.coronaARect != null && Mathf.Abs(profile.coronaARotationSpeed) > 0.01f)
                parts.coronaARect.localRotation = Quaternion.Euler(0f, 0f, time * profile.coronaARotationSpeed);
            if (parts.coronaBRect != null && Mathf.Abs(profile.coronaBRotationSpeed) > 0.01f)
                parts.coronaBRect.localRotation = Quaternion.Euler(0f, 0f, time * profile.coronaBRotationSpeed);

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
        c.a = Mathf.Clamp01(alpha);
        img.color = c;
    }
}
