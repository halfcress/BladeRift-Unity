using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
#endif

public static class SceneSnapshotExporter
{
    // -----------------------------
    // Data models (JsonUtility-safe)
    // -----------------------------
    [Serializable]
    public class FullSnapshot
    {
        public Meta meta = new Meta();
        public SceneSnapshot scene = new SceneSnapshot();
        public ProjectSnapshot project = new ProjectSnapshot();
        public CodeSnapshot code = new CodeSnapshot();
    }

    [Serializable]
    public class Meta
    {
        public string exportedAtLocalTime;
        public string unityVersion;
        public string platform;
        public string note;
    }

    [Serializable]
    public class SceneSnapshot
    {
        public string sceneName;
        public string scenePath;
        public List<GameObjectData> roots = new List<GameObjectData>();
    }

    [Serializable]
    public class ProjectSnapshot
    {
        public RenderPipelineData renderPipeline = new RenderPipelineData();
        public TimeData time = new TimeData();
        public QualityData quality = new QualityData();

        public List<TextFileData> projectFiles = new List<TextFileData>(); // ProjectSettings/ Packages/ etc.
    }

    [Serializable]
    public class CodeSnapshot
    {
        public List<TextFileData> csFiles = new List<TextFileData>(); // Assets/**/*.cs
    }

    [Serializable]
    public class RenderPipelineData
    {
        public string graphicsCurrentRenderPipeline;
        public string qualityRenderPipeline;
    }

    [Serializable]
    public class TimeData
    {
        public float fixedDeltaTime;
        public float maximumDeltaTime;
        public int targetFrameRate;
        public float timeScale;
    }

    [Serializable]
    public class QualityData
    {
        public int qualityLevelIndex;
        public string qualityLevelName;
        public int vSyncCount;
    }

    [Serializable]
    public class TextFileData
    {
        public string relativePath;
        public long sizeBytes;
        public string sha256;
        public string text; // full content
    }

    [Serializable]
    public class GameObjectData
    {
        public string name;
        public string path;                 // Root/Child/SubChild
        public bool activeSelf;
        public bool activeInHierarchy;

        public string tag;
        public int layer;

        public string staticFlags;          // Editor static flags text

        public PrefabData prefab = new PrefabData();

        public TransformData transform = new TransformData();
        public BoundsData rendererBounds = new BoundsData(); // combined bounds of all Renderers under this GO
        public BoundsData colliderBounds = new BoundsData(); // combined bounds of all Colliders under this GO

        public List<ComponentData> components = new List<ComponentData>();
        public List<GameObjectData> children = new List<GameObjectData>();
    }

    [Serializable]
    public class PrefabData
    {
        public string instanceStatus;       // Connected/Disconnected/NotAPrefab
        public string assetType;            // Regular/Model/Variant/NotAPrefab
        public string sourceName;           // prefab asset name (if any)
        public string sourcePath;           // prefab asset path (if any)
    }

    [Serializable]
    public class TransformData
    {
        public float[] localPosition;
        public float[] localRotationEuler;
        public float[] localScale;

        public float[] worldPosition;
        public float[] worldRotationEuler;
        public float[] worldScaleLossy;
    }

    [Serializable]
    public class BoundsData
    {
        public bool hasBounds;
        public float[] center;
        public float[] size;
        public float[] min;
        public float[] max;
    }

    [Serializable]
    public class ComponentData
    {
        public string type;
        public List<FieldKV> fields = new List<FieldKV>();
    }

    [Serializable]
    public class FieldKV
    {
        public string name;
        public string value;
    }

#if UNITY_EDITOR
    // -----------------------------
    // Menu entry (ONE BUTTON)
    // -----------------------------
    [MenuItem("Tools/BladeRift/Export FULL Snapshot (Scene+Project+Code)")]
    public static void ExportFullSnapshotMenu()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("Active scene is not valid.");
            return;
        }

        FullSnapshot full = new FullSnapshot();
        full.meta.exportedAtLocalTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        full.meta.unityVersion = Application.unityVersion;
        full.meta.platform = Application.platform.ToString();
        full.meta.note = "BladeRift FULL snapshot: Scene + Project files + all Assets C# sources.";

        // Scene
        full.scene.sceneName = scene.name;
        full.scene.scenePath = scene.path;

        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            full.scene.roots.Add(SerializeGameObjectRecursive(root, root.name));
        }

        // Project
        FillProjectSnapshot(full.project);

        // Code
        FillCodeSnapshot(full.code);

        // Write JSON
        string outFolder = Path.Combine(Application.dataPath, "Docs/Snapshots");
        Directory.CreateDirectory(outFolder);

        string safeSceneName = string.IsNullOrWhiteSpace(scene.name) ? "UntitledScene" : scene.name;
        string fileName = $"BladeRift_FULL_{safeSceneName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string fullPath = Path.Combine(outFolder, fileName);

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(full, settings);

        File.WriteAllText(fullPath, json, Encoding.UTF8);

        Debug.Log($"FULL snapshot exported: {fullPath}");
        AssetDatabase.Refresh();
    }

    // -----------------------------
    // Scene serialization
    // -----------------------------
    private static GameObjectData SerializeGameObjectRecursive(GameObject go, string path)
    {
        GameObjectData data = new GameObjectData();
        data.name = go.name;
        data.path = path;
        data.activeSelf = go.activeSelf;
        data.activeInHierarchy = go.activeInHierarchy;

        data.tag = go.tag;
        data.layer = go.layer;

        // Static flags (editor only)
        try
        {
            var flags = GameObjectUtility.GetStaticEditorFlags(go);
            data.staticFlags = flags.ToString();
        }
        catch
        {
            data.staticFlags = "";
        }

        // Prefab info (editor only)
        FillPrefabData(go, data.prefab);

        // Transform (local + world)
        Transform t = go.transform;
        data.transform.localPosition = Vec3(t.localPosition);
        data.transform.localRotationEuler = Vec3(t.localEulerAngles);
        data.transform.localScale = Vec3(t.localScale);

        data.transform.worldPosition = Vec3(t.position);
        data.transform.worldRotationEuler = Vec3(t.eulerAngles);
        data.transform.worldScaleLossy = Vec3(t.lossyScale);

        // Combined bounds
        data.rendererBounds = GetCombinedRendererBounds(go.transform);
        data.colliderBounds = GetCombinedColliderBounds(go.transform);

        // Components + fields
        var comps = go.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null) continue;

            ComponentData cd = new ComponentData();
            cd.type = c.GetType().FullName;

            // Skip Transform fields (we already store transform)
            if (!(c is Transform))
            {
                CaptureFields(c, cd.fields);
                CaptureCameraExtrasIfAny(c, cd.fields);
            }

            data.components.Add(cd);
        }

        // Children
        for (int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i).gameObject;
            data.children.Add(SerializeGameObjectRecursive(child, path + "/" + child.name));
        }

        return data;
    }

    private static void FillPrefabData(GameObject go, PrefabData prefab)
    {
        try
        {
            var instStatus = PrefabUtility.GetPrefabInstanceStatus(go);
            var assetType = PrefabUtility.GetPrefabAssetType(go);

            prefab.instanceStatus = instStatus.ToString();
            prefab.assetType = assetType.ToString();

            var source = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (source != null)
            {
                prefab.sourceName = source.name;
                prefab.sourcePath = AssetDatabase.GetAssetPath(source);
            }
            else
            {
                prefab.sourceName = "";
                prefab.sourcePath = "";
            }
        }
        catch
        {
            prefab.instanceStatus = "";
            prefab.assetType = "";
            prefab.sourceName = "";
            prefab.sourcePath = "";
        }
    }

    private static BoundsData GetCombinedRendererBounds(Transform root)
    {
        Renderer[] rs = root.GetComponentsInChildren<Renderer>(true);
        if (rs == null || rs.Length == 0)
            return new BoundsData { hasBounds = false };

        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++)
            b.Encapsulate(rs[i].bounds);

        return ToBoundsData(b);
    }

    private static BoundsData GetCombinedColliderBounds(Transform root)
    {
        Collider[] cs = root.GetComponentsInChildren<Collider>(true);
        if (cs == null || cs.Length == 0)
            return new BoundsData { hasBounds = false };

        Bounds b = cs[0].bounds;
        for (int i = 1; i < cs.Length; i++)
            b.Encapsulate(cs[i].bounds);

        return ToBoundsData(b);
    }

    private static BoundsData ToBoundsData(Bounds b)
    {
        return new BoundsData
        {
            hasBounds = true,
            center = Vec3(b.center),
            size = Vec3(b.size),
            min = Vec3(b.min),
            max = Vec3(b.max)
        };
    }

    // -----------------------------
    // Field capture (public + [SerializeField] private)
    // -----------------------------
    private static void CaptureFields(Component component, List<FieldKV> outFields)
    {
        try
        {
            var type = component.GetType();

            // Include public fields
            var publicFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var f in publicFields)
            {
                if (f.IsLiteral || f.IsInitOnly) continue;
                AddField(component, f, outFields);
            }

            // Include [SerializeField] private/protected fields
            var nonPublicFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var f in nonPublicFields)
            {
                if (f.IsLiteral || f.IsInitOnly) continue;

                // Only dump explicitly serialized fields to avoid noise
                bool hasSerializeField = Attribute.IsDefined(f, typeof(SerializeField));
                if (!hasSerializeField) continue;

                AddField(component, f, outFields);
            }
        }
        catch
        {
            // ignore reflection issues
        }
    }

    private static void AddField(Component component, FieldInfo f, List<FieldKV> outFields)
    {
        object v = null;
        try { v = f.GetValue(component); } catch { }

        outFields.Add(new FieldKV
        {
            name = f.Name,
            value = ValueToString(v)
        });
    }

    private static void CaptureCameraExtrasIfAny(Component component, List<FieldKV> outFields)
    {
        // Camera is super relevant for corridor perception.
        if (component is Camera cam)
        {
            outFields.Add(new FieldKV { name = "Camera.fieldOfView", value = cam.fieldOfView.ToString("0.###") });
            outFields.Add(new FieldKV { name = "Camera.nearClipPlane", value = cam.nearClipPlane.ToString("0.###") });
            outFields.Add(new FieldKV { name = "Camera.farClipPlane", value = cam.farClipPlane.ToString("0.###") });
            outFields.Add(new FieldKV { name = "Camera.clearFlags", value = cam.clearFlags.ToString() });
            outFields.Add(new FieldKV { name = "Camera.depth", value = cam.depth.ToString("0.###") });
            outFields.Add(new FieldKV { name = "Camera.orthographic", value = cam.orthographic ? "true" : "false" });
        }
    }

    private static string ValueToString(object v)
    {
        if (v == null) return "null";

        try
        {
            switch (v)
            {
                case string s: return s;
                case bool b: return b ? "true" : "false";
                case int i: return i.ToString();
                case float f: return f.ToString("0.#####");
                case double d: return d.ToString("0.#####");
                case Vector2 v2: return $"({v2.x:0.#####},{v2.y:0.#####})";
                case Vector3 v3: return $"({v3.x:0.#####},{v3.y:0.#####},{v3.z:0.#####})";
                case Vector4 v4: return $"({v4.x:0.#####},{v4.y:0.#####},{v4.z:0.#####},{v4.w:0.#####})";
                case Color c: return $"({c.r:0.###},{c.g:0.###},{c.b:0.###},{c.a:0.###})";
                case UnityEngine.Object uo:
                    {
                        // Try to include asset path if any
                        string p = AssetDatabase.GetAssetPath(uo);
                        if (!string.IsNullOrEmpty(p)) return $"{uo.name} @ {p}";
                        return uo.name;
                    }
                default:
                    // Attempt simple ToString (avoid huge dumps)
                    return v.ToString();
            }
        }
        catch
        {
            return "<?>"; // fallback
        }
    }

    private static float[] Vec3(Vector3 v) => new float[] { v.x, v.y, v.z };

    // -----------------------------
    // Project snapshot (ProjectSettings + Packages)
    // -----------------------------
    private static void FillProjectSnapshot(ProjectSnapshot ps)
    {
        // Render pipeline references
        try
        {
            var rp = GraphicsSettings.currentRenderPipeline;
            ps.renderPipeline.graphicsCurrentRenderPipeline = rp != null ? rp.name : "null";
        }
        catch { ps.renderPipeline.graphicsCurrentRenderPipeline = ""; }

        try
        {
            var rpQ = QualitySettings.renderPipeline;
            ps.renderPipeline.qualityRenderPipeline = rpQ != null ? rpQ.name : "null";
        }
        catch { ps.renderPipeline.qualityRenderPipeline = ""; }

        // Time / Quality
        ps.time.fixedDeltaTime = Time.fixedDeltaTime;
        ps.time.maximumDeltaTime = Time.maximumDeltaTime;
        ps.time.targetFrameRate = Application.targetFrameRate;
        ps.time.timeScale = Time.timeScale;

        ps.quality.qualityLevelIndex = QualitySettings.GetQualityLevel();
        ps.quality.qualityLevelName = QualitySettings.names != null && QualitySettings.names.Length > 0
            ? QualitySettings.names[ps.quality.qualityLevelIndex]
            : "";
        ps.quality.vSyncCount = QualitySettings.vSyncCount;

        // Include full file contents from ProjectSettings and Packages
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        // ProjectSettings (full)
        string projectSettingsDir = Path.Combine(projectRoot, "ProjectSettings");
        AddAllFilesRecursive(projectSettingsDir, projectRoot, ps.projectFiles);

        // Packages (manifest + lock are most important, but include all files for completeness)
        string packagesDir = Path.Combine(projectRoot, "Packages");
        AddAllFilesRecursive(packagesDir, projectRoot, ps.projectFiles);
    }

    // -----------------------------
    // Code snapshot (Assets/**/*.cs)
    // -----------------------------
    private static void FillCodeSnapshot(CodeSnapshot cs)
    {
        string assetsDir = Application.dataPath;
        string projectRoot = Directory.GetParent(assetsDir).FullName;

        var files = Directory.GetFiles(assetsDir, "*.cs", SearchOption.AllDirectories);
        foreach (var f in files)
        {
            // Skip Library/Temp (not under Assets anyway), and ignore generated .cs if any
            cs.csFiles.Add(ReadTextFile(f, projectRoot));
        }
    }

    // -----------------------------
    // File helpers
    // -----------------------------
    private static void AddAllFilesRecursive(string folder, string projectRoot, List<TextFileData> list)
    {
        if (!Directory.Exists(folder)) return;

        var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        foreach (var f in files)
        {
            // Skip huge binary-like files by extension (rare here, but safe)
            string ext = Path.GetExtension(f).ToLowerInvariant();
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga" || ext == ".psd" || ext == ".fbx")
                continue;

            list.Add(ReadTextFile(f, projectRoot));
        }
    }

    private static TextFileData ReadTextFile(string fullPath, string projectRoot)
    {
        TextFileData t = new TextFileData();
        t.relativePath = MakeRelativePath(fullPath, projectRoot);

        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            t.sizeBytes = bytes.LongLength;
            t.sha256 = ComputeSha256(bytes);

            // Try decode as UTF8 text (fallback to Latin1-ish)
            // If it contains many nulls, it's likely binary; we still store as base64-like marker
            string text = null;
            try
            {
                text = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                text = Encoding.Default.GetString(bytes);
            }

            // Binary-ish heuristic: too many '\0'
            int nullCount = 0;
            int sample = Math.Min(text.Length, 2000);
            for (int i = 0; i < sample; i++)
                if (text[i] == '\0') nullCount++;

            if (sample > 0 && ((float)nullCount / sample) > 0.05f)
            {
                t.text = "<BINARY_OR_NON_TEXT_FILE_SKIPPED>";
            }
            else
            {
                t.text = text;
            }
        }
        catch (Exception e)
        {
            t.sizeBytes = 0;
            t.sha256 = "";
            t.text = "<READ_ERROR> " + e.Message;
        }

        return t;
    }

    private static string ComputeSha256(byte[] bytes)
    {
        using (var sha = SHA256.Create())
        {
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    private static string MakeRelativePath(string fullPath, string root)
    {
        try
        {
            var uriPath = new Uri(fullPath);
            var uriRoot = new Uri(root.EndsWith(Path.DirectorySeparatorChar.ToString()) ? root : root + Path.DirectorySeparatorChar);
            return Uri.UnescapeDataString(uriRoot.MakeRelativeUri(uriPath).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
        catch
        {
            return fullPath;
        }
    }
#endif
}