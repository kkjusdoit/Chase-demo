using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Enemy : MonoBehaviour
{
    [Header("移动设置")]
    public float minSpeed = 2f; // 最小移动速度
    public float maxSpeed = 6f; // 最大移动速度
    
    [Header("生命周期设置")]
    public float minLifeTime = 3f; // 最小生存时间
    public float maxLifeTime = 8f; // 最大生存时间
    public float respawnDelay = 1f; // 重新激活前的延迟时间
    
    [Header("得分系统")]
    public TextMeshProUGUI scoreText; // 显示分数的UI文本
    
    private RectTransform rectTransform;
    private float screenWidth;
    private float imageWidth = 100f; // UI Image的宽度
    private float currentSpeed;
    private float moveDirection; // 移动方向：-1为左，1为右
    private Player player; // 引用Player用于碰撞检测
    private bool gameOver = false;
    private float currentLifeTime; // 当前生存时间
    private float lifeTimer; // 生存计时器
    private bool isRespawning = false; // 是否正在重生过程中
    private float respawnTimer = 0f; // 重生计时器
    private UnityEngine.UI.Image enemyImage; // 敌人的Image组件
    private static int score = 0; // 静态分数，所有Enemy实例共享
    private bool hasScored = false; // 标记当前轮次是否已经得分

    public Button button;
    
    void Start()
    {
        button.onClick.AddListener(OnButtonClick);
        // 获取RectTransform组件
        rectTransform = GetComponent<RectTransform>();
        
        // 获取Image组件
        enemyImage = GetComponent<UnityEngine.UI.Image>();
        
        // 获取屏幕宽度
        screenWidth = Screen.width;
        
        // 查找Player对象
        player = FindObjectOfType<Player>();
        
        // 根据玩家速度调整敌人的最大速度
        if (player != null)
        {
            float playerSpeed = player.moveSpeed;
            maxSpeed = Mathf.Min(maxSpeed, playerSpeed); // 确保敌人最大速度不超过玩家速度
        }
        
        // 随机初始化
        InitializeRandomly(true);
        
        // 更新分数显示
        UpdateScoreDisplay();
    }

    void OnButtonClick()
    {
        RestartGame();
    }

    void Update()
    {
        if (!gameOver)
        {
            if (isRespawning)
            {
                HandleRespawn();
            }
            else
            {
                HandleMovement();
                CheckCollision();
                HandleLifeCycle();
            }
        }
    }
    
    private void InitializeRandomly(bool isFirst = false)
    {
        // 随机生成初始位置（在屏幕宽度范围内）
        float halfImageWidth = imageWidth * 0.5f;
        float minX = -screenWidth * 0.5f + halfImageWidth;
        float maxX = screenWidth * 0.5f - halfImageWidth;
        
        Vector3 randomPos = rectTransform.anchoredPosition;
        randomPos.x = Random.Range(minX, maxX);
        randomPos.x = 400f;
        if (isFirst)
        {
            rectTransform.anchoredPosition = randomPos;
        }
        
        // 随机生成移动速度
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        
        // 随机生成移动方向
        moveDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
        
        // 随机生成生存时间并重置计时器
        currentLifeTime = Random.Range(minLifeTime, maxLifeTime);
        lifeTimer = 0f;
        
        // 确保敌人可见
        if (enemyImage != null)
        {
            enemyImage.enabled = true;
        }
        
        // 重置重生状态
        isRespawning = false;
        respawnTimer = 0f;
        
        // 重置得分标记
        hasScored = false;
    }
    
    private void HandleMovement()
    {
        // 计算新位置
        Vector3 currentPos = rectTransform.anchoredPosition;
        currentPos.x += moveDirection * currentSpeed * Time.deltaTime * 100f; // 乘以100是因为UI坐标系的缩放
        
        // 处理屏幕环绕效果
        float halfImageWidth = imageWidth * 0.5f;
        float minX = -screenWidth * 0.5f - halfImageWidth;
        float maxX = screenWidth * 0.5f + halfImageWidth;
        
        if (currentPos.x < minX)
        {
            // 从左边出去，从右边进来
            currentPos.x = screenWidth * 0.5f + halfImageWidth;
        }
        else if (currentPos.x > maxX)
        {
            // 从右边出去，从左边进来
            currentPos.x = -screenWidth * 0.5f - halfImageWidth;
        }
        
        rectTransform.anchoredPosition = currentPos;
    }
    
    private void CheckCollision()
    {
        // 只有在敌人可见时才检测碰撞
        if (player != null && enemyImage != null && enemyImage.enabled)
        {
            float playerX = player.GetXPosition();
            float enemyX = rectTransform.anchoredPosition.x;
            
            // 检测x坐标差值是否小于100
            if (Mathf.Abs(playerX - enemyX) < 100f)
            {
                GameOver();
            }
        }
    }
    
    private void HandleLifeCycle()
    {
        lifeTimer += Time.deltaTime;
        
        // 如果达到生存时间，则开始重生过程
        if (lifeTimer >= currentLifeTime)
        {
            StartRespawn();
        }
    }
    
    private void StartRespawn()
    {
        // 如果玩家成功躲过这一轮，增加分数
        if (!hasScored && !gameOver)
        {
            score++;
            hasScored = true;
            UpdateScoreDisplay();
            Debug.Log("玩家躲过一轮攻击！当前分数：" + score);
        }
        
        // 隐藏敌人（通过禁用Image组件）
        if (enemyImage != null)
        {
            enemyImage.enabled = false;
        }
        
        // 开始重生计时
        isRespawning = true;
        respawnTimer = 0f;
    }
    
    private void HandleRespawn()
    {
        respawnTimer += Time.deltaTime;
        
        // 如果重生延迟时间到了，重新激活敌人
        if (respawnTimer >= respawnDelay)
        {
            InitializeRandomly();
        }
    }
    
    private void GameOver()
    {
        gameOver = true;
        
        // 暂停游戏
        Time.timeScale = 0f;
        
        // 输出游戏结束信息
        Debug.Log("游戏结束！玩家与敌人发生碰撞！最终分数：" + score);
        
        // 这里可以添加更多游戏结束的逻辑，比如显示游戏结束UI等
    }
    
    // 获取当前x坐标位置
    public float GetXPosition()
    {
        return rectTransform.anchoredPosition.x;
    }
    
    // 重新开始游戏的方法
    public void RestartGame()
    {
        rectTransform.anchoredPosition = new Vector2(300f, 0f);
        player.SetPosition(0f);

        // 重新根据玩家速度调整敌人的最大速度
        if (player != null)
        {
            float playerSpeed = player.moveSpeed;
            maxSpeed = Mathf.Min(6f, playerSpeed); // 使用原始最大速度6f和玩家速度的最小值
        }

        // 重置分数
        score = 0;
        UpdateScoreDisplay();

        gameOver = false;
        Time.timeScale = 1f;
        
        // 重新初始化敌人
        InitializeRandomly(true);
    }
    
    // 更新分数显示
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
    
    // 获取当前分数（供外部访问）
    public static int GetScore()
    {
        return score;
    }
}
