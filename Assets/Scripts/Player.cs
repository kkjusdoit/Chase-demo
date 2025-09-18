using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 4f; // 移动速度
    
    [Header("玩家设置")]
    public float imageWidth = 100f; // UI Image的宽度
    
    [Header("无敌状态视觉效果")]
    public float invincibleBlinkSpeed = 5f; // 无敌状态闪烁速度
    
    private RectTransform rectTransform;
    private float canvasWidth;
    private float currentDirection = 1f; // 当前移动方向：1为右，-1为左
    private UnityEngine.UI.Image playerImage; // 玩家的Image组件
    private Color originalColor; // 原始颜色
    
    void Start()
    {
        // 获取RectTransform组件
        rectTransform = GetComponent<RectTransform>();
        
        // 获取Image组件
        playerImage = GetComponent<UnityEngine.UI.Image>();
        if (playerImage != null)
        {
            originalColor = playerImage.color;
        }
        
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
            Debug.LogWarning("未找到Canvas，使用Screen.width作为备用");
        }
        
        // 设置初始位置在屏幕中央
        Vector3 startPos = rectTransform.anchoredPosition;
        startPos.x = 0;
        rectTransform.anchoredPosition = startPos;
        
        // 设置初始朝向
        UpdateScale(currentDirection);
    }

    public void SetPosition(float x)
    {
        rectTransform.anchoredPosition = new Vector2(x, 0f);
    }

    void Update()
    {
        // 只有在游戏未结束时才持续移动
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
        {
            ContinuousMove();
            UpdateInvincibleVisualEffect();
        }
    }
    
    // 更新无敌状态的视觉效果
    private void UpdateInvincibleVisualEffect()
    {
        if (GameManager.Instance != null && playerImage != null)
        {
            if (GameManager.Instance.IsPlayerInvincible())
            {
                // 无敌状态：闪烁效果
                float alpha = (Mathf.Sin(Time.time * invincibleBlinkSpeed) + 1f) * 0.3f + 0.4f; // 0.4-1.0之间闪烁
                Color currentColor = originalColor;
                currentColor.a = alpha;
                playerImage.color = currentColor;
            }
            else
            {
                // 正常状态：恢复原始透明度
                playerImage.color = originalColor;
            }
        }
    }
    
    // 持续移动
    private void ContinuousMove()
    {
        // 计算新位置
        Vector3 currentPos = rectTransform.anchoredPosition;
        currentPos.x += currentDirection * moveSpeed * Time.deltaTime * 100f; // 乘以100是因为UI坐标系的缩放
        
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

    // 改变移动方向
    public void ChangeDirection()
    {
        currentDirection = -currentDirection;
        UpdateScale(currentDirection);
        Debug.Log($"玩家改变方向，当前方向：{(currentDirection > 0 ? "右" : "左")}");
    }
    
    // 重置方向（游戏重启时使用）
    public void ResetDirection()
    {
        currentDirection = 1f; // 重置为向右
        UpdateScale(currentDirection);
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
    
    // 获取当前x坐标位置（用于碰撞检测）
    public float GetXPosition()
    {
        return rectTransform.anchoredPosition.x;
    }
}
