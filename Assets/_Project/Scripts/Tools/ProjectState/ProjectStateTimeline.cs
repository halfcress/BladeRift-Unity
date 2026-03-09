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
// SNAPSHOT TIMELINE
// Her snapshot'tan sonra otomatik guncellenir
// Manuel: Tools > Analysis > Generate Snapshot Timeline (full rebuild)
// -----------------------------

public static class ProjectStateTimeline
{
    private static string TimelinePath =>
        Path.Combine(ProjectStatePaths.StateRoot, "SNAPSHOT_TIMELINE.md");

#if UNITY_EDITOR

    // Manuel full rebuild — tum snapshot dosyalarini tarar
    // MenuItem Exporter'da tanimli
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
        File.WriteAllText(TimelinePath, report, Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"[Timeline] Full rebuild complete: {entries.Count} snapshots.");
        EditorUtility.RevealInFinder(TimelinePath);
    }

    // Her snapshot'tan sonra otomatik cagirilir
    public static void UpdateTimeline(FullSnapshot latest)
    {
        try
        {
            // Onceki timeline varsa oku, yoksa full rebuild yap
            if (!File.Exists(TimelinePath))
            {
                var all = CollectAllSnapshots();
                string report = BuildTimelineReport(all);
                Directory.CreateDirectory(ProjectStatePaths.StateRoot);
                File.WriteAllText(TimelinePath, report, Encoding.UTF8);
                return;
            }

            // Hicbir sey degismediyse yazma
            var prev = LoadPreviousEntry();
            if (prev != null && HasNothingChanged(latest, prev))
            {
                Debug.Log("[Timeline] No changes, skipping.");
                return;
            }

            string newEntry = BuildSingleEntry(latest, prev);

            // Dosyanin sonuna ekle (SUMMARY'den once)
            string existing = File.ReadAllText(TimelinePath, Encoding.UTF8);
            int summaryIdx  = existing.IndexOf("\n---\n\n## SUMMARY", StringComparison.Ordinal);

            string updated;
            if (summaryIdx >= 0)
                updated = existing.Substring(0, summaryIdx) + "\n" + newEntry + existing.Substring(summaryIdx);
            else
                updated = existing.TrimEnd() + "\n\n" + newEntry;

            // SUMMARY'yi guncelle
            updated = RebuildSummarySection(updated);

            File.WriteAllText(TimelinePath, updated, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Timeline] UpdateTimeline failed: {e.Message}");
        }
    }

    // -----------------------------
    // COLLECT (full rebuild icin)
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
            foreach (var file in Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly))
            {
                var entry = LoadEntryFromFile(file);
                if (entry != null) entries.Add(entry);
            }
        }

        entries.Sort((a, b) => string.Compare(a.date, b.date, StringComparison.Ordinal));
        return entries;
    }

    private static TimelineEntry LoadEntryFromFile(string filePath)
    {
        try
        {
            string json     = File.ReadAllText(filePath, Encoding.UTF8);
            var snapshot    = JsonConvert.DeserializeObject<FullSnapshot>(json);
            if (snapshot == null) return null;

            var entry = new TimelineEntry
            {
                fileName         = Path.GetFileName(filePath),
                kind             = snapshot.meta.snapshotKind ?? "?",
                date             = snapshot.meta.exportedAtLocalTime ?? "",
                sceneName        = snapshot.meta.activeSceneName ?? "",
                commitShort      = snapshot.meta.headCommitShort ?? "",
                commitMessage    = snapshot.meta.headCommitMessage ?? "",
                rootObjectCount  = snapshot.meta.rootObjectCount,
                totalObjectCount = snapshot.meta.totalGameObjectCount,
            };

            if (snapshot.code?.csFiles != null)
                foreach (var f in snapshot.code.csFiles)
                {
                    if (f?.relativePath == null) continue;
                    entry.csFiles.Add(f.relativePath);
                    entry.csHashes[f.relativePath] = f.sha256 ?? "";
                }

            return entry;
        }
        catch { return null; }
    }

    // Onceki snapshot ile karsilastir — commit ayni ve hicbir cs hash degismediyse true
    private static bool HasNothingChanged(FullSnapshot latest, TimelineEntry prev)
    {
        // Commit farkli ise degisiklik var
        string latestCommit = latest.meta.headCommitShort ?? "";
        if (latestCommit != prev.commitShort) return false;

        // Kod dosyasi sayisi farkli ise degisiklik var
        if (latest.code?.csFiles == null) return true;
        if (latest.code.csFiles.Count != prev.csHashes.Count) return false;

        // Her hash'i karsilastir
        foreach (var f in latest.code.csFiles)
        {
            if (f?.relativePath == null) continue;
            if (!prev.csHashes.TryGetValue(f.relativePath, out string oldHash)) return false;
            if (oldHash != (f.sha256 ?? "")) return false;
        }

        return true;
    }

    // Mevcut timeline'daki en son entry'den hash bilgisi al
    private static TimelineEntry LoadPreviousEntry()
    {
        // En son DEBUG veya WORKING snapshot dosyasini bul
        var candidates = new List<string>();
        foreach (var folder in new[] { ProjectStatePaths.WorkingRoot, ProjectStatePaths.DebugRoot })
        {
            if (!Directory.Exists(folder)) continue;
            candidates.AddRange(Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly));
        }

        if (candidates.Count == 0) return null;
        candidates.Sort((a, b) => File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));

        // En yeni 2 dosyadan eskisini al (yenisi zaten mevcut snapshot)
        string prevFile = candidates.Count >= 2 ? candidates[1] : candidates[0];
        return LoadEntryFromFile(prevFile);
    }

    // -----------------------------
    // REPORT BUILDER
    // -----------------------------

    private static string BuildTimelineReport(List<TimelineEntry> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# SNAPSHOT_TIMELINE");
        sb.AppendLine();
        sb.AppendLine($"Total snapshots: {entries.Count}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        for (int i = 0; i < entries.Count; i++)
            sb.Append(BuildSingleEntry(entries[i], i > 0 ? entries[i - 1] : null));

        sb.Append(BuildSummaryBlock(entries));
        return sb.ToString();
    }

    private static string BuildSingleEntry(FullSnapshot snapshot, TimelineEntry prev)
    {
        var cur = new TimelineEntry
        {
            fileName         = "",
            kind             = snapshot.meta.snapshotKind ?? "?",
            date             = snapshot.meta.exportedAtLocalTime ?? "",
            sceneName        = snapshot.meta.activeSceneName ?? "",
            commitShort      = snapshot.meta.headCommitShort ?? "",
            commitMessage    = snapshot.meta.headCommitMessage ?? "",
            rootObjectCount  = snapshot.meta.rootObjectCount,
            totalObjectCount = snapshot.meta.totalGameObjectCount,
        };

        if (snapshot.code?.csFiles != null)
            foreach (var f in snapshot.code.csFiles)
            {
                if (f?.relativePath == null) continue;
                cur.csFiles.Add(f.relativePath);
                cur.csHashes[f.relativePath] = f.sha256 ?? "";
            }

        return BuildSingleEntry(cur, prev);
    }

    private static string BuildSingleEntry(TimelineEntry cur, TimelineEntry prev)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## [{cur.kind}] {cur.date}");
        sb.AppendLine($"- Scene: {cur.sceneName}");
        sb.AppendLine($"- Commit: `{cur.commitShort}` — \"{cur.commitMessage}\"");
        sb.AppendLine($"- Objects: {cur.rootObjectCount} root / {cur.totalObjectCount} total");

        if (prev != null)
        {
            var added    = new List<string>();
            var removed  = new List<string>();
            var modified = new List<string>();

            foreach (var f in cur.csFiles)
                if (!prev.csHashes.ContainsKey(f)) added.Add(ShortPath(f));

            foreach (var f in prev.csFiles)
                if (!cur.csHashes.ContainsKey(f)) removed.Add(ShortPath(f));

            foreach (var kvp in cur.csHashes)
            {
                if (!prev.csHashes.ContainsKey(kvp.Key)) continue;
                if (prev.csHashes[kvp.Key] != kvp.Value) modified.Add(ShortPath(kvp.Key));
            }

            if (added.Count > 0 || removed.Count > 0 || modified.Count > 0)
            {
                sb.AppendLine("- Code changes:");
                foreach (var f in added)    sb.AppendLine($"  - `+ {f}`");
                foreach (var f in removed)  sb.AppendLine($"  - `- {f}`");
                foreach (var f in modified) sb.AppendLine($"  - `~ {f}`");
            }
            else sb.AppendLine("- Code changes: none");
        }
        else sb.AppendLine($"- Code files: {cur.csFiles.Count}");

        sb.AppendLine();
        return sb.ToString();
    }

    private static string BuildSummaryBlock(List<TimelineEntry> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## SUMMARY");

        var commits = new HashSet<string>();
        int working = 0, debug = 0, auto = 0;
        foreach (var e in entries)
        {
            if (!string.IsNullOrEmpty(e.commitShort)) commits.Add(e.commitShort);
            if (e.kind == "WORKING") working++;
            else if (e.kind == "DEBUG") debug++;
            else if (e.kind == "AUTO") auto++;
        }

        sb.AppendLine($"- Total snapshots: {entries.Count}");
        sb.AppendLine($"- Unique commits: {commits.Count}");
        sb.AppendLine($"- WORKING: {working} | DEBUG: {debug} | AUTO: {auto}");
        return sb.ToString();
    }

    private static string RebuildSummarySection(string content)
    {
        int idx = content.IndexOf("\n---\n\n## SUMMARY", StringComparison.Ordinal);
        if (idx < 0) return content;

        // Tum entry'leri say
        int count = 0;
        int pos = 0;
        while ((pos = content.IndexOf("\n## [", pos + 1, StringComparison.Ordinal)) >= 0) count++;

        string prefix = content.Substring(0, idx);
        return prefix + $"\n---\n\n## SUMMARY\n- Total snapshots: {count}\n- (Run Generate Snapshot Timeline for full stats)\n";
    }

    private static string ShortPath(string path)
    {
        int idx = path.IndexOf("Scripts", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0) return path.Substring(idx).Replace('\\', '/');
        return path.Replace('\\', '/');
    }

#endif
}
