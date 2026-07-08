@echo off
setlocal enabledelayedexpansion

echo ================================
echo MemCommit Monitor - 自动化测试
echo ================================
echo.

:: 设置颜色
set "GREEN=[92m"
set "RED=[91m"
set "YELLOW=[93m"
set "NC=[0m"

set PASS=0
set FAIL=0

echo [1/6] 检查 .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo %RED%✗ 失败%NC%: 未找到 .NET SDK
    set /a FAIL+=1
) else (
    for /f %%i in ('dotnet --version') do set DOTNET_VER=%%i
    echo %GREEN%✓ 通过%NC%: .NET SDK !DOTNET_VER!
    set /a PASS+=1
)

echo.
echo [2/6] 检查项目文件...
if exist "MemCommitMonitor\MemCommitMonitor.csproj" (
    echo %GREEN%✓ 通过%NC%: 项目文件存在
    set /a PASS+=1
) else (
    echo %RED%✗ 失败%NC%: 项目文件不存在
    set /a FAIL+=1
)

echo.
echo [3/6] 还原 NuGet 包...
dotnet restore >nul 2>&1
if errorlevel 1 (
    echo %RED%✗ 失败%NC%: NuGet 包还原失败
    set /a FAIL+=1
) else (
    echo %GREEN%✓ 通过%NC%: NuGet 包还原成功
    set /a PASS+=1
)

echo.
echo [4/6] 编译项目（Debug）...
dotnet build -c Debug >nul 2>&1
if errorlevel 1 (
    echo %RED%✗ 失败%NC%: 编译失败
    set /a FAIL+=1
    echo 查看详细错误:
    dotnet build -c Debug
) else (
    echo %GREEN%✓ 通过%NC%: 编译成功
    set /a PASS+=1
)

echo.
echo [5/6] 检查可执行文件...
if exist "MemCommitMonitor\bin\x64\Debug\net8.0-windows\MemCommitMonitor.exe" (
    echo %GREEN%✓ 通过%NC%: 可执行文件已生成
    set /a PASS+=1
) else (
    echo %RED%✗ 失败%NC%: 可执行文件不存在
    set /a FAIL+=1
)

echo.
echo [6/6] 检查程序运行状态...
tasklist | findstr /i "MemCommitMonitor.exe" >nul 2>&1
if errorlevel 1 (
    echo %YELLOW%⚠ 警告%NC%: 程序未运行
    echo 请手动运行: dotnet run --project MemCommitMonitor
) else (
    echo %GREEN%✓ 通过%NC%: 程序正在运行
    set /a PASS+=1
)

echo.
echo ================================
echo 测试总结
echo ================================
echo 通过: %GREEN%%PASS%%NC%
echo 失败: %RED%%FAIL%%NC%
echo.

if %FAIL% equ 0 (
    echo %GREEN%所有测试通过！%NC%
    echo.
    echo 下一步: 进行人工功能测试
    echo 参考文档: TEST_REPORT.md
) else (
    echo %RED%发现 %FAIL% 个失败项，请检查！%NC%
)

echo.
pause
