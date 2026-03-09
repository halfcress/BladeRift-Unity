using System;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;

// -----------------------------
// COMPILE ERROR TRACKER
// Unity compile hatalarini otomatik DEBUG_JOURNAL'a yazar
// -----------------------------

[InitializeOnLoad]
public static class ProjectStateCompileTracker
{
    private const string PrefKey = "BladeRift_CompileTracker_Enabled";
    private const string LastErrorKey = "BladeRift_CompileTracker_LastError";

    static ProjectStateCompileTracker()
    {
        CompilationPipeline.compilationStarted += OnCompilationStarted;
        CompilationPipeline.compilationFinished += OnCompilationFinished;
    }

    // -----------------------------
    // MENU
    // -----------------------------

    [MenuItem("Tools/BladeRift/Project State/Compile Tracker/Enable Compile Error Tracking")]
    private static void Enable()
    {
        EditorPrefs.SetBool(PrefKey, true);
        Debug.Log("[CompileTracker] Enabled.");
    }

    [MenuItem("Tools/BladeRift/Project State/Compile Tracker/Enable Compile Error Tracking", true)]
    private static bool Enable_Validate() => !EditorPrefs.GetBool(PrefKey, true);

    [MenuItem("Tools/BladeRift/Project State/Compile Tracker/Disable Compile Error Tracking")]
    private static void Disable()
    {
        EditorPrefs.SetBool(PrefKey, false);
        Debug.Log("[CompileTracker] Disabled.");
    }

    [MenuItem("Tools/BladeRift/Project State/Compile Tracker/Disable Compile Error Tracking", true)]
    private static bool Disable_Validate() => EditorPrefs.GetBool(PrefKey, true);

    // -----------------------------
    // HANDLERS
    // -----------------------------

    private static void OnCompilationStarted(object obj)
    {
        EditorPrefs.SetString(LastErrorKey, "");
    }

    private static void OnCompilationFinished(object obj)
    {
        if (!EditorPrefs.GetBool(PrefKey, true)) return;
        EditorApplication.delayCall += CheckForCompileErrors;
    }

    private static void CheckForCompileErrors()
    {
        bool hasErrors = EditorUtility.scriptCompilationFailed;
        if (!hasErrors) return;

        string errorKey = $"{DateTime.Now:yyyy-MM-dd HH}";
        string lastKey = EditorPrefs.GetString(LastErrorKey, "");
        if (lastKey == errorKey) return;

        EditorPrefs.SetString(LastErrorKey, errorKey);
        AppendCompileErrorToJournal();
    }

    private static void AppendCompileErrorToJournal()
    {
        try
        {
            ProjectStateExporter.EnsureFolders();
            ProjectStateJournal.EnsureDebugJournalExists();

            string commitShort = ProjectStateGit.SafeGit("rev-parse --short HEAD");
            string commitMessage = ProjectStateGit.SafeGit("log -1 --pretty=%s");

            string entry =
$@"

## {DateTime.Now:yyyy-MM-dd HH:mm:ss} - Compile Error Detected

### Problem
- Unity compile hatasi tespit edildi.
- Hata detaylari icin Console penceresini kontrol et.

### Context
- Commit: {(string.IsNullOrEmpty(commitShort) ? "unknown" : commitShort)}
- Commit message: {(string.IsNullOrEmpty(commitMessage) ? "unknown" : commitMessage)}

### Attempts
- [ ] Console hatalarini incele
- [ ] Ilgili .cs dosyasini duzelt

### Result
- [ ] Fail
- [ ] Fixed

### Decision
- [ ] Sonraki adim
";

            File.AppendAllText(ProjectStatePaths.DebugJournalPath, entry, Encoding.UTF8);
            Debug.LogWarning("[CompileTracker] Compile error logged to DEBUG_JOURNAL.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CompileTracker] Failed to log: {e.Message}");
        }
    }
}

#endif
