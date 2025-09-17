# Unity WebGL 部署到 GitHub Pages 完整指南：踩坑与解决方案

## 前言

最近将Unity WebGL游戏部署到GitHub Pages时遇到了不少坑，经过一番折腾终于成功部署。本文记录了完整的部署过程以及遇到的各种问题和解决方案，希望能帮助其他开发者避免这些坑。

## 项目背景

- **项目类型**：Unity 2D游戏
- **构建目标**：WebGL
- **部署平台**：GitHub Pages
- **仓库**：https://github.com/kkjusdoit/Chase-demo

## 部署过程概览

### 第一步：Unity WebGL构建
1. 在Unity中选择 `File → Build Settings`
2. 选择 `WebGL` 平台
3. 点击 `Build` 并选择输出文件夹（我选择了`build`文件夹）

构建完成后，`build`文件夹包含：
```
build/
├── index.html          # 主页面
├── Build/              # Unity构建文件
│   ├── build.data
│   ├── build.framework.js
│   ├── build.loader.js
│   └── build.wasm
└── TemplateData/       # 样式和资源文件
    ├── style.css
    ├── favicon.ico
    └── 各种图标文件...
```

### 第二步：Git初始化和GitHub仓库创建
```bash
# 初始化Git仓库
git init

# 创建.gitignore文件（重要！）
# 忽略Unity不需要的文件，但保留build文件夹
```

## 遇到的主要问题和解决方案

### 🚫 坑1：GitHub Pages文件夹限制

**问题**：GitHub Pages只支持从根目录(`/`)或`/docs`文件夹部署，不支持自定义文件夹如`/build`。

**错误尝试**：
- 尝试在GitHub Pages设置中选择`build`文件夹 ❌
- 手动将文件复制到根目录，导致文件结构混乱 ❌

**正确解决方案**：使用GitHub Actions自动部署
```yaml
# .github/workflows/deploy.yml
name: Deploy Unity WebGL to GitHub Pages

on:
  push:
    branches: [ main ]
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: "pages"
  cancel-in-progress: false

jobs:
  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        
      - name: Setup Pages
        uses: actions/configure-pages@v4
        
      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          # 上传 build 文件夹的内容
          path: './build'
          
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

### 🚫 坑2：文件路径错误

**问题**：控制台错误 `GET https://kkjusdoit.github.io/Chase-demo/Build/build.loader.js net::ERR_ABORTED 404 (Not Found)`

**原因分析**：
- `index.html`中引用的路径是`"Build"`
- 但实际文件位置与引用路径不匹配

**错误的文件结构**：
```
根目录/
├── index.html
├── Build/              # 空文件夹或文件位置错误
├── build.data         # 文件位置错误
├── build.framework.js # 文件位置错误
└── ...
```

**正确的文件结构**：
```
build/                 # 这是GitHub Actions要部署的文件夹
├── index.html
├── Build/             # Unity构建文件必须在这里
│   ├── build.data
│   ├── build.framework.js
│   ├── build.loader.js
│   └── build.wasm
└── TemplateData/      # 样式文件必须在这里
    └── ...
```

### 🚫 坑3：Git认证问题

**问题**：推送时出现权限错误
```
ERROR: Permission to kkjusdoit/Chase-demo.git denied to linkunkun-SecretLisa.
fatal: Could not read from remote repository.
```

**原因**：
- 本地Git配置的用户信息与GitHub账户不匹配
- SSH密钥与当前账户不对应
- 使用了错误的远程仓库URL格式

**解决方案**：
```bash
# 1. 更新Git用户配置
git config user.name "kkjusdoit"
git config user.email "kkjusdoit@users.noreply.github.com"

# 2. 使用HTTPS格式的远程仓库URL
git remote set-url origin https://github.com/kkjusdoit/Chase-demo.git

# 3. 使用GitHub Desktop（推荐）
# 或者配置Personal Access Token
```

### 🚫 坑4：GitHub Pages设置错误

**问题**：设置了GitHub Pages但游戏无法加载

**错误设置**：
- Source: "Deploy from a branch"
- Branch: "main"
- Folder: "/ (root)"

**正确设置**：
- Source: "GitHub Actions" ✅

### 🚫 坑5：文件意外删除

**问题**：在调试过程中意外删除了build文件夹的内容

**解决方案**：使用Git恢复
```bash
# 从之前的提交恢复文件
git checkout <commit-hash> -- build/

# 或者使用
git restore build/
```

## 最终正确的部署流程

### 1. 准备工作
```bash
# 初始化Git仓库
git init

# 创建.gitignore（包含Unity常见忽略文件，但保留build文件夹）
echo "!/build/" >> .gitignore
```

### 2. 创建GitHub Actions工作流
创建`.github/workflows/deploy.yml`文件（内容见上面的YAML配置）

### 3. 提交并推送代码
```bash
git add .
git commit -m "初始提交：Unity WebGL项目"
git remote add origin https://github.com/username/repo-name.git
git push -u origin main
```

### 4. 配置GitHub Pages
1. 访问仓库的Settings → Pages
2. Source选择"GitHub Actions"
3. 保存设置

### 5. 等待自动部署
- GitHub Actions会自动运行
- 创建`gh-pages`分支
- 部署完成后访问：`https://username.github.io/repo-name/`

## 关键要点总结

### ✅ 正确做法
1. **保持原始文件结构**：不要移动build文件夹中的文件
2. **使用GitHub Actions**：这是处理自定义文件夹的最佳方案
3. **正确的Git配置**：确保用户名和邮箱与GitHub账户匹配
4. **使用HTTPS URL**：避免SSH密钥问题
5. **GitHub Desktop**：简化Git操作，避免命令行认证问题

### ❌ 避免的错误
1. 不要手动复制文件到根目录
2. 不要尝试从非标准文件夹部署
3. 不要忽略文件路径的大小写敏感性
4. 不要在调试时随意删除文件

## 性能优化建议

### 1. Unity构建优化
```csharp
// 在Player Settings中：
// - 启用Code Stripping
// - 选择Minimal或Low质量的Compression Format
// - 禁用不必要的Auto Graphics API
```

### 2. 文件大小优化
- 压缩纹理
- 优化音频格式
- 移除未使用的资源

### 3. 加载优化
- 使用Unity的Progressive Download
- 实现自定义加载界面
- 添加预加载提示

## 调试技巧

### 1. 浏览器开发者工具
- 检查Network标签页查看文件加载情况
- 查看Console错误信息
- 验证文件路径是否正确

### 2. GitHub Actions调试
- 查看Actions标签页的运行日志
- 检查artifact上传是否成功
- 验证部署步骤是否完成

### 3. 本地测试
```bash
# 使用Python简单HTTP服务器测试
cd build
python -m http.server 8000
# 访问 http://localhost:8000
```

## 结语

Unity WebGL部署到GitHub Pages看似简单，但实际操作中会遇到各种坑。关键是理解GitHub Pages的限制，正确使用GitHub Actions，以及保持正确的文件结构。希望这篇文章能帮助其他开发者顺利完成部署！

## 参考资源

- [GitHub Pages文档](https://docs.github.com/en/pages)
- [GitHub Actions文档](https://docs.github.com/en/actions)
- [Unity WebGL构建指南](https://docs.unity3d.com/Manual/webgl-building.html)

---

**项目地址**：https://github.com/kkjusdoit/Chase-demo  
**在线演示**：https://kkjusdoit.github.io/Chase-demo/

*如果这篇文章对你有帮助，欢迎点个Star！⭐*
