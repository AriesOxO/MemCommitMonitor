# 贡献指南 / Contributing Guide

感谢你对 MemCommit Monitor 项目的关注！

## 如何贡献

### 报告问题 (Issues)

如果你发现了 Bug 或有功能建议：

1. 搜索现有 Issues，确认问题未被报告
2. 创建新 Issue，包含：
   - 清晰的标题和描述
   - 复现步骤（如果是 Bug）
   - 预期行为 vs 实际行为
   - 系统环境（Windows 版本、.NET 版本）
   - 截图（如果适用）

### 提交代码 (Pull Requests)

1. **Fork 仓库**
   ```bash
   git clone https://github.com/yourusername/MemCommitMonitor.git
   cd MemCommitMonitor
   ```

2. **创建特性分支**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **开发并测试**
   - 遵循现有代码风格
   - 添加必要的注释（中文）
   - 确保编译通过
   - 测试你的更改

4. **提交更改**
   ```bash
   git add .
   git commit -m "feat: 添加某某功能"
   ```

   Commit 消息格式：
   - `feat:` 新功能
   - `fix:` Bug 修复
   - `docs:` 文档更新
   - `style:` 代码格式调整
   - `refactor:` 代码重构
   - `test:` 测试相关

5. **推送到 GitHub**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **创建 Pull Request**
   - 前往 GitHub 仓库
   - 点击 "New Pull Request"
   - 填写 PR 描述，说明：
     - 做了什么改动
     - 为什么这样改
     - 如何测试

## 开发环境设置

### 必需工具

- **Visual Studio 2022** 或 **JetBrains Rider**
- **.NET 8 SDK**

### 构建项目

```bash
# 还原依赖
dotnet restore

# 编译
dotnet build -c Debug

# 运行
dotnet run --project MemCommitMonitor
```

## 代码规范

- 使用 C# 标准命名约定
- 公共 API 添加 XML 文档注释（中文）
- 保持代码简洁，避免过度工程
- 每个类职责单一

## 目录结构

```
MemCommitMonitor/
├── Core/           # 核心业务逻辑
├── Models/         # 数据模型
├── Utils/          # 工具类
├── MainWindow.xaml # UI 界面
└── App.xaml        # 应用入口
```

## 联系方式

有任何问题欢迎在 Issues 中提问或讨论！

