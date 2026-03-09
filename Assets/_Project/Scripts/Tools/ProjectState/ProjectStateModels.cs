using System;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------
// DATA MODELS — v5
// -----------------------------

[Serializable]
public class FullSnapshot
{
    public string[]      readingOrder = new string[]
    {
        "docs.chatState",
        "docs.gameConcept",
        "docs.architecture",
        "docs.todo",
        "docs.debugJournal",
        "docs.milestoneLog",
        "docs.other",
        "meta",
        "scene",
        "code.csFiles"
    };

    public Meta           meta    = new Meta();
    public DocsSnapshot   docs    = new DocsSnapshot();
    public SceneSnapshot  scene   = new SceneSnapshot();
    public ProjectSnapshot project = new ProjectSnapshot();
    public CodeSnapshot   code    = new CodeSnapshot();
}

[Serializable]
public class Meta
{
    public string snapshotKind;
    public string exportedAtLocalTime;
    public string unityVersion;
    public string platform;
    public string activeSceneName;
    public string note;
    public int    rootObjectCount;
    public int    totalGameObjectCount;
    public string headCommitShort;
    public string headCommitFull;
    public string headCommitMessage;
}

// -----------------------------
// DOCS SNAPSHOT
// Tum .md dosyalarini JSON'a gomulur
// AI okuma sirasi readingOrder'da tanimli
// -----------------------------

[Serializable]
public class DocsSnapshot
{
    public string chatState;     // CHAT_STATE.md
    public string gameConcept;   // GAME_CONCEPT_TR.md
    public string architecture;  // ARCHITECTURE_TR.md
    public string todo;          // TODO_TR.md
    public string debugJournal;  // DEBUG_JOURNAL.md
    public string milestoneLog;  // MILESTONE_LOG.md
    public List<DocFileData> other = new List<DocFileData>(); // Diger .md dosyalari
}

[Serializable]
public class DocFileData
{
    public string relativePath;
    public string text;
}

// -----------------------------
// SCENE
// -----------------------------

[Serializable]
public class SceneSnapshot
{
    public string sceneName;
    public string scenePath;
    public List<GameObjectData> roots = new List<GameObjectData>();
}

[Serializable]
public class ProjectSnapshot
{
    public RenderPipelineData renderPipeline = new RenderPipelineData();
    public TimeData           time           = new TimeData();
    public QualityData        quality        = new QualityData();
    public List<TextFileData> projectFiles   = new List<TextFileData>();
}

[Serializable]
public class CodeSnapshot
{
    public List<TextFileData> csFiles = new List<TextFileData>();
}

[Serializable]
public class RenderPipelineData
{
    public string graphicsCurrentRenderPipeline;
    public string qualityRenderPipeline;
}

[Serializable]
public class TimeData
{
    public float fixedDeltaTime;
    public float maximumDeltaTime;
    public int   targetFrameRate;
    public float timeScale;
}

[Serializable]
public class QualityData
{
    public int    qualityLevelIndex;
    public string qualityLevelName;
    public int    vSyncCount;
}

[Serializable]
public class TextFileData
{
    public string relativePath;
    public long   sizeBytes;
    public string sha256;
    public string text;
}

[Serializable]
public class GameObjectData
{
    public string name;
    public string path;
    public bool   activeSelf;
    public bool   activeInHierarchy;
    public string tag;
    public int    layer;
    public string staticFlags;

    public PrefabData     prefab          = new PrefabData();
    public TransformData  transform       = new TransformData();
    public BoundsData     rendererBounds  = new BoundsData();
    public BoundsData     colliderBounds  = new BoundsData();

    public List<ComponentData>  components = new List<ComponentData>();
    public List<GameObjectData> children   = new List<GameObjectData>();
}

// -----------------------------
// PREFAB — GUID + path
// -----------------------------

[Serializable]
public class PrefabData
{
    public string instanceStatus;
    public string assetType;
    public string sourceName;
    public string sourcePath;
    public string sourceGuid;   // v5: AssetDatabase.AssetPathToGUID ile dolduruluyor
}

[Serializable]
public class TransformData
{
    public float[] localPosition;
    public float[] localRotationEuler;
    public float[] localScale;
    public float[] worldPosition;
    public float[] worldRotationEuler;
    public float[] worldScaleLossy;
}

[Serializable]
public class BoundsData
{
    public bool    hasBounds;
    public float[] center;
    public float[] size;
    public float[] min;
    public float[] max;
}

[Serializable]
public class ComponentData
{
    public string          type;
    public List<FieldKV>   fields = new List<FieldKV>();
}

[Serializable]
public class FieldKV
{
    public string name;
    public string value;
}
