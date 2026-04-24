#!/bin/bash
# ==========================================
#    Smart Desktop Assistant - Build Verify
# ==========================================

set -e

ERRORS=0
WARNINGS=0

echo "=========================================="
echo "   Smart Desktop Assistant - Build Verify"
echo "=========================================="
echo ""

# ========== 1. 检查项目文件存在性 ==========
echo "[1/5] 检查项目文件完整性..."
echo ""

REQUIRED_FILES=(
    "SmartDesktopAssistant/SmartDesktopAssistant.csproj"
    "Widgets/WeatherWidget/WeatherWidget.csproj"
    "Widgets/TodoWidget/TodoWidget.csproj"
    "Widgets/FilesWidget/FilesWidget.csproj"
    "Widgets/SettingsWidget/SettingsWidget.csproj"
    "WidgetsStandalone/WidgetsStandalone.csproj"
    "SmartDesktopAssistant.sln"
)

for file in "${REQUIRED_FILES[@]}"; do
    if [ ! -f "$file" ]; then
        echo "    ❌ 缺失: $file"
        ERRORS=1
    else
        echo "    ✅ $file"
    fi
done

if [ $ERRORS -eq 1 ]; then
    echo ""
    echo "[ERROR] 项目文件缺失，无法继续！"
    exit 1
fi

echo ""
echo "[PASS] 所有项目文件存在"
echo ""

# ========== 2. 检查解决方案引用 ==========
echo "[2/5] 检查解决方案引用一致性..."
echo ""

dotnet sln SmartDesktopAssistant.sln list >/dev/null 2>&1
if [ $? -ne 0 ]; then
    echo "    ❌ 解决方案文件有问题"
    ERRORS=1
else
    echo "    ✅ 解决方案结构正确"
fi

echo ""

# ========== 3. 恢复NuGet包 ==========
echo "[3/5] 恢复NuGet包..."
echo ""

dotnet restore SmartDesktopAssistant.sln --verbosity quiet
if [ $? -ne 0 ]; then
    echo "    ❌ NuGet包恢复失败"
    ERRORS=1
else
    echo "    ✅ NuGet包恢复成功"
fi

echo ""

# ========== 4. 执行编译测试 ==========
echo "[4/5] 执行编译测试（Release配置）..."
echo ""

# 注意：在Linux上可能无法构建Windows项目，这里给出友好提示
if [[ "$(dotnet --info | grep -i ' RID:')" == *"linux"* ]] || [[ "$(dotnet --info | grep -i 'os name')" == *"linux"* ]]; then
    echo "    ⚠️  当前为Linux环境，Windows项目需要Wine或Windows虚拟机"
    echo "    💡 建议在Windows环境中执行完整编译验证"
    echo ""
    echo "    仅验证项目结构完整性..."
else
    dotnet build SmartDesktopAssistant.sln -c Release --no-restore -v quiet
    if [ $? -ne 0 ]; then
        echo ""
        echo "    ❌ 编译失败！详细信息："
        echo ""
        dotnet build SmartDesktopAssistant.sln -c Release --no-restore
        ERRORS=1
    else
        echo "    ✅ 编译成功"
    fi
fi

echo ""

# ========== 5. 验证输出文件（仅在有构建结果时） ==========
if [ -d "SmartDesktopAssistant/bin/Release/net8.0-windows" ]; then
    echo "[5/5] 验证输出文件..."
    echo ""
    
    OUTPUT_DIR="SmartDesktopAssistant/bin/Release/net8.0-windows"
    
    if [ ! -f "$OUTPUT_DIR/SmartDesktopAssistant.exe" ]; then
        echo "    ❌ SmartDesktopAssistant.exe 未生成"
        ERRORS=1
    else
        echo "    ✅ SmartDesktopAssistant.exe 生成"
    fi
    
    if [ ! -f "$OUTPUT_DIR/WeatherWidget.dll" ]; then
        echo "    ❌ WeatherWidget.dll 未生成"
        ERRORS=1
    else
        echo "    ✅ WeatherWidget.dll 生成"
    fi
    
    if [ ! -f "$OUTPUT_DIR/TodoWidget.dll" ]; then
        echo "    ❌ TodoWidget.dll 未生成"
        ERRORS=1
    else
        echo "    ✅ TodoWidget.dll 生成"
    fi
    
    echo ""
else
    echo "[5/5] 跳过输出验证（未执行编译）"
    echo ""
fi

# ========== 汇总报告 ==========
echo "=========================================="
echo "              验证结果汇总"
echo "=========================================="
echo ""

if [ $ERRORS -eq 0 ]; then
    echo "    🎉 所有检查通过！"
    echo ""
    echo "    项目结构完整"
    echo ""
    exit 0
else
    echo "    ❌ 存在问题，请修复后重试！"
    echo ""
    exit 1
fi
