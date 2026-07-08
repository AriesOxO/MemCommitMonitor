@echo off
echo ========================================
echo 端到端测试 - 当前状态检查
echo ========================================
echo.

echo [检查 1] MemoryEater 进程
tasklist | findstr /i "MemoryEater.exe"
if errorlevel 1 (
    echo   状态: 未运行
    echo   操作: 需要启动 MemoryEater
) else (
    echo   状态: 正在运行
)

echo.
echo [检查 2] MemCommitMonitor 进程
tasklist | findstr /i "MemCommitMonitor.exe"
if errorlevel 1 (
    echo   状态: 未运行
    echo   操作: 需要启动 MemCommitMonitor
) else (
    echo   状态: 正在运行
)

echo.
echo ========================================
echo 下一步操作
echo ========================================
echo.
echo 1. 切换到 MemCommitMonitor 窗口
echo 2. 点击 "刷新" 按钮
echo 3. 在列表中找到 "MemoryEater" 进程
echo 4. 观察其内存占用（应为 ~820 MB 已提交，~847 MB 工作集）
echo 5. 选中 MemoryEater 并点击 "释放选中进程"
echo 6. 观察释放效果
echo.
echo 详细测试步骤请参考: E2E_TEST.md
echo.
pause
