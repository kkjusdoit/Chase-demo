using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class UltraSimpleBuild : MonoBehaviour
{
    [MenuItem("Build/Ultra Simple WebGL Build")]
    public static void BuildWebGLUltraSimple()
    {
        Debug.Log("开始超简化WebGL构建...");
        
        // 只设置最基本和最稳定的WebGL优化
        SetMinimalWebGLSettings();
        
        // 构建
        string[] scenes = { "Assets/Scenes/SampleScene.unity" };
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "build",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("✅ 构建成功！");
            Debug.Log($"📦 总大小: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"⏱️ 构建时间: {report.summary.totalTime}");
            
            // 显示文件大小
            ShowFileSizes();
        }
        else
        {
            Debug.LogError("❌ 构建失败！");
        }
    }
    
    private static void SetMinimalWebGLSettings()
    {
        Debug.Log("设置最小化WebGL优化...");
        
        // 只使用最稳定的设置
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.memorySize = 32;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.debugSymbols = false;
        
        // 基本优化
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.colorSpace = ColorSpace.Gamma;
        
        Debug.Log("最小化设置完成");
    }
    
    private static void ShowFileSizes()
    {
        string buildPath = "build";
        string[] files = { "build.wasm", "build.data", "build.framework.js", "build.loader.js" };
        
        Debug.Log("=== 文件大小信息 ===");
        long totalSize = 0;
        
        foreach (string fileName in files)
        {
            string filePath = System.IO.Path.Combine(buildPath, fileName);
            if (System.IO.File.Exists(filePath))
            {
                long size = new System.IO.FileInfo(filePath).Length;
                totalSize += size;
                Debug.Log($"{fileName}: {size / (1024 * 1024)} MB");
            }
        }
        
        Debug.Log($"📊 总计: {totalSize / (1024 * 1024)} MB");
        Debug.Log($"🎯 优化前: 46.8 MB → 优化后: {totalSize / (1024 * 1024)} MB");
        
        if (totalSize < 46800000) // 如果小于46.8MB
        {
            float reduction = (1 - (float)totalSize / 46800000f) * 100;
            Debug.Log($"🎉 体积减少了 {reduction:F1}%！");
        }
    }
}
