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
// SNAPSHOT COMPARE (v4)
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
        string debugFile = ProjectStateSerializer.GetLatestSnapshotFile(ProjectStatePaths.DebugRoot);

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
        FullSnapshot debug = LoadSnapshot(debugFile);

        if (working == null || debug == null)
        {
            Debug.LogError("[Compare] Failed to load one or both snapshots.");
            return;
        }

        string report = BuildCompareReport(
            working, debug,
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
        // Flatten both hierarchies into path-keyed dictionaries
        var wMap = new Dictionary<string, GameObjectData>();
        var dMap = new Dictionary<string, GameObjectData>();

        if (working.scene?.roots != null) BuildObjectMap(working.scene.roots, wMap);
        if (debug.scene?.roots != null) BuildObjectMap(debug.scene.roots, dMap);

        // Classify objects
        var added = new List<string>();
        var removed = new List<string>();
        var common = new List<string>();

        foreach (var key in dMap.Keys)
            if (!wMap.ContainsKey(key)) added.Add(key);

        foreach (var key in wMap.Keys)
            if (!dMap.ContainsKey(key)) removed.Add(key);
            else common.Add(key);

        // Diff common objects
        var stateChanges = new List<string>(); // activeSelf / activeInHierarchy
        var transformChanges = new List<string>(); // localPosition / Rotation / Scale
        var componentChanges = new List<string>(); // component fields
        int fieldChangeCount = 0;                  // exact changed-field count for summary

        foreach (var path in common)
        {
            var w = wMap[path];
            var d = dMap[path];

            CollectStateChanges(stateChanges, path, w, d);
            CollectTransformChanges(transformChanges, path, w.transform, d.transform);
            fieldChangeCount += CollectComponentChanges(componentChanges, path, w.components, d.components);
        }

        // Write report
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

        WriteMetaSection(sb, working.meta, debug.meta);

        WriteSeparator(sb);
        sb.AppendLine("## OBJECTS ADDED");
        if (added.Count == 0)
            sb.AppendLine("None.");
        else
            foreach (var name in added)
                sb.AppendLine($"- {name}");

        WriteSeparator(sb);
        sb.AppendLine("## OBJECTS REMOVED");
        if (removed.Count == 0)
            sb.AppendLine("None.");
        else
            foreach (var name in removed)
                sb.AppendLine($"- {name}");

        WriteSeparator(sb);
        sb.AppendLine("## OBJECT STATE CHANGES");
        if (stateChanges.Count == 0)
            sb.AppendLine("No changes.");
        else
            foreach (var line in stateChanges)
                sb.AppendLine(line);

        WriteSeparator(sb);
        sb.AppendLine("## TRANSFORM CHANGES");
        if (transformChanges.Count == 0)
            sb.AppendLine("No changes.");
        else
            foreach (var line in transformChanges)
                sb.AppendLine(line);

        WriteSeparator(sb);
        sb.AppendLine("## COMPONENT FIELD CHANGES");
        if (componentChanges.Count == 0)
            sb.AppendLine("No changes.");
        else
            foreach (var line in componentChanges)
                sb.AppendLine(line);

        WriteSeparator(sb);
        sb.AppendLine("## SUMMARY");
        sb.AppendLine($"Objects added:          {added.Count}");
        sb.AppendLine($"Objects removed:        {removed.Count}");
        sb.AppendLine($"Object state changes:   {CountObjects(stateChanges)}");
        sb.AppendLine($"Transform changes:      {CountObjects(transformChanges)}");
        sb.AppendLine($"Component field changes: {fieldChangeCount}");
        sb.AppendLine();

        return sb.ToString();
    }

    // -----------------------------
    // META SECTION
    // -----------------------------

    private static void WriteMetaSection(StringBuilder sb, Meta w, Meta d)
    {
        if (w == null || d == null) return;

        var lines = new List<string>();
        CheckMeta(lines, "scene name", w.activeSceneName, d.activeSceneName);
        CheckMeta(lines, "unity version", w.unityVersion, d.unityVersion);
        CheckMeta(lines, "root object count", w.rootObjectCount.ToString(), d.rootObjectCount.ToString());
        CheckMeta(lines, "total object count", w.totalGameObjectCount.ToString(), d.totalGameObjectCount.ToString());
        CheckMeta(lines, "head commit", w.headCommitShort, d.headCommitShort);
        CheckMeta(lines, "commit message", w.headCommitMessage, d.headCommitMessage);

        if (lines.Count == 0) return;

        WriteSeparator(sb);
        sb.AppendLine("## META CHANGES");
        foreach (var line in lines)
            sb.AppendLine(line);
    }

    private static void CheckMeta(List<string> lines, string label, string wVal, string dVal)
    {
        string w = wVal ?? "";
        string d = dVal ?? "";
        if (w != d)
            lines.Add($"- {label}: [{w}] -> [{d}]");
    }

    // -----------------------------
    // STATE CHANGES
    // -----------------------------

    private static void CollectStateChanges(List<string> lines, string path, GameObjectData w, GameObjectData d)
    {
        var changes = new List<string>();

        if (w.activeSelf != d.activeSelf)
            changes.Add($"  activeSelf: {w.activeSelf} -> {d.activeSelf}");

        if (w.activeInHierarchy != d.activeInHierarchy)
            changes.Add($"  activeInHierarchy: {w.activeInHierarchy} -> {d.activeInHierarchy}");

        if (changes.Count > 0)
        {
            lines.Add($"Object: {path}");
            lines.AddRange(changes);
            lines.Add("");
        }
    }

    // -----------------------------
    // TRANSFORM CHANGES
    // -----------------------------

    private static void CollectTransformChanges(List<string> lines, string path, TransformData w, TransformData d)
    {
        if (w == null || d == null) return;

        var changes = new List<string>();

        DiffVec3(changes, "localPosition", w.localPosition, d.localPosition);
        DiffVec3(changes, "localRotationEuler", w.localRotationEuler, d.localRotationEuler);
        DiffVec3(changes, "localScale", w.localScale, d.localScale);

        if (changes.Count > 0)
        {
            lines.Add($"Object: {path}");
            lines.AddRange(changes);
            lines.Add("");
        }
    }

    private static void DiffVec3(List<string> changes, string label, float[] w, float[] d)
    {
        if (w == null || d == null) return;
        if (w.Length < 3 || d.Length < 3) return;

        if (Math.Abs(w[0] - d[0]) > 0.0001f ||
            Math.Abs(w[1] - d[1]) > 0.0001f ||
            Math.Abs(w[2] - d[2]) > 0.0001f)
        {
            changes.Add($"  {label}");
            changes.Add($"  ({w[0]:0.###},{w[1]:0.###},{w[2]:0.###}) -> ({d[0]:0.###},{d[1]:0.###},{d[2]:0.###})");
        }
    }

    // -----------------------------
    // COMPONENT CHANGES
    // -----------------------------

    private static int CollectComponentChanges(
        List<string> lines,
        string path,
        List<ComponentData> wComps,
        List<ComponentData> dComps)
    {
        int fieldCount = 0;
        if (wComps == null || dComps == null) return fieldCount;

        // Build type-keyed lookups (first occurrence wins)
        var wMap = new Dictionary<string, ComponentData>();
        var dMap = new Dictionary<string, ComponentData>();

        foreach (var c in wComps) if (c != null && c.type != null && !wMap.ContainsKey(c.type)) wMap[c.type] = c;
        foreach (var c in dComps) if (c != null && c.type != null && !dMap.ContainsKey(c.type)) dMap[c.type] = c;

        foreach (var kvp in wMap)
        {
            string type = kvp.Key;
            if (!dMap.ContainsKey(type)) continue;

            var fieldChanges = DiffFields(kvp.Value.fields, dMap[type].fields);
            if (fieldChanges.Count == 0) continue;

            // fieldChanges contains pairs: [fieldName, val->val] so divide by 2
            fieldCount += fieldChanges.Count / 2;

            lines.Add($"Object: {path}");
            lines.Add($"  Component: {ShortTypeName(type)}");
            foreach (var fc in fieldChanges)
                lines.Add(fc);
            lines.Add("");
        }

        return fieldCount;
    }

    private static List<string> DiffFields(List<FieldKV> wFields, List<FieldKV> dFields)
    {
        var result = new List<string>();
        if (wFields == null || dFields == null) return result;

        var wMap = new Dictionary<string, string>();
        var dMap = new Dictionary<string, string>();

        foreach (var f in wFields) if (f?.name != null) wMap[f.name] = f.value ?? "";
        foreach (var f in dFields) if (f?.name != null) dMap[f.name] = f.value ?? "";

        foreach (var kvp in wMap)
        {
            string dVal;
            if (!dMap.TryGetValue(kvp.Key, out dVal)) continue;
            if (kvp.Value != dVal)
            {
                result.Add($"    {kvp.Key}");
                result.Add($"    {kvp.Value} -> {dVal}");
            }
        }

        return result;
    }

    // -----------------------------
    // HELPERS
    // -----------------------------

    private static void BuildObjectMap(List<GameObjectData> objects, Dictionary<string, GameObjectData> map)
    {
        if (objects == null) return;
        foreach (var obj in objects)
        {
            if (obj == null) continue;
            string key = obj.path ?? obj.name ?? "";
            if (!string.IsNullOrEmpty(key) && !map.ContainsKey(key))
                map[key] = obj;
            if (obj.children != null)
                BuildObjectMap(obj.children, map);
        }
    }

    private static string ShortTypeName(string fullType)
    {
        if (string.IsNullOrEmpty(fullType)) return "?";
        int dot = fullType.LastIndexOf('.');
        return dot >= 0 ? fullType.Substring(dot + 1) : fullType;
    }

    private static void WriteSeparator(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
    }

    // Counts how many "Object: ..." headers are in a section's lines
    private static int CountObjects(List<string> lines)
    {
        int count = 0;
        foreach (var line in lines)
            if (line.StartsWith("Object:")) count++;
        return count;
    }

#endif
}