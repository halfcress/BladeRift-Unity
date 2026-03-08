using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// -----------------------------
// SNAPSHOT COMPARE (v3)
// -----------------------------

public static class ProjectStateCompare
{
#if UNITY_EDITOR

    // -----------------------------
    // ENTRY POINT
    // -----------------------------

    public static void CompareLatestSnapshots()
    {
        string workingFile = ProjectStateSerializer.GetLatestSnapshotFile(ProjectStatePaths.WorkingRoot);
        string debugFile   = ProjectStateSerializer.GetLatestSnapshotFile(ProjectStatePaths.DebugRoot);

        if (string.IsNullOrEmpty(workingFile))
        {
            Debug.LogError("[Compare] No WORKING snapshot found.");
            return;
        }

        if (string.IsNullOrEmpty(debugFile))
        {
            Debug.LogError("[Compare] No DEBUG snapshot found.");
            return;
        }

        FullSnapshot working = LoadSnapshot(workingFile);
        FullSnapshot debug   = LoadSnapshot(debugFile);

        if (working == null || debug == null)
        {
            Debug.LogError("[Compare] Failed to load one or both snapshots.");
            return;
        }

        string report = BuildCompareReport(working, debug,
            Path.GetFileName(workingFile),
            Path.GetFileName(debugFile));

        Directory.CreateDirectory(ProjectStatePaths.StateRoot);
        string outputPath = Path.Combine(ProjectStatePaths.StateRoot, "SNAPSHOT_COMPARE.md");
        File.WriteAllText(outputPath, report, Encoding.UTF8);

        AssetDatabase.Refresh();
        Debug.Log($"[Compare] Report saved: {outputPath}");
        EditorUtility.RevealInFinder(outputPath);
    }

    // -----------------------------
    // LOAD
    // -----------------------------

    private static FullSnapshot LoadSnapshot(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath, Encoding.UTF8);
            return JsonConvert.DeserializeObject<FullSnapshot>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Compare] Could not parse snapshot '{filePath}': {e.Message}");
            return null;
        }
    }

    // -----------------------------
    // REPORT BUILDER
    // -----------------------------

    public static string BuildCompareReport(
        FullSnapshot working,
        FullSnapshot debug,
        string workingFileName,
        string debugFileName)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# SNAPSHOT_COMPARE");
        sb.AppendLine();
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("Working Snapshot:");
        sb.AppendLine($"- {workingFileName}");
        sb.AppendLine();
        sb.AppendLine("Debug Snapshot:");
        sb.AppendLine($"- {debugFileName}");
        sb.AppendLine();

        var metaLines      = CompareMeta(working.meta, debug.meta);
        var rootLines      = CompareRootObjects(working.scene, debug.scene);
        var componentLines = CompareComponents(working.scene, debug.scene);

        WriteSection(sb, "Meta Changes",                   metaLines);
        WriteSection(sb, "Missing / Added Root Objects",   rootLines);
        WriteSection(sb, "Changed Component Fields",       componentLines);

        sb.AppendLine("## Summary");
        sb.AppendLine($"- Meta differences:            {metaLines.Count}");
        sb.AppendLine($"- Root object differences:     {rootLines.Count}");
        sb.AppendLine($"- Component/field differences: {componentLines.Count}");
        sb.AppendLine();

        return sb.ToString();
    }

    // -----------------------------
    // META COMPARE
    // -----------------------------

    private static List<string> CompareMeta(Meta w, Meta d)
    {
        var lines = new List<string>();
        if (w == null || d == null) return lines;

        CheckField(lines, "scene name",         w.activeSceneName,                d.activeSceneName);
        CheckField(lines, "total object count", w.totalGameObjectCount.ToString(), d.totalGameObjectCount.ToString());
        CheckField(lines, "root object count",  w.rootObjectCount.ToString(),      d.rootObjectCount.ToString());
        CheckField(lines, "unity version",      w.unityVersion,                   d.unityVersion);
        CheckField(lines, "head commit",        w.headCommitShort,                d.headCommitShort);
        CheckField(lines, "commit message",     w.headCommitMessage,              d.headCommitMessage);

        return lines;
    }

    private static void CheckField(List<string> lines, string label, string wVal, string dVal)
    {
        string w = wVal ?? "";
        string d = dVal ?? "";
        if (w != d)
            lines.Add($"- {label}: [{w}] -> [{d}]");
    }

    // -----------------------------
    // ROOT OBJECT COMPARE
    // -----------------------------

    private static List<string> CompareRootObjects(SceneSnapshot w, SceneSnapshot d)
    {
        var lines = new List<string>();
        if (w == null || d == null) return lines;

        var wNames = new HashSet<string>();
        var dNames = new HashSet<string>();

        if (w.roots != null) foreach (var r in w.roots) wNames.Add(r.name ?? "");
        if (d.roots != null) foreach (var r in d.roots) dNames.Add(r.name ?? "");

        foreach (var name in wNames)
            if (!dNames.Contains(name))
                lines.Add($"- REMOVED in debug: {name}");

        foreach (var name in dNames)
            if (!wNames.Contains(name))
                lines.Add($"- ADDED in debug:   {name}");

        return lines;
    }

    // -----------------------------
    // COMPONENT COMPARE
    // -----------------------------

    private static List<string> CompareComponents(SceneSnapshot w, SceneSnapshot d)
    {
        var lines = new List<string>();
        if (w == null || d == null) return lines;
        if (w.roots == null || d.roots == null) return lines;

        var wMap = new Dictionary<string, GameObjectData>();
        var dMap = new Dictionary<string, GameObjectData>();

        BuildObjectMap(w.roots, wMap);
        BuildObjectMap(d.roots, dMap);

        foreach (var kvp in wMap)
        {
            string path = kvp.Key;
            if (!dMap.ContainsKey(path)) continue;

            var wObj = kvp.Value;
            var dObj = dMap[path];

            if (wObj.activeSelf != dObj.activeSelf)
                lines.Add($"- [{path}] activeSelf: {wObj.activeSelf} -> {dObj.activeSelf}");

            if (wObj.activeInHierarchy != dObj.activeInHierarchy)
                lines.Add($"- [{path}] activeInHierarchy: {wObj.activeInHierarchy} -> {dObj.activeInHierarchy}");

            CompareTransform(lines, path, wObj.transform, dObj.transform);
            CompareComponentLists(lines, path, wObj.components, dObj.components);
        }

        return lines;
    }

    private static void BuildObjectMap(List<GameObjectData> objects, Dictionary<string, GameObjectData> map)
    {
        if (objects == null) return;
        foreach (var obj in objects)
        {
            if (obj == null) continue;
            string key = obj.path ?? obj.name ?? "";
            if (!map.ContainsKey(key))
                map[key] = obj;
            if (obj.children != null)
                BuildObjectMap(obj.children, map);
        }
    }

    private static void CompareTransform(List<string> lines, string path, TransformData w, TransformData d)
    {
        if (w == null || d == null) return;

        CompareVec3(lines, path, "localPosition", w.localPosition,      d.localPosition);
        CompareVec3(lines, path, "localRotation", w.localRotationEuler, d.localRotationEuler);
        CompareVec3(lines, path, "localScale",    w.localScale,         d.localScale);
    }

    private static void CompareVec3(List<string> lines, string path, string label, float[] w, float[] d)
    {
        if (w == null || d == null) return;
        if (w.Length < 3 || d.Length < 3) return;

        if (Math.Abs(w[0] - d[0]) > 0.0001f ||
            Math.Abs(w[1] - d[1]) > 0.0001f ||
            Math.Abs(w[2] - d[2]) > 0.0001f)
        {
            lines.Add($"- [{path}] {label}: ({w[0]:0.###},{w[1]:0.###},{w[2]:0.###}) -> ({d[0]:0.###},{d[1]:0.###},{d[2]:0.###})");
        }
    }

    private static void CompareComponentLists(
        List<string> lines,
        string path,
        List<ComponentData> wComps,
        List<ComponentData> dComps)
    {
        if (wComps == null || dComps == null) return;

        var wMap = new Dictionary<string, ComponentData>();
        var dMap = new Dictionary<string, ComponentData>();

        foreach (var c in wComps) if (c != null && !wMap.ContainsKey(c.type ?? "")) wMap[c.type ?? ""] = c;
        foreach (var c in dComps) if (c != null && !dMap.ContainsKey(c.type ?? "")) dMap[c.type ?? ""] = c;

        foreach (var kvp in wMap)
        {
            string type = kvp.Key;
            if (!dMap.ContainsKey(type)) continue;
            CompareFields(lines, path, type, kvp.Value.fields, dMap[type].fields);
        }
    }

    private static void CompareFields(
        List<string> lines,
        string path,
        string compType,
        List<FieldKV> wFields,
        List<FieldKV> dFields)
    {
        if (wFields == null || dFields == null) return;

        var wMap = new Dictionary<string, string>();
        var dMap = new Dictionary<string, string>();

        foreach (var f in wFields) if (f != null) wMap[f.name ?? ""] = f.value ?? "";
        foreach (var f in dFields) if (f != null) dMap[f.name ?? ""] = f.value ?? "";

        foreach (var kvp in wMap)
        {
            string fieldName = kvp.Key;
            string wVal = kvp.Value;
            string dVal;
            if (!dMap.TryGetValue(fieldName, out dVal)) continue;
            if (wVal != dVal)
            {
                string shortType = ShortTypeName(compType);
                lines.Add($"- [{path}] {shortType}.{fieldName}: {wVal} -> {dVal}");
            }
        }
    }

    private static string ShortTypeName(string fullType)
    {
        if (string.IsNullOrEmpty(fullType)) return "?";
        int dot = fullType.LastIndexOf('.');
        return dot >= 0 ? fullType.Substring(dot + 1) : fullType;
    }

    // -----------------------------
    // SECTION HELPER
    // -----------------------------

    private static void WriteSection(StringBuilder sb, string title, List<string> lines)
    {
        sb.AppendLine($"## {title}");
        if (lines == null || lines.Count == 0)
            sb.AppendLine("No changes.");
        else
            foreach (var line in lines)
                sb.AppendLine(line);
        sb.AppendLine();
    }

#endif
}
