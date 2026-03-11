using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonLabelPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform labelRect;
    [SerializeField] private TMP_Text labelText;

    [Header("Pressed")]
    [SerializeField] private float pressedYOffset = -1f;
    [SerializeField] private float pressedScaleMultiplier = 0.9f;
    [SerializeField] private Color pressedColor = new Color32(220, 190, 190, 255);

    private Vector2 normalPos;
    private Vector3 normalScale;
    private Color normalColor;
    private bool isCached;

    private void Awake()
    {
        CacheNormalState();
        ResetToNormal();
    }

    private void OnValidate()
    {
        if (labelRect == null)
            labelRect = GetComponentInChildren<TMP_Text>()?.rectTransform;

        if (labelText == null)
            labelText = GetComponentInChildren<TMP_Text>();
    }

    private void CacheNormalState()
    {
        if (labelRect == null)
            labelRect = GetComponentInChildren<TMP_Text>()?.rectTransform;

        if (labelText == null)
            labelText = GetComponentInChildren<TMP_Text>();

        if (labelRect == null || labelText == null)
        {
            isCached = false;
            return;
        }

        normalPos = labelRect.anchoredPosition;
        normalScale = labelRect.localScale;
        normalColor = labelText.color;
        isCached = true;
    }

    private void ResetToNormal()
    {
        if (!isCached)
            CacheNormalState();

        if (!isCached)
            return;

        labelRect.anchoredPosition = normalPos;
        labelRect.localScale = normalScale;
        labelText.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isCached)
            CacheNormalState();

        if (!isCached)
            return;

        labelRect.anchoredPosition = normalPos + new Vector2(0f, pressedYOffset);
        labelRect.localScale = normalScale * pressedScaleMultiplier;
        labelText.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetToNormal();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetToNormal();
    }
}