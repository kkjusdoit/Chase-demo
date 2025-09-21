using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    [Header("Firebase配置")]
    [SerializeField] private string databaseURL = "https://fir-49cdb-default-rtdb.asia-southeast1.firebasedatabase.app";
    
    [Header("WebGL Firebase配置")]
    [SerializeField] private string webApiKey = "AIzaSyAAgtl1W9xncW8WgOuyEFdy9X096vsEBzk";
    [SerializeField] private string webAuthDomain = "fir-49cdb.firebaseapp.com";
    [SerializeField] private string webProjectId = "fir-49cdb";
    
    [Header("Firebase状态")]
    public bool isInitialized = false;
    public string initializationStatus = "未开始";
    
    // Firebase实例
    private FirebaseAuth auth;
    private DatabaseReference database;
    
    // 单例模式
    public static FirebaseManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeFirebase();
    }
    
    async void InitializeFirebase()
    {
        try
        {
            initializationStatus = "检查依赖中...";
            Debug.Log("🔄 开始Firebase依赖检查...");
            Debug.Log($"🌐 当前平台: {Application.platform}");
            
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("🌐 WebGL平台检测 - 使用Web配置");
            initializationStatus = "WebGL平台配置";
            Debug.Log("✅ WebGL平台 - 将使用浏览器Firebase SDK");
#else
            Debug.Log("🖥️ 非WebGL平台 - 使用默认配置");
#endif
            
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            
            if (dependencyStatus == DependencyStatus.Available)
            {
                initializationStatus = "依赖检查通过";
                Debug.Log("✅ Firebase依赖检查通过");
                
                // 获取Firebase应用
                var app = FirebaseApp.DefaultInstance;
                Debug.Log($"🔗 Firebase应用名称: {app.Name}");
                Debug.Log($"🔗 Firebase应用选项: {app.Options.AppId}");
                
                // 初始化Firebase组件
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("✅ FirebaseAuth初始化完成");
                
                // 确保不使用模拟器（生产环境）
                Debug.Log($"🔧 Auth模拟器状态检查...");
                
                // 在Unity编辑器中确保连接到真实Firebase而非模拟器
#if UNITY_EDITOR
                Debug.Log("🖥️ Unity编辑器模式 - 确保使用生产Firebase");
#endif
                
                // 设置数据库URL
                database = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("✅ FirebaseDatabase初始化完成");
                
                isInitialized = true;
                initializationStatus = "初始化完成";
                Debug.Log("🔥 Firebase初始化成功!");
                
                // WebGL平台说明
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("🌐 WebGL平台 - 认证状态将自动保存到浏览器");
#endif
            }
            else
            {
                initializationStatus = $"依赖检查失败: {dependencyStatus}";
                Debug.LogError($"❌ Firebase依赖状态: {dependencyStatus}");
                Debug.LogError("❌ 请检查Firebase配置文件和网络连接");
                
                // 对于WebGL，即使依赖检查失败也尝试继续
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("🌐 WebGL平台 - 尝试强制初始化");
                try
                {
                    auth = FirebaseAuth.DefaultInstance;
                    database = FirebaseDatabase.DefaultInstance.RootReference;
                    isInitialized = true;
                    initializationStatus = "强制初始化完成";
                    Debug.Log("✅ WebGL强制初始化完成");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ WebGL强制初始化失败: {e.Message}");
                }
#endif
            }
        }
        catch (System.Exception e)
        {
            initializationStatus = $"初始化异常: {e.GetType().Name}";
            Debug.LogError($"❌ Firebase初始化异常: Type={e.GetType().Name}");
            Debug.LogError($"❌ 错误消息: {e.Message}");
            Debug.LogError($"❌ 堆栈跟踪: {e.StackTrace}");
        }
    }
    
    // 用户注册
    public async Task<bool> RegisterUser(string email, string password)
    {
        if (!isInitialized) 
        {
            Debug.LogError("❌ Firebase未初始化");
            return false;
        }
        
        if (auth == null)
        {
            Debug.LogError("❌ FirebaseAuth实例为空");
            return false;
        }
        
        try
        {
            Debug.Log($"🔄 开始注册用户: {email}");
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            Debug.Log($"✅ 注册成功: {result.User.Email}");
            return true;
        }
        catch (Firebase.FirebaseException e)
        {
            Debug.LogError($"❌ Firebase注册失败: ErrorCode={e.ErrorCode}, Message={e.Message}");
            Debug.LogError($"💡 可能解决方案: 检查Firebase控制台是否启用Email/Password认证");
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 一般注册失败: Type={e.GetType().Name}, Message={e.Message}");
            Debug.LogError($"❌ 堆栈跟踪: {e.StackTrace}");
            return false;
        }
    }
    
    // 用户登录
    public async Task<bool> LoginUser(string email, string password)
    {
        if (!isInitialized)
        {
            Debug.LogError("❌ Firebase未初始化");
            return false;
        }
        
        if (auth == null)
        {
            Debug.LogError("❌ FirebaseAuth实例为空");
            return false;
        }
        
        try
        {
            Debug.Log($"🔄 开始登录用户: {email}");
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            Debug.Log($"✅ 登录成功: {result.User.Email}");
            return true;
        }
        catch (Firebase.FirebaseException e)
        {
            Debug.LogError($"❌ Firebase登录失败: ErrorCode={e.ErrorCode}, Message={e.Message}");
            Debug.LogError($"💡 可能解决方案: 检查Firebase控制台是否启用Email/Password认证");
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 一般登录失败: Type={e.GetType().Name}, Message={e.Message}");
            Debug.LogError($"❌ 堆栈跟踪: {e.StackTrace}");
            return false;
        }
    }
    
    // 保存最高分
    public async Task<bool> SaveBestScore(int score)
    {
        if (!isInitialized || auth.CurrentUser == null) return false;
        
        try
        {
            string userId = auth.CurrentUser.UserId;
            var userRef = database.Child("users").Child(userId);
            
            await userRef.Child("bestScore").SetValueAsync(score);
            await userRef.Child("email").SetValueAsync(auth.CurrentUser.Email);
            await userRef.Child("lastUpdated").SetValueAsync(System.DateTime.UtcNow.ToString());
            
            Debug.Log($"✅ 分数保存成功: {score}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 分数保存失败: {e.Message}");
            return false;
        }
    }
    
    // 获取我的最高分
    public async Task<int> GetMyBestScore()
    {
        if (!isInitialized || auth.CurrentUser == null) return 0;
        
        try
        {
            string userId = auth.CurrentUser.UserId;
            var snapshot = await database.Child("users").Child(userId).Child("bestScore").GetValueAsync();
            
            if (snapshot.Exists)
            {
                return int.Parse(snapshot.Value.ToString());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 获取分数失败: {e.Message}");
        }
        
        return 0;
    }
    
    // 检查用户是否已登录
    public bool IsUserLoggedIn()
    {
        return isInitialized && auth != null && auth.CurrentUser != null;
    }
    
    // 获取当前用户邮箱
    public string GetCurrentUserEmail()
    {
        if (IsUserLoggedIn())
        {
            return auth.CurrentUser.Email;
        }
        return "";
    }
    
    // 登出
    public void Logout()
    {
        if (auth != null)
        {
            auth.SignOut();
            Debug.Log("✅ 用户已登出");
        }
    }

    // 测试用匿名登录
    public async Task<bool> TestAnonymousLogin()
    {
        if (!isInitialized || auth == null) 
        {
            Debug.LogError("❌ Firebase未就绪");
            return false;
        }
        
        try
        {
            Debug.Log("🔄 尝试匿名登录...");
            var result = await auth.SignInAnonymouslyAsync();
            Debug.Log($"✅ 匿名登录成功: {result.User.UserId}");
            return true;
        }
        catch (Firebase.FirebaseException e)
        {
            Debug.LogError($"❌ Firebase匿名登录失败: ErrorCode={e.ErrorCode}, Message={e.Message}");
            Debug.LogError($"💡 解决方案: 在Firebase控制台 → Authentication → Sign-in method 中启用Anonymous认证");
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 匿名登录失败: Type={e.GetType().Name}, Message={e.Message}");
            Debug.LogError($"💡 解决方案: 检查网络连接和Firebase控制台认证设置");
            Debug.LogError($"❌ 详细错误: {e.StackTrace}");
            return false;
        }
    }
    
    // 将匿名用户升级为正式账户
    public async Task<bool> LinkAnonymousToEmailPassword(string email, string password)
    {
        if (!IsUserLoggedIn() || !auth.CurrentUser.IsAnonymous)
        {
            Debug.LogError("❌ 当前不是匿名用户或未登录");
            return false;
        }
        
        try
        {
            var credential = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
            var result = await auth.CurrentUser.LinkWithCredentialAsync(credential);
            
            Debug.Log($"✅ 匿名用户成功升级为: {result.User.Email}");
            return true;
        }
        catch (Firebase.FirebaseException e)
        {
            Debug.LogError($"❌ 账户升级失败: ErrorCode={e.ErrorCode}, Message={e.Message}");
            return false;
        }
    }
    
    // 检查是否为匿名用户
    public bool IsAnonymousUser()
    {
        return IsUserLoggedIn() && auth.CurrentUser.IsAnonymous;
    }
    
    // 获取用户类型信息
    public string GetUserTypeInfo()
    {
        if (!IsUserLoggedIn()) return "未登录";
        if (auth.CurrentUser.IsAnonymous) return "匿名用户";
        return $"正式用户: {auth.CurrentUser.Email}";
    }
} 