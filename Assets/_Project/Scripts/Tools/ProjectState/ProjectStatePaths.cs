using System.IO;
using UnityEngine;

// -----------------------------
// PATHS — v6
// -----------------------------

public static class ProjectStatePaths
{
    public static string DocsRoot => Path.Combine(Application.dataPath, "_Project", "Docs");
    public static string StateRoot => Path.Combine(DocsRoot, "State");
    public static string DesignRoot => Path.Combine(DocsRoot, "Design");
    public static string SnapshotsRoot => Path.Combine(DocsRoot, "Snapshots");
    public static string WorkingRoot => Path.Combine(SnapshotsRoot, "Working");
    public static string DebugRoot => Path.Combine(SnapshotsRoot, "Debug");
    public static string ArchiveRoot => Path.Combine(SnapshotsRoot, "Archive");
    public static string MiniWorkingRoot => Path.Combine(SnapshotsRoot, "MiniWorking");
    public static string MiniDebugRoot => Path.Combine(SnapshotsRoot, "MiniDebug");

    // State dosyalari
    public static string ChatStatePath => Path.Combine(StateRoot, "CHAT_STATE.md");
    public static string DebugJournalPath => Path.Combine(StateRoot, "DEBUG_JOURNAL.md");
    public static string SnapshotIndexPath => Path.Combine(StateRoot, "SNAPSHOT_INDEX.md");
    public static string MilestoneLogPath => Path.Combine(StateRoot, "MILESTONE_LOG.md");
    public static string TodoPath => FindTodoPath();

    // Design dosyalari
    public static string GameConceptPath => Path.Combine(DesignRoot, "GAME_CONCEPT_TR.md");
    public static string ArchitecturePath => Path.Combine(DocsRoot, "Architecture", "ARCHITECTURE_TR.md");

    // Repo root
    public static string RepoRoot => Directory.GetParent(Application.dataPath).FullName;

    private static string FindTodoPath()
    {
        var candidates = new[]
        {
            Path.Combine(StateRoot,  "TODO_TR.md"),
            Path.Combine(DocsRoot,   "TODO_TR.md"),
            Path.Combine(DesignRoot, "TODO_TR.md"),
            Path.Combine(RepoRoot,   "TODO_TR.md"),
        };
        foreach (var p in candidates)
            if (File.Exists(p)) return p;
        return Path.Combine(StateRoot, "TODO_TR.md");
    }
}