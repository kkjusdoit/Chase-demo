using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("移动设置")]
    public float minSpeed = 2f; // 最小移动速度
    public float maxSpeed = 6f; // 最大移动速度
    
    [Header("生命周期设置")]
    public float minLifeTime = 3f; // 最小生存时间
    public float maxLifeTime = 8f; // 最大生存时间
    public float respawnDelay = 1f; // 重新激活前的延迟时间
    
    [Header("敌人设置")]
    public float imageWidth = 100f; // UI Image的宽度
    
    private RectTransform rectTransform;
    private float canvasWidth;
    private float currentSpeed;
    private float moveDirection; // 移动方向：-1为左，1为右
    private float currentLifeTime; // 当前生存时间
    private float lifeTimer; // 生存计时器
    private bool isRespawning = false; // 是否正在重生过程中
    private float respawnTimer = 0f; // 重生计时器
    private UnityEngine.UI.Image enemyImage; // 敌人的Image组件
    private bool hasScored = false; // 标记当前轮次是否已经得分
    
    void Start()
    {
        // 获取RectTransform组件
        rectTransform = GetComponent<RectTransform>();
        
        // 获取Image组件
        enemyImage = GetComponent<UnityEngine.UI.Image>();
        
        // 获取Canvas的实际宽度而不是屏幕宽度
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasWidth = canvasRect.rect.width;
        }
        else
        {
            // 如果找不到Canvas，使用屏幕宽度作为备用
            canvasWidth = Screen.width;
            Debug.LogWarning("Enemy: 未找到Canvas，使用Screen.width作为备用");
        }
        
        // 根据玩家速度调整敌人的速度，使其与玩家保持一致
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            float playerSpeed = player.moveSpeed;
            maxSpeed = Mathf.Abs(playerSpeed); // 使用玩家速度的绝对值
            minSpeed = maxSpeed; // 最小速度也设为相同值，确保一致性
        }
        
        // 随机初始化
        InitializeRandomly(true);
    }

    void Update()
    {
        // 只有在游戏未结束时才处理敌人逻辑
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
        {
            if (isRespawning)
            {
                HandleRespawn();
            }
            else
            {
                HandleMovement();
                HandleLifeCycle();
            }
        }
    }
    
    private void InitializeRandomly(bool isFirst = false)
    {
        // 随机生成初始位置（在Canvas宽度范围内）
        float halfImageWidth = imageWidth * 0.5f;
        float minX = -canvasWidth * 0.5f + halfImageWidth;
        float maxX = canvasWidth * 0.5f - halfImageWidth;
        
        Vector3 randomPos = rectTransform.anchoredPosition;
        randomPos.x = Random.Range(minX, maxX);
        randomPos.x = 400f;
        if (isFirst)
        {
            rectTransform.anchoredPosition = randomPos;
        }
        
        // 设置移动速度与玩家一致
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            currentSpeed = player.moveSpeed;
        }
        else
        {
            currentSpeed = Random.Range(minSpeed, maxSpeed); // 备用方案
        }
        
        // 随机生成移动方向
        moveDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
        
        // 根据移动方向设置初始scale
        UpdateScale(moveDirection);
        
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
        // 根据移动方向调整scale
        UpdateScale(moveDirection);
        
        // 计算新位置
        Vector3 currentPos = rectTransform.anchoredPosition;
        currentPos.x += moveDirection * currentSpeed * Time.deltaTime * 100f; // 乘以100是因为UI坐标系的缩放
        
        // 处理Canvas环绕效果
        float halfImageWidth = imageWidth * 0.5f;
        float minX = -canvasWidth * 0.5f - halfImageWidth;
        float maxX = canvasWidth * 0.5f + halfImageWidth;
        
        if (currentPos.x < minX)
        {
            // 从左边出去，从右边进来
            currentPos.x = canvasWidth * 0.5f + halfImageWidth;
        }
        else if (currentPos.x > maxX)
        {
            // 从右边出去，从左边进来
            currentPos.x = -canvasWidth * 0.5f - halfImageWidth;
        }
        
        rectTransform.anchoredPosition = currentPos;
    }
    
    // 根据移动方向更新scale
    private void UpdateScale(float direction)
    {
        Vector3 scale = rectTransform.localScale;
        
        if (direction < 0)
        {
            // 向左移动，翻转sprite
            scale.x = Mathf.Abs(scale.x);
        }
        else if (direction > 0)
        {
            // 向右移动，正常显示
            scale.x = -Mathf.Abs(scale.x);
        }
        
        rectTransform.localScale = scale;
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
        // 如果玩家成功躲过这一轮，通过GameManager增加分数
        if (!hasScored && GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(1);
            hasScored = true;
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
    
    
    // 获取当前x坐标位置
    public float GetXPosition()
    {
        return rectTransform.anchoredPosition.x;
    }
    
    // 重置敌人状态的方法（由GameManager调用）
    public void ResetEnemy()
    {
        // 重置位置
        rectTransform.anchoredPosition = new Vector2(400f, 0f);

        // 重新根据玩家速度调整敌人的速度，使其与玩家保持一致
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            float playerSpeed = player.moveSpeed;
            maxSpeed = Mathf.Abs(playerSpeed); // 使用玩家速度的绝对值
            minSpeed = maxSpeed; // 确保速度一致性
            currentSpeed = maxSpeed; // 立即更新当前速度
        }
        
        // 重新初始化敌人
        InitializeRandomly(true);
    }
    
    // 检查敌人是否可见
    public bool IsVisible()
    {
        return enemyImage != null && enemyImage.enabled;
    }
}
