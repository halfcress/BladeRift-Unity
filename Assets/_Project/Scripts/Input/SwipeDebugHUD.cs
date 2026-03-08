using TMPro;
using UnityEngine;

public class SwipeDebugHUD : MonoBehaviour
{
    [SerializeField] private SwipeInput swipe;
    [SerializeField] private TMP_Text text;

    private void Reset()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (swipe == null || text == null) return;

        Vector2 d = swipe.DeltaPx;
        Vector2 n = swipe.DeltaNormalized;

        text.text =
            $"IsDown: {swipe.IsDown}\n" +
            $"DeltaPx: ({d.x:0.0}, {d.y:0.0})\n" +
            $"DeltaNorm: ({n.x:0.00}, {n.y:0.00})";
    }
}