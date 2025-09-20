using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("游戏对象引用")]
    public Player player;
    public Enemy enemy;

    public Button restartButton;
    public Button changeDirectionButton;
    public Text scoreText;
    public Text maxScoreText;
    public Text enmeySpeedText;
    public Text playerSpeedText;
    
    [Header("碰撞检测设置")]
    public float collisionDistance = 100f; // 碰撞检测距离
    public float bonusCollisionDistance = 70f; // bonus碰撞检测距离
    
    [Header("游戏状态")]
    private bool isGameOver = false;
    private int currentScore = 0;
    private int bestScore = 0; // 最佳分数
    private bool isPlayerInvincible = false; // 玩家无敌状态
    private float invincibleTimer = 0f; // 无敌计时器


    
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
        InitializeGame();
        
        // 绑定重启按钮事件
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        // 自动查找游戏对象（如果没有手动分配）
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
        
        if (enemy == null)
        {
            enemy = FindObjectOfType<Enemy>();
        }
        
        // 更新分数显示
        UpdateScoreDisplay();
    }
    
    void Update()
    {
        if (!isGameOver)
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
    
    private void InitializeGame()
    {
        isGameOver = false;
        currentScore = 0;
        Time.timeScale = 1f;
        
        // 加载最佳分数
        LoadBestScore();
        
        // 隐藏重启按钮
        SetRestartButtonVisible(false);
        
        // 清理所有bonus道具
        ClearAllBonuses();
        
        UpdateScoreDisplay();
        Debug.Log("游戏初始化完成");
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
        SetRestartButtonVisible(true);
        
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
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            SaveBestScore();
            Debug.Log($"新纪录！最佳分数更新为：{bestScore}");
        }
        
        // 更新UI显示
        UpdateScoreDisplay();
    }
    
    public void RestartGame()
    {
        Debug.Log("重新开始游戏");
        
        // 重置游戏状态
        isGameOver = false;
        currentScore = 0;
        Time.timeScale = 1f;
        
        // 隐藏重启按钮
        SetRestartButtonVisible(false);
        
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
        
        if (maxScoreText != null)
        {
            maxScoreText.text = "Best: " + bestScore;
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
        if (player != null)
        {
            player.ChangeDirection();
        }
    }
    
    // 控制重启按钮的显示和隐藏
    private void SetRestartButtonVisible(bool visible)
    {
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(visible);
            Debug.Log($"重启按钮{(visible ? "显示" : "隐藏")}");
        }
    }
}
