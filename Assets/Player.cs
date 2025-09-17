using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f; // 移动速度
    
    [Header("玩家设置")]
    public float imageWidth = 100f; // UI Image的宽度
    
    private RectTransform rectTransform;
    private float canvasWidth;
    
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
        // 只有在游戏未结束时才处理移动
        if (GameManager.Instance != null && !GameManager.Instance.IsGameOver())
        {
            HandleMovement();
        }
    }
    
    private void HandleMovement()
    {
        float horizontalInput = 0f;
        
        // 检测键盘输入（桌面端）
        horizontalInput = Input.GetAxis("Horizontal");
        
        // 检测触摸输入（移动端）
        if (horizontalInput == 0f && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                // 获取触摸位置相对于屏幕中心的偏移
                Vector2 touchPos = touch.position;
                float screenCenterX = Screen.width * 0.5f;
                float touchOffset = (touchPos.x - screenCenterX) / screenCenterX;
                
                // 将触摸偏移转换为移动输入 (-1 到 1)
                horizontalInput = Mathf.Clamp(touchOffset, -1f, 1f);
            }
        }
        
        // 检测鼠标输入（备用方案，也适用于移动端点击）
        if (horizontalInput == 0f && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            float screenCenterX = Screen.width * 0.5f;
            float mouseOffset = (mousePos.x - screenCenterX) / screenCenterX;
            horizontalInput = Mathf.Clamp(mouseOffset, -1f, 1f);
        }
        
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
