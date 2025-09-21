using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FirebaseDiagnostic : MonoBehaviour
{
    [Header("诊断UI")]
    public Text diagnosticText;
    public Button runDiagnosticButton;
    public Button testConnectionButton;
    public ScrollRect scrollRect;
    
    private string diagnosticLog = "";
    
    void Start()
    {
        if (runDiagnosticButton != null)
            runDiagnosticButton.onClick.AddListener(RunCompleteDiagnostic);
            
        if (testConnectionButton != null)
            testConnectionButton.onClick.AddListener(TestFirebaseConnection);
    }
    
    public void RunCompleteDiagnostic()
    {
        DiagnosticRoutine();
    }
    
    public void TestFirebaseConnection()
    {
        TestConnectionRoutine();
    }
    
    async void DiagnosticRoutine()
    {
        ClearLog();
        LogMessage("🔧 开始Firebase诊断...");
        
        // 1. 检查平台
        LogMessage($"📱 当前平台: {Application.platform}");
        LogMessage($"🌐 是否WebGL: {Application.platform == RuntimePlatform.WebGLPlayer}");
        
        // 2. 检查FirebaseManager
        if (FirebaseManager.Instance == null)
        {
            LogMessage("❌ FirebaseManager实例未找到");
            return;
        }
        
        LogMessage("✅ FirebaseManager实例存在");
        LogMessage($"📊 初始化状态: {FirebaseManager.Instance.initializationStatus}");
        LogMessage($"🔥 是否已初始化: {FirebaseManager.Instance.isInitialized}");
        
        // 3. 等待初始化完成
        LogMessage("⏳ 等待Firebase初始化...");
        float timeout = 30f; // 30秒超时
        float elapsed = 0f;
        
        while (!FirebaseManager.Instance.isInitialized && elapsed < timeout)
        {
            elapsed += 0.5f;
            LogMessage($"⌛ 等待中... {elapsed:F1}s - 状态: {FirebaseManager.Instance.initializationStatus}");
            await System.Threading.Tasks.Task.Delay(500);
        }
        
        if (!FirebaseManager.Instance.isInitialized)
        {
            LogMessage("❌ Firebase初始化超时");
            LogMessage("💡 可能的解决方案:");
            LogMessage("   1. 检查网络连接");
            LogMessage("   2. 检查Firebase配置文件");
            LogMessage("   3. 检查Firebase控制台设置");
            return;
        }
        
        LogMessage("✅ Firebase初始化成功");
        
        // 4. 测试匿名登录
        LogMessage("🔄 测试匿名登录...");
        bool anonymousResult = await FirebaseManager.Instance.TestAnonymousLogin();
        
        if (anonymousResult)
        {
            LogMessage("✅ 匿名登录测试成功");
        }
        else
        {
            LogMessage("❌ 匿名登录测试失败");
            LogMessage("💡 可能原因:");
            LogMessage("   1. Firebase控制台未启用匿名认证");
            LogMessage("   2. 网络连接问题");
            LogMessage("   3. WebGL平台配置问题");
        }
        
        // 5. 检查当前用户状态
        if (FirebaseManager.Instance.IsUserLoggedIn())
        {
            LogMessage($"👤 当前用户: {FirebaseManager.Instance.GetCurrentUserEmail()}");
        }
        else
        {
            LogMessage("👤 当前无用户登录");
        }
        
        LogMessage("🏁 诊断完成");
    }
    
    async void TestConnectionRoutine()
    {
        ClearLog();
        LogMessage("🌐 测试Firebase连接...");
        
        if (FirebaseManager.Instance == null)
        {
            LogMessage("❌ FirebaseManager未找到");
            return;
        }
        
        if (!FirebaseManager.Instance.isInitialized)
        {
            LogMessage("❌ Firebase未初始化");
            return;
        }
        
        // 测试认证连接
        LogMessage("🔄 测试认证服务连接...");
        bool authResult = await FirebaseManager.Instance.TestAnonymousLogin();
        
        if (authResult)
        {
            LogMessage("✅ 认证服务连接正常");
            
            // 测试数据库连接
            LogMessage("🔄 测试数据库连接...");
            bool dbResult = await FirebaseManager.Instance.SaveBestScore(999);
            
            if (dbResult)
            {
                LogMessage("✅ 数据库连接正常");
                
                // 读取测试
                int score = await FirebaseManager.Instance.GetMyBestScore();
                LogMessage($"📊 读取测试分数: {score}");
            }
            else
            {
                LogMessage("❌ 数据库连接失败");
            }
        }
        else
        {
            LogMessage("❌ 认证服务连接失败");
        }
        
        LogMessage("🏁 连接测试完成");
    }
    
    void LogMessage(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logEntry = $"[{timestamp}] {message}";
        
        diagnosticLog += logEntry + "\n";
        
        if (diagnosticText != null)
        {
            diagnosticText.text = diagnosticLog;
        }
        
        // 自动滚动到底部
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
        
        // 同时输出到Unity控制台
        Debug.Log(logEntry);
    }
    
    void ClearLog()
    {
        diagnosticLog = "";
        if (diagnosticText != null)
        {
            diagnosticText.text = "";
        }
    }
} 