using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// -----------------------------
// MAIN ORCHESTRATOR
// -----------------------------

public static class ProjectStateExporter
{
    public enum SnapshotKind { Working, Debug }

#if UNITY_EDITOR

    // -----------------------------
    // MENU — Snapshots
    // -----------------------------

    [MenuItem("Tools/BladeRift/Project State/Snapshots/Export WORKING Snapshot")]
    public static void ExportWorkingSnapshotMenu() => ExportSnapshot(SnapshotKind.Working);

    [MenuItem("Tools/BladeRift/Project State/Snapshots/Export DEBUG Snapshot")]
    public static void ExportDebugSnapshotMenu() => ExportSnapshot(SnapshotKind.Debug);

    [MenuItem("Tools/BladeRift/Project State/Snapshots/Cleanup Snapshots")]
    public static void CleanupSnapshotsMenu()
    {
        EnsureFolders();
        CleanupFolderKeepNewest(ProjectStatePaths.WorkingRoot, 3, Path.Combine(ProjectStatePaths.ArchiveRoot, "Working"));
        CleanupFolderKeepNewest(ProjectStatePaths.DebugRoot,   1, Path.Combine(ProjectStatePaths.ArchiveRoot, "Debug"));
        ProjectStateIndex.UpdateSnapshotIndex();
        AssetDatabase.Refresh();
        Debug.Log("Snapshot cleanup completed.");
    }

    // -----------------------------
    // MENU — Analysis
    // -----------------------------

    [MenuItem("Tools/BladeRift/Project State/Analysis/Compare Latest Working vs Debug")]
    public static void CompareLatestSnapshotsMenu() => ProjectStateCompare.CompareLatestSnapshots();

    [MenuItem("Tools/BladeRift/Project State/Analysis/Generate Snapshot Timeline")]
    public static void GenerateTimelineMenu() => ProjectStateTimeline.GenerateTimeline();

    // -----------------------------
    // MENU — Open
    // -----------------------------

    [MenuItem("Tools/BladeRift/Project State/Open/Open Milestone Log")]
    public static void OpenMilestoneLogMenu() => ProjectStateMilestone.OpenMilestoneLog();

    [MenuItem("Tools/BladeRift/Project State/Open/Open Docs Folder")]
    public static void OpenDocsFolderMenu()
    {
        EnsureFolders();
        EditorUtility.RevealInFinder(ProjectStatePaths.DocsRoot);
    }

    [MenuItem("Tools/BladeRift/Project State/Open/Open DEBUG_JOURNAL")]
    public static void OpenDebugJournalMenu()
    {
        EnsureFolders();
        ProjectStateJournal.EnsureDebugJournalExists();
        EditorUtility.RevealInFinder(ProjectStatePaths.DebugJournalPath);
    }

    // -----------------------------
    // MENU — Debug
    // -----------------------------

    [MenuItem("Tools/BladeRift/Project State/Debug/Print Scene Metrics")]
    public static void PrintSceneMetricsMenu() => ProjectStateSceneMetrics.PrintMetrics();

    [MenuItem("Tools/BladeRift/Project State/Debug/Print Todo Summary")]
    public static void PrintTodoSummaryMenu() => ProjectStateTodoSync.PrintTodoSummary();

    // -----------------------------
    // CORE EXPORT
    // -----------------------------

    public static void ExportSnapshot(SnapshotKind kind)
    {
        EnsureFolders();

        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid()) { Debug.LogError("Active scene is not valid."); return; }

        FullSnapshot full = new FullSnapshot();
        full.meta.snapshotKind        = kind.ToString().ToUpperInvariant();
        full.meta.exportedAtLocalTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        full.meta.unityVersion        = Application.unityVersion;
        full.meta.platform            = Application.platform.ToString();
        full.meta.activeSceneName     = scene.name;
        full.meta.note                = "BladeRift project state snapshot.";
        full.scene.sceneName          = scene.name;
        full.scene.scenePath          = scene.path;

        var roots = scene.GetRootGameObjects();
        full.meta.rootObjectCount      = roots.Length;
        full.meta.totalGameObjectCount = ProjectStateSerializer.CountAllSceneObjects(scene);
        full.meta.headCommitShort      = ProjectStateGit.SafeGit("rev-parse --short HEAD");
        full.meta.headCommitFull       = ProjectStateGit.SafeGit("rev-parse HEAD");
        full.meta.headCommitMessage    = ProjectStateGit.SafeGit("log -1 --pretty=%s");

        foreach (var root in roots)
            full.scene.roots.Add(ProjectStateSerializer.SerializeGameObjectRecursive(root, root.name));

        ProjectStateSerializer.FillProjectSnapshot(full.project);
        ProjectStateSerializer.FillCodeSnapshot(full.code);

        string targetFolder  = kind == SnapshotKind.Working ? ProjectStatePaths.WorkingRoot : ProjectStatePaths.DebugRoot;
        string safeSceneName = string.IsNullOrWhiteSpace(scene.name) ? "UntitledScene" : scene.name;
        string fileName      = $"BladeRift_{kind.ToString().ToUpperInvariant()}_{safeSceneName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string fullPath      = Path.Combine(targetFolder, fileName);

        var settings = new JsonSerializerSettings { Formatting = Formatting.Indented, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        File.WriteAllText(fullPath, JsonConvert.SerializeObject(full, settings), Encoding.UTF8);

        // Otomatik guncellenenler
        ProjectStateIndex.UpdateSnapshotIndex();
        ProjectStateChatStateWriter.UpdateAutoBlock(full);
        ProjectStateReadmeUpdater.UpdateReadme(full);
        ProjectStateTimeline.UpdateTimeline(full);

        AssetDatabase.Refresh();
        Debug.Log($"{kind} snapshot exported: {fullPath}");
        EditorUtility.RevealInFinder(fullPath);
    }

    // -----------------------------
    // FOLDER HELPERS
    // -----------------------------

    public static void EnsureFolders()
    {
        Directory.CreateDirectory(ProjectStatePaths.DocsRoot);
        Directory.CreateDirectory(ProjectStatePaths.StateRoot);
        Directory.CreateDirectory(ProjectStatePaths.SnapshotsRoot);
        Directory.CreateDirectory(ProjectStatePaths.WorkingRoot);
        Directory.CreateDirectory(ProjectStatePaths.DebugRoot);
        Directory.CreateDirectory(ProjectStatePaths.ArchiveRoot);

        if (!File.Exists(ProjectStatePaths.ChatStatePath))
            File.WriteAllText(ProjectStatePaths.ChatStatePath, "# CHAT_STATE\n", Encoding.UTF8);

        ProjectStateJournal.EnsureDebugJournalExists();
        ProjectStateIndex.EnsureSnapshotIndexExists();
    }

    private static void CleanupFolderKeepNewest(string sourceFolder, int keepCount, string archiveFolder)
    {
        Directory.CreateDirectory(archiveFolder);

        var files = Directory.GetFiles(sourceFolder, "*.json", SearchOption.TopDirectoryOnly);
        Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));

        for (int i = keepCount; i < files.Length; i++)
        {
            string src = files[i];
            string dst = Path.Combine(archiveFolder, Path.GetFileName(src));
            if (File.Exists(dst)) dst = Path.Combine(archiveFolder, $"{Path.GetFileNameWithoutExtension(src)}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            File.Move(src, dst);
        }
    }

#endif
}
