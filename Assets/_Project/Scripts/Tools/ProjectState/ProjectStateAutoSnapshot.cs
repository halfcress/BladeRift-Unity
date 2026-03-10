using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

// -----------------------------
// AUTO SNAPSHOT ON PLAY — v6
// Auto snapshot artik MINI_DEBUG aliyor (Full degil)
// -----------------------------

[InitializeOnLoad]
public static class ProjectStateAutoSnapshot
{
    private const string PrefKey = "BladeRift_AutoSnapshot_Enabled";

    static ProjectStateAutoSnapshot()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Tools/BladeRift/Project State/Auto Snapshot/Enable Auto Snapshot on Play")]
    private static void EnableAutoSnapshot()
    {
        EditorPrefs.SetBool(PrefKey, true);
        Debug.Log("[AutoSnapshot] Enabled.");
    }

    [MenuItem("Tools/BladeRift/Project State/Auto Snapshot/Enable Auto Snapshot on Play", true)]
    private static bool EnableAutoSnapshot_Validate() => !EditorPrefs.GetBool(PrefKey, true);

    [MenuItem("Tools/BladeRift/Project State/Auto Snapshot/Disable Auto Snapshot on Play")]
    private static void DisableAutoSnapshot()
    {
        EditorPrefs.SetBool(PrefKey, false);
        Debug.Log("[AutoSnapshot] Disabled.");
    }

    [MenuItem("Tools/BladeRift/Project State/Auto Snapshot/Disable Auto Snapshot on Play", true)]
    private static bool DisableAutoSnapshot_Validate() => EditorPrefs.GetBool(PrefKey, true);

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode) return;
        if (!EditorPrefs.GetBool(PrefKey, true)) return;
        TakeAutoSnapshot();
    }

    private static void TakeAutoSnapshot()
    {
        try
        {
            ProjectStateExporter.EnsureFolders();

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid()) { Debug.LogWarning("[AutoSnapshot] Scene not valid."); return; }

            MiniSnapshot snap = new MiniSnapshot();
            ProjectStateSerializer.FillMiniSnapshot(snap, "MINI_DEBUG");

            string safeSceneName = string.IsNullOrWhiteSpace(scene.name) ? "UntitledScene" : scene.name;
            string fileName      = $"BladeRift_AUTO_{safeSceneName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string fullPath      = Path.Combine(ProjectStatePaths.MiniDebugRoot, fileName);

            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            File.WriteAllText(fullPath, JsonConvert.SerializeObject(snap, settings), Encoding.UTF8);

            CleanupAutoSnapshots(5);

            Debug.Log($"[AutoSnapshot] Saved: {fileName}");
        }
        catch (Exception e) { Debug.LogError($"[AutoSnapshot] Failed: {e.Message}"); }
    }

    private static void CleanupAutoSnapshots(int maxKeep)
    {
        string miniDebugRoot = ProjectStatePaths.MiniDebugRoot;
        string archiveRoot   = Path.Combine(ProjectStatePaths.ArchiveRoot, "Auto");
        Directory.CreateDirectory(archiveRoot);

        var autoFiles = Directory.GetFiles(miniDebugRoot, "BladeRift_AUTO_*.json", SearchOption.TopDirectoryOnly);
        Array.Sort(autoFiles, (a, b) => File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));

        for (int i = maxKeep; i < autoFiles.Length; i++)
        {
            string src = autoFiles[i];
            string dst = Path.Combine(archiveRoot, Path.GetFileName(src));
            if (File.Exists(dst)) dst = Path.Combine(archiveRoot, $"{Path.GetFileNameWithoutExtension(src)}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            File.Move(src, dst);
        }
    }
}

#endif
