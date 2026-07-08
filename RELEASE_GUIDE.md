# GitHub Release 发布指南

## 创建 v3.5.0 Release

### 步骤 1：前往 GitHub

访问：https://github.com/AriesOxO/MemCommitMonitor/releases/new

### 步骤 2：填写信息

**Tag**: `v3.5.0`  
**Target**: `master`  
**Release Title**: `v3.5.0 - Enterprise Edition`

**Description**:

```markdown
## 🎉 v3.5.0 - Enterprise Edition Release

This is a **major milestone release** that transforms MemCommit Monitor from a basic tool into an **enterprise-ready application** with professional infrastructure.

### 🚀 What's New

#### Phase 1: Critical Infrastructure ✅

1. **Professional Logging System** 🔍
   - NLog 6.1.3 integration
   - Automatic log rotation (30-day retention)
   - Separate error logs (90-day retention)
   - Log location: `%AppData%\MemCommitMonitor\Logs\`

2. **Global Exception Handling** 🛡️
   - Triple protection (UI, non-UI, async)
   - User-friendly error dialogs
   - Graceful degradation
   - Automatic crash reports

3. **Configuration System** ⚙️
   - JSON-based persistent config
   - Auto save on exit
   - Auto restore on startup
   - Location: `%AppData%\MemCommitMonitor\config.json`

4. **Administrator Privilege Management** 👑
   - Automatic privilege detection
   - UAC elevation prompt
   - Visual status indicator (🛡️/⚠️)
   - Permission checks before operations

#### Phase 2: Important Features ✅

5. **Performance Monitoring System** 📊
   - Real-time operation timing
   - Slow operation detection (>1s warning)
   - Performance report on exit
   - Key metrics: Load ~26ms, Scan ~7ms

6. **System Tray Icon** 📱
   - Custom blue icon with "M"
   - Double-click to show/hide
   - Right-click menu (Show/Refresh/Exit)
   - Minimize to tray option

7. **Auto-Update System** 🔄
   - GitHub Releases integration
   - Background check (3-second delay)
   - Update notification dialog
   - Manual check button (🔄)

### 📊 Statistics

- **New Code**: ~1,930 lines
- **New Features**: 7 major systems
- **Performance**: All operations <50ms
- **Compatibility**: Windows 10/11 x64

### 📝 Complete Changelog

See [CHANGELOG.md](https://github.com/AriesOxO/MemCommitMonitor/blob/master/CHANGELOG.md) for detailed changes.

### 📥 Download

Download `MemCommitMonitor-v3.5.0.zip` below, extract and run `MemCommitMonitor.exe`.

### ⚙️ Requirements

- **OS**: Windows 10/11 (x64)
- **.NET**: .NET 8.0 Desktop Runtime
- **Privileges**: Administrator recommended

### 🐛 Known Issues

- GitHub API rate limiting: 60 requests/hour (unauthenticated)

### 🔜 What's Next

See our [roadmap](https://github.com/AriesOxO/MemCommitMonitor#roadmap) for upcoming features:
- v3.6.0: Help system, export, keyboard shortcuts
- v4.0.0: Unit tests, multi-language, dark theme

---

**Full Changelog**: https://github.com/AriesOxO/MemCommitMonitor/compare/v2.0.0...v3.5.0
```

### 步骤 3：上传文件

准备发布包：

1. 打开文件夹：
   ```
   D:\WORK\MemCommitMonitor\MemCommitMonitor\bin\Release\net8.0-windows\
   ```

2. 选择以下文件并压缩为 `MemCommitMonitor-v3.5.0.zip`：
   - MemCommitMonitor.exe
   - MemCommitMonitor.dll
   - NLog.config
   - 所有 .dll 文件（NLog, System.Management 等）

3. 在 GitHub Release 页面上传 `MemCommitMonitor-v3.5.0.zip`

### 步骤 4：发布

1. 勾选 "Set as the latest release"
2. 点击 "Publish release"

---

## 完成后

Release 发布后：
1. 自动更新功能将开始工作
2. 用户可以通过 🔄 按钮检查更新
3. 下载链接会显示在 README 徽章中

---

## 验证

发布后测试：
1. 启动旧版本程序
2. 等待 3 秒自动检查更新
3. 或点击 🔄 按钮手动检查
4. 应该弹出更新通知对话框
