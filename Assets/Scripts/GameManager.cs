using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("游戏对象引用")]
    public Player player;
    public Enemy enemy;
    public Button restartButton;
    public Button changeDirectionButton;
    public Text scoreText;
    
    [Header("碰撞检测设置")]
    public float collisionDistance = 100f; // 碰撞检测距离
    
    [Header("游戏状态")]
    private bool isGameOver = false;
    private int currentScore = 0;
    private bool isPlayerInvincible = false; // 玩家无敌状态
    private float invincibleTimer = 0f; // 无敌计时器
    
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
        }
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
        UpdateScoreDisplay();
        Debug.Log("游戏初始化完成");
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
        
        // 暂停游戏
        Time.timeScale = 0f;
        
        // 输出游戏结束信息
        Debug.Log($"游戏结束！玩家与敌人发生碰撞！最终分数：{currentScore}");
        
        // 这里可以添加游戏结束UI的显示逻辑
        // 例如：显示游戏结束面板、播放音效等
    }
    
    public void RestartGame()
    {
        Debug.Log("重新开始游戏");
        
        // 重置游戏状态
        isGameOver = false;
        currentScore = 0;
        Time.timeScale = 1f;
        
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
    
    public void AddScore(int points = 1)
    {
        if (!isGameOver)
        {
            currentScore += points;
            UpdateScoreDisplay();
            Debug.Log($"玩家躲过一轮攻击！当前分数：{currentScore}");
        }
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }
    
    // 获取当前分数
    public int GetCurrentScore()
    {
        return currentScore;
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
}
