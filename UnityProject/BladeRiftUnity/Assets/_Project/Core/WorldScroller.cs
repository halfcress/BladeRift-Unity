using UnityEngine;

public class WorldScroller : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField] private float speed = 4f;

    private void Update()
    {
        // Dünya oyuncuya doðru aksýn (Z ekseninde geriye)
        transform.position += Vector3.back * (speed * Time.deltaTime);
    }
}