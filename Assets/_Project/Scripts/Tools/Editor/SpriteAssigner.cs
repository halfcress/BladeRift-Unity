using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public static class SpriteAssigner
{
    [MenuItem("Tools/BladeRift/Setup/Assign Arrow Sprite")]
public static void AssignArrowSprite()
    {
        string assetPath = "Assets/_Project/Art/arrow.png";

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("[SpriteAssigner] TextureImporter bulunamadi!");
            return;
        }

        // Her zaman Single yap ve yeniden import et
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
        {
            Debug.LogError("[SpriteAssigner] Reimport sonrasi Sprite hala null!");
            return;
        }

        GameObject arrow = GameObject.Find("Arrow");
        if (arrow == null) { Debug.LogError("[SpriteAssigner] Arrow objesi bulunamadi!"); return; }

        Image img = arrow.GetComponent<Image>();
        if (img == null) { Debug.LogError("[SpriteAssigner] Image component yok!"); return; }

        Undo.RecordObject(img, "Assign Arrow Sprite");
        img.sprite = sprite;
        EditorUtility.SetDirty(img);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"[SpriteAssigner] OK - sprite atandi: {sprite.name}");
    }
}
