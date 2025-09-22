using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("游戏对象引用")]
    public Player player;
    public Enemy enemy;

    public Button restartFullScreenButton;
    public Button changeDirectionButton;

    [Header("Register")]
    public Button registerButton;
    public Text registerStatusText;

    public GameObject registerPanel;

    public Text scoreText;
    public Text maxScoreText; // 现在用作云端分数显示
    public Text enmeySpeedText;
    public Text playerSpeedText;
    
    
    [Header("云端分数")]
    public CloudScoreManager cloudScoreManager;

    public Text leaderboardText;

    [Header("玩家名字")]
    public InputField playerNameInputField;
    public Text playerNameText;
    
    [Header("碰撞检测设置")]
    public float collisionDistance = 100f; // 碰撞检测距离
    public float bonusCollisionDistance = 70f; // bonus碰撞检测距离
    
    [Header("游戏状态")]
    private bool isGameOver = false;
    private bool isGameStarted = false; // 游戏是否已开始（名字已输入）
    private int currentScore = 0;
    private int bestScore = 0; // 本地最佳分数
    private int cloudBestScore = 0; // 云端最佳分数
    private bool isPlayerInvincible = false; // 玩家无敌状态
    private float invincibleTimer = 0f; // 无敌计时器
    private string playerName = ""; // 玩家名字


    
    [Header("Bonus系统")]
    private List<GameObject> activeBonuses = new List<GameObject>(); // 活跃的bonus道具列表
    
    
    // 单例模式
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // 确保只有一个GameManager实例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (changeDirectionButton != null)
        {
            changeDirectionButton.onClick.AddListener(ChangeDirection);
        }
    }
    
    void Start()
    {
        // 绑定重启按钮事件
        if (restartFullScreenButton != null)
        {
            restartFullScreenButton.onClick.AddListener(RestartGame);
        }

        if (registerButton != null)
        {
            registerButton.onClick.AddListener(SubmitPlayerName);
        }
        
        // 自动查找游戏对象（如果没有手动分配）
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }
        
        if (enemy == null)
        {
            enemy = FindFirstObjectByType<Enemy>();
        }
        
        // 自动查找云端分数管理器
        if (cloudScoreManager == null)
        {
            cloudScoreManager = FindFirstObjectByType<CloudScoreManager>();
        }
        
        // 移除输入框的自动提交事件，现在只通过按钮提交
        // if (playerNameInputField != null)
        // {
        //     playerNameInputField.onEndEdit.AddListener(OnPlayerNameSubmitted);
        // }
        
        // 设置玩家名字文本点击事件（如果有Button组件）
        if (playerNameText != null)
        {
            Button nameButton = playerNameText.GetComponent<Button>();
            if (nameButton != null)
            {
                nameButton.onClick.AddListener(EditPlayerName);
            }
        }
        
        // 初始状态：暂停游戏，等待登录检查
        Time.timeScale = 0f;
        isGameStarted = false;
        
        // 基于云端数据进行自动登录检查
        StartCoroutine(CheckCloudLoginStatus());
        
        // 监听云端分数提交事件
        CloudScoreManager.OnScoreSubmitted += OnCloudScoreSubmitted;
        
        // 监听排行榜加载事件
        CloudScoreManager.OnLeaderboardLoaded += DisplayLeaderboard;
    }
    
    void Update()
    {
        // 只有游戏开始且未结束时才处理游戏逻辑
        if (isGameStarted && !isGameOver)
        {
            HandleKeyboardInput();
            HandleInvincibleTimer();
            CheckCollision();
            CheckBonusCollisions(); // 检查bonus碰撞
            UpdateSpeedDisplay(); // 实时更新速度显示
        }
    }
    
    // 实时更新速度显示
    private void UpdateSpeedDisplay()
    {
        // 显示玩家速度
        if (playerSpeedText != null && player != null)
        {
            float playerSpeed = player.GetMoveSpeed();
            playerSpeedText.text = $"Player Speed: {playerSpeed:F2}";
        }
        
        // 显示敌人速度
        if (enmeySpeedText != null && enemy != null)
        {
            float enemySpeed = enemy.GetCurrentSpeed();
            enmeySpeedText.text = $"Enemy Speed: {enemySpeed:F2}";
        }
    }
    
    // 检查bonus道具碰撞
    private void CheckBonusCollisions()
    {
        if (player == null) return;
        
        float playerX = player.GetXPosition();
        
        // 检查所有活跃的bonus道具
        for (int i = activeBonuses.Count - 1; i >= 0; i--)
        {
            if (activeBonuses[i] != null)
            {
                RectTransform bonusRect = activeBonuses[i].GetComponent<RectTransform>();
                if (bonusRect != null)
                {
                    float bonusX = bonusRect.anchoredPosition.x;
                    
                    // 检测碰撞
                    if (Mathf.Abs(playerX - bonusX) < bonusCollisionDistance)
                    {
                        // 收集bonus道具
                        CollectBonus(activeBonuses[i]);
                        activeBonuses.RemoveAt(i);
                    }
                }
            }
            else
            {
                // 移除空引用
                activeBonuses.RemoveAt(i);
            }
        }
    }
    
    // 收集bonus道具
    private void CollectBonus(GameObject bonus)
    {
        if (bonus != null)
        {
            // 加分
            currentScore += 1;
            UpdateScoreDisplay();
            // Debug.Log($"收集到bonus道具！当前分数：{currentScore}");
            
            // 销毁bonus道具
            Destroy(bonus);
        }
    }
    
    // 注册bonus道具
    public void RegisterBonus(GameObject bonus)
    {
        if (bonus != null && !activeBonuses.Contains(bonus))
        {
            activeBonuses.Add(bonus);
        }
    }
    
    // 清理所有bonus道具
    public void ClearAllBonuses()
    {
        foreach (GameObject bonus in activeBonuses)
        {
            if (bonus != null)
            {
                Destroy(bonus);
            }
        }
        activeBonuses.Clear();
    }
    
    // 处理无敌状态计时器
    private void HandleInvincibleTimer()
    {
        if (isPlayerInvincible && invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isPlayerInvincible = false;
                Debug.Log("玩家无敌状态结束");
            }
        }
    }
    
    // 设置玩家无敌状态
    public void SetPlayerInvincible(bool invincible, float duration = 0f)
    {
        isPlayerInvincible = invincible;
        if (invincible && duration > 0f)
        {
            invincibleTimer = duration;
            Debug.Log($"玩家进入无敌状态，持续时间：{duration}秒");
        }
        else if (!invincible)
        {
            invincibleTimer = 0f;
            Debug.Log("玩家退出无敌状态");
        }
    }
    
    // 检查玩家是否处于无敌状态
    public bool IsPlayerInvincible()
    {
        return isPlayerInvincible;
    }
    
    // 处理键盘输入（可选，用于测试）
    private void HandleKeyboardInput()
    {
        // 检测空格键或其他按键来改变方向
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeDirection();
        }
    }
    
    // 检查云端登录状态
    private IEnumerator CheckCloudLoginStatus()
    {
        // 等待CloudScoreManager初始化完成
        while (cloudScoreManager == null)
        {
            cloudScoreManager = FindFirstObjectByType<CloudScoreManager>();
            yield return new WaitForSeconds(0.1f);
        }
        
        // 显示加载提示
        ShowRegisterStatus("正在检查用户数据...", false);
        
        // 尝试根据设备ID获取玩家数据
        yield return StartCoroutine(cloudScoreManager.GetPlayerDataByDeviceId());
    }
    
    // 云端数据加载完成后的回调
    private void OnCloudPlayerDataLoaded(object[] data)
    {
        bool hasData = (bool)data[0];
        string playerName = (string)data[1]; 
        int bestScore = (int)data[2];
        
        HandleCloudPlayerDataLoaded(hasData, playerName, bestScore);
    }
    
    // 处理云端玩家数据加载结果
    private void HandleCloudPlayerDataLoaded(bool hasData, string playerName, int bestScore)
    {
        if (hasData)
        {
            // 找到云端数据，自动登录
            this.playerName = playerName;
            this.cloudBestScore = bestScore;
            
            // 隐藏注册面板
            if (registerPanel != null)
            {
                registerPanel.SetActive(false);
            }
            
            // 显示玩家名字
            if (playerNameText != null)
            {
                playerNameText.gameObject.SetActive(true);
                playerNameText.text = $"Player: {playerName}";
            }
            
            // 设置云端分数管理器的玩家名字
            if (cloudScoreManager != null)
            {
                cloudScoreManager.SetPlayerName(playerName);
            }
            
            // 更新分数显示
            UpdateScoreDisplay();
            
            // 更新排行榜
            UpdateLeaderboardDisplay();
            
            // 开始游戏
            StartGame();
            
            Debug.Log($"自动登录成功！玩家：{playerName}，最高分：{bestScore}");
        }
        else
        {
            // 没有找到云端数据，显示注册界面
            ShowPlayerNameInput();
            Debug.Log("未找到云端用户数据，显示注册界面");
        }
    }
    
    // 显示玩家名字输入界面
    private void ShowPlayerNameInput()
    {
        isGameStarted = false;
        Time.timeScale = 0f; // 暂停游戏
        
        // 清除状态信息
        ClearRegisterStatus();
        
        // 显示注册面板
        if (registerPanel != null)
        {
            registerPanel.SetActive(true);
        }
        
        // 隐藏玩家名字文本
        if (playerNameText != null)
        {
            playerNameText.gameObject.SetActive(false);
        }
        
        // 激活输入框
        if (playerNameInputField != null)
        {
            playerNameInputField.Select();
            playerNameInputField.ActivateInputField();
            playerNameInputField.text = "";
        }
        
        Debug.Log("请输入玩家名字开始游戏");
    }
    
    // 处理玩家名字提交（现在只通过按钮触发，保留方法以防其他地方调用）
    private void OnPlayerNameSubmitted(string inputName)
    {
        // 移除自动提交逻辑，现在只通过register按钮提交
        // if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        // {
        //     SubmitPlayerName();
        // }
    }
    
    // 提交玩家名字
    public void SubmitPlayerName()
    {
        if (playerNameInputField == null) return;
        
        // 清除之前的状态信息
        ClearRegisterStatus();
        
        string inputName = playerNameInputField.text.Trim();
        
        if (string.IsNullOrEmpty(inputName))
        {
            ShowRegisterStatus("玩家名字不能为空！", true);
            return;
        }
        
        // 检查名字长度
        if (inputName.Length > 20)
        {
            ShowRegisterStatus("玩家名字不能超过20个字符！", true);
            return;
        }
        
        // 检查是否包含特殊字符（可选）
        if (inputName.Contains("<") || inputName.Contains(">") || inputName.Contains("&"))
        {
            ShowRegisterStatus("玩家名字不能包含特殊字符！", true);
            return;
        }
        
        // 过滤和验证名字
        inputName = FilterPlayerName(inputName);
        
        if (string.IsNullOrEmpty(inputName))
        {
            ShowRegisterStatus("请输入有效的玩家名字！", true);
            return;
        }
        
        // 显示成功状态
        ShowRegisterStatus("注册成功！", false);
        
        // 设置玩家名字（将通过云端API保存）
        playerName = inputName;
        
        // 延迟隐藏注册面板并开始游戏，让用户看到成功消息
        StartCoroutine(HandleRegisterSuccessWithDelay(1.5f));
        
        // 显示玩家名字文本
        if (playerNameText != null)
        {
            playerNameText.gameObject.SetActive(true);
            playerNameText.text = $"Player: {playerName}";
        }
        
        // 设置云端分数管理器的玩家名字
        if (cloudScoreManager != null)
        {
            cloudScoreManager.SetPlayerName(playerName);
        }
        
        Debug.Log($"玩家名字设置为：{playerName}");
    }
    
    // 过滤玩家名字
    private string FilterPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        
        name = name.Trim();
        
        // 限制长度
        if (name.Length > 20)
        {
            name = name.Substring(0, 20);
        }
        
        // 这里可以添加更多过滤规则
        // 比如过滤敏感词等
        
        return name;
    }
    
    // 显示注册状态信息
    private void ShowRegisterStatus(string message, bool isError = true)
    {
        if (registerStatusText != null)
        {
            registerStatusText.text = message;
            registerStatusText.gameObject.SetActive(true);
            
            // 可以根据是否为错误设置不同颜色
            if (isError)
            {
                registerStatusText.color = Color.red;
            }
            else
            {
                registerStatusText.color = Color.green;
            }
        }
        
        Debug.Log($"注册状态: {message}");
    }
    
    // 清除注册状态信息
    private void ClearRegisterStatus()
    {
        if (registerStatusText != null)
        {
            registerStatusText.text = "";
            registerStatusText.gameObject.SetActive(false);
        }
    }
    
    // 处理注册成功后的延时流程
    private IEnumerator HandleRegisterSuccessWithDelay(float delay)
    {
        registerButton.gameObject.SetActive(false);
        Debug.Log("注册成功后的延时流程");
        yield return new WaitForSecondsRealtime(delay);
        
        Debug.Log("等待延时" +  registerPanel != null);
        // 隐藏注册面板
        if (registerPanel != null)
        {

            registerPanel.SetActive(false);
        }
        Debug.Log("隐藏注册面板");
        ClearRegisterStatus();
        Debug.Log("清除注册状态");
        // 延时后开始游戏或恢复游戏
        if (isGameStarted)
        {
            Debug.Log("恢复游戏时间");
            // 恢复游戏时间
            if (!isGameOver)
            {
                Time.timeScale = 1f;
            }
        }
        else
        {
            Debug.Log("开始新游戏");
            // 更新排行榜
            UpdateLeaderboardDisplay();
            // 开始新游戏
            StartGame();
        }
    }
    
    // 延迟隐藏注册UI的协程（保留用于其他可能的用途）
    private IEnumerator HideRegisterUIAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        // 隐藏注册面板
        if (registerPanel != null)
        {
            registerPanel.SetActive(false);
        }
        
        ClearRegisterStatus();
    }
    
    // 开始游戏
    private void StartGame()
    {
        isGameStarted = true;
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        isGameOver = false;
        currentScore = 0;
        Time.timeScale = 1f;
        
        // 本地不再存储最佳分数，使用云端数据
        
        // 隐藏重启按钮
        SetRestartFullScreenButtonVisible(false);
        
        // 清理所有bonus道具
        ClearAllBonuses();
        
        // 更新分数显示
        UpdateScoreDisplay();
        
        // 加载云端分数
        LoadCloudScore();
        
        Debug.Log($"游戏开始！玩家：{playerName}");
    }
    
    
    private void CheckCollision()
    {
        // 只有在玩家不是无敌状态且敌人可见时才检测碰撞
        if (player != null && enemy != null && enemy.IsVisible() && !isPlayerInvincible)
        {
            float playerX = player.GetXPosition();
            float enemyX = enemy.GetXPosition();
            
            // 检测x坐标差值是否小于设定的碰撞距离
            if (Mathf.Abs(playerX - enemyX) < collisionDistance)
            {
                TriggerGameOver();
            }
        }
    }
    
    public void TriggerGameOver()
    {
        if (isGameOver) return; // 防止重复触发
        
        isGameOver = true;
        
        // 检查并更新最佳分数
        CheckAndUpdateBestScore();
        
        // 更新排行榜（游戏结束后可能有新的高分）
        UpdateLeaderboardDisplay();
        
        // 显示重启按钮
        SetRestartFullScreenButtonVisible(true);
        
        // 暂停游戏
        Time.timeScale = 0f;
        
        // 清理所有bonus道具
        ClearAllBonuses();
        
        // 输出游戏结束信息
        Debug.Log($"游戏结束！玩家与敌人发生碰撞！最终分数：{currentScore}，最佳分数：{bestScore}");
        
        // 这里可以添加游戏结束UI的显示逻辑
        // 例如：显示游戏结束面板、播放音效等
    }
    
    // 检查并更新最佳分数
    private void CheckAndUpdateBestScore()
    {
        bool isNewRecord = false;
        
        // 本地不再存储最佳分数，新记录检查完全由云端处理
        
        // 检查是否超过当前显示的最高分（云端分数）
        if (currentScore > cloudBestScore)
        {
            cloudBestScore = currentScore;
            Debug.Log($"新纪录！超越云端最佳分数：{cloudBestScore}");
            isNewRecord = true;
        }
        
        // 提交分数到云端
        if (cloudScoreManager != null)
        {
            cloudScoreManager.SubmitScore(currentScore);
        }
        
        // 更新UI显示
        UpdateScoreDisplay();
        
        // 如果创造了新纪录，显示特殊提示
        if (isNewRecord)
        {
            Debug.Log("🎉 恭喜！创造了新的最高分记录！");
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("重新开始游戏");
        
        // 重置游戏状态
        isGameOver = false;
        isGameStarted = true; // 重启时保持游戏开始状态
        currentScore = 0;
        Time.timeScale = 1f;
        
        // 隐藏重启按钮
        SetRestartFullScreenButtonVisible(false);
        
        // 清理所有bonus道具
        ClearAllBonuses();
        
        // 重置玩家位置和方向
        if (player != null)
        {
            player.SetPosition(0f);
            player.ResetDirection();
        }
        
        // 重置敌人状态
        if (enemy != null)
        {
            enemy.ResetEnemy();
        }
        
        // 更新分数显示
        UpdateScoreDisplay();
    }
    
    // 移除原有的自动加分逻辑，现在只通过bonus道具加分
    public void AddScore(int points = 1)
    {
        // 这个方法保留但不再自动调用，只用于bonus道具加分
        if (!isGameOver)
        {
            currentScore += points;
            UpdateScoreDisplay();
        }
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
        
        // maxScoreText 现在显示云端最高分
        if (maxScoreText != null)
        {
            maxScoreText.text = "Best: " + cloudBestScore;
        }
    }
    
    // 更新排行榜显示
    private void UpdateLeaderboardDisplay()
    {
        if (cloudScoreManager != null)
        {
            Debug.Log("开始加载排行榜数据...");
            cloudScoreManager.LoadLeaderboard(10);
        }
    }
    
    // 显示排行榜数据
    private void DisplayLeaderboard(LeaderboardEntry[] entries)
    {
        if (leaderboardText == null || entries == null)
        {
            Debug.LogWarning("排行榜文本控件未设置或数据为空");
            return;
        }
        
        string leaderboardContent = "<b><color=#FFD700>🏆 Leaderboard 🏆</color></b>\n\n";
        
        for (int i = 0; i < entries.Length; i++)
        {
            string rankIcon = GetRankIcon(i + 1);
            string nameColor = GetRankColor(i + 1);
            string scoreColor = "#00FF00"; // 绿色分数
            
            leaderboardContent += $"{rankIcon} <color={nameColor}><b>{entries[i].player_name}</b></color>: <color={scoreColor}><b>{entries[i].best_score}</b></color>\n";
        }
        
        // 移除最后的换行符
        if (leaderboardContent.Length > 0)
        {
            leaderboardContent = leaderboardContent.TrimEnd('\n');
        }
        
        leaderboardText.text = leaderboardContent;
        Debug.Log($"排行榜已更新，显示{entries.Length}条记录");
    }
    
    // 获取排名图标
    private string GetRankIcon(int rank)
    {
        switch (rank)
        {
            default: return $"<b>{rank}.</b>"; // 数字排名
        }
    }
    
    // 获取排名颜色
    private string GetRankColor(int rank)
    {
        switch (rank)
        {
            case 1: return "#FFD700"; // 金色
            case 2: return "#C0C0C0"; // 银色
            case 3: return "#CD7F32"; // 铜色
            case 4:
            case 5: return "#9370DB"; // 紫色
            case 6:
            case 7:
            case 8: return "#4169E1"; // 蓝色
            default: return "#FFFFFF"; // 白色
        }
    }
    
    // 获取当前分数
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    // 获取最佳分数
    public int GetBestScore()
    {
        return bestScore;
    }
    
    // 检查游戏是否结束
    public bool IsGameOver()
    {
        return isGameOver;
    }
    
    // 设置碰撞距离
    public void SetCollisionDistance(float distance)
    {
        collisionDistance = distance;
    }
    
    // 获取碰撞距离
    public float GetCollisionDistance()
    {
        return collisionDistance;
    }

    public void ChangeDirection()
    {
        // 只有游戏开始且未结束时才能改变方向
        if (isGameStarted && !isGameOver && player != null)
        {
            player.ChangeDirection();
        }
    }
    
    // 控制重启按钮的显示和隐藏
    private void SetRestartFullScreenButtonVisible(bool visible)
    {
        if (restartFullScreenButton != null)
        {
            restartFullScreenButton.gameObject.SetActive(visible);
            Debug.Log($"重启全屏按钮{(visible ? "显示" : "隐藏")}");
        }
    }
    
    // 加载云端分数
    private void LoadCloudScore()
    {
        if (cloudScoreManager != null)
        {
            cloudScoreManager.GetCloudHighScore((score) => {
                cloudBestScore = score;
                UpdateScoreDisplay();
                Debug.Log($"加载云端最佳分数：{cloudBestScore}");
                
                // 如果本地分数比云端分数高，更新云端分数
                if (bestScore > cloudBestScore)
                {
                    Debug.Log($"本地分数({bestScore})高于云端分数({cloudBestScore})，提交到云端");
                    cloudScoreManager.SubmitScore(bestScore);
                }
            });
        }
    }
    
    // 获取云端最佳分数
    public int GetCloudBestScore()
    {
        return cloudBestScore;
    }
    
    // 获取当前显示的最高分（现在就是云端分数）
    public int GetDisplayedBestScore()
    {
        return cloudBestScore;
    }
    
    // 获取综合最佳分数（本地和云端的最高值）
    public int GetOverallBestScore()
    {
        return Mathf.Max(bestScore, cloudBestScore);
    }
    
    // 获取玩家名字
    public string GetPlayerName()
    {
        return playerName;
    }
    
    // 检查游戏是否已开始（名字已输入）
    public bool IsGameStarted()
    {
        return isGameStarted;
    }
    
    // 编辑玩家名字（点击名字文本时调用）
    public void EditPlayerName()
    {
        // 暂停游戏
        Time.timeScale = 0f;
        
        // 清除状态信息
        ClearRegisterStatus();
        
        // 显示注册面板
        if (registerPanel != null)
        {
            registerPanel.SetActive(true);
        }
        
        // 隐藏玩家名字文本
        if (playerNameText != null)
        {
            playerNameText.gameObject.SetActive(false);
        }
        
        // 预填充当前名字并激活输入框
        if (playerNameInputField != null)
        {
            playerNameInputField.text = playerName; // 预填充当前名字
            playerNameInputField.Select();
            playerNameInputField.ActivateInputField();
        }
        
        Debug.Log("编辑玩家名字");
    }
    
    // 处理云端分数提交完成事件
    private void OnCloudScoreSubmitted(bool success, string message)
    {
        if (success)
        {
            Debug.Log($"云端分数提交成功: {message}");
            // 重新加载云端分数以确保显示最新数据
            LoadCloudScore();
        }
        else
        {
            Debug.LogWarning($"云端分数提交失败: {message}");
        }
    }
    
    void OnDestroy()
    {
        // 取消事件监听
        CloudScoreManager.OnScoreSubmitted -= OnCloudScoreSubmitted;
        CloudScoreManager.OnLeaderboardLoaded -= DisplayLeaderboard;
    }
}
