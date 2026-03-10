using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SlashTrail : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwipeInput swipeInput;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Material trailMaterial;

    [Header("Trail Ayarları")]
    [SerializeField] private float trailDepth = 2f;
    [SerializeField] private float startWidth = 0.18f;
    [SerializeField] private float endWidth = 0.0f;
    [SerializeField] private float pointLifetime = 0.3f;
    [SerializeField] private float minPointDistancePx = 6f;

    [Header("Neon Renk")]
    [SerializeField] private Color colorStart = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color colorMid = new Color(0.4f, 0.8f, 1f, 0.9f);
    [SerializeField] private Color colorEnd = new Color(0.1f, 0.5f, 1f, 0f);

    private struct TrailPoint
    {
        public Vector3 localPosition; // kamera local uzayında
        public float birthTime;
    }

    private List<TrailPoint> points = new List<TrailPoint>();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private Vector2 lastAddedScreenPos;

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

        meshRenderer.sortingOrder = 10;
    }

    /// Ekran koordinatını kamera local uzayına çevirir
    private Vector3 ScreenToLocal(Vector2 screenPos)
    {
        // Viewport koordinatına çevir (0-1 arası)
        float vx = screenPos.x / Screen.width;
        float vy = screenPos.y / Screen.height;

        // Kameranın görüş alanındaki gerçek boyutu hesapla
        float depth = targetCamera.nearClipPlane + trailDepth;
        float halfH = Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * depth;
        float halfW = halfH * targetCamera.aspect;

        // Local uzayda pozisyon (-1..1 aralığını gerçek boyuta çevir)
        float lx = (vx * 2f - 1f) * halfW;
        float ly = (vy * 2f - 1f) * halfH;
        float lz = depth;

        return new Vector3(lx, ly, lz);
    }

    private void Update()
    {
        if (swipeInput == null || targetCamera == null) return;

        float now = Time.time;
        points.RemoveAll(p => now - p.birthTime > pointLifetime);

        if (swipeInput.IsDown)
        {
            bool shaking = FeedbackManager.Instance != null && FeedbackManager.Instance.IsShaking;

            if (!shaking)
            {
                Vector2 fingerPos = swipeInput.FingerPosition;
                float screenDist = Vector2.Distance(fingerPos, lastAddedScreenPos);
                bool shouldAdd = points.Count == 0 || screenDist >= minPointDistancePx;

                if (shouldAdd)
                {
                    Vector3 localPos = ScreenToLocal(fingerPos);
                    points.Add(new TrailPoint { localPosition = localPos, birthTime = now });
                    lastAddedScreenPos = fingerPos;
                }
            }
        }
        else
        {
            lastAddedScreenPos = Vector2.zero;
        }

        BuildMesh(now);
    }

    private void BuildMesh(float now)
    {
        mesh.Clear();

        int count = points.Count;
        if (count < 2) return;

        var vertices = new Vector3[count * 2];
        var colors = new Color[count * 2];
        var uvs = new Vector2[count * 2];
        var triangles = new int[(count - 1) * 6];

        // Kamera local uzayında Z eksenine dik normal
        Vector3[] normals = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 dir;
            if (i == 0)
                dir = (points[1].localPosition - points[0].localPosition).normalized;
            else if (i == count - 1)
                dir = (points[count - 1].localPosition - points[count - 2].localPosition).normalized;
            else
            {
                Vector3 d1 = (points[i].localPosition - points[i - 1].localPosition).normalized;
                Vector3 d2 = (points[i + 1].localPosition - points[i].localPosition).normalized;
                dir = ((d1 + d2) * 0.5f).normalized;
            }
            // Kamera local uzayında Z eksenine dik → X/Y düzleminde normal
            normals[i] = new Vector3(-dir.y, dir.x, 0f).normalized;
        }

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            float age = (now - points[i].birthTime) / pointLifetime;
            float alpha = Mathf.Clamp01(1f - age);
            float headFade = Mathf.Clamp01((float)(count - 1 - i) / Mathf.Max(1, count * 0.15f));
            alpha *= headFade;

            float width = Mathf.Lerp(endWidth, startWidth, 1f - t) * 0.5f;

            vertices[i * 2] = points[i].localPosition + normals[i] * width;
            vertices[i * 2 + 1] = points[i].localPosition - normals[i] * width;

            Color c = t < 0.5f
                ? Color.Lerp(colorStart, colorMid, t * 2f)
                : Color.Lerp(colorMid, colorEnd, (t - 0.5f) * 2f);
            c.a *= alpha;

            colors[i * 2] = c;
            colors[i * 2 + 1] = c;
            uvs[i * 2] = new Vector2(t, 0f);
            uvs[i * 2 + 1] = new Vector2(t, 1f);
        }

        int tri = 0;
        for (int i = 0; i < count - 1; i++)
        {
            int a = i * 2, b = i * 2 + 1, c = i * 2 + 2, d = i * 2 + 3;
            triangles[tri++] = a; triangles[tri++] = c; triangles[tri++] = b;
            triangles[tri++] = b; triangles[tri++] = c; triangles[tri++] = d;
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private void OnDestroy()
    {
        if (mesh != null) Destroy(mesh);
    }
}