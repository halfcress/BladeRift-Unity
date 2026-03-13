using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Editor / scene icinde combat zincirini manuel tetiklemek icin basit test scripti.
/// Direction yerine Zone domain'i ile calisir.
/// </summary>
public class CombatTriggerTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatDirector combatDirector;

    [Header("Test Pattern")]
    [SerializeField] private WeakpointZone[] testPattern =
    {
        WeakpointZone.Chest,
        WeakpointZone.LeftShoulder,
        WeakpointZone.RightShoulder
    };

    [Header("Input")]
    [SerializeField] private KeyCode triggerKey = KeyCode.T;
    [SerializeField] private bool triggerOnStart = false;

    private void Awake()
    {
        if (combatDirector == null)
            combatDirector = FindFirstObjectByType<CombatDirector>();
    }

    private void Start()
    {
        if (triggerOnStart)
            Trigger();
    }

    private void Update()
    {
        if (Input.GetKeyDown(triggerKey))
            Trigger();
    }

    [ContextMenu("Trigger Combat Test")]
    public void Trigger()
    {
        if (combatDirector == null)
        {
            Debug.LogError("[CombatTriggerTest] CombatDirector yok!");
            return;
        }

        if (testPattern == null || testPattern.Length == 0)
        {
            Debug.LogError("[CombatTriggerTest] testPattern bos!");
            return;
        }

        combatDirector.StartCombatSequence(new List<WeakpointZone>(testPattern));
        Debug.Log($"[CombatTriggerTest] Test chain tetiklendi. Count={testPattern.Length}");
    }
}
