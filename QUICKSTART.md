# MemCommit Monitor v3.5.0

## 快速开始指南

### 📥 下载

前往 [GitHub Releases](https://github.com/AriesOxO/MemCommitMonitor/releases/latest) 下载最新版本。

### 📦 安装

1. 解压 `MemCommitMonitor-v3.5.0.zip` 到任意目录
2. 运行 `MemCommitMonitor.exe`

**首次启动会询问是否以管理员身份运行，建议选择"是"以获得完整功能。**

### 🎯 主要功能

#### 1. 监控内存
- 实时显示已提交内存和物理内存
- 自动刷新进程列表

#### 2. 释放内存
- 右键点击进程 → 选择操作：
  - **释放工作集** - 安全释放（不关闭程序）
  - **终止进程** - 强制关闭（真正释放内存）
  - **查看详情** - 查看进程信息

#### 3. 系统托盘
- 最小化窗口 → 自动隐藏到托盘
- 双击托盘图标 → 恢复窗口
- 右键托盘图标 → 快速刷新 / 退出

#### 4. 自动更新
- 点击 🔄 按钮检查更新
- 或等待启动后自动检查

### ⚙️ 配置

配置文件位于：`%AppData%\MemCommitMonitor\config.json`

可配置项：
- 窗口大小和位置
- 最小化到托盘
- 自动更新检查
- 日志级别

### 📝 日志

日志位于：`%AppData%\MemCommitMonitor\Logs\`

包含：
- 所有操作记录
- 错误诊断信息
- 性能统计数据

### 🆘 常见问题

**Q: 为什么需要管理员权限？**  
A: 某些系统进程和深度内存操作需要管理员权限。不以管理员运行也能使用基础功能。

**Q: 释放工作集后为什么内存又涨了？**  
A: 这是正常的。释放工作集只是将内存换出到页面文件，程序再次使用时会重新加载。

**Q: 如何真正释放内存？**  
A: 使用"终止进程"功能，但这会关闭程序，可能导致数据丢失。

### 📚 更多信息

- [完整文档](https://github.com/AriesOxO/MemCommitMonitor/blob/master/README.md)
- [更新日志](https://github.com/AriesOxO/MemCommitMonitor/blob/master/CHANGELOG.md)
- [问题反馈](https://github.com/AriesOxO/MemCommitMonitor/issues)

### 📄 许可证

MIT License - 可自由使用、修改和分发

---

**Made with ❤️ by AriesOxO**  
Version 3.5.0 - Enterprise Edition
