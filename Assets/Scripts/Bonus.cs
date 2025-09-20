using UnityEngine;
using UnityEngine.UI;

public class Bonus : MonoBehaviour
{
    [Header("道具设置")]
    public float imageWidth = 70f; // UI Image的宽度
    
    [Header("生命周期设置")]
    public float lifeTime = 10f; // 生存时间
    
    private RectTransform rectTransform;
    private UnityEngine.UI.Image bonusImage; // bonus的Image组件
    private float lifeTimer = 0f; // 生存计时器
    private bool isInitialized = false;
    private bool hasCustomPosition = false; // 标记是否设置了自定义位置
    
    void Start()
    {
        // 获取组件
        rectTransform = GetComponent<RectTransform>();
        bonusImage = GetComponent<UnityEngine.UI.Image>();
        
        // 注册到GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterBonus(gameObject);
        }
        
        isInitialized = true;
    }
    
    void Update()
    {
        if (!isInitialized || GameManager.Instance == null || GameManager.Instance.IsGameOver())
            return;
            
        UpdateLifeCycle();
    }
    
    // 初始化bonus道具位置（现在只设置位置，不需要方向）
    public void Initialize(float xPosition)
    {
        // 确保rectTransform已获取
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        if (rectTransform != null)
        {
            Vector3 pos = rectTransform.anchoredPosition;
            pos.x = xPosition;
            rectTransform.anchoredPosition = pos;
            hasCustomPosition = true;
            
            Debug.Log($"Bonus道具初始化：位置 {xPosition}（静态），实际位置：{rectTransform.anchoredPosition.x}");
        }
        else
        {
            Debug.LogError("无法获取RectTransform组件！");
        }
        
        lifeTimer = 0f;
    }
    
    // 处理生命周期
    private void UpdateLifeCycle()
    {
        lifeTimer += Time.deltaTime;
        
        // 如果超过生存时间，自动销毁
        if (lifeTimer >= lifeTime)
        {
            DestroyBonus();
        }
    }
    
    // 销毁bonus道具
    public void DestroyBonus()
    {
        Debug.Log("Bonus道具生命周期结束，自动销毁");
        Destroy(gameObject);
    }
    
    // 获取当前x坐标位置
    public float GetXPosition()
    {
        return rectTransform.anchoredPosition.x;
    }
} 