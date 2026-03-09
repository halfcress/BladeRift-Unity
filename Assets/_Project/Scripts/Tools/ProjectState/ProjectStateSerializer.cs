using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering;
#endif

// -----------------------------
// SERIALIZER — v5
// Degisiklikler:
// 1. FillDocsSnapshot() — .md dosyalarini JSON'a gomer
// 2. FillPrefabData() — GUID eklendi
// 3. ValueToString() — UnityEngine.Object referanslari duzgun cozuluyor, <?> kalmiyor
// -----------------------------

public static class ProjectStateSerializer
{
    // -----------------------------
    // SCENE SERIALIZATION
    // -----------------------------

    public static GameObjectData SerializeGameObjectRecursive(GameObject go, string path)
    {
        GameObjectData data = new GameObjectData();
        data.name = go.name;
        data.path = path;
        data.activeSelf = go.activeSelf;
        data.activeInHierarchy = go.activeInHierarchy;
        data.tag = go.tag;
        data.layer = go.layer;

#if UNITY_EDITOR
        try
        {
            var flags = GameObjectUtility.GetStaticEditorFlags(go);
            data.staticFlags = flags.ToString();
        }
        catch { data.staticFlags = ""; }

        FillPrefabData(go, data.prefab);
#endif

        Transform t = go.transform;
        data.transform.localPosition = Vec3(t.localPosition);
        data.transform.localRotationEuler = Vec3(t.localEulerAngles);
        data.transform.localScale = Vec3(t.localScale);
        data.transform.worldPosition = Vec3(t.position);
        data.transform.worldRotationEuler = Vec3(t.eulerAngles);
        data.transform.worldScaleLossy = Vec3(t.lossyScale);

        data.rendererBounds = GetCombinedRendererBounds(go.transform);
        data.colliderBounds = GetCombinedColliderBounds(go.transform);

        var comps = go.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            ComponentData cd = new ComponentData();
            cd.type = c.GetType().FullName;

            if (!(c is Transform))
            {
                CaptureFields(c, cd.fields);
                CaptureCameraExtrasIfAny(c, cd.fields);
            }

            data.components.Add(cd);
        }

        for (int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i).gameObject;
            data.children.Add(SerializeGameObjectRecursive(child, path + "/" + child.name));
        }

        return data;
    }

    // -----------------------------
    // PREFAB INFO — v5: GUID eklendi
    // -----------------------------

#if UNITY_EDITOR
    private static void FillPrefabData(GameObject go, PrefabData prefab)
    {
        try
        {
            prefab.instanceStatus = PrefabUtility.GetPrefabInstanceStatus(go).ToString();
            prefab.assetType = PrefabUtility.GetPrefabAssetType(go).ToString();

            var source = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (source != null)
            {
                prefab.sourceName = source.name;
                prefab.sourcePath = AssetDatabase.GetAssetPath(source);
                prefab.sourceGuid = AssetDatabase.AssetPathToGUID(prefab.sourcePath);
            }
            else
            {
                prefab.sourceName = "";
                prefab.sourcePath = "";
                prefab.sourceGuid = "";
            }
        }
        catch
        {
            prefab.instanceStatus = "";
            prefab.assetType = "";
            prefab.sourceName = "";
            prefab.sourcePath = "";
            prefab.sourceGuid = "";
        }
    }
#endif

    // -----------------------------
    // BOUNDS
    // -----------------------------

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

    private static BoundsData ToBoundsData(Bounds b) => new BoundsData
    {
        hasBounds = true,
        center = Vec3(b.center),
        size = Vec3(b.size),
        min = Vec3(b.min),
        max = Vec3(b.max)
    };

    // -----------------------------
    // FIELD CAPTURE
    // -----------------------------

    private static void CaptureFields(Component component, List<FieldKV> outFields)
    {
        try
        {
            var type = component.GetType();

            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (f.IsLiteral || f.IsInitOnly) continue;
                AddField(component, f, outFields);
            }

            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (f.IsLiteral || f.IsInitOnly) continue;
                if (!Attribute.IsDefined(f, typeof(SerializeField))) continue;
                AddField(component, f, outFields);
            }
        }
        catch { }
    }

    private static void AddField(Component component, FieldInfo f, List<FieldKV> outFields)
    {
        object v = null;
        try { v = f.GetValue(component); } catch { }

        outFields.Add(new FieldKV
        {
            name = f.Name,
            value = ValueToString(v, component)
        });
    }

    // -----------------------------
    // CAMERA EXTRAS
    // -----------------------------

    private static void CaptureCameraExtrasIfAny(Component component, List<FieldKV> outFields)
    {
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

    // -----------------------------
    // VALUE TO STRING — v5: <?> kalmiyor
    // UnityEngine.Object referanslari tam cozuluyor:
    //   - Asset ise: "name @ assetPath [GUID]"
    //   - Scene objesi ise: "name (ScenePath/To/Object) [instanceID]"
    //   - null ise: "null"
    // -----------------------------

    private static string ValueToString(object v, Component context = null)
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
                    return ResolveUnityObjectReference(uo);

                default: return v.ToString();
            }
        }
        catch
        {
            return "<SERIALIZE_ERROR>";
        }
    }

    private static string ResolveUnityObjectReference(UnityEngine.Object uo)
    {
        if (uo == null) return "null";

        try
        {
#if UNITY_EDITOR
            // Asset mi? (prefab, material, texture vs.)
            string assetPath = AssetDatabase.GetAssetPath(uo);
            if (!string.IsNullOrEmpty(assetPath))
            {
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                return $"{uo.name} @ {assetPath} [GUID:{guid}]";
            }
#endif
            // Scene objesi — GameObject veya Component
            if (uo is Component comp)
            {
                string goPath = GetGameObjectPath(comp.gameObject);
                return $"{comp.GetType().Name} on \"{goPath}\" [id:{uo.GetInstanceID()}]";
            }

            if (uo is GameObject go)
            {
                string goPath = GetGameObjectPath(go);
                return $"GameObject \"{goPath}\" [id:{uo.GetInstanceID()}]";
            }

            // Fallback — en azindan isim ve instanceID
            return $"{uo.name} [id:{uo.GetInstanceID()}]";
        }
        catch
        {
            // Hic bir sekilde cozemedik, en azindan isim ver
            try { return $"{uo.name} [unresolved]"; }
            catch { return "<unresolved_ref>"; }
        }
    }

    // Objenin tam hiyerarsi yolunu dondurur: "GameRoot/InputRoot/SwipeInput"
    private static string GetGameObjectPath(GameObject go)
    {
        if (go == null) return "<null>";
        var parts = new System.Collections.Generic.List<string>();
        Transform t = go.transform;
        while (t != null)
        {
            parts.Insert(0, t.name);
            t = t.parent;
        }
        return string.Join("/", parts);
    }

    private static float[] Vec3(Vector3 v) => new float[] { v.x, v.y, v.z };

    // -----------------------------
    // DOCS SNAPSHOT — v5: .md dosyalarini gomer
    // -----------------------------

#if UNITY_EDITOR
    public static void FillDocsSnapshot(DocsSnapshot docs)
    {
        docs.chatState = ReadMdManualOnly(ProjectStatePaths.ChatStatePath);
        docs.gameConcept = ReadMdSafe(ProjectStatePaths.GameConceptPath);
        docs.architecture = ReadMdSafe(ProjectStatePaths.ArchitecturePath);
        docs.todo = ReadMdSafe(ProjectStatePaths.TodoPath);
        docs.debugJournal = ReadMdSafe(ProjectStatePaths.DebugJournalPath);
        docs.milestoneLog = ReadMdSafe(ProjectStatePaths.MilestoneLogPath);

        // Docs altindaki diger .md dosyalari (yukarida tanimlananlar haric)
        var knownPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ProjectStatePaths.ChatStatePath,
            ProjectStatePaths.GameConceptPath,
            ProjectStatePaths.ArchitecturePath,
            ProjectStatePaths.TodoPath,
            ProjectStatePaths.DebugJournalPath,
            ProjectStatePaths.MilestoneLogPath,
            ProjectStatePaths.SnapshotIndexPath,
        };

        if (Directory.Exists(ProjectStatePaths.DocsRoot))
        {
            var mdFiles = Directory.GetFiles(ProjectStatePaths.DocsRoot, "*.md", SearchOption.AllDirectories);
            foreach (var mdFile in mdFiles)
            {
                // Snapshot dosyalari ve bilinen dosyalari atla
                if (mdFile.Contains("Snapshots")) continue;
                if (mdFile.Contains("SNAPSHOT_")) continue;
                if (knownPaths.Contains(mdFile)) continue;

                string rel = mdFile.Replace(ProjectStatePaths.DocsRoot, "Docs").Replace('\\', '/');
                docs.other.Add(new DocFileData
                {
                    relativePath = rel,
                    text = ReadMdSafe(mdFile)
                });
            }
        }
    }

    private static string ReadMdSafe(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;
        try { return File.ReadAllText(path, Encoding.UTF8); }
        catch { return null; }
    }

    // CHAT_STATE icin: autogenerated blogu strip eder, sadece manuel kismi dondurur
    private static string ReadMdManualOnly(string path)
    {
        string content = ReadMdSafe(path);
        if (content == null) return null;

        const string autoBlockStart = "<!-- AUTOGENERATED - DO NOT EDIT BELOW THIS LINE -->";
        int idx = content.IndexOf(autoBlockStart, StringComparison.Ordinal);
        if (idx >= 0)
            content = content.Substring(0, idx).TrimEnd();

        return string.IsNullOrWhiteSpace(content) ? null : content;
    }
#endif

    // -----------------------------
    // PROJECT SNAPSHOT
    // -----------------------------

#if UNITY_EDITOR
    public static void FillProjectSnapshot(ProjectSnapshot ps)
    {
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

        ps.time.fixedDeltaTime = Time.fixedDeltaTime;
        ps.time.maximumDeltaTime = Time.maximumDeltaTime;
        ps.time.targetFrameRate = Application.targetFrameRate;
        ps.time.timeScale = Time.timeScale;

        ps.quality.qualityLevelIndex = QualitySettings.GetQualityLevel();
        ps.quality.qualityLevelName = QualitySettings.names != null && QualitySettings.names.Length > 0
            ? QualitySettings.names[ps.quality.qualityLevelIndex] : "";
        ps.quality.vSyncCount = QualitySettings.vSyncCount;

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        AddAllFilesRecursive(Path.Combine(projectRoot, "ProjectSettings"), projectRoot, ps.projectFiles);
        AddAllFilesRecursive(Path.Combine(projectRoot, "Packages"), projectRoot, ps.projectFiles);
    }
#endif

    // -----------------------------
    // CODE SNAPSHOT
    // -----------------------------

    public static void FillCodeSnapshot(CodeSnapshot cs)
    {
        string assetsDir = Application.dataPath;
        string projectRoot = Directory.GetParent(assetsDir).FullName;

        var files = Directory.GetFiles(assetsDir, "*.cs", SearchOption.AllDirectories);
        foreach (var f in files)
            cs.csFiles.Add(ReadTextFile(f, projectRoot));
    }

    // -----------------------------
    // FILE HELPERS
    // -----------------------------

    private static void AddAllFilesRecursive(string folder, string projectRoot, List<TextFileData> list)
    {
        if (!Directory.Exists(folder)) return;
        var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        foreach (var f in files)
        {
            string ext = Path.GetExtension(f).ToLowerInvariant();
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" ||
                ext == ".tga" || ext == ".psd" || ext == ".fbx") continue;
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

            string text;
            try { text = Encoding.UTF8.GetString(bytes); }
            catch { text = Encoding.Default.GetString(bytes); }

            int nullCount = 0;
            int sample = Math.Min(text.Length, 2000);
            for (int i = 0; i < sample; i++)
                if (text[i] == '\0') nullCount++;

            t.text = (sample > 0 && ((float)nullCount / sample) > 0.05f)
                ? "<BINARY_OR_NON_TEXT_FILE_SKIPPED>"
                : text;
        }
        catch (Exception e)
        {
            t.sizeBytes = 0;
            t.sha256 = "";
            t.text = "<READ_ERROR> " + e.Message;
        }

        return t;
    }

    public static string ComputeSha256(byte[] bytes)
    {
        using (var sha = SHA256.Create())
        {
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    public static string MakeRelativePath(string fullPath, string root)
    {
        try
        {
            var uriPath = new Uri(fullPath);
            var uriRoot = new Uri(root.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? root
                : root + Path.DirectorySeparatorChar);
            return Uri.UnescapeDataString(
                uriRoot.MakeRelativeUri(uriPath).ToString()
                       .Replace('/', Path.DirectorySeparatorChar));
        }
        catch { return fullPath; }
    }

    public static int CountAllSceneObjects(UnityEngine.SceneManagement.Scene scene)
    {
        int count = 0;
        foreach (var root in scene.GetRootGameObjects())
            count += CountRecursive(root.transform);
        return count;
    }

    private static int CountRecursive(Transform t)
    {
        int count = 1;
        for (int i = 0; i < t.childCount; i++)
            count += CountRecursive(t.GetChild(i));
        return count;
    }

    public static string GetLatestSnapshotFile(string folder)
    {
        if (!Directory.Exists(folder)) return null;
        var files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
        if (files.Length == 0) return null;
        Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));
        return files[0];
    }
}