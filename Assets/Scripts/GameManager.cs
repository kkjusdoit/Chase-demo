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
    public Text scoreText;
    public Text maxScoreText; // 现在用作云端分数显示
    public Text enmeySpeedText;
    public Text playerSpeedText;
    
    
    [Header("云端分数")]
    public CloudScoreManager cloudScoreManager;

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
    
    // PlayerPrefs键名
    private const string BEST_SCORE_KEY = "BestScore";
    
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
        
        // 设置玩家名字输入框事件
        if (playerNameInputField != null)
        {
            playerNameInputField.onEndEdit.AddListener(OnPlayerNameSubmitted);
        }
        
        // 设置玩家名字文本点击事件（如果有Button组件）
        if (playerNameText != null)
        {
            Button nameButton = playerNameText.GetComponent<Button>();
            if (nameButton != null)
            {
                nameButton.onClick.AddListener(EditPlayerName);
            }
        }
        
        // 检查是否已有保存的玩家名字
        CheckSavedPlayerName();
        
        // 监听云端分数提交事件
        CloudScoreManager.OnScoreSubmitted += OnCloudScoreSubmitted;
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
            Debug.Log($"收集到bonus道具！当前分数：{currentScore}");
            
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
    
    // 检查是否已有保存的玩家名字
    private void CheckSavedPlayerName()
    {
        string savedName = PlayerPrefs.GetString("PlayerName", "");
        
        if (!string.IsNullOrEmpty(savedName))
        {
            // 有保存的名字，显示名字文本并直接开始游戏
            playerName = savedName;
            
            // 隐藏输入框，显示玩家名字文本
            if (playerNameInputField != null)
            {
                playerNameInputField.gameObject.SetActive(false);
            }
            
            if (playerNameText != null)
            {
                playerNameText.gameObject.SetActive(true);
                playerNameText.text = $"Player: {playerName}";
            }
            
            StartGame();
        }
        else
        {
            // 没有保存的名字，显示输入框
            ShowPlayerNameInput();
        }
    }
    
    // 显示玩家名字输入界面
    private void ShowPlayerNameInput()
    {
        isGameStarted = false;
        Time.timeScale = 0f; // 暂停游戏
        
        // 显示输入框，隐藏显示文本
        if (playerNameInputField != null)
        {
            playerNameInputField.gameObject.SetActive(true);
            playerNameInputField.Select();
            playerNameInputField.ActivateInputField();
            playerNameInputField.text = "";
        }
        
        if (playerNameText != null)
        {
            playerNameText.gameObject.SetActive(false);
        }
        
        Debug.Log("请输入玩家名字开始游戏");
    }
    
    // 处理玩家名字提交
    private void OnPlayerNameSubmitted(string inputName)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitPlayerName();
        }
    }
    
    // 提交玩家名字
    public void SubmitPlayerName()
    {
        if (playerNameInputField == null) return;
        
        string inputName = playerNameInputField.text.Trim();
        
        if (string.IsNullOrEmpty(inputName))
        {
            Debug.Log("玩家名字不能为空！");
            return;
        }
        
        // 过滤和验证名字
        inputName = FilterPlayerName(inputName);
        
        if (string.IsNullOrEmpty(inputName))
        {
            Debug.Log("请输入有效的玩家名字！");
            return;
        }
        
        // 保存玩家名字
        playerName = inputName;
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
        
        // 隐藏输入框，显示玩家名字文本
        if (playerNameInputField != null)
        {
            playerNameInputField.gameObject.SetActive(false);
        }
        
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
        
        // 如果游戏已经开始过，直接恢复游戏；否则开始新游戏
        if (isGameStarted)
        {
            // 恢复游戏时间
            if (!isGameOver)
            {
                Time.timeScale = 1f;
            }
        }
        else
        {
            // 开始新游戏
            StartGame();
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
        
        // 加载最佳分数
        LoadBestScore();
        
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
    
    // 加载最佳分数
    private void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        Debug.Log($"加载最佳分数：{bestScore}");
    }
    
    // 保存最佳分数
    private void SaveBestScore()
    {
        PlayerPrefs.SetInt(BEST_SCORE_KEY, bestScore);
        PlayerPrefs.Save();
        Debug.Log($"保存最佳分数：{bestScore}");
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
        
        // 更新本地最佳分数
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            SaveBestScore();
            Debug.Log($"新纪录！本地最佳分数更新为：{bestScore}");
            isNewRecord = true;
        }
        
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
        
        // 显示输入框，隐藏文本
        if (playerNameInputField != null)
        {
            playerNameInputField.gameObject.SetActive(true);
            playerNameInputField.text = playerName; // 预填充当前名字
            playerNameInputField.Select();
            playerNameInputField.ActivateInputField();
        }
        
        if (playerNameText != null)
        {
            playerNameText.gameObject.SetActive(false);
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
    }
}
