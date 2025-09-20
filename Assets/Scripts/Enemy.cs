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
    
    [Header("Bonus道具生成设置")]
    public int bonusCount = 3; // 每次生成的bonus道具数量
    public float bonusSpacing = 200f; // bonus道具之间的间距
    public GameObject bonusPrefab; // bonus道具预制体，需要在Inspector中指定
    public float bonusRandomOffset = 50f; // bonus位置的随机偏移范围
    
    private RectTransform rectTransform;
    private float canvasWidth;
    private float currentSpeed;
    private float moveDirection; // 移动方向：-1为左，1为右
    private float currentLifeTime; // 当前生存时间
    private float lifeTimer; // 生存计时器
    private bool isRespawning = false; // 是否正在重生过程中
    private float respawnTimer = 0f; // 重生计时器
    private UnityEngine.UI.Image enemyImage; // 敌人的Image组件
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
        
        // 根据玩家的动态速度调整敌人的速度，使其与玩家保持一致
        UpdateSpeedFromPlayer();
        
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
                UpdateSpeedFromPlayer(); // 重新计算移动速度
                Debug.Log($"Enemy: Canvas宽度更新为 {canvasWidth}，重新计算速度");
            }
        }
        else
        {
            // 如果找不到Canvas，使用默认宽度
            canvasWidth = 1080f;
            Debug.LogWarning("Enemy: 未找到Canvas，使用默认宽度1080");
        }
    }
    
    // 从玩家获取动态计算的移动速度
    private void UpdateSpeedFromPlayer()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            float playerSpeed = player.GetMoveSpeed();
            maxSpeed = playerSpeed;
            minSpeed = playerSpeed;
            currentSpeed = playerSpeed;
            Debug.Log($"Enemy: 从Player获取速度 {playerSpeed}");
        }
        else
        {
            // 如果找不到玩家，使用默认速度
            currentSpeed = 4f;
            Debug.LogWarning("Enemy: 未找到Player，使用默认速度4");
        }
    }
    
    private void InitializeRandomly(bool isFirst = false)
    {
        // 获取玩家当前位置
        Player player = FindObjectOfType<Player>();
        Vector3 randomPos = rectTransform.anchoredPosition;
        
        if (player != null)
        {
            float playerX = player.GetXPosition();
            
            // 随机选择在玩家左边或右边300单位
            bool spawnOnLeft = Random.Range(0, 2) == 0;
            randomPos.x = playerX + (spawnOnLeft ? -300f : 300f);
            
            Debug.Log($"Enemy重生位置：玩家在{playerX}，Enemy在{randomPos.x}（距离300单位）");
        }
        else
        {
            // 如果找不到玩家，使用默认位置
            randomPos.x = 400f;
            Debug.LogWarning("未找到Player，使用默认位置400");
        }
        
        if (isFirst)
        {
            rectTransform.anchoredPosition = randomPos;
        }
        
        // 设置移动速度与玩家一致
        UpdateSpeedFromPlayer();
        
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
        
        // 生成bonus道具
        SpawnBonuses();
        
        // 开始出场动画
        StartSpawnAnimation();
    }
    
    // 生成bonus道具
    private void SpawnBonuses()
    {
        if (bonusPrefab == null || GameManager.Instance == null)
        {
            Debug.LogWarning("bonusPrefab未设置或GameManager不存在，无法生成bonus道具");
            return;
        }
        
        // 清理旧的bonus道具
        GameManager.Instance.ClearAllBonuses();
        
        // 根据屏幕宽度和bonus数量动态计算间距
        float usableWidth = canvasWidth * 0.8f; // 使用屏幕宽度的80%，留出边距
        float dynamicSpacing;
        
        if (bonusCount <= 1)
        {
            dynamicSpacing = 0f; // 只有一个bonus时不需要间距
        }
        else
        {
            dynamicSpacing = usableWidth / (bonusCount - 1); // 平均分布
        }
        
        // 计算起始位置（屏幕中心向左偏移）
        float startX = -usableWidth * 0.5f;
        
        Debug.Log($"动态生成{bonusCount}个bonus道具，屏幕宽度：{canvasWidth}，可用宽度：{usableWidth}，计算间距：{dynamicSpacing}，起始位置：{startX}");
        
        // 生成bonus道具
        for (int i = 0; i < bonusCount; i++)
        {
            // 创建bonus道具
            GameObject bonusObj = Instantiate(bonusPrefab, transform.parent);
            bonusObj.SetActive(true);
            
            // 计算bonus位置：在屏幕宽度内均匀分布
            float bonusX;
            if (bonusCount == 1)
            {
                // 只有一个bonus，放在屏幕中心
                bonusX = 0f;
            }
            else
            {
                // 多个bonus：在屏幕宽度内均匀分布
                bonusX = startX + i * dynamicSpacing;
            }
            
            // 添加随机偏移
            float originalX = bonusX;
            bonusX += Random.Range(-bonusRandomOffset, bonusRandomOffset);
            
            Debug.Log($"准备生成bonus {i + 1}，原始位置：{originalX:F1}，随机偏移后：{bonusX:F1}");
            
            // 获取bonus脚本组件并初始化
            Bonus bonusScript = bonusObj.GetComponent<Bonus>();
            if (bonusScript != null)
            {
                // 只设置位置，不需要方向（因为bonus是静态的）
                bonusScript.Initialize(bonusX);
            }
            else
            {
                // 如果没有Bonus脚本，直接设置位置
                RectTransform bonusRect = bonusObj.GetComponent<RectTransform>();
                if (bonusRect != null)
                {
                    Vector3 bonusPos = bonusRect.anchoredPosition;
                    bonusPos.x = bonusX;
                    bonusRect.anchoredPosition = bonusPos;
                    Debug.Log($"直接设置bonus位置：{bonusX}，实际位置：{bonusRect.anchoredPosition.x}");
                }
            }
            
            Debug.Log($"生成静态bonus道具 {i + 1}/{bonusCount} 目标位置：{bonusX}");
        }
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
                float baseSpeed = player.GetMoveSpeed();
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
        // 移除原有的自动加分逻辑，现在只通过bonus道具加分
        
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
    
    // 获取当前移动速度（供GameManager显示使用）
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    // 重置敌人状态的方法（由GameManager调用）
    public void ResetEnemy()
    {
        // 重新根据玩家的动态速度调整敌人的速度，使其与玩家保持一致
        UpdateSpeedFromPlayer();
        
        // 重新初始化敌人（使用距离player 300单位的逻辑）
        InitializeRandomly(true);
    }
    
    // 检查敌人是否可见
    public bool IsVisible()
    {
        return enemyImage != null && enemyImage.enabled && !isSpawning;
    }
}
