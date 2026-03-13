using System.Collections.Generic;
using UnityEngine;

public class CorridorLoop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform reference;           // Main Camera
    [SerializeField] private List<Transform> segments;      // Corridor_01, Corridor_02, ...
    [SerializeField] private CombatDirector combatDirector; // GameRoot üzerindeki CombatDirector

    [Header("Motion")]
    [SerializeField] private float speed = 6f;

    [Header("Recycle")]
    [Tooltip("Segment tamamen kameranın arkasına geçince recycle olsun.")]
    [SerializeField] private float recycleBehind = 5f;

    private void Reset()
    {
        if (reference == null && Camera.main != null)
            reference = Camera.main.transform;
    }

    private void Awake()
    {
        if (combatDirector == null)
            combatDirector = GetComponent<CombatDirector>();

        if (combatDirector == null)
            combatDirector = FindFirstObjectByType<CombatDirector>();
    }

    private void Start()
    {
        if (reference == null && Camera.main != null)
            reference = Camera.main.transform;

        // Başlangıçta segmentleri gerçek uçlarına göre arka arkaya diz
        AlignSegmentsByBounds();
    }

    private void Update()
    {
        if (combatDirector != null && combatDirector.IsWorldBlocked)
            return;

        if (segments == null || segments.Count == 0) return;

        // 1) Hepsini geriye kaydır
        float dz = speed * Time.deltaTime;
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null) continue;
            segments[i].position += Vector3.back * dz;
        }

        if (reference == null) return;

        // 2) Kamera arkasına tamamen düşen segmentleri en öne yapıştır
        float thresholdZ = reference.position.z - recycleBehind;

        for (int i = 0; i < segments.Count; i++)
        {
            Transform seg = segments[i];
            if (seg == null) continue;

            Bounds b = GetWorldBounds(seg);

            // Segmentin en ön ucu bile threshold'un arkasına geçtiyse tamamen arkada demektir
            if (b.max.z < thresholdZ)
            {
                float frontMostMaxZ = FindFrontMostMaxZ();

                // Bu segmentin current bounds.min.z ile pivot arasındaki offset'i koruyarak taşı
                float pivotToMin = seg.position.z - b.min.z;

                Vector3 p = seg.position;
                p.z = frontMostMaxZ + pivotToMin;   // minZ => frontMostMaxZ olacak şekilde
                seg.position = p;
            }
        }
    }

    private void AlignSegmentsByBounds()
    {
        // segments listesindeki sıraya göre diziyoruz: 0 en arkada, sonra 1,2...
        // İstersen 0'ı başlangıç segmentin olarak sahnede istediğin yere koy, diğerleri ona yapışsın.

        if (segments == null || segments.Count == 0) return;

        // İlk segmenti referans al
        Transform first = segments[0];
        if (first == null) return;

        Bounds firstB = GetWorldBounds(first);
        float currentFrontMaxZ = firstB.max.z;

        for (int i = 1; i < segments.Count; i++)
        {
            Transform seg = segments[i];
            if (seg == null) continue;

            Bounds b = GetWorldBounds(seg);
            float pivotToMin = seg.position.z - b.min.z;

            Vector3 p = seg.position;
            // Bu segmentin minZ'sini bir öncekinin maxZ'sine yapıştır
            p.z = currentFrontMaxZ + pivotToMin;
            seg.position = p;

            // Yeni front max güncelle
            Bounds newB = GetWorldBounds(seg);
            currentFrontMaxZ = newB.max.z;
        }
    }

    private float FindFrontMostMaxZ()
    {
        float maxZ = float.NegativeInfinity;
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null) continue;
            Bounds b = GetWorldBounds(segments[i]);
            if (b.max.z > maxZ) maxZ = b.max.z;
        }
        return maxZ;
    }

    private Bounds GetWorldBounds(Transform root)
    {
        // Root altındaki TÜM rendererlardan birleşik bounds çıkarır
        Renderer[] rs = root.GetComponentsInChildren<Renderer>();
        if (rs == null || rs.Length == 0)
        {
            // Renderer yoksa fallback: küçük bir bounds
            return new Bounds(root.position, Vector3.one);
        }

        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++)
            b.Encapsulate(rs[i].bounds);

        return b;
    }
}
