using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("UI组件")]
    public InputField emailInput;
    public InputField passwordInput;
    public Button loginButton;
    public Button registerButton;
    public Button testScoreButton;
    public Button testAnonymousLoginButton;
    
    [Header("诊断功能")]
    public Button diagnosticButton;
    public Button quickTestButton;
    
    public Text statusText;
    
    [Header("测试用")]
    public int testScore = 100;
    
    void Start()
    {
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginClick);
        
        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegisterClick);
            
        if (testScoreButton != null)
            testScoreButton.onClick.AddListener(OnTestScoreClick);
        
        if (testAnonymousLoginButton != null)
            testAnonymousLoginButton.onClick.AddListener(OnTestAnonymousLoginClick);
            
        // 新增诊断按钮
        if (diagnosticButton != null)
            diagnosticButton.onClick.AddListener(OnDiagnosticClick);
            
        if (quickTestButton != null)
            quickTestButton.onClick.AddListener(OnQuickTestClick);
        
    // 等待Firebase初始化
        StartCoroutine(WaitForFirebaseInit());
    }
    
    System.Collections.IEnumerator WaitForFirebaseInit()
    {
        // 显示初始化状态
        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.isInitialized)
        {
            if (FirebaseManager.Instance != null && statusText != null)
            {
                statusText.text = $"Firebase状态: {FirebaseManager.Instance.initializationStatus}";
            }
            else if (statusText != null)
            {
                statusText.text = "等待Firebase Manager初始化...";
            }
            yield return new WaitForSeconds(0.5f);
        }
        
        if (statusText != null)
            statusText.text = "Firebase已就绪，可以登录";
    }
    
    async void OnLoginClick()
    {
        if (!ValidateInputs()) return;
        
        string email = emailInput.text;
        string password = passwordInput.text;
        
        statusText.text = "登录中...";
        bool success = await FirebaseManager.Instance.LoginUser(email, password);
        
        if (success)
        {
            statusText.text = $"登录成功! 用户: {FirebaseManager.Instance.GetCurrentUserEmail()}";
            EnableTestButton();
        }
        else
        {
            statusText.text = "登录失败，请检查邮箱和密码";
        }
    }
    
    async void OnRegisterClick()
    {
        if (!ValidateInputs()) return;
        
        string email = emailInput.text;
        string password = passwordInput.text;
        
        if (password.Length < 6)
        {
            statusText.text = "密码至少需要6位";
            return;
        }
        
        statusText.text = "注册中...";
        bool success = await FirebaseManager.Instance.RegisterUser(email, password);
        
        if (success)
        {
            statusText.text = $"注册成功! 用户: {FirebaseManager.Instance.GetCurrentUserEmail()}";
            EnableTestButton();
        }
        else
        {
            statusText.text = "注册失败，邮箱可能已存在";
        }
    }
    
    async void OnTestScoreClick()
    {
        if (!FirebaseManager.Instance.IsUserLoggedIn())
        {
            statusText.text = "请先登录";
            return;
        }
        
        statusText.text = "保存测试分数中...";
        bool success = await FirebaseManager.Instance.SaveBestScore(testScore);
        
        if (success)
        {
            statusText.text = $"测试分数 {testScore} 保存成功!";
            
            // 立即读取验证
            int cloudScore = await FirebaseManager.Instance.GetMyBestScore();
            statusText.text += $"\n从云端读取: {cloudScore}";
        }
        else
        {
            statusText.text = "分数保存失败";
        }
    }
    
    bool ValidateInputs()
    {
        if (emailInput == null || passwordInput == null)
        {
            statusText.text = "UI组件未配置";
            return false;
        }
        
        if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            statusText.text = "请输入邮箱和密码";
            return false;
        }
        
        return true;
    }
    
    void EnableTestButton()
    {
        if (testScoreButton != null)
        {
            testScoreButton.interactable = true;
        }
    }

    async void OnTestAnonymousLoginClick()
    {
        statusText.text = "尝试匿名登录...";
        bool success = await FirebaseManager.Instance.TestAnonymousLogin();
        if (success)
        {
            statusText.text = "匿名登录成功";
        }
    }

    // 新增：运行完整诊断
    async void OnDiagnosticClick()
    {
        statusText.text = "🔧 开始Firebase诊断...\n";
        
        // 检查平台
        statusText.text += $"📱 当前平台: {Application.platform}\n";
        statusText.text += $"🌐 是否WebGL: {Application.platform == RuntimePlatform.WebGLPlayer}\n";
        
        // 检查FirebaseManager
        if (FirebaseManager.Instance == null)
        {
            statusText.text += "❌ FirebaseManager实例未找到\n";
            return;
        }
        
        statusText.text += "✅ FirebaseManager实例存在\n";
        statusText.text += $"📊 初始化状态: {FirebaseManager.Instance.initializationStatus}\n";
        statusText.text += $"🔥 是否已初始化: {FirebaseManager.Instance.isInitialized}\n";
        
        // 等待初始化
        if (!FirebaseManager.Instance.isInitialized)
        {
            statusText.text += "⏳ 等待Firebase初始化...\n";
            
            float timeout = 10f; // 10秒超时（编辑器中较短）
            float elapsed = 0f;
            
            while (!FirebaseManager.Instance.isInitialized && elapsed < timeout)
            {
                await System.Threading.Tasks.Task.Delay(500);
                elapsed += 0.5f;
                statusText.text += $"⌛ 等待中... {elapsed:F1}s\n";
            }
            
            if (!FirebaseManager.Instance.isInitialized)
            {
                statusText.text += "❌ Firebase初始化超时\n";
                statusText.text += "💡 请检查网络连接和Firebase配置\n";
                return;
            }
        }
        
        statusText.text += "✅ Firebase初始化成功\n";
        
        // 测试匿名登录
        statusText.text += "🔄 测试匿名登录...\n";
        bool anonymousResult = await FirebaseManager.Instance.TestAnonymousLogin();
        
        if (anonymousResult)
        {
            statusText.text += "✅ 匿名登录测试成功\n";
        }
        else
        {
            statusText.text += "❌ 匿名登录测试失败\n";
            statusText.text += "💡 请检查Firebase控制台认证设置\n";
        }
        
        statusText.text += "🏁 诊断完成\n";
    }
    
    // 新增：快速测试
    async void OnQuickTestClick()
    {
        if (FirebaseManager.Instance == null)
        {
            statusText.text = "❌ FirebaseManager未找到";
            return;
        }
        
        statusText.text = $"🔄 状态: {FirebaseManager.Instance.initializationStatus}\n";
        statusText.text += $"🔥 已初始化: {FirebaseManager.Instance.isInitialized}\n";
        
        if (FirebaseManager.Instance.isInitialized)
        {
            statusText.text += "🔄 测试匿名登录...\n";
            bool result = await FirebaseManager.Instance.TestAnonymousLogin();
            statusText.text += result ? "✅ 连接正常" : "❌ 连接失败";
        }
    }
} 