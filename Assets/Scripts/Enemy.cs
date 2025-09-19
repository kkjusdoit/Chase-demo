using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("移动设置")]
    public float minSpeed = 2f; // 最小移动速度
    public float maxSpeed = 6f; // 最大移动速度
    
    [Header("生命周期设置")]
    public float minLifeTime = 3f; // 最小生存时间
    public float maxLifeTime = 8f; // 最大生存时间
    public float respawnDelay = 1f; // 重新激活前的延迟时间
    
    [Header("方向变化设置")]
    public float minDirectionChangeTime = 2f; // 最小方向变化间隔
    public float maxDirectionChangeTime = 3f; // 最大方向变化间隔
    
    [Header("敌人设置")]
    public float imageWidth = 100f; // UI Image的宽度
    
    [Header("出场动画设置")]
    public float spawnAnimationDuration = 1.5f; // 出场动画持续时间
    public AnimationCurve spawnScaleCurve = AnimationCurve.EaseInOut(0, 0.3f, 1, 1f); // 缩放动画曲线
    public AnimationCurve spawnAlphaCurve = AnimationCurve.EaseInOut(0, 0f, 1, 1f); // 透明度动画曲线
    
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
    private Canvas parentCanvas; // 父级Canvas引用
    private float lastCanvasWidth; // 上一次的Canvas宽度，用于检测变化
    
    // 方向变化相关变量
    private float directionChangeTimer = 0f; // 方向变化计时器
    private float currentDirectionChangeTime; // 当前方向变化间隔时间
    
    // 出场动画相关变量
    private bool isSpawning = false; // 是否正在播放出场动画
    private Vector3 originalScale; // 原始缩放值
    private Color originalColor; // 原始颜色
    
    void Start()
    {
        // 获取RectTransform组件
        rectTransform = GetComponent<RectTransform>();
        
        // 获取Image组件
        enemyImage = GetComponent<UnityEngine.UI.Image>();
        
        // 保存原始缩放和颜色
        originalScale = rectTransform.localScale;
        if (enemyImage != null)
        {
            originalColor = enemyImage.color;
        }
        
        // 获取Canvas引用并初始化宽度
        parentCanvas = GetComponentInParent<Canvas>();
        UpdateCanvasWidth();
        
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
            // 检查Canvas宽度是否发生变化
            UpdateCanvasWidth();
            
            if (isRespawning)
            {
                HandleRespawn();
            }
            else if (!isSpawning) // 只有在不播放出场动画时才处理正常逻辑
            {
                HandleMovement();
                HandleLifeCycle();
                HandleDirectionChange();
            }
        }
    }
    
    // 更新Canvas宽度
    private void UpdateCanvasWidth()
    {
        if (parentCanvas != null)
        {
            RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
            float currentWidth = canvasRect.rect.width;
            
            // 如果宽度发生变化，更新并记录
            if (Mathf.Abs(currentWidth - lastCanvasWidth) > 0.1f)
            {
                canvasWidth = currentWidth;
                lastCanvasWidth = currentWidth;
                Debug.Log($"Enemy: Canvas宽度更新为 {canvasWidth}");
            }
        }
        else
        {
            // 如果找不到Canvas，使用默认宽度
            canvasWidth = 1080f;
            Debug.LogWarning("Enemy: 未找到Canvas，使用默认宽度1080");
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
        
        // 随机生成方向变化时间并重置计时器
        currentDirectionChangeTime = Random.Range(minDirectionChangeTime, maxDirectionChangeTime);
        directionChangeTimer = 0f;
        
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
        
        // 开始出场动画
        StartSpawnAnimation();
    }
    
    // 开始出场动画
    private void StartSpawnAnimation()
    {
        isSpawning = true;
        
        // 通知GameManager玩家进入无敌状态
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerInvincible(true, spawnAnimationDuration);
        }
        
        StartCoroutine(PlaySpawnAnimation());
    }
    
    // 播放出场动画协程
    private IEnumerator PlaySpawnAnimation()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < spawnAnimationDuration)
        {
            float progress = elapsedTime / spawnAnimationDuration;
            
            // 计算当前缩放值
            float scaleMultiplier = spawnScaleCurve.Evaluate(progress);
            Vector3 currentScale = originalScale;
            // 保持x轴的方向性（正负值）
            currentScale.x = currentScale.x > 0 ? 
                originalScale.x * scaleMultiplier : 
                -Mathf.Abs(originalScale.x) * scaleMultiplier;
            currentScale.y = originalScale.y * scaleMultiplier;
            rectTransform.localScale = currentScale;
            
            // 计算当前透明度
            if (enemyImage != null)
            {
                float alpha = spawnAlphaCurve.Evaluate(progress);
                Color currentColor = originalColor;
                currentColor.a = alpha;
                enemyImage.color = currentColor;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终状态正确
        rectTransform.localScale = originalScale;
        if (enemyImage != null)
        {
            enemyImage.color = originalColor;
        }
        
        // 动画结束
        isSpawning = false;
        
        // 通知GameManager玩家退出无敌状态
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerInvincible(false, 0f);
        }
    }
    
    // 检查敌人是否正在播放出场动画
    public bool IsSpawning()
    {
        return isSpawning;
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
    
    private void HandleDirectionChange()
    {
        directionChangeTimer += Time.deltaTime;
        
        // 如果达到方向变化时间，则随机改变方向
        if (directionChangeTimer >= currentDirectionChangeTime)
        {
            // 随机生成新的移动方向
            moveDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
            
            // 随机调整速度：加快或减慢10%
            float speedMultiplier = Random.Range(0, 2) == 0 ? 0.9f : 1.03f; // 90%或110%
            currentSpeed *= speedMultiplier;
            
            // 确保速度不会偏离基础速度太多（限制在原速度的70%-130%范围内）
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                float baseSpeed = Mathf.Abs(player.moveSpeed);
                currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed * 0.7f, baseSpeed * 1.3f);
            }
            
            // 更新sprite的scale
            UpdateScale(moveDirection);
            
            // 重置方向变化计时器并生成新的间隔时间
            directionChangeTimer = 0f;
            currentDirectionChangeTime = Random.Range(minDirectionChangeTime, maxDirectionChangeTime);
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
        return enemyImage != null && enemyImage.enabled && !isSpawning;
    }
}
