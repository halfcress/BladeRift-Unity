using System;
using System.IO;
using System.Text;
using UnityEngine;

// -----------------------------
// SNAPSHOT INDEX
// -----------------------------

public static class ProjectStateIndex
{
    public static void EnsureSnapshotIndexExists()
    {
        if (!File.Exists(ProjectStatePaths.SnapshotIndexPath))
        {
            File.WriteAllText(ProjectStatePaths.SnapshotIndexPath,
@"# SNAPSHOT_INDEX

> Bu dosya mevcut working/debug snapshot dosyalarinin hizli indeksidir.

", Encoding.UTF8);
        }
    }

    public static void UpdateSnapshotIndex()
    {
        // Ensure state folder exists before writing
        Directory.CreateDirectory(ProjectStatePaths.StateRoot);

        var sb = new StringBuilder();
        sb.AppendLine("# SNAPSHOT_INDEX");
        sb.AppendLine();
        sb.AppendLine("> Bu dosya mevcut working/debug snapshot dosyalarinin hizli indeksidir.");
        sb.AppendLine();
        sb.AppendLine($"Guncelleme zamani: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        WriteSnapshotSection(sb, "Working",         ProjectStatePaths.WorkingRoot);
        WriteSnapshotSection(sb, "Debug",           ProjectStatePaths.DebugRoot);
        WriteSnapshotSection(sb, "Archive/Working", Path.Combine(ProjectStatePaths.ArchiveRoot, "Working"));
        WriteSnapshotSection(sb, "Archive/Debug",   Path.Combine(ProjectStatePaths.ArchiveRoot, "Debug"));

        File.WriteAllText(ProjectStatePaths.SnapshotIndexPath, sb.ToString(), Encoding.UTF8);
    }

    private static void WriteSnapshotSection(StringBuilder sb, string title, string folder)
    {
        sb.AppendLine($"## {title}");

        if (!Directory.Exists(folder))
        {
            sb.AppendLine("- none");
            sb.AppendLine();
            return;
        }

        var files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
        if (files.Length == 0)
        {
            sb.AppendLine("- none");
            sb.AppendLine();
            return;
        }

        Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));

        foreach (var file in files)
        {
            var fi = new FileInfo(file);
            sb.AppendLine($"- {fi.Name} | {fi.LastWriteTime:yyyy-MM-dd HH:mm:ss} | {(fi.Length / 1024f):0.0} KB");
        }

        sb.AppendLine();
    }
}
