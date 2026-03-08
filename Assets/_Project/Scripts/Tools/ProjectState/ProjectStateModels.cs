using System;
using System.Collections.Generic;
using UnityEngine;

// -----------------------------
// DATA MODELS
// -----------------------------

[Serializable]
public class FullSnapshot
{
    public Meta meta = new Meta();
    public SceneSnapshot scene = new SceneSnapshot();
    public ProjectSnapshot project = new ProjectSnapshot();
    public CodeSnapshot code = new CodeSnapshot();
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
    public int rootObjectCount;
    public int totalGameObjectCount;
    public string headCommitShort;
    public string headCommitFull;
    public string headCommitMessage;
}

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
    public TimeData time = new TimeData();
    public QualityData quality = new QualityData();
    public List<TextFileData> projectFiles = new List<TextFileData>();
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
    public int targetFrameRate;
    public float timeScale;
}

[Serializable]
public class QualityData
{
    public int qualityLevelIndex;
    public string qualityLevelName;
    public int vSyncCount;
}

[Serializable]
public class TextFileData
{
    public string relativePath;
    public long sizeBytes;
    public string sha256;
    public string text;
}

[Serializable]
public class GameObjectData
{
    public string name;
    public string path;
    public bool activeSelf;
    public bool activeInHierarchy;

    public string tag;
    public int layer;
    public string staticFlags;

    public PrefabData prefab = new PrefabData();
    public TransformData transform = new TransformData();
    public BoundsData rendererBounds = new BoundsData();
    public BoundsData colliderBounds = new BoundsData();

    public List<ComponentData> components = new List<ComponentData>();
    public List<GameObjectData> children = new List<GameObjectData>();
}

[Serializable]
public class PrefabData
{
    public string instanceStatus;
    public string assetType;
    public string sourceName;
    public string sourcePath;
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
    public bool hasBounds;
    public float[] center;
    public float[] size;
    public float[] min;
    public float[] max;
}

[Serializable]
public class ComponentData
{
    public string type;
    public List<FieldKV> fields = new List<FieldKV>();
}

[Serializable]
public class FieldKV
{
    public string name;
    public string value;
}
