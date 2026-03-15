using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ClaudeSnapshotTool : EditorWindow
{
    private string extractCommand = "";
    private string statusMessage = "";
    private bool snapshotReady = false;

    private static readonly string ToolFolder = "Assets/_Project/DevTool/ClaudeSnapshot";
    private static readonly string SnapshotPath = "Assets/_Project/DevTool/ClaudeSnapshot/claude_snapshot.json";
    private static readonly string ExtractPath = "Assets/_Project/DevTool/ClaudeSnapshot/claude_extract.md";
    private static readonly string IndexPath = "Assets/_Project/DevTool/ClaudeSnapshot/claude_index.md";

    [MenuItem("Tools/BladeRift/Project State/Claude Snapshot")]
    public static void OpenTool()
    {
        TakeSnapshot();

        var window = GetWindow<ClaudeSnapshotTool>("Claude Extract");
        window.minSize = new Vector2(460, 220);
        window.snapshotReady = true;
        window.statusMessage = "Snapshot alındı. Extract komutunu yapıştır.";
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Claude Snapshot Tool", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (!snapshotReady)
        {
            EditorGUILayout.HelpBox("Önce menüden Claude Snapshot çalıştır.", MessageType.Info);
            return;
        }

        GUILayout.Label("Extract komutu:");
        GUILayout.Label("Örnek: CombatDirector, RageManager --scene all --logs", EditorStyles.miniLabel);
        GUILayout.Label("Örnek: CombatDirector --scene Prototype_CombatCore,Prototype_VFXLab", EditorStyles.miniLabel);
        extractCommand = EditorGUILayout.TextField(extractCommand);

        GUILayout.Space(10);

        if (GUILayout.Button("Extract", GUILayout.Height(30)))
        {
            RunExtract();
        }

        GUILayout.Space(10);

        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }
    }

    private static void TakeSnapshot()
    {
        Directory.CreateDirectory(ToolFolder);

        var activeScene = EditorSceneManager.GetActiveScene();
        if (!activeScene.IsValid())
        {
            Debug.LogError("[ClaudeSnapshot] Active scene is not valid.");
            return;
        }

        var full = new FullSnapshot();
        full.meta.snapshotKind = "CLAUDE";
        full.meta.exportedAtLocalTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        full.meta.unityVersion = Application.unityVersion;
        full.meta.platform = Application.platform.ToString();
        full.meta.activeSceneName = activeScene.name;
        full.meta.note = "Claude AI snapshot.";
        full.scene.sceneName = activeScene.name;
        full.scene.scenePath = activeScene.path;

        var roots = activeScene.GetRootGameObjects();
        full.meta.rootObjectCount = roots.Length;
        full.meta.totalGameObjectCount = ProjectStateSerializer.CountAllSceneObjects(activeScene);
        full.meta.headCommitShort = ProjectStateGit.SafeGit("rev-parse --short HEAD");
        full.meta.headCommitFull = ProjectStateGit.SafeGit("rev-parse HEAD");
        full.meta.headCommitMessage = ProjectStateGit.SafeGit("log -1 --pretty=%s");

        foreach (var root in roots)
            full.scene.roots.Add(ProjectStateSerializer.SerializeGameObjectRecursive(root, root.name));

        ProjectStateSerializer.FillProjectSnapshot(full.project);
        ProjectStateSerializer.FillCodeSnapshot(full.code);
        ProjectStateSerializer.FillDocsSnapshot(full.docs);

        var sceneBundle = ExportAllProjectScenes();

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var serializer = JsonSerializer.Create(settings);
        var rootJson = JObject.FromObject(full, serializer);
        rootJson["sceneCatalog"] = JArray.FromObject(sceneBundle.catalog, serializer);
        rootJson["allScenes"] = JObject.FromObject(sceneBundle.scenes, serializer);

        File.WriteAllText(SnapshotPath, rootJson.ToString(Formatting.Indented), Encoding.UTF8);

        GenerateIndex(full, sceneBundle);

        AssetDatabase.Refresh();
        Debug.Log($"[ClaudeSnapshot] Snapshot alındı: {SnapshotPath}");
    }

    private static void GenerateIndex(FullSnapshot full, SceneBundle bundle)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# CLAUDE INDEX");
        sb.AppendLine($"commit: {full.meta.headCommitShort} | message: {full.meta.headCommitMessage}");
        sb.AppendLine($"scene: {full.meta.activeSceneName} | roots: {full.meta.rootObjectCount} | objects: {full.meta.totalGameObjectCount}");
        sb.AppendLine($"unity: {full.meta.unityVersion} | export: {full.meta.exportedAtLocalTime}");
        sb.AppendLine();

        bool hasCompileErrors = false;
#if UNITY_EDITOR
        hasCompileErrors = EditorUtility.scriptCompilationFailed;
#endif
        sb.AppendLine($"compile: {(hasCompileErrors ? "ERRORS" : "clean")}");
        sb.AppendLine();

        sb.AppendLine($"allScenes: {bundle.catalog.Count}");
        foreach (var entry in bundle.catalog)
            sb.AppendLine($"  {entry.sceneName} — {entry.scenePath}");
        sb.AppendLine();

        sb.AppendLine("## Files");
        foreach (var f in full.code.csFiles)
        {
            string rp = f.relativePath.Replace("\\", "/");
            if (rp.Contains("ProjectState") || rp.Contains("TutorialInfo") || rp.Contains("DevTool"))
                continue;

            string name = Path.GetFileNameWithoutExtension(rp);
            string size = f.sizeBytes < 1024 ? $"{f.sizeBytes}B" : $"{f.sizeBytes / 1024}K";
            sb.AppendLine($"  {name} ({size}) — {rp}");
        }
        sb.AppendLine();

        sb.AppendLine("## Components");
        foreach (var root in full.scene.roots)
            WriteComponentMapCompact(sb, root, 0);
        sb.AppendLine();

        File.WriteAllText(IndexPath, sb.ToString(), Encoding.UTF8);
    }

    private static void WriteComponentMapCompact(StringBuilder sb, GameObjectData obj, int depth)
    {
        string indent = new string(' ', depth * 2);
        var customComps = obj.components
            .Where(c => !c.type.StartsWith("UnityEngine.") && !c.type.StartsWith("TMPro."))
            .Select(c => c.type)
            .ToList();

        bool hasChildren = obj.children != null && obj.children.Count > 0;

        if (customComps.Count > 0)
        {
            sb.AppendLine($"{indent}{obj.name} [{string.Join(" | ", customComps)}]");
        }
        else if (depth == 0 || hasChildren)
        {
            sb.AppendLine($"{indent}{obj.name}");
        }

        if (obj.children != null)
        {
            foreach (var child in obj.children)
                WriteComponentMapCompact(sb, child, depth + 1);
        }
    }

    private void RunExtract()
    {
        if (!File.Exists(SnapshotPath))
        {
            statusMessage = "Snapshot bulunamadı. Önce Claude Snapshot al.";
            return;
        }

        FullSnapshot full;
        JObject snapshotRoot;
        try
        {
            string json = File.ReadAllText(SnapshotPath, Encoding.UTF8);
            if (json.Length > 0 && json[0] == '\uFEFF')
                json = json.Substring(1);

            snapshotRoot = JObject.Parse(json);
            full = snapshotRoot.ToObject<FullSnapshot>();
        }
        catch (Exception ex)
        {
            statusMessage = $"Snapshot okunamadı: {ex.Message}";
            return;
        }

        ParseResult parsed = ParseCommand(extractCommand);

        var sb = new StringBuilder();
        sb.AppendLine("# CLAUDE EXTRACT");
        sb.AppendLine($"snapshot: {full.meta.headCommitShort} | {full.meta.exportedAtLocalTime}");
        sb.AppendLine($"compile: {(EditorUtility.scriptCompilationFailed ? "ERRORS" : "clean")}");
        sb.AppendLine($"command: {extractCommand}");
        sb.AppendLine();

        int fileCount = 0;

        if (parsed.fileNames.Count > 0)
        {
            foreach (string requested in parsed.fileNames)
            {
                var match = full.code.csFiles.FirstOrDefault(f =>
                {
                    string name = Path.GetFileNameWithoutExtension(f.relativePath.Replace("\\", "/"));
                    return string.Equals(name, requested, StringComparison.OrdinalIgnoreCase);
                });

                if (match != null)
                {
                    sb.AppendLine($"## {Path.GetFileName(match.relativePath)}");
                    sb.AppendLine($"path: {match.relativePath}");
                    sb.AppendLine("```csharp");
                    sb.AppendLine(match.text);
                    sb.AppendLine("```");
                    sb.AppendLine();
                    fileCount++;
                }
                else
                {
                    sb.AppendLine($"## {requested}.cs — BULUNAMADI");
                    sb.AppendLine();
                }
            }
        }

        if (parsed.includeScene)
        {
            AppendRequestedScenes(snapshotRoot, full, parsed, sb);
        }

        if (parsed.includeLogs)
        {
            sb.AppendLine("## Console Logs (son 20)");
            sb.AppendLine("(Console log yakalama için MINI snapshot kullanılabilir)");
            sb.AppendLine();
        }

        if (EditorUtility.scriptCompilationFailed)
        {
            sb.AppendLine("## Compile Errors");
            sb.AppendLine("UYARI: Compile hatası mevcut. Detay için Unity Console'a bak.");
            sb.AppendLine();
        }

        File.WriteAllText(ExtractPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();

        statusMessage = $"Extract tamamlandı. {fileCount} dosya çıkarıldı.\n{ExtractPath}";
        EditorUtility.RevealInFinder(ExtractPath);
    }

    private static void AppendRequestedScenes(JObject snapshotRoot, FullSnapshot full, ParseResult parsed, StringBuilder sb)
    {
        var allScenesToken = snapshotRoot["allScenes"] as JObject;

        if (parsed.sceneMode == SceneExtractMode.Active || allScenesToken == null)
        {
            sb.AppendLine($"## Scene Hierarchy — {full.scene.sceneName}");
            foreach (var root in full.scene.roots)
                WriteSceneTree(sb, root, 0);
            sb.AppendLine();
            return;
        }

        if (parsed.sceneMode == SceneExtractMode.All)
        {
            foreach (var prop in allScenesToken.Properties())
            {
                var sceneSnapshot = prop.Value.ToObject<SceneSnapshot>();
                sb.AppendLine($"## Scene Hierarchy — {sceneSnapshot.sceneName}");
                foreach (var root in sceneSnapshot.roots)
                    WriteSceneTree(sb, root, 0);
                sb.AppendLine();
            }
            return;
        }

        if (parsed.sceneMode == SceneExtractMode.Named)
        {
            foreach (string requestedName in parsed.sceneNames)
            {
                var match = FindSceneToken(allScenesToken, requestedName);
                if (match == null)
                {
                    sb.AppendLine($"## Scene Hierarchy — {requestedName} — BULUNAMADI");
                    sb.AppendLine();
                    continue;
                }

                var sceneSnapshot = match.Value.ToObject<SceneSnapshot>();
                sb.AppendLine($"## Scene Hierarchy — {sceneSnapshot.sceneName}");
                foreach (var root in sceneSnapshot.roots)
                    WriteSceneTree(sb, root, 0);
                sb.AppendLine();
            }
        }
    }

    private static JProperty FindSceneToken(JObject allScenesToken, string requestedName)
    {
        foreach (var prop in allScenesToken.Properties())
        {
            var sceneSnapshot = prop.Value.ToObject<SceneSnapshot>();

            if (string.Equals(prop.Name, requestedName, StringComparison.OrdinalIgnoreCase))
                return prop;

            if (sceneSnapshot != null)
            {
                if (string.Equals(sceneSnapshot.sceneName, requestedName, StringComparison.OrdinalIgnoreCase))
                    return prop;

                if (string.Equals(sceneSnapshot.scenePath, requestedName, StringComparison.OrdinalIgnoreCase))
                    return prop;
            }
        }

        return null;
    }

    private static void WriteSceneTree(StringBuilder sb, GameObjectData obj, int depth)
    {
        string indent = new string(' ', depth * 2);
        var comps = obj.components
            .Select(c => c.type.Replace("UnityEngine.", "").Replace("UnityEngine.Rendering.Universal.", ""))
            .Where(c => c != "Transform" && c != "RectTransform")
            .ToList();

        string compStr = comps.Count > 0 ? $" [{string.Join(" | ", comps)}]" : "";
        sb.AppendLine($"{indent}{obj.name}{compStr}");

        foreach (var child in obj.children)
            WriteSceneTree(sb, child, depth + 1);
    }

    private enum SceneExtractMode
    {
        None,
        Active,
        Named,
        All
    }

    private struct ParseResult
    {
        public List<string> fileNames;
        public bool includeScene;
        public bool includeLogs;
        public SceneExtractMode sceneMode;
        public List<string> sceneNames;
    }

    private static ParseResult ParseCommand(string cmd)
    {
        var result = new ParseResult
        {
            fileNames = new List<string>(),
            includeScene = false,
            includeLogs = false,
            sceneMode = SceneExtractMode.None,
            sceneNames = new List<string>()
        };

        if (string.IsNullOrWhiteSpace(cmd))
            return result;

        string remaining = cmd;

        var sceneMatch = Regex.Match(remaining, @"--scene(?:\s+(.+?))?(?=\s--|$)", RegexOptions.IgnoreCase);
        if (sceneMatch.Success)
        {
            result.includeScene = true;
            remaining = remaining.Remove(sceneMatch.Index, sceneMatch.Length).Trim();

            string sceneArg = sceneMatch.Groups[1].Value.Trim();

            if (string.IsNullOrWhiteSpace(sceneArg))
            {
                result.sceneMode = SceneExtractMode.Active;
            }
            else if (string.Equals(sceneArg, "all", StringComparison.OrdinalIgnoreCase))
            {
                result.sceneMode = SceneExtractMode.All;
            }
            else
            {
                result.sceneMode = SceneExtractMode.Named;
                var sceneNames = sceneArg
                    .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s));

                result.sceneNames.AddRange(sceneNames);
            }
        }

        if (remaining.Contains("--logs"))
        {
            result.includeLogs = true;
            remaining = remaining.Replace("--logs", "").Trim();
        }

        if (!string.IsNullOrWhiteSpace(remaining))
        {
            var names = remaining.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var n in names)
            {
                string clean = n.Trim().Replace(".cs", "");
                if (!string.IsNullOrEmpty(clean))
                    result.fileNames.Add(clean);
            }
        }

        return result;
    }

    private static SceneBundle ExportAllProjectScenes()
    {
        var bundle = new SceneBundle();
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" });

        var previousSetup = EditorSceneManager.GetSceneManagerSetup();
        string previousActiveScenePath = SceneManager.GetActiveScene().path;

        try
        {
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var sceneSnapshot = new SceneSnapshot
                {
                    sceneName = scene.name,
                    scenePath = scene.path
                };

                foreach (var root in scene.GetRootGameObjects())
                    sceneSnapshot.roots.Add(ProjectStateSerializer.SerializeGameObjectRecursive(root, root.name));

                bundle.catalog.Add(new SceneCatalogEntry
                {
                    sceneName = sceneSnapshot.sceneName,
                    scenePath = sceneSnapshot.scenePath,
                    rootObjectCount = sceneSnapshot.roots.Count
                });

                bundle.scenes[sceneSnapshot.sceneName] = sceneSnapshot;
            }
        }
        finally
        {
            if (previousSetup != null && previousSetup.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
            else if (!string.IsNullOrEmpty(previousActiveScenePath))
            {
                EditorSceneManager.OpenScene(previousActiveScenePath, OpenSceneMode.Single);
            }
        }

        return bundle;
    }

    [Serializable]
    private class SceneBundle
    {
        public List<SceneCatalogEntry> catalog = new List<SceneCatalogEntry>();
        public Dictionary<string, SceneSnapshot> scenes = new Dictionary<string, SceneSnapshot>();
    }

    [Serializable]
    private class SceneCatalogEntry
    {
        public string sceneName;
        public string scenePath;
        public int rootObjectCount;
    }
}

#endif
