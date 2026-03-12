using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

public class ClaudeSnapshotTool : EditorWindow
{
    private string extractCommand = "";
    private string statusMessage = "";
    private bool snapshotReady = false;

    // Sabit yollar — her seferinde üzerine yazılır
    private static readonly string ToolFolder = "Assets/_Project/DevTool/ClaudeSnapshot";
    private static readonly string SnapshotPath = "Assets/_Project/DevTool/ClaudeSnapshot/claude_snapshot.json";
    private static readonly string ExtractPath = "Assets/_Project/DevTool/ClaudeSnapshot/claude_extract.md";
    private static readonly string IndexPath = "Assets/_Project/DevTool/ClaudeSnapshot/claude_index.md";

    // ─────────────────────────────────────
    // MENU
    // ─────────────────────────────────────

    [MenuItem("Tools/BladeRift/Project State/Claude Snapshot")]
    public static void OpenTool()
    {
        // 1) Snapshot al
        TakeSnapshot();

        // 2) Pencereyi aç
        var window = GetWindow<ClaudeSnapshotTool>("Claude Extract");
        window.minSize = new Vector2(420, 200);
        window.snapshotReady = true;
        window.statusMessage = "Snapshot alındı. Extract komutunu yapıştır.";
        window.Show();
    }

    // ─────────────────────────────────────
    // GUI
    // ─────────────────────────────────────

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
        GUILayout.Label("Örnek: CombatDirector, RageManager --scene --logs", EditorStyles.miniLabel);
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

    // ─────────────────────────────────────
    // SNAPSHOT
    // ─────────────────────────────────────

    private static void TakeSnapshot()
    {
        Directory.CreateDirectory(ToolFolder);

        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("[ClaudeSnapshot] Active scene is not valid.");
            return;
        }

        FullSnapshot full = new FullSnapshot();
        full.meta.snapshotKind = "CLAUDE";
        full.meta.exportedAtLocalTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        full.meta.unityVersion = Application.unityVersion;
        full.meta.platform = Application.platform.ToString();
        full.meta.activeSceneName = scene.name;
        full.meta.note = "Claude AI snapshot.";
        full.scene.sceneName = scene.name;
        full.scene.scenePath = scene.path;

        var roots = scene.GetRootGameObjects();
        full.meta.rootObjectCount = roots.Length;
        full.meta.totalGameObjectCount = ProjectStateSerializer.CountAllSceneObjects(scene);
        full.meta.headCommitShort = ProjectStateGit.SafeGit("rev-parse --short HEAD");
        full.meta.headCommitFull = ProjectStateGit.SafeGit("rev-parse HEAD");
        full.meta.headCommitMessage = ProjectStateGit.SafeGit("log -1 --pretty=%s");

        foreach (var root in roots)
            full.scene.roots.Add(ProjectStateSerializer.SerializeGameObjectRecursive(root, root.name));

        ProjectStateSerializer.FillProjectSnapshot(full.project);
        ProjectStateSerializer.FillCodeSnapshot(full.code);
        ProjectStateSerializer.FillDocsSnapshot(full.docs);

        // Konsol loglarını da yakala
        // (MiniSnapshot'taki gibi ama FullSnapshot'a ek alan olarak değil, ayrıca saklayacağız)

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        File.WriteAllText(SnapshotPath, JsonConvert.SerializeObject(full, settings), Encoding.UTF8);

        // INDEX her snapshot'ta otomatik üretilir
        GenerateIndex(full);

        AssetDatabase.Refresh();
        Debug.Log($"[ClaudeSnapshot] Snapshot alındı: {SnapshotPath}");
    }

    // ─────────────────────────────────────
    // INDEX — her zaman otomatik üretilir
    // ─────────────────────────────────────

    private static void GenerateIndex(FullSnapshot full)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# CLAUDE INDEX");
        sb.AppendLine($"commit: {full.meta.headCommitShort} | message: {full.meta.headCommitMessage}");
        sb.AppendLine($"scene: {full.meta.activeSceneName} | roots: {full.meta.rootObjectCount} | objects: {full.meta.totalGameObjectCount}");
        sb.AppendLine($"unity: {full.meta.unityVersion} | export: {full.meta.exportedAtLocalTime}");
        sb.AppendLine();

        // Compile check
        bool hasCompileErrors = false;
#if UNITY_EDITOR
        // Basit kontrol — compile error varsa Unity zaten loglar
        hasCompileErrors = EditorUtility.scriptCompilationFailed;
#endif
        sb.AppendLine($"compile: {(hasCompileErrors ? "ERRORS" : "clean")}");
        sb.AppendLine();

        // CS dosya listesi (sadece game code, DevTool/TutorialInfo hariç)
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

        // Component map (kısa — her root altında ne var)
        sb.AppendLine("## Components");
        foreach (var root in full.scene.roots)
        {
            WriteComponentMapCompact(sb, root, 0);
        }
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
            // Root her zaman göster + child'ı olan ara node'ları da göster
            sb.AppendLine($"{indent}{obj.name}");
        }

        if (obj.children != null)
        {
            foreach (var child in obj.children)
                WriteComponentMapCompact(sb, child, depth + 1);
        }
    }

    // ─────────────────────────────────────
    // EXTRACT
    // ─────────────────────────────────────

    private void RunExtract()
    {
        if (!File.Exists(SnapshotPath))
        {
            statusMessage = "Snapshot bulunamadı. Önce Claude Snapshot al.";
            return;
        }

        // Snapshot'ı oku
        FullSnapshot full;
        try
        {
            string json = File.ReadAllText(SnapshotPath, Encoding.UTF8);
            // BOM varsa temizle
            if (json.Length > 0 && json[0] == '\uFEFF')
                json = json.Substring(1);
            full = JsonConvert.DeserializeObject<FullSnapshot>(json);
        }
        catch (Exception ex)
        {
            statusMessage = $"Snapshot okunamadı: {ex.Message}";
            return;
        }

        // Komutu parse et
        ParseResult parsed = ParseCommand(extractCommand);

        var sb = new StringBuilder();
        sb.AppendLine("# CLAUDE EXTRACT");
        sb.AppendLine($"snapshot: {full.meta.headCommitShort} | {full.meta.exportedAtLocalTime}");
        sb.AppendLine($"compile: {(EditorUtility.scriptCompilationFailed ? "ERRORS" : "clean")}");
        sb.AppendLine($"command: {extractCommand}");
        sb.AppendLine();

        int fileCount = 0;

        // İstenen CS dosyalarını çıkar
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

        // Scene
        if (parsed.includeScene)
        {
            sb.AppendLine("## Scene Hierarchy");
            foreach (var root in full.scene.roots)
            {
                WriteSceneTree(sb, root, 0);
            }
            sb.AppendLine();
        }

        // Console logs
        if (parsed.includeLogs)
        {
            sb.AppendLine("## Console Logs (son 20)");
            // FullSnapshot'ta consoleLogs yok, MiniSnapshot'ta var
            // Burada Unity console'dan direkt okuyalım
            sb.AppendLine("(Console log yakalama için MINI snapshot kullanılabilir)");
            sb.AppendLine();
        }

        // Compile errors
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

    // ─────────────────────────────────────
    // PARSE
    // ─────────────────────────────────────

    private struct ParseResult
    {
        public List<string> fileNames;
        public bool includeScene;
        public bool includeLogs;
    }

    private static ParseResult ParseCommand(string cmd)
    {
        var result = new ParseResult
        {
            fileNames = new List<string>(),
            includeScene = false,
            includeLogs = false
        };

        if (string.IsNullOrWhiteSpace(cmd))
            return result;

        // Flags
        string remaining = cmd;

        if (remaining.Contains("--scene"))
        {
            result.includeScene = true;
            remaining = remaining.Replace("--scene", "").Trim();
        }

        if (remaining.Contains("--logs"))
        {
            result.includeLogs = true;
            remaining = remaining.Replace("--logs", "").Trim();
        }

        // Kalan kısım virgülle ayrılmış dosya isimleri
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
}

#endif