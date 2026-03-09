using TMPro;
using UnityEngine;

public class SwipeDebugHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwipeInput swipe;
    [SerializeField] private SwipeInterpreter interpreter;
    [SerializeField] private TMP_Text text;

    private void Reset()
    {
        if (swipe == null)
            swipe = GetComponent<SwipeInput>();

        if (interpreter == null)
            interpreter = GetComponent<SwipeInterpreter>();
    }

    private void Awake()
    {
        if (swipe == null)
            swipe = GetComponent<SwipeInput>();

        if (interpreter == null)
            interpreter = GetComponent<SwipeInterpreter>();
    }

    private void Update()
    {
        if (text == null)
            return;

        string swipePart = "Swipe: missing";
        if (swipe != null)
        {
            swipePart =
                $"IsDown: {swipe.IsDown}\n" +
                $"DeltaPx: {swipe.DeltaPx}\n" +
                $"DeltaNorm: {swipe.DeltaNormalized}";
        }

        string interpreterPart = "Interpreter: missing";
        if (interpreter != null)
        {
            interpreterPart =
                $"CurrentDir: {interpreter.CurrentDirection}\n" +
                $"LastCommitted: {interpreter.LastCommittedDirection}\n" +
                $"AccumPx: {interpreter.AccumulatedDeltaPx}\n" +
                $"CommittedThisPress: {interpreter.HasCommittedThisPress}";
        }

        text.text = swipePart + "\n\n" + interpreterPart;
    }
}
