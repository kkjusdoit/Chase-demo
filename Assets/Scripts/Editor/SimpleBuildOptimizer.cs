using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class SimpleBuildOptimizer : MonoBehaviour
{
    [MenuItem("Build/Simple Optimized WebGL Build")]
    public static void BuildWebGLOptimizedSimple()
    {
        Debug.Log("开始简化优化构建...");
        
        // 设置基本的WebGL优化
        SetBasicWebGLSettings();
        
        // 构建设置
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
        buildPlayerOptions.locationPathName = "build";
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        // 执行构建
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"构建成功！总大小: {summary.totalSize} bytes");
            Debug.Log($"构建时间: {summary.totalTime}");
        }
        else
        {
            Debug.LogError("构建失败！");
        }
    }
    
    private static void SetBasicWebGLSettings()
    {
        Debug.Log("设置基本WebGL优化...");
        
        // 基本WebGL设置（这些API比较稳定）
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.memorySize = 32; // 32MB
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.nameFilesAsHashes = true;
        PlayerSettings.WebGL.dataCaching = false;
        PlayerSettings.WebGL.debugSymbols = false;
        
        // 代码剥离（兼容性更好的设置）
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.stripUnusedMeshComponents = true;
        
        // 图形设置
        PlayerSettings.colorSpace = ColorSpace.Gamma;
        
        // 禁用不需要的功能
        PlayerSettings.enableInternalProfiler = false;
        
        Debug.Log("基本WebGL优化设置完成");
    }
}
