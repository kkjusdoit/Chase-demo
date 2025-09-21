using UnityEngine;
using UnityEditor;
using System.IO;

public class FixFirebaseWebGL : EditorWindow
{
    [MenuItem("Tools/Fix Firebase WebGL")]
    static void FixFirebasePlatformSettings()
    {
        string[] dlls = {
            "Assets/Firebase/Plugins/Firebase.App.dll",
            "Assets/Firebase/Plugins/Firebase.Auth.dll", 
            "Assets/Firebase/Plugins/Firebase.Database.dll",
            "Assets/Firebase/Plugins/Firebase.Platform.dll",
            "Assets/Firebase/Plugins/Firebase.TaskExtension.dll",
            "Assets/Firebase/Plugins/Google.MiniJson.dll"
        };

        foreach (string dllPath in dlls)
        {
            if (File.Exists(dllPath))
            {
                PluginImporter pluginImporter = AssetImporter.GetAtPath(dllPath) as PluginImporter;
                if (pluginImporter != null)
                {
                    pluginImporter.SetCompatibleWithPlatform(BuildTarget.WebGL, true);
                    pluginImporter.SaveAndReimport();
                    Debug.Log($"已启用 {dllPath} 的WebGL支持");
                }
            }
        }
        
        Debug.Log("Firebase WebGL平台支持修复完成！");
    }
} 