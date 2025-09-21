using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerNameUI : MonoBehaviour
{
    [Header("UI Elements")]
    public InputField nameInputField;
    public Button saveButton;
    public Button cancelButton;
    public Text currentNameText;
    public GameObject nameInputPanel;
    
    [Header("Settings")]
    public int maxNameLength = 20;
    public string defaultName = "Anonymous";
    
    private CloudScoreManager cloudScoreManager;
    
    void Start()
    {
        // 查找云端分数管理器
        cloudScoreManager = FindFirstObjectByType<CloudScoreManager>();
        
        // 设置按钮事件
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SavePlayerName);
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelEdit);
        }
        
        // 设置输入框限制
        if (nameInputField != null)
        {
            nameInputField.characterLimit = maxNameLength;
            nameInputField.onEndEdit.AddListener(OnInputFieldEndEdit);
        }
        
        // 初始化显示
        UpdateCurrentNameDisplay();
        
        // 默认隐藏输入面板
        SetInputPanelVisible(false);
    }
    
    void Update()
    {
        // 按Enter键保存
        if (nameInputPanel != null && nameInputPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Return))
        {
            SavePlayerName();
        }
        
        // 按Escape键取消
        if (nameInputPanel != null && nameInputPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelEdit();
        }
    }
    
    // 显示昵称编辑面板
    public void ShowNameEditPanel()
    {
        if (cloudScoreManager != null)
        {
            string currentName = cloudScoreManager.GetPlayerName();
            if (nameInputField != null)
            {
                nameInputField.text = currentName;
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }
        }
        
        SetInputPanelVisible(true);
    }
    
    // 保存玩家昵称
    public void SavePlayerName()
    {
        if (nameInputField == null || cloudScoreManager == null) return;
        
        string newName = nameInputField.text.Trim();
        
        // 验证昵称
        if (string.IsNullOrEmpty(newName))
        {
            newName = defaultName;
        }
        
        // 过滤特殊字符（可选）
        newName = FilterName(newName);
        
        // 保存昵称
        cloudScoreManager.SetPlayerName(newName);
        
        // 更新显示
        UpdateCurrentNameDisplay();
        
        // 隐藏面板
        SetInputPanelVisible(false);
        
        Debug.Log($"玩家昵称已保存：{newName}");
    }
    
    // 取消编辑
    public void CancelEdit()
    {
        SetInputPanelVisible(false);
    }
    
    // 输入框结束编辑时的处理
    private void OnInputFieldEndEdit(string value)
    {
        // 如果按了Enter键，自动保存
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SavePlayerName();
        }
    }
    
    // 更新当前昵称显示
    private void UpdateCurrentNameDisplay()
    {
        if (currentNameText != null && cloudScoreManager != null)
        {
            string playerName = cloudScoreManager.GetPlayerName();
            currentNameText.text = $"Player: {playerName}";
        }
    }
    
    // 控制输入面板的显示/隐藏
    private void SetInputPanelVisible(bool visible)
    {
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(visible);
            
            // 如果显示面板，暂停游戏时间
            if (visible)
            {
                Time.timeScale = 0f;
            }
            else
            {
                // 只有在游戏没有结束时才恢复时间
                if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
                {
                    Time.timeScale = 1f;
                }
            }
        }
    }
    
    // 过滤昵称中的特殊字符
    private string FilterName(string name)
    {
        if (string.IsNullOrEmpty(name)) return defaultName;
        
        // 移除前后空格
        name = name.Trim();
        
        // 限制长度
        if (name.Length > maxNameLength)
        {
            name = name.Substring(0, maxNameLength);
        }
        
        // 这里可以添加更多过滤规则，比如移除特殊字符
        // name = System.Text.RegularExpressions.Regex.Replace(name, @"[^\w\s-]", "");
        
        // 如果过滤后为空，使用默认名称
        if (string.IsNullOrEmpty(name.Trim()))
        {
            name = defaultName;
        }
        
        return name;
    }
    
    // 公共方法：获取当前玩家昵称
    public string GetCurrentPlayerName()
    {
        if (cloudScoreManager != null)
        {
            return cloudScoreManager.GetPlayerName();
        }
        return defaultName;
    }
    
    // 公共方法：检查是否有有效的昵称
    public bool HasValidPlayerName()
    {
        string name = GetCurrentPlayerName();
        return !string.IsNullOrEmpty(name) && name != defaultName;
    }
    
    // 在游戏开始时可以调用这个方法提醒玩家设置昵称
    public void PromptForNameIfDefault()
    {
        if (!HasValidPlayerName())
        {
            StartCoroutine(DelayedPrompt());
        }
    }
    
    private IEnumerator DelayedPrompt()
    {
        yield return new WaitForSeconds(2f); // 等待2秒
        
        // 如果游戏还没开始，提示设置昵称
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
        {
            Debug.Log("提示：点击玩家名称可以设置你的昵称");
            // 这里可以添加UI提示效果
        }
    }
} 