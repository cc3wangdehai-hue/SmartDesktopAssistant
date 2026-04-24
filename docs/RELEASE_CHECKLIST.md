# 智能桌面助手 - 交付前检查清单

## 📋 发布前必检项目

每次发布前，请按顺序执行以下检查项：

---

### 1️⃣ 项目文件完整性检查

```bash
# 检查所有必需的.csproj文件是否存在
check_required_files() {
    local required_files=(
        "SmartDesktopAssistant/SmartDesktopAssistant.csproj"
        "Widgets/WeatherWidget/WeatherWidget.csproj"
        "Widgets/TodoWidget/TodoWidget.csproj"
        "Widgets/FilesWidget/FilesWidget.csproj"
        "Widgets/SettingsWidget/SettingsWidget.csproj"
        "WidgetsStandalone/WidgetsStandalone.csproj"
        "SmartDesktopAssistant.sln"
    )
    
    for file in "${required_files[@]}"; do
        if [ ! -f "$file" ]; then
            echo "❌ 缺失文件: $file"
            return 1
        fi
    done
    echo "✅ 所有项目文件完整"
    return 0
}
```

- [ ] SmartDesktopAssistant.csproj 存在
- [ ] WeatherWidget.csproj 存在
- [ ] TodoWidget.csproj 存在
- [ ] FilesWidget.csproj 存在
- [ ] SettingsWidget.csproj 存在
- [ ] WidgetsStandalone.csproj 存在
- [ ] SmartDesktopAssistant.sln 存在

---

### 2️⃣ 脚本文件语法检查

- [ ] run.bat 不含 `-o Output` 参数（dotnet build不支持此参数）
- [ ] install.bat 路径引用正确
- [ ] package.bat（如存在）无硬编码路径

**⚠️ 常见错误模式：**
```batch
# ❌ 错误：-o 参数不被 dotnet build 支持
dotnet build xxx.sln -o Output

# ✅ 正确：直接构建或使用输出目录属性
dotnet build xxx.sln -c Release
```

---

### 3️⃣ 项目引用一致性检查

执行以下命令验证：

```bash
dotnet sln SmartDesktopAssistant.sln list
```

预期输出应包含：
- SmartDesktopAssistant
- WeatherWidget
- TodoWidget  
- FilesWidget
- SettingsWidget

检查 SmartDesktopAssistant.csproj 中的 `<ProjectReference>` 是否与实际引用的项目匹配。

---

### 4️⃣ 编译验证

```bash
# 清理并重新构建
dotnet clean SmartDesktopAssistant.sln
dotnet build SmartDesktopAssistant.sln -c Release --no-restore

# 验证exe生成
ls -la SmartDesktopAssistant/bin/Release/net8.0-windows/*.exe
```

- [ ] dotnet build 执行成功（exit code 0）
- [ ] SmartDesktopAssistant.exe 生成
- [ ] 所有Widget的dll文件生成

---

### 5️⃣ 打包文件结构检查

**主程序打包结构：**
```
Output/
├── SmartDesktopAssistant.exe      # ✅ 必需
├── SmartDesktopAssistant.dll
├── SmartDesktopAssistant.deps.json
├── SmartDesktopAssistant.runtimeconfig.json
├── WeatherWidget.dll              # ✅ 必需
├── TodoWidget.dll                 # ✅ 必需
├── FilesWidget.dll
├── SettingsWidget.dll
└── *.dll                          # 运行时依赖
```

**独立版打包结构：**
```
WidgetsStandalone/
├── bin/
│   └── Release/
│       └── net8.0-windows/
│           ├── WidgetsStandalone.exe
│           └── *.dll
```

- [ ] run.bat 位于根目录
- [ ] install.bat 位于根目录
- [ ] 所有必需文件存在于正确位置

---

### 6️⃣ 配置文件检查

- [ ] SmartDesktopAssistant/app.manifest 存在（如项目引用）
- [ ] NuGet包可正常还原

```bash
dotnet restore SmartDesktopAssistant.sln
```

---

### 7️⃣ 回归测试

| 测试项 | 预期结果 | 状态 |
|--------|----------|------|
| run.bat 执行选项1 | 独立Widget启动 | ⬜ |
| run.bat 执行选项2 | 主程序启动 | ⬜ |
| install.bat | 编译成功 | ⬜ |
| 编译无警告 | clean build | ⬜ |

---

## 🔧 快速验证脚本

```bash
# Windows
verify_build.bat

# Linux/Mac
chmod +x verify_build.sh && ./verify_build.sh
```

---

## 📝 发布前确认清单

在确认发布前，请确认：

- [ ] 所有检查项均为 ✅ 通过
- [ ] 编译无任何 Error（Warning可接受）
- [ ] 至少在一个环境测试通过
- [ ] 更新版本号（如有需要）

---

**最后更新**: 2024-04-24
**维护者**: 开发团队
