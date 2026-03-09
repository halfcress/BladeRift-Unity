using UnityEngine;

public class CombatDirector : MonoBehaviour
{
    [SerializeField] private SwipeDirection lastReceivedDirection = SwipeDirection.None;

    public void OnSwipeDirectionCommitted(SwipeDirection dir)
    {
        lastReceivedDirection = dir;
        Debug.Log($"CombatDirector received swipe: {dir}");
    }
}