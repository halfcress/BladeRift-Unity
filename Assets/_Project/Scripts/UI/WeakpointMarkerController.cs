using UnityEngine;
using UnityEngine.UI;

public class WeakpointMarkerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform markerRect;
    [SerializeField] private Transform worldTarget;

    [Header("Behavior")]
    [SerializeField] private bool hideWhenBehindCamera = true;
    [SerializeField] private Vector2 screenOffset = Vector2.zero;

    private RectTransform canvasRect;
    private Graphic markerGraphic;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (markerRect == null)
            markerRect = transform as RectTransform;

        if (canvas != null)
            canvasRect = canvas.transform as RectTransform;

        markerGraphic = GetComponent<Graphic>();
    }

    private void LateUpdate()
    {
        if (worldCamera == null || canvas == null || canvasRect == null || markerRect == null || worldTarget == null)
            return;

        Vector3 screenPoint = worldCamera.WorldToScreenPoint(worldTarget.position);
        bool isBehind = screenPoint.z <= 0f;

        if (hideWhenBehindCamera && isBehind)
        {
            SetMarkerVisible(false);
            return;
        }

        SetMarkerVisible(true);

        Vector2 screenPos = new Vector2(screenPoint.x, screenPoint.y) + screenOffset;
        Vector2 anchoredPos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldCamera,
            out anchoredPos))
        {
            markerRect.anchoredPosition = anchoredPos;
        }
    }

    private void SetMarkerVisible(bool visible)
    {
        if (markerGraphic != null)
            markerGraphic.enabled = visible;
    }
}
