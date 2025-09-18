using UnityEngine;
using UnityEditor;
using System.IO;

public class ProjectCleaner : EditorWindow
{
    [MenuItem("Tools/Clean Project for WebGL")]
    public static void CleanProject()
    {
        Debug.Log("开始清理项目...");
        
        // 删除不必要的资源文件
        DeleteUnnecessaryAssets();
        
        // 优化项目设置
        OptimizeProjectSettings();
        
        // 刷新资源数据库
        AssetDatabase.Refresh();
        
        Debug.Log("项目清理完成！");
    }
    
    private static void DeleteUnnecessaryAssets()
    {
        // 删除示例场景（如果不需要）
        string[] unnecessaryPaths = {
            "Assets/New Scene.unity",
            "Assets/New Scene.unity.meta"
        };
        
        foreach (string path in unnecessaryPaths)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"删除文件: {path}");
            }
        }
        
        // 删除默认的Volume Profile（如果使用URP但不需要后处理）
        if (File.Exists("Assets/DefaultVolumeProfile.asset"))
        {
            AssetDatabase.DeleteAsset("Assets/DefaultVolumeProfile.asset");
            Debug.Log("删除默认Volume Profile");
        }
    }
    
    private static void OptimizeProjectSettings()
    {
        // 禁用不必要的图形设置
        var graphicsSettings = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        
        // 设置最小的质量等级
        QualitySettings.SetQualityLevel(0, true); // 使用最低质量设置
        
        // 禁用实时GI
        Lightmapping.realtimeGI = false;
        Lightmapping.bakedGI = false;
        
        Debug.Log("项目设置优化完成");
    }
}
