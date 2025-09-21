using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject leaderboardPanel;
    public Button showLeaderboardButton;
    public Button closeLeaderboardButton;
    public Button refreshButton;
    public Transform leaderboardContent;
    public GameObject leaderboardEntryPrefab;
    public Text loadingText;
    public Text errorText;
    public Text noDataText;
    
    [Header("Settings")]
    public int maxEntries = 10;
    public bool autoRefreshOnShow = true;
    public float refreshCooldown = 5f; // 防止频繁刷新
    
    private CloudScoreManager cloudScoreManager;
    private List<GameObject> entryObjects = new List<GameObject>();
    private float lastRefreshTime = 0f;
    
    void Start()
    {
        // 查找云端分数管理器
        cloudScoreManager = FindFirstObjectByType<CloudScoreManager>();
        
        // 设置按钮事件
        if (showLeaderboardButton != null)
        {
            showLeaderboardButton.onClick.AddListener(ShowLeaderboard);
        }
        
        if (closeLeaderboardButton != null)
        {
            closeLeaderboardButton.onClick.AddListener(HideLeaderboard);
        }
        
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshLeaderboard);
        }
        
        // 订阅云端分数管理器的事件
        CloudScoreManager.OnLeaderboardLoaded += OnLeaderboardLoaded;
        CloudScoreManager.OnError += OnError;
        
        // 默认隐藏排行榜面板
        SetPanelVisible(false);
        
        // 创建简单的排行榜条目预制体（如果没有指定）
        if (leaderboardEntryPrefab == null)
        {
            CreateDefaultEntryPrefab();
        }
    }
    
    void OnDestroy()
    {
        // 取消订阅事件
        CloudScoreManager.OnLeaderboardLoaded -= OnLeaderboardLoaded;
        CloudScoreManager.OnError -= OnError;
    }
    
    void Update()
    {
        // 按Escape键关闭排行榜
        if (leaderboardPanel != null && leaderboardPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            HideLeaderboard();
        }
    }
    
    // 显示排行榜
    public void ShowLeaderboard()
    {
        SetPanelVisible(true);
        
        if (autoRefreshOnShow)
        {
            RefreshLeaderboard();
        }
    }
    
    // 隐藏排行榜
    public void HideLeaderboard()
    {
        SetPanelVisible(false);
    }
    
    // 刷新排行榜
    public void RefreshLeaderboard()
    {
        // 防止频繁刷新
        if (Time.time - lastRefreshTime < refreshCooldown)
        {
            ShowMessage(errorText, $"请等待 {(refreshCooldown - (Time.time - lastRefreshTime)):F1} 秒后再刷新");
            return;
        }
        
        if (cloudScoreManager == null)
        {
            ShowMessage(errorText, "云端分数管理器未找到");
            return;
        }
        
        lastRefreshTime = Time.time;
        
        // 显示加载状态
        ShowMessage(loadingText, "正在加载排行榜...");
        HideMessage(errorText);
        HideMessage(noDataText);
        
        // 清空现有条目
        ClearLeaderboardEntries();
        
        // 请求排行榜数据
        cloudScoreManager.LoadLeaderboard(maxEntries);
    }
    
    // 处理排行榜数据加载完成
    private void OnLeaderboardLoaded(LeaderboardEntry[] entries)
    {
        HideMessage(loadingText);
        
        if (entries == null || entries.Length == 0)
        {
            ShowMessage(noDataText, "暂无排行榜数据");
            return;
        }
        
        // 创建排行榜条目
        CreateLeaderboardEntries(entries);
    }
    
    // 处理错误
    private void OnError(string errorMessage)
    {
        HideMessage(loadingText);
        ShowMessage(errorText, errorMessage);
    }
    
    // 创建排行榜条目
    private void CreateLeaderboardEntries(LeaderboardEntry[] entries)
    {
        // 清空现有条目
        ClearLeaderboardEntries();
        
        for (int i = 0; i < entries.Length; i++)
        {
            GameObject entryObj = CreateLeaderboardEntry(entries[i], i + 1);
            if (entryObj != null)
            {
                entryObjects.Add(entryObj);
            }
        }
    }
    
    // 创建单个排行榜条目
    private GameObject CreateLeaderboardEntry(LeaderboardEntry entry, int rank)
    {
        if (leaderboardEntryPrefab == null || leaderboardContent == null)
        {
            Debug.LogError("Leaderboard entry prefab or content parent is null");
            return null;
        }
        
        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
        
        // 查找子组件并设置数据
        Text rankText = entryObj.transform.Find("RankText")?.GetComponent<Text>();
        Text nameText = entryObj.transform.Find("NameText")?.GetComponent<Text>();
        Text scoreText = entryObj.transform.Find("ScoreText")?.GetComponent<Text>();
        
        if (rankText != null)
        {
            rankText.text = rank.ToString();
        }
        
        if (nameText != null)
        {
            nameText.text = entry.player_name;
        }
        
        if (scoreText != null)
        {
            scoreText.text = entry.best_score.ToString();
        }
        
        // 设置排名颜色（前三名特殊处理）
        Color rankColor = Color.white;
        switch (rank)
        {
            case 1: rankColor = Color.yellow; break;
            case 2: rankColor = Color.gray; break;
            case 3: rankColor = new Color(1f, 0.5f, 0f); break; // 橙色
        }
        
        if (rankText != null)
        {
            rankText.color = rankColor;
        }
        
        return entryObj;
    }
    
    // 清空排行榜条目
    private void ClearLeaderboardEntries()
    {
        foreach (GameObject obj in entryObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        entryObjects.Clear();
    }
    
    // 控制面板显示/隐藏
    private void SetPanelVisible(bool visible)
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(visible);
            
            // 暂停/恢复游戏
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
    
    // 显示消息
    private void ShowMessage(Text messageText, string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
        }
    }
    
    // 隐藏消息
    private void HideMessage(Text messageText)
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
    
    // 创建默认的排行榜条目预制体
    private void CreateDefaultEntryPrefab()
    {
        // 这里可以程序化创建一个简单的排行榜条目
        GameObject prefab = new GameObject("LeaderboardEntry");
        prefab.AddComponent<RectTransform>();
        
        // 添加背景
        Image background = prefab.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.1f);
        
        // 添加排名文本
        GameObject rankObj = new GameObject("RankText");
        rankObj.transform.SetParent(prefab.transform);
        Text rankText = rankObj.AddComponent<Text>();
        rankText.text = "1";
        rankText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        rankText.fontSize = 16;
        rankText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform rankRect = rankObj.GetComponent<RectTransform>();
        rankRect.anchorMin = new Vector2(0, 0);
        rankRect.anchorMax = new Vector2(0.2f, 1);
        rankRect.offsetMin = Vector2.zero;
        rankRect.offsetMax = Vector2.zero;
        
        // 添加昵称文本
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(prefab.transform);
        Text nameText = nameObj.AddComponent<Text>();
        nameText.text = "Player Name";
        nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameText.fontSize = 16;
        nameText.alignment = TextAnchor.MiddleLeft;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.2f, 0);
        nameRect.anchorMax = new Vector2(0.7f, 1);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        // 添加分数文本
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(prefab.transform);
        Text scoreText = scoreObj.AddComponent<Text>();
        scoreText.text = "9999";
        scoreText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        scoreText.fontSize = 16;
        scoreText.alignment = TextAnchor.MiddleRight;
        
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.7f, 0);
        scoreRect.anchorMax = new Vector2(1, 1);
        scoreRect.offsetMin = Vector2.zero;
        scoreRect.offsetMax = Vector2.zero;
        
        // 设置高度
        RectTransform prefabRect = prefab.GetComponent<RectTransform>();
        prefabRect.sizeDelta = new Vector2(0, 30);
        
        leaderboardEntryPrefab = prefab;
        
        Debug.Log("创建了默认的排行榜条目预制体");
    }
    
    // 公共方法：检查排行榜是否显示
    public bool IsLeaderboardVisible()
    {
        return leaderboardPanel != null && leaderboardPanel.activeInHierarchy;
    }
    
    // 公共方法：切换排行榜显示状态
    public void ToggleLeaderboard()
    {
        if (IsLeaderboardVisible())
        {
            HideLeaderboard();
        }
        else
        {
            ShowLeaderboard();
        }
    }
} 