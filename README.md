# 智能桌面助手 C# 版

基于开源项目 **Desktop Fences+** 和 **Desktop Widgets Board** 开发的智能桌面软件，采用 **C# WinForms + WPF ElementHost** 架构。

## 📁 项目结构

```
智能桌面助手_CSharp/
├── DesktopFences/              # 桌面图标管理核心（WinForms + WPF混合）
│   └── Code/Desktop Fences/
│       ├── FenceManager.cs     # 围栏管理核心（449KB）
│       ├── NonActivatingWindow.cs  # 非激活窗口实现
│       └── ... (52个源文件)
│
├── DesktopWidgetsBoard/        # 桌面小组件（WPF）
│   └── Desktop Widgets Board/
│       ├── MainWindow.xaml     # 主窗口UI定义
│       ├── App.xaml            # 样式资源
│       └── UserControls/       # 用户控件
│
├── ElementHostTest/           # ElementHost集成测试
│   ├── MainForm.cs            # WinForms主窗体
│   └── WeatherCardControl.cs   # WPF天气卡片控件
│
├── docs/                       # 文档
│   ├── 第一期技术方案.md       # 详细技术方案
│   └── 用户安装指南.md         # 安装使用指南
│
└── install.bat                # 一键安装脚本
```

## 🚀 快速开始

### 方式一：一键安装（推荐）

1. 双击运行 `install.bat`
2. 等待自动完成环境检查和编译

### 方式二：手动安装

1. 安装 **Visual Studio 2022**（需要 .NET 桌面开发工作负载）
2. 安装 **.NET 8.0 SDK**
3. 克隆两个开源项目源码
4. 用 VS 打开解决方案文件

详细步骤请参考 [用户安装指南](./docs/用户安装指南.md)

## 📚 技术架构

```
┌─────────────────────────────────────────────────┐
│              智能桌面助手主程序                  │
│                 (WinForms)                      │
│  ┌───────────────────────────────────────────┐  │
│  │            ElementHost                     │  │
│  │    (WPF控件嵌入WinForms的官方方案)         │  │
│  │  ┌─────────────────────────────────────┐   │  │
│  │  │       WPF 小组件层                   │   │  │
│  │  │  ┌─────────┐ ┌─────────┐ ┌───────┐  │   │  │
│  │  │  │天气Widget│ │TODOWidget│ │日历Widget│ │   │  │
│  │  │  └─────────┘ └─────────┘ └───────┘  │   │  │
│  │  └─────────────────────────────────────┘   │  │
│  └───────────────────────────────────────────┘  │
│                                                  │
│  ┌───────────────────────────────────────────┐  │
│  │       桌面图标管理层 (WinForms)            │  │
│  │  ┌─────────┐ ┌─────────┐ ┌───────┐      │  │
│  │  │ Fence 1 │ │ Fence 2 │ │Fence 3│      │  │
│  │  └─────────┘ └─────────┘ └───────┘      │  │
│  └───────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

### 核心技术点

| 技术 | 用途 | 参考 |
|------|------|------|
| ElementHost | WinForms嵌入WPF控件 | 微软官方方案 |
| NonActivatingWindow | 透明置顶窗口 | Desktop Fences+ |
| Open-Meteo API | 免费天气数据 | 无需API Key |
| 飞书开放API | TODO数据同步 | 多维表格 |

## 📋 开发任务

### 第一期 ✅ 已完成
- [x] 环境搭建
- [x] 源码克隆
- [x] 项目架构分析
- [x] ElementHost集成验证
- [x] 技术方案文档

### 第二期 🔲 待开发
- [ ] 提取天气Widget为独立控件
- [ ] 天气API集成（Open-Meteo）
- [ ] 透明背景窗口支持
- [ ] 日历Widget移植

### 第三期 🔲 待开发
- [ ] 飞书TODO API集成
- [ ] Fence与Widget位置同步
- [ ] 系统配置界面
- [ ] 自动更新机制

## 🔧 编译说明

### Desktop Fences+
```bash
cd DesktopFences/Code
dotnet build "Desktop Fences.sln" -c Release
```

### Desktop Widgets Board
```bash
cd Desktop-Widgets-Board
dotnet build "Desktop Widgets Board.sln" -c Release
```

### ElementHostTest
```bash
cd ElementHostTest
dotnet build -c Release
dotnet run
```

## 📖 详细文档

- [第一期技术方案](./docs/第一期技术方案.md) - 完整的架构分析和技术细节
- [用户安装指南](./docs/用户安装指南.md) - 详细安装步骤和常见问题

## 🔗 相关资源

| 资源 | 链接 |
|------|------|
| Desktop Fences+ | https://github.com/limbo666/DesktopFences |
| Desktop Widgets Board | https://github.com/hhlitval/Desktop-Widgets-Board/ |
| ElementHost官方教程 | https://learn.microsoft.com/dotnet/desktop/wpf/advanced/walkthrough-hosting-a-wpf-composite-control-in-windows-forms |
| Open-Meteo天气API | https://open-meteo.com/ |
| 飞书开放平台 | https://open.larksuite.com/ |

## ⚠️ 注意事项

1. **Desktop Fences+ 会修改桌面图标布局**，测试前请备份重要数据
2. 编译需要 **Visual Studio 2022** 和 **.NET 8.0 SDK**
3. 天气Widget目前使用静态数据，需要集成API才能显示实时天气

## 📄 许可证

本项目基于 MIT 许可证，继承上游开源项目的许可证。

---

**版本**: v1.0  
**状态**: 第一期完成
