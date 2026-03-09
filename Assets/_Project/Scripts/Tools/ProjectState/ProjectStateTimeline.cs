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
// SNAPSHOT TIMELINE (v1)
// Tum snapshot'larin kronolojik listesi
// Hangi commit'te ne degisti gorunur
// -----------------------------

public static class ProjectStateTimeline
{
#if UNITY_EDITOR

    [MenuItem("Tools/BladeRift/Project State/Generate Snapshot Timeline")]
    public static void GenerateTimeline()
    {
        var entries = CollectAllSnapshots();

        if (entries.Count == 0)
        {
            Debug.LogWarning("[Timeline] No snapshots found.");
            return;
        }

        string report = BuildTimelineReport(entries);

        Directory.CreateDirectory(ProjectStatePaths.StateRoot);
        string outputPath = Path.Combine(ProjectStatePaths.StateRoot, "SNAPSHOT_TIMELINE.md");
        File.WriteAllText(outputPath, report, Encoding.UTF8);

        AssetDatabase.Refresh();
        Debug.Log($"[Timeline] Report saved: {outputPath}");
        EditorUtility.RevealInFinder(outputPath);
    }

    // -----------------------------
    // COLLECT
    // -----------------------------

    private class TimelineEntry
    {
        public string fileName;
        public string kind;
        public string date;
        public string sceneName;
        public string commitShort;
        public string commitMessage;
        public int rootObjectCount;
        public int totalObjectCount;
        public List<string> csFiles = new List<string>();
        public Dictionary<string, string> csHashes = new Dictionary<string, string>();
    }

    private static List<TimelineEntry> CollectAllSnapshots()
    {
        var entries = new List<TimelineEntry>();

        var folders = new[]
        {
            ProjectStatePaths.WorkingRoot,
            ProjectStatePaths.DebugRoot,
            Path.Combine(ProjectStatePaths.ArchiveRoot, "Working"),
            Path.Combine(ProjectStatePaths.ArchiveRoot, "Debug"),
            Path.Combine(ProjectStatePaths.ArchiveRoot, "Auto"),
        };

        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder)) continue;

            var files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var entry = LoadEntry(file);
                if (entry != null) entries.Add(entry);
            }
        }

        // Tarihe gore sirala (eskiden yeniye)
        entries.Sort((a, b) => string.Compare(a.date, b.date, StringComparison.Ordinal));

        return entries;
    }

    private static TimelineEntry LoadEntry(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath, Encoding.UTF8);
            var snapshot = JsonConvert.DeserializeObject<FullSnapshot>(json);
            if (snapshot == null) return null;

            var entry = new TimelineEntry
            {
                fileName       = Path.GetFileName(filePath),
                kind           = snapshot.meta.snapshotKind ?? "?",
                date           = snapshot.meta.exportedAtLocalTime ?? "",
                sceneName      = snapshot.meta.activeSceneName ?? "",
                commitShort    = snapshot.meta.headCommitShort ?? "",
                commitMessage  = snapshot.meta.headCommitMessage ?? "",
                rootObjectCount  = snapshot.meta.rootObjectCount,
                totalObjectCount = snapshot.meta.totalGameObjectCount,
            };

            if (snapshot.code?.csFiles != null)
            {
                foreach (var f in snapshot.code.csFiles)
                {
                    if (f?.relativePath == null) continue;
                    entry.csFiles.Add(f.relativePath);
                    entry.csHashes[f.relativePath] = f.sha256 ?? "";
                }
            }

            return entry;
        }
        catch
        {
            return null;
        }
    }

    // -----------------------------
    // REPORT BUILDER
    // -----------------------------

    private static string BuildTimelineReport(List<TimelineEntry> entries)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# SNAPSHOT_TIMELINE");
        sb.AppendLine();
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total snapshots: {entries.Count}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        for (int i = 0; i < entries.Count; i++)
        {
            var cur  = entries[i];
            var prev = i > 0 ? entries[i - 1] : null;

            sb.AppendLine($"## [{cur.kind}] {cur.date}");
            sb.AppendLine($"- File: `{cur.fileName}`");
            sb.AppendLine($"- Scene: {cur.sceneName}");
            sb.AppendLine($"- Commit: `{cur.commitShort}` — \"{cur.commitMessage}\"");
            sb.AppendLine($"- Objects: {cur.rootObjectCount} root / {cur.totalObjectCount} total");

            // Kod degisiklikleri (onceki snapshot'a gore)
            if (prev != null)
            {
                var added    = new List<string>();
                var removed  = new List<string>();
                var modified = new List<string>();

                // Added
                foreach (var f in cur.csFiles)
                    if (!prev.csHashes.ContainsKey(f))
                        added.Add(ShortPath(f));

                // Removed
                foreach (var f in prev.csFiles)
                    if (!cur.csHashes.ContainsKey(f))
                        removed.Add(ShortPath(f));

                // Modified
                foreach (var kvp in cur.csHashes)
                {
                    if (!prev.csHashes.ContainsKey(kvp.Key)) continue;
                    if (prev.csHashes[kvp.Key] != kvp.Value)
                        modified.Add(ShortPath(kvp.Key));
                }

                if (added.Count > 0 || removed.Count > 0 || modified.Count > 0)
                {
                    sb.AppendLine("- Code changes:");
                    foreach (var f in added)    sb.AppendLine($"  - `+ {f}`");
                    foreach (var f in removed)  sb.AppendLine($"  - `- {f}`");
                    foreach (var f in modified) sb.AppendLine($"  - `~ {f}`");
                }
                else
                {
                    sb.AppendLine("- Code changes: none");
                }
            }
            else
            {
                sb.AppendLine($"- Code files: {cur.csFiles.Count}");
            }

            sb.AppendLine();
        }

        // Ozet
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## SUMMARY");

        // Unique commitler
        var commits = new HashSet<string>();
        foreach (var e in entries)
            if (!string.IsNullOrEmpty(e.commitShort))
                commits.Add(e.commitShort);

        sb.AppendLine($"- Total snapshots: {entries.Count}");
        sb.AppendLine($"- Unique commits: {commits.Count}");

        // Kind dagilimi
        int working = 0, debug = 0, auto = 0;
        foreach (var e in entries)
        {
            if (e.kind == "WORKING") working++;
            else if (e.kind == "DEBUG") debug++;
            else if (e.kind == "AUTO") auto++;
        }
        sb.AppendLine($"- WORKING: {working} | DEBUG: {debug} | AUTO: {auto}");

        return sb.ToString();
    }

    private static string ShortPath(string path)
    {
        // Assets\_Project\Scripts\Combat\CombatDirector.cs -> Combat/CombatDirector.cs
        int idx = path.IndexOf("Scripts", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
            return path.Substring(idx).Replace('\\', '/');
        return path.Replace('\\', '/');
    }

#endif
}
