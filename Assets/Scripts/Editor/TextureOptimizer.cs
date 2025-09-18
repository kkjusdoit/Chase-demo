using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureOptimizer : EditorWindow
{
    [MenuItem("Tools/Optimize Textures for WebGL")]
    public static void OptimizeTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources" });
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            
            if (textureImporter != null)
            {
                Debug.Log($"优化纹理: {assetPath}");
                
                // 设置通用的纹理导入设置
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.mipmapEnabled = false;
                textureImporter.isReadable = false;
                textureImporter.streamingMipmaps = false;
                
                // WebGL平台特定设置
                TextureImporterPlatformSettings webglSettings = new TextureImporterPlatformSettings();
                webglSettings.name = "WebGL";
                webglSettings.overridden = true;
                webglSettings.maxTextureSize = 512; // 限制最大纹理尺寸
                webglSettings.format = TextureImporterFormat.DXT5; // 使用压缩格式
                webglSettings.compressionQuality = 50; // 中等压缩质量
                webglSettings.crunchedCompression = true;
                webglSettings.allowsAlphaSplitting = false;
                
                textureImporter.SetPlatformTextureSettings(webglSettings);
                
                // 应用设置
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log("纹理优化完成！");
    }
}
