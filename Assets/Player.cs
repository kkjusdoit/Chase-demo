using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f; // 移动速度
    
    private RectTransform rectTransform;
    private float canvasWidth;
    private float imageWidth = 100f; // UI Image的宽度
    
    void Start()
    {
        // 获取RectTransform组件
        rectTransform = GetComponent<RectTransform>();
        
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
    }

    public void SetPosition(float x)
    {
        rectTransform.anchoredPosition = new Vector2(x, 0f);
    }

    void Update()
    {
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        
        if (horizontalInput != 0)
        {
            // 计算新位置
            Vector3 currentPos = rectTransform.anchoredPosition;
            currentPos.x += horizontalInput * moveSpeed * Time.deltaTime * 100f; // 乘以100是因为UI坐标系的缩放
            
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
    }
    
    // 获取当前x坐标位置（用于碰撞检测）
    public float GetXPosition()
    {
        return rectTransform.anchoredPosition.x;
    }
}
