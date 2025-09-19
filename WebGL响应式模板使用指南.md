# WebGL响应式模板使用指南

## 🎯 解决的问题

每次Unity重新打包WebGL都会生成新的`index.html`文件，覆盖之前的手机适配修改。现在通过自定义WebGL模板彻底解决这个问题！

## 📁 已创建的文件

```
Assets/WebGLTemplates/ResponsiveTemplate/
├── index.html          # 响应式模板文件
├── README.txt          # 模板说明
└── thumbnail.png       # 模板缩略图（可选）
```

## 🔧 在Unity中启用模板

### 步骤1：刷新Unity项目
1. 在Unity编辑器中，按 `Ctrl+R` (Windows) 或 `Cmd+R` (Mac) 刷新项目
2. 或者在Project窗口右键选择 "Refresh"

### 步骤2：配置WebGL模板
1. 打开 `File → Build Settings`
2. 选择 `WebGL` 平台
3. 点击 `Player Settings` 按钮
4. 在Inspector中找到 `Publishing Settings` 部分
5. 在 `WebGL Template` 下拉菜单中选择 `ResponsiveTemplate`

### 步骤3：正常打包
1. 点击 `Build` 或 `Build And Run`
2. 选择输出目录（比如 `build` 文件夹）
3. 等待打包完成

## ✨ 模板特性

### 🖥️ 电脑端适配
- 游戏画面适中大小（最大540x960px）
- 居中显示，黑色背景
- 严格保持9:16的宽高比
- 不会占满整个屏幕

### 📱 移动端适配  
- 智能适配屏幕尺寸，保持游戏比例
- 居中显示，避免变形
- 自动添加移动端viewport设置
- 支持屏幕旋转

### 🔄 智能检测
- 自动检测设备类型（桌面/移动）
- 实时响应窗口大小变化
- 处理屏幕方向改变
- **关键修复**：确保显示尺寸与Unity逻辑尺寸一致

### 🚨 重要修复
- **解决Canvas宽度不一致问题**：确保HTML显示尺寸与Unity内部Canvas.rect.width匹配
- **避免游戏边界错误**：Player和Enemy的移动边界与实际显示完全一致
- **保持游戏比例**：严格按照Unity中设置的9:16比例显示，不拉伸变形

## 🚀 优势

1. **一劳永逸**：设置一次，以后每次打包都自动适配
2. **无需手动修改**：不再需要每次打包后修改HTML文件
3. **完美兼容**：支持所有现代浏览器和设备
4. **维护简单**：所有适配逻辑都在模板中，统一管理

## 🧪 测试建议

打包完成后，在以下环境测试：

### 桌面浏览器
- Chrome、Firefox、Edge、Safari
- 尝试调整浏览器窗口大小
- 检查游戏画面是否居中，大小合适

### 移动设备
- iPhone (Safari)
- Android (Chrome)
- 测试竖屏和横屏模式
- 检查是否全屏显示

## 🔧 自定义修改

如果需要调整样式，编辑 `Assets/WebGLTemplates/ResponsiveTemplate/index.html`：

```css
/* 修改桌面端最大尺寸 */
@media (min-width: 768px) and (orientation: landscape) {
    #unity-canvas {
        max-width: 540px;    /* 调整这里 */
        max-height: 960px;   /* 调整这里 */
    }
}
```

## 📝 注意事项

1. 修改模板后需要重新打包才能生效
2. 确保Unity能识别到新模板（可能需要刷新项目）
3. 如果模板没有出现在下拉菜单中，检查文件路径是否正确
4. 保留原始模板作为备份

## 🆘 故障排除

### 模板没有出现在Unity中
1. 检查文件路径：`Assets/WebGLTemplates/ResponsiveTemplate/index.html`
2. 刷新Unity项目：`Ctrl+R` 或 `Cmd+R`
3. 重启Unity编辑器

### 打包后样式不生效
1. 确认已选择正确的模板
2. 检查浏览器控制台是否有JavaScript错误
3. 清除浏览器缓存后重新测试

现在你可以放心打包了！每次打包都会自动生成适配所有设备的HTML文件，不再需要手动修改！ 🎉
