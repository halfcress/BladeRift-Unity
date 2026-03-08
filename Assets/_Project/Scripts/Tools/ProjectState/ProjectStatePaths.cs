using System.IO;
using UnityEngine;

// -----------------------------
// PATHS
// -----------------------------

public static class ProjectStatePaths
{
    public static string DocsRoot      => Path.Combine(Application.dataPath, "_Project", "Docs");
    public static string StateRoot     => Path.Combine(DocsRoot, "State");
    public static string SnapshotsRoot => Path.Combine(DocsRoot, "Snapshots");
    public static string WorkingRoot   => Path.Combine(SnapshotsRoot, "Working");
    public static string DebugRoot     => Path.Combine(SnapshotsRoot, "Debug");
    public static string ArchiveRoot   => Path.Combine(SnapshotsRoot, "Archive");

    public static string ChatStatePath     => Path.Combine(StateRoot, "CHAT_STATE.md");
    public static string DebugJournalPath  => Path.Combine(StateRoot, "DEBUG_JOURNAL.md");
    public static string SnapshotIndexPath => Path.Combine(StateRoot, "SNAPSHOT_INDEX.md");
}
