# Cloudflare Pages 自动部署指南

本指南将帮你设置从Unity WebGL构建到Cloudflare Pages的自动化部署流程。

## 🎯 目标

- 每次推送代码到GitHub后自动部署到Cloudflare Pages
- 无需手动上传build文件夹
- 支持预览环境和生产环境

## 📋 前提条件

1. GitHub 仓库（已有）
2. Cloudflare 账户
3. Unity WebGL 构建文件（已有）

## 🚀 设置步骤

### 步骤1：获取Cloudflare API凭据

1. 登录 [Cloudflare Dashboard](https://dash.cloudflare.com/)
2. 点击右上角头像 → "My Profile"
3. 选择 "API Tokens" 标签
4. 点击 "Create Token"
5. 使用 "Custom token" 模板
6. 设置以下权限：
   ```
   Account: Cloudflare Pages:Edit
   Zone: Zone:Read (如果你有自定义域名)
   ```
7. 复制生成的API Token

### 步骤2：获取Account ID

1. 在Cloudflare Dashboard右侧边栏找到 "Account ID"
2. 复制Account ID

### 步骤3：设置GitHub Secrets

1. 进入你的GitHub仓库
2. 点击 Settings → Secrets and variables → Actions
3. 添加以下secrets：
   - `CLOUDFLARE_API_TOKEN`: 步骤1中的API Token
   - `CLOUDFLARE_ACCOUNT_ID`: 步骤2中的Account ID

### 步骤4：创建Cloudflare Pages项目

1. 在Cloudflare Dashboard中，点击 "Pages"
2. 点击 "Create a project"
3. 选择 "Direct Upload" 或 "Connect to Git"
4. 项目名称设为 `chase-game` (或你想要的名称)

## 🔧 使用方法

### 方法1：GitHub Actions自动部署（推荐）

已创建的 `.github/workflows/deploy-cloudflare.yml` 会在你推送代码时自动运行：

```bash
# 推送代码即可自动部署
git add .
git commit -m "更新游戏内容"
git push origin main
```

### 方法2：手动脚本部署

使用已创建的 `deploy-to-cloudflare.sh` 脚本：

```bash
# 首次使用需要登录
npx wrangler login

# 部署
./deploy-to-cloudflare.sh
```

### 方法3：使用wrangler CLI

```bash
# 安装wrangler CLI
npm install -g wrangler

# 登录
wrangler login

# 部署
wrangler pages deploy build --project-name=chase-game
```

## 🛠️ 高级配置

### 自定义域名

如果你有自己的域名，可以在Cloudflare Pages项目设置中添加：

1. 进入你的Pages项目
2. 点击 "Custom domains"
3. 添加你的域名

### 环境变量

如果你的游戏需要环境变量，可以在wrangler.toml中添加：

```toml
[env.production.vars]
API_URL = "https://api.example.com"
GAME_VERSION = "1.0.0"
```

### 构建优化

为了更快的部署，确保你的Unity构建已经优化：

1. 在Unity中选择 `File → Build Settings`
2. 选择 `WebGL` 平台
3. 点击 `Player Settings`
4. 在Publishing Settings中：
   - Compression Format: Brotli
   - Code Optimization: Master
   - Managed Stripping Level: High

## 📊 部署状态监控

### GitHub Actions
- 在仓库的 "Actions" 标签页查看部署状态
- 绿色✅表示部署成功
- 红色❌表示部署失败，点击查看详细日志

### Cloudflare Dashboard
- 在Pages项目中查看部署历史
- 查看访问统计和性能数据

## 🔧 故障排除

### 常见问题

1. **部署失败：API Token无效**
   - 检查CLOUDFLARE_API_TOKEN是否正确设置
   - 确认API Token权限包含Cloudflare Pages:Edit

2. **部署失败：Account ID错误**
   - 检查CLOUDFLARE_ACCOUNT_ID是否正确复制

3. **游戏无法加载**
   - 检查build文件夹结构是否正确
   - 确认index.html在build文件夹根目录

4. **CORS错误**
   - Cloudflare Pages默认支持CORS，通常不需要额外配置

### 调试命令

```bash
# 检查wrangler配置
wrangler whoami

# 查看项目列表
wrangler pages project list

# 查看部署历史
wrangler pages deployment list --project-name=chase-game
```

## 🎉 完成！

设置完成后，你的工作流程将变成：

1. 在Unity中构建WebGL项目到build文件夹
2. 提交并推送代码到GitHub
3. GitHub Actions自动部署到Cloudflare Pages
4. 几分钟后你的游戏就可以通过Cloudflare URL访问了！

## 🔗 有用链接

- [Cloudflare Pages文档](https://developers.cloudflare.com/pages/)
- [Wrangler CLI文档](https://developers.cloudflare.com/workers/wrangler/)
- [GitHub Actions文档](https://docs.github.com/en/actions)

---

**项目URL**: 部署后会显示在Cloudflare Dashboard中
**自定义域名**: 可在Cloudflare Pages设置中配置
