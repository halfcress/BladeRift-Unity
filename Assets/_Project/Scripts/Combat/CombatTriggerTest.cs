using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SADECE TEST AMACLI. Nihai sistem değildir.
/// Play'e basınca 1 saniye bekler, sonra otomatik bir zincir başlatır.
/// Konsol loglarını izleyerek hit/miss test edebilirsin.
/// </summary>
public class CombatTriggerTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatDirector combatDirector;

    [Header("Test Chain")]
    [SerializeField] private List<WeakpointDirection> testChain = new List<WeakpointDirection>
    {
        WeakpointDirection.Right,
        WeakpointDirection.Up,
        WeakpointDirection.Left
    };

    [Header("Settings")]
    [Tooltip("Play'den kaç saniye sonra zincir baslasin.")]
    [SerializeField] private float delaySeconds = 1.5f;

    [Tooltip("Zincir bitince kaç saniye sonra yeniden baslasin. 0 = tekrar baslatma.")]
    [SerializeField] private float repeatAfterSeconds = 3f;

    private void Awake()
    {
        if (combatDirector == null)
            combatDirector = FindFirstObjectByType<CombatDirector>();
    }

    private void Start()
    {
        StartCoroutine(TriggerLoop());
    }

private IEnumerator TriggerLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(delaySeconds);

            Debug.Log("CombatTriggerTest: Zincir baslatiliyor...");
            combatDirector.StartCombatSequence(new List<WeakpointDirection>(testChain));

            // Zincir bitene kadar bekle (Done veya Idle olana kadar)
            yield return new WaitUntil(() =>
            {
                var seq = combatDirector.GetSequence();
                if (seq == null) return true;
                var phase = seq.CurrentPhase;
                return phase == WeakpointSequence.Phase.Done ||
                       phase == WeakpointSequence.Phase.Idle;
            });

            if (repeatAfterSeconds <= 0f)
                yield break;

            yield return new WaitForSecondsRealtime(repeatAfterSeconds);
        }
    }
}
