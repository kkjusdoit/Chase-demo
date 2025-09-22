using UnityEngine;
using UnityEditor;

/// <summary>
/// PlayerPrefs管理器编辑器工具
/// 提供一键清除PlayerPrefs本地数据的功能
/// </summary>
public class PlayerPrefsManager : EditorWindow
{
    private Vector2 scrollPosition;
    private string[] commonKeys = {
        "HighScore",
        "PlayerName", 
        "BestTime",
        "Level",
        "Coins",
        "Settings"
    };

    [MenuItem("Tools/PlayerPrefs管理器")]
    public static void ShowWindow()
    {
        PlayerPrefsManager window = GetWindow<PlayerPrefsManager>("PlayerPrefs管理器");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("PlayerPrefs 本地数据管理", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(10);
        
        // 警告信息
        EditorGUILayout.HelpBox("注意：清除PlayerPrefs数据是不可逆的操作！请确保你真的想要删除所有本地保存的数据。", MessageType.Warning);
        
        EditorGUILayout.Space(10);
        
        // 一键清除所有数据按钮
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("🗑️ 清除所有PlayerPrefs数据", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("确认清除", 
                "你确定要清除所有PlayerPrefs数据吗？\n\n这将删除：\n• 最高分记录\n• 玩家设置\n• 游戏进度\n• 其他所有本地保存的数据\n\n此操作无法撤销！", 
                "确认清除", "取消"))
            {
                ClearAllPlayerPrefs();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(20);
        
        // 常见键值显示和单独删除
        GUILayout.Label("常见的PlayerPrefs键值:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (string key in commonKeys)
        {
            EditorGUILayout.BeginHorizontal();
            
            // 显示键名
            EditorGUILayout.LabelField(key, GUILayout.Width(120));
            
            // 显示值（如果存在）
            if (PlayerPrefs.HasKey(key))
            {
                string value = GetPlayerPrefsValue(key);
                EditorGUILayout.LabelField($"值: {value}", GUILayout.Width(150));
                
                // 单独删除按钮
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    if (EditorUtility.DisplayDialog("确认删除", 
                        $"确定要删除键值 '{key}' 吗？", "删除", "取消"))
                    {
                        PlayerPrefs.DeleteKey(key);
                        PlayerPrefs.Save();
                        Debug.Log($"已删除PlayerPrefs键值: {key}");
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.LabelField("(不存在)", GUILayout.Width(150));
                GUI.enabled = false;
                GUILayout.Button("删除", GUILayout.Width(50));
                GUI.enabled = true;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.Space(20);
        
        // 刷新按钮
        if (GUILayout.Button("🔄 刷新数据"))
        {
            Repaint();
        }
        
        EditorGUILayout.Space(10);
        
        // 信息说明
        EditorGUILayout.HelpBox("提示：\n• 点击'刷新数据'可以更新当前显示的键值状态\n• 删除单个键值后会自动保存\n• 清除所有数据后需要重启游戏才能看到效果", MessageType.Info);
    }

    /// <summary>
    /// 清除所有PlayerPrefs数据
    /// </summary>
    private void ClearAllPlayerPrefs()
    {
        try
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            Debug.Log("✅ 已成功清除所有PlayerPrefs数据！");
            
            // 显示成功对话框
            EditorUtility.DisplayDialog("清除成功", 
                "所有PlayerPrefs数据已被清除！\n\n建议重启Unity编辑器和游戏以确保更改生效。", "确定");
                
            // 刷新窗口显示
            Repaint();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"清除PlayerPrefs数据时发生错误: {e.Message}");
            EditorUtility.DisplayDialog("清除失败", 
                $"清除PlayerPrefs数据时发生错误:\n{e.Message}", "确定");
        }
    }

    /// <summary>
    /// 获取PlayerPrefs键值的字符串表示
    /// </summary>
    private string GetPlayerPrefsValue(string key)
    {
        // 尝试不同的数据类型
        if (PlayerPrefs.HasKey(key))
        {
            // 先尝试作为字符串
            string stringValue = PlayerPrefs.GetString(key, "");
            if (!string.IsNullOrEmpty(stringValue))
            {
                return $"\"{stringValue}\"";
            }
            
            // 尝试作为整数
            int intValue = PlayerPrefs.GetInt(key, int.MinValue);
            if (intValue != int.MinValue)
            {
                return intValue.ToString();
            }
            
            // 尝试作为浮点数
            float floatValue = PlayerPrefs.GetFloat(key, float.MinValue);
            if (floatValue != float.MinValue)
            {
                return floatValue.ToString("F2");
            }
            
            return "(未知类型)";
        }
        
        return "(不存在)";
    }
}
