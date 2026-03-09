using UnityEngine;

/// <summary>
/// WeakpointSequence event'lerini dinler ve WeakpointDirectionView'ı günceller.
/// Telegraph ve ExecutionWindow phase'lerinde ekranda sıradaki yönü gösterir.
/// </summary>
public class WeakpointUIBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeakpointSequence sequence;
    [SerializeField] private WeakpointDirectionView directionView;

private void Awake() { if (sequence == null) sequence = FindFirstObjectByType<WeakpointSequence>(); if (directionView == null) directionView = FindFirstObjectByType<WeakpointDirectionView>(FindObjectsInactive.Include); if (directionView != null) { directionView.gameObject.SetActive(false); directionView.transform.parent.gameObject.SetActive(true); } }

    private void OnEnable()
    {
        if (sequence == null) return;

        sequence.OnTelegraphStep       += HandleTelegraphStep;
        sequence.OnExecutionWindowStart += HandleExecutionWindowStart;
        sequence.OnExecutionWindowEnd  += HandleExecutionWindowEnd;
        sequence.OnChainAdvance        += HandleChainAdvance;
        sequence.OnChainSuccess        += HandleChainEnd;
        sequence.OnChainFail           += HandleChainFail;
    }

    private void OnDisable()
    {
        if (sequence == null) return;

        sequence.OnTelegraphStep        -= HandleTelegraphStep;
        sequence.OnExecutionWindowStart -= HandleExecutionWindowStart;
        sequence.OnExecutionWindowEnd   -= HandleExecutionWindowEnd;
        sequence.OnChainAdvance         -= HandleChainAdvance;
        sequence.OnChainSuccess         -= HandleChainEnd;
        sequence.OnChainFail            -= HandleChainFail;
    }

    // Telegraph: sıradaki yönü göster (soluk)
    private void HandleTelegraphStep(int index)
    {
        if (directionView == null) return;

        directionView.gameObject.SetActive(true);
        directionView.SetDirection(ConvertDirection(sequence.CurrentTarget));

        // Telegraph sırasında marker'ı yarı saydam yap
        var group = directionView.GetComponentInChildren<UnityEngine.UI.Image>();
        if (group != null)
        {
            var c = group.color;
            c.a = 0.4f;
            group.color = c;
        }
    }

    // Execution window: tam görünür, parlak
    private void HandleExecutionWindowStart(float duration)
    {
        if (directionView == null) return;

        directionView.gameObject.SetActive(true);
        directionView.SetDirection(ConvertDirection(sequence.CurrentTarget));

        var img = directionView.GetComponentInChildren<UnityEngine.UI.Image>();
        if (img != null)
        {
            var c = img.color;
            c.a = 1f;
            img.color = c;
        }
    }

    // Zincir ilerleyince sıradaki yönü göster
    private void HandleChainAdvance(int newIndex)
    {
        if (directionView == null) return;

        directionView.SetDirection(ConvertDirection(sequence.CurrentTarget));
    }

    // Bitti: marker'ı gizle
    private void HandleExecutionWindowEnd()
    {
        if (directionView != null)
            directionView.gameObject.SetActive(false);
    }

    private void HandleChainEnd()
    {
        if (directionView != null)
            directionView.gameObject.SetActive(false);
    }

    private void HandleChainFail(string reason)
    {
        if (directionView != null)
            directionView.gameObject.SetActive(false);
    }

    private WeakpointDirection ConvertDirection(WeakpointDirection dir) => dir;
}
