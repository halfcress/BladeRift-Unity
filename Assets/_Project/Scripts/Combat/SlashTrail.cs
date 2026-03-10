using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fruit Ninja tarzı slash trail.
/// TrailRenderer kullanmaz — her frame dinamik mesh üretir.
///
/// Kurulum:
/// 1. Sahneye boş obje ekle → adı "SlashTrail"
/// 2. Bu script'i ekle
/// 3. MeshRenderer ve MeshFilter otomatik eklenir
/// 4. Inspector'da MAT_SlashTrail material'ini ata
///    (URP/Unlit, Transparent, Surface Type=Transparent)
/// 5. SwipeInput ve Camera referanslarını bağla
///    (boş bırakılırsa otomatik bulur)
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SlashTrail : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwipeInput swipeInput;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Material trailMaterial;

    [Header("Trail Ayarları")]
    [Tooltip("Kameradan Z uzaklığı — kameranın near clip'inden büyük olmalı")]
    [SerializeField] private float trailDepth = 1.5f;

    [Tooltip("Trail başlangıç kalınlığı (dünya birimi)")]
    [SerializeField] private float startWidth = 0.15f;

    [Tooltip("Trail bitiş kalınlığı")]
    [SerializeField] private float endWidth = 0.0f;

    [Tooltip("Trail ömrü — bu süre sonunda nokta siliner (saniye)")]
    [SerializeField] private float pointLifetime = 0.2f;

    [Tooltip("Yeni nokta eklemek için minimum pixel mesafesi")]
    [SerializeField] private float minPointDistancePx = 8f;

    [Tooltip("Trail rengi — başlangıç (parlak)")]
    [SerializeField] private Color colorStart = new Color(1f, 0.95f, 0.8f, 1f);

    [Tooltip("Trail rengi — bitiş (şeffaf)")]
    [SerializeField] private Color colorEnd = new Color(1f, 0.7f, 0.3f, 0f);

    // Her nokta: dünya pozisyonu + doğum zamanı
    private struct TrailPoint
    {
        public Vector3 position;
        public float birthTime;
    }

    private List<TrailPoint> points = new List<TrailPoint>();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private Vector3 lastAddedWorldPos;

    private void Awake()
    {
        if (swipeInput == null)
            swipeInput = FindFirstObjectByType<SwipeInput>();
        if (targetCamera == null)
            targetCamera = Camera.main;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh();
        mesh.name = "SlashTrailMesh";
        meshFilter.mesh = mesh;

        if (trailMaterial != null)
            meshRenderer.material = trailMaterial;

        // Sorting: UI'ın üstünde görünsün
        meshRenderer.sortingOrder = 10;

        Debug.Log($"[SlashTrail] Awake. Camera={targetCamera?.name} SwipeInput={swipeInput?.name}");
    }

    private void Update()
    {
        if (swipeInput == null || targetCamera == null) return;

        float now = Time.time;

        // Eski noktaları temizle
        points.RemoveAll(p => now - p.birthTime > pointLifetime);

        if (swipeInput.IsDown)
        {
            // Parmak pozisyonunu dünya uzayına çevir
            Vector3 screenPos = new Vector3(
                swipeInput.FingerPosition.x,
                swipeInput.FingerPosition.y,
                trailDepth
            );
            Vector3 worldPos = targetCamera.ScreenToWorldPoint(screenPos);

            // Minimum mesafe kontrolü — çok yakın noktaları atlat
            bool shouldAdd = points.Count == 0;
            if (!shouldAdd)
            {
                // Dünya mesafesi yerine screen mesafesi kullan — daha tutarlı
                float screenDist = Vector2.Distance(
                    swipeInput.FingerPosition,
                    lastAddedWorldPos == Vector3.zero ? swipeInput.FingerPosition :
                    (Vector2)targetCamera.WorldToScreenPoint(lastAddedWorldPos)
                );
                shouldAdd = screenDist >= minPointDistancePx;
            }

            if (shouldAdd)
            {
                points.Add(new TrailPoint { position = worldPos, birthTime = now });
                lastAddedWorldPos = worldPos;
            }
        }
        else
        {
            lastAddedWorldPos = Vector3.zero;
        }

        BuildMesh(now);
    }

    private void BuildMesh(float now)
    {
        mesh.Clear();

        int count = points.Count;
        if (count < 2)
            return;

        // Her nokta için 2 vertex (üst + alt) → quad şeridi
        var vertices = new Vector3[count * 2];
        var colors = new Color[count * 2];
        var uvs = new Vector2[count * 2];
        var triangles = new int[(count - 1) * 6];

        for (int i = 0; i < count; i++)
        {
            TrailPoint p = points[i];

            // t: 0 = en yeni (baş), 1 = en eski (kuyruk)
            float t = (float)i / (count - 1);

            // Age ratio: ne kadar eskidi
            float age = (now - p.birthTime) / pointLifetime;
            float alpha = Mathf.Lerp(1f, 0f, age);

            // Kalınlık: başta kalın, sonda ince
            float width = Mathf.Lerp(startWidth, endWidth, t) * 0.5f;

            // Hareket yönüne dik vektör (normal)
            Vector3 dir;
            if (i == 0)
                dir = points[1].position - points[0].position;
            else if (i == count - 1)
                dir = points[count - 1].position - points[count - 2].position;
            else
                dir = points[i + 1].position - points[i - 1].position;

            // Kameraya bakan düzlemde normal hesapla
            Vector3 toCamera = targetCamera.transform.position - p.position;
            Vector3 right = Vector3.Cross(dir.normalized, toCamera.normalized).normalized;

            vertices[i * 2]     = p.position + right * width;
            vertices[i * 2 + 1] = p.position - right * width;

            // Renk: interpolasyon + alpha
            Color c = Color.Lerp(colorStart, colorEnd, t);
            c.a *= alpha;
            colors[i * 2]     = c;
            colors[i * 2 + 1] = c;

            uvs[i * 2]     = new Vector2(t, 0f);
            uvs[i * 2 + 1] = new Vector2(t, 1f);
        }

        // Triangle strip
        int tri = 0;
        for (int i = 0; i < count - 1; i++)
        {
            int a = i * 2;
            int b = i * 2 + 1;
            int c = i * 2 + 2;
            int d = i * 2 + 3;

            triangles[tri++] = a;
            triangles[tri++] = c;
            triangles[tri++] = b;

            triangles[tri++] = b;
            triangles[tri++] = c;
            triangles[tri++] = d;
        }

        mesh.vertices  = vertices;
        mesh.colors    = colors;
        mesh.uv        = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private void OnDestroy()
    {
        if (mesh != null)
            Destroy(mesh);
    }
}
