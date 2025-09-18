#!/bin/bash

# Cloudflare Pages 自动部署脚本
# 使用方法: ./deploy-to-cloudflare.sh

set -e

echo "🚀 开始部署到 Cloudflare Pages..."

# 检查是否安装了 wrangler CLI
if ! command -v wrangler &> /dev/null; then
    echo "❌ 未找到 wrangler CLI"
    echo "请先安装: npm install -g wrangler"
    echo "或者使用 npx: npx wrangler"
    exit 1
fi

# 检查 build 文件夹是否存在
if [ ! -d "build" ]; then
    echo "❌ 未找到 build 文件夹"
    echo "请先在 Unity 中构建 WebGL 项目"
    exit 1
fi

# 检查必要的文件
if [ ! -f "build/index.html" ]; then
    echo "❌ build/index.html 文件不存在"
    exit 1
fi

echo "✅ 检查通过，开始部署..."

# 部署到 Cloudflare Pages
# 注意：首次使用需要运行 wrangler login 进行认证
wrangler pages deploy build --project-name=chase-game --compatibility-date=2024-01-01

echo "🎉 部署完成！"
echo "📱 你的游戏现在可以通过 Cloudflare Pages 访问了"
echo "🔗 URL 会在上面的输出中显示"
