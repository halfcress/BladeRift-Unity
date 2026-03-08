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
    private enum SnapshotKind
    {
        Working,
        Debug
    }

#if UNITY_EDITOR

    // -----------------------------
    // MENU ITEMS
    // -----------------------------

    [MenuItem("Tools/BladeRift/Project State/Export WORKING Snapshot")]
    public static void ExportWorkingSnapshotMenu()
    {
        ExportSnapshot(SnapshotKind.Working);
    }

    [MenuItem("Tools/BladeRift/Project State/Export DEBUG Snapshot")]
    public static void ExportDebugSnapshotMenu()
    {
        ExportSnapshot(SnapshotKind.Debug);
    }

    [MenuItem("Tools/BladeRift/Project State/Cleanup Snapshots")]
    public static void CleanupSnapshotsMenu()
    {
        EnsureFolders();

        CleanupFolderKeepNewest(ProjectStatePaths.WorkingRoot, 3, Path.Combine(ProjectStatePaths.ArchiveRoot, "Working"));
        CleanupFolderKeepNewest(ProjectStatePaths.DebugRoot,   1, Path.Combine(ProjectStatePaths.ArchiveRoot, "Debug"));

        AssetDatabase.Refresh();
        Debug.Log("Snapshot cleanup completed.");

        ProjectStateIndex.UpdateSnapshotIndex();
    }

    [MenuItem("Tools/BladeRift/Project State/Append DEBUG_JOURNAL Entry")]
    public static void AppendDebugJournalEntryMenu()
    {
        EnsureFolders();
        ProjectStateJournal.EnsureDebugJournalExists();
        ProjectStateJournal.AppendManualEntry();

        AssetDatabase.Refresh();
        Debug.Log($"DEBUG_JOURNAL entry appended: {ProjectStatePaths.DebugJournalPath}");
        EditorUtility.RevealInFinder(ProjectStatePaths.DebugJournalPath);
    }

    [MenuItem("Tools/BladeRift/Project State/Update Snapshot Index")]
    public static void UpdateSnapshotIndexMenu()
    {
        ProjectStateIndex.UpdateSnapshotIndex();
        AssetDatabase.Refresh();
        Debug.Log("Snapshot index updated.");
    }

    [MenuItem("Tools/BladeRift/Project State/Compare Latest Working vs Debug")]
    public static void CompareLatestSnapshotsMenu()
    {
        ProjectStateCompare.CompareLatestSnapshots();
    }

    [MenuItem("Tools/BladeRift/Project State/Open Docs Folder")]
    public static void OpenDocsFolderMenu()
    {
        EnsureFolders();
        EditorUtility.RevealInFinder(ProjectStatePaths.DocsRoot);
    }

    // -----------------------------
    // CORE EXPORT
    // -----------------------------

    private static void ExportSnapshot(SnapshotKind kind)
    {
        EnsureFolders();

        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("Active scene is not valid.");
            return;
        }

        FullSnapshot full = new FullSnapshot();
        full.meta.snapshotKind       = kind.ToString().ToUpperInvariant();
        full.meta.exportedAtLocalTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        full.meta.unityVersion       = Application.unityVersion;
        full.meta.platform           = Application.platform.ToString();
        full.meta.activeSceneName    = scene.name;
        full.meta.note               = "BladeRift project state snapshot.";

        full.scene.sceneName = scene.name;
        full.scene.scenePath = scene.path;

        var roots = scene.GetRootGameObjects();

        full.meta.rootObjectCount      = roots.Length;
        full.meta.totalGameObjectCount = ProjectStateSerializer.CountAllSceneObjects(scene);

        full.meta.headCommitShort   = ProjectStateGit.SafeGit("rev-parse --short HEAD");
        full.meta.headCommitFull    = ProjectStateGit.SafeGit("rev-parse HEAD");
        full.meta.headCommitMessage = ProjectStateGit.SafeGit("log -1 --pretty=%s");

        foreach (var root in roots)
        {
            full.scene.roots.Add(ProjectStateSerializer.SerializeGameObjectRecursive(root, root.name));
        }

        ProjectStateSerializer.FillProjectSnapshot(full.project);
        ProjectStateSerializer.FillCodeSnapshot(full.code);

        string targetFolder = kind == SnapshotKind.Working ? ProjectStatePaths.WorkingRoot : ProjectStatePaths.DebugRoot;
        string safeSceneName = string.IsNullOrWhiteSpace(scene.name) ? "UntitledScene" : scene.name;
        string fileName = $"BladeRift_{kind.ToString().ToUpperInvariant()}_{safeSceneName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string fullPath = Path.Combine(targetFolder, fileName);

        var settings = new JsonSerializerSettings
        {
            Formatting            = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(full, settings);
        File.WriteAllText(fullPath, json, Encoding.UTF8);

        ProjectStateIndex.UpdateSnapshotIndex();

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
        {
            File.WriteAllText(ProjectStatePaths.ChatStatePath, "# CHAT_STATE\n", Encoding.UTF8);
        }

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

            if (File.Exists(dst))
            {
                string name = Path.GetFileNameWithoutExtension(dst);
                string ext  = Path.GetExtension(dst);
                dst = Path.Combine(archiveFolder, $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}");
            }

            File.Move(src, dst);
        }
    }

#endif
}
