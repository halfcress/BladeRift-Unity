using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    [Header("Progression")]
    [SerializeField] private int killCount = 0;

    [Header("Weights")]
    [SerializeField] private int commonWeight = 80;
    [SerializeField] private int fastWeight = 20;
    [SerializeField] private int eliteWeight = 0;

    public int KillCount => killCount;
    public int CommonWeight => commonWeight;
    public int FastWeight => fastWeight;
    public int EliteWeight => eliteWeight;

    public void RegisterKill()
    {
        killCount++;
        RecalculateWeights();
    }

    private void RecalculateWeights()
    {
        if (killCount >= 5)
        {
            commonWeight = 65;
            fastWeight = 35;
        }

        if (killCount >= 10)
        {
            commonWeight = 55;
            fastWeight = 45;
        }

        if (killCount >= 15)
        {
            commonWeight = 50;
            fastWeight = 50;
        }
    }
}