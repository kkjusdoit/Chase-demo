using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public class BuildOptimizer : MonoBehaviour
{
    [MenuItem("Build/Optimized WebGL Build")]
    public static void BuildWebGLOptimized()
    {
        Debug.Log("开始优化构建...");
        
        // 设置构建选项
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/MainScene.unity" };
        buildPlayerOptions.locationPathName = "build";
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        // 优化Player设置
        OptimizePlayerSettings();
        
        // 执行构建
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"构建成功: {summary.totalSize / (1024 * 1024)} MB");
            
            // 显示文件大小信息
            ShowBuildSizeInfo();
        }
        else
        {
            Debug.LogError("构建失败");
        }
    }
    
    private static void OptimizePlayerSettings()
    {
        Debug.Log("应用优化设置...");
        
        // WebGL设置优化
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.memorySize = 32; // 32MB内存限制
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.nameFilesAsHashes = true;
        PlayerSettings.WebGL.dataCaching = false;
        PlayerSettings.WebGL.debugSymbols = false;
        
        // 代码剥离设置
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.High);
        PlayerSettings.stripUnusedMeshComponents = true;
        
        // 脚本编译优化
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.WebGL, ApiCompatibilityLevel.NET_Standard_2_0);
        
        // 图形设置优化
        PlayerSettings.colorSpace = ColorSpace.Gamma; // 使用Gamma颜色空间
        PlayerSettings.gpuSkinning = false; // 禁用GPU蒙皮
        
        // 禁用不需要的功能
        PlayerSettings.SetVirtualRealitySupported(BuildTargetGroup.WebGL, false);
        PlayerSettings.enableInternalProfiler = false;
        
        // 移除Unity启动画面和Logo
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[0];
        
        // 进一步的WebGL优化
        try
        {
            PlayerSettings.WebGL.template = "APPLICATION:Minimal"; // 使用最小模板
            PlayerSettings.WebGL.threadsSupport = false; // 禁用线程支持
            PlayerSettings.WebGL.showDiagnostics = false; // 禁用诊断信息
            
            // 新的Unity版本使用decompressionFallback来控制WASM流式加载
            #if UNITY_2022_1_OR_NEWER
            PlayerSettings.WebGL.decompressionFallback = true; // 启用解压缩回退，禁用WASM流式加载
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"某些WebGL设置可能不支持当前Unity版本: {e.Message}");
        }
        
        Debug.Log("Unity品牌元素已移除");
        
        Debug.Log("优化设置已应用");
    }
    
    private static void ShowBuildSizeInfo()
    {
        string buildPath = Path.Combine(Application.dataPath, "../build");
        
        if (Directory.Exists(buildPath))
        {
            string[] files = { "build.wasm", "build.data", "build.framework.js", "build.loader.js" };
            long totalSize = 0;
            
            Debug.Log("=== 构建文件大小信息 ===");
            
            foreach (string file in files)
            {
                string filePath = Path.Combine(buildPath, file);
                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    long sizeInMB = fileInfo.Length / (1024 * 1024);
                    totalSize += fileInfo.Length;
                    
                    Debug.Log($"{file}: {sizeInMB} MB ({fileInfo.Length} bytes)");
                }
            }
            
            Debug.Log($"总大小: {totalSize / (1024 * 1024)} MB ({totalSize} bytes)");
        }
    }
}
