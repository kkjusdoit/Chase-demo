# Firebase WebGL 问题解决方案

## 当前项目状态分析

### 已发现的问题
1. **配置文件问题**: 您的`google-services.json`是为Android配置的，WebGL需要Web应用配置
2. **平台差异**: WebGL平台的Firebase初始化方式与移动平台不同
3. **认证设置**: 可能需要在Firebase控制台启用相应的认证方法

### 已实施的修复

#### 1. 创建了WebGL专用配置
- 文件: `Assets/StreamingAssets/firebase-config.js`
- 包含WebGL平台的Firebase配置

#### 2. 更新了WebGL模板
- 文件: `Assets/WebGLTemplates/ResponsiveTemplate/index.html`
- 添加了Firebase JavaScript SDK引用

#### 3. 优化了FirebaseManager
- 添加了WebGL平台检测
- 改进了初始化流程
- 添加了详细的状态显示

#### 4. 创建了诊断工具
- 文件: `Assets/Scripts/FirebaseDiagnostic.cs`
- 提供完整的Firebase状态检测

## Firebase控制台必需设置

### 1. 启用认证方法
在Firebase控制台中进行以下设置：

1. 前往 Firebase控制台 → Authentication → Sign-in method
2. 启用以下认证方法：
   - **Email/Password**: 启用
   - **Anonymous**: 启用（用于测试）

### 2. 添加Web应用
1. 在Firebase项目设置中点击"添加应用"
2. 选择"Web"图标
3. 填写应用昵称（如"Chase Game Web"）
4. 勾选"Also set up Firebase Hosting"（可选）
5. 复制生成的配置信息

### 3. 配置数据库规则
在 Database → Rules 中设置：

```json
{
  "rules": {
    "users": {
      "$userId": {
        ".read": "$userId === auth.uid",
        ".write": "$userId === auth.uid"
      }
    }
  }
}
```

## 测试步骤

### 1. 在Unity编辑器中测试
1. 打开Test场景
2. 运行场景
3. 使用FirebaseDiagnostic工具检测问题

### 2. WebGL构建测试
1. File → Build Settings → WebGL
2. Player Settings → WebGL settings：
   - 确保启用"Exception Support"
   - 内存大小设置为至少512MB
3. 构建并运行

### 3. 使用诊断工具
FirebaseDiagnostic脚本提供：
- 完整的初始化检测
- 连接性测试
- 详细的错误日志

## 常见问题解决方案

### 问题1: "An internal error has occurred"
**解决方案**:
1. 检查Firebase控制台是否启用了Email/Password认证
2. 确保包名/应用ID匹配
3. 重新生成并替换配置文件
4. 检查网络连接

### 问题2: WebGL初始化失败
**解决方案**:
1. 确保WebGL模板包含Firebase SDK
2. 检查浏览器控制台错误
3. 验证firebase-config.js配置正确
4. 检查CORS设置

### 问题3: 数据库连接失败
**解决方案**:
1. 检查数据库规则设置
2. 确保用户已认证
3. 验证数据库URL正确

### 问题4: 认证持久化问题
**解决方案**:
1. WebGL使用localStorage存储认证状态
2. 检查浏览器是否允许localStorage
3. 清除浏览器缓存重试

## 完整的测试流程

### Unity编辑器测试
```csharp
// 1. 运行FirebaseDiagnostic
FirebaseDiagnostic.RunCompleteDiagnostic();

// 2. 测试匿名登录
await FirebaseManager.Instance.TestAnonymousLogin();

// 3. 测试用户注册
await FirebaseManager.Instance.RegisterUser("test@example.com", "password123");

// 4. 测试分数保存
await FirebaseManager.Instance.SaveBestScore(100);
```

### WebGL构建测试
1. 构建WebGL版本
2. 在支持WebGL的浏览器中运行
3. 打开浏览器开发者工具查看错误
4. 测试完整的认证流程

## 需要您确认的信息

请检查以下内容并告诉我当前状态：

1. **Firebase控制台设置**:
   - [ ] 是否已启用Email/Password认证？
   - [ ] 是否已启用Anonymous认证？
   - [ ] 是否已添加Web应用配置？

2. **当前遇到的具体错误**:
   - Unity控制台显示什么错误信息？
   - 在哪个步骤失败（初始化/登录/注册/保存数据）？

3. **测试环境**:
   - 主要在Unity编辑器还是WebGL构建中测试？
   - 使用什么浏览器？

## 下一步行动

1. **立即执行**: 运行FirebaseDiagnostic工具获取详细状态
2. **检查控制台**: 确认Firebase控制台设置
3. **测试流程**: 按照上述步骤逐步测试
4. **反馈结果**: 将诊断结果和错误信息反馈给我

这样我们就能精确定位问题并提供针对性的解决方案。 