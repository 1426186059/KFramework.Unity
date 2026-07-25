# KFramework —— Unity 轻量级通用框架

> **🎓 面试作品 / 个人技术展示**
>
> 这是一套由本人独立设计、封装与维护的 **Unity 通用开发框架**，定位为**面试作品集（Portfolio）**，
> 用于展示在游戏/应用开发中对 **架构设计、模块化封装、编辑器工具链、性能优化** 的理解与实践能力。
>
> 框架以「开箱即用、低侵入、可裁剪」为原则，覆盖从 **UI 系统、动画缓动、网络、加解密、资源管理、编辑器提效**
> 到 **移动端适配与发布** 的完整业务开发链路，可直接作为新项目的起步基座。

---

## 一、项目简介

KFramework 是一套面向 **Unity（C#）** 的轻量级运行时 + 编辑器一体化框架，目标是把日常开发中高频复用的能力沉淀为可复用模块：

- **运行时模块（Runtime）**：单例、协程、Json、加解密、缓动动画、UI 系统、网络、资源序列化、屏幕适配、Android 振动、FPS 显示等。
- **编辑器模块（Editor）**：资源依赖查找、缓存清理、资源序列化面板、一键打包（Android / iOS）、宏定义管理等。

整个框架以文件夹（`Assets/KFramework`）形式提供，**拖入工程即可使用**，无外部 Unity Package 依赖，易于阅读与二次改造。

---

## 二、目录结构

```
Assets/
└── KFramework/
    ├── Rumtime/                 # 运行时模块（随游戏打包）
    │   ├── Tools/               # 通用工具集（30+ 个工具类）
    │   ├── Encryption/          # 内容加解密
    │   ├── KTween/              # 缓动动画系统（含 Unity 扩展）
    │   ├── KUISystem/           # UI 框架（DLL）
    │   ├── AKNet/               # 网络层（Common / WebSocket，DLL）
    │   ├── Newtonsoft.Json/     # Json 序列化依赖
    │   └── NoLogo/              # 启动隐藏 Unity Logo 工具
    └── Editor/                  # 编辑器扩展（仅编辑器期）
        ├── BuildAPK/            # 一键打包 Android / iOS
        ├── CommonEditor.cs      # 编辑器通用能力
        └── ...                  # 资源序列化、依赖查找、缓存清理等
```

---

## 三、核心内容一览

### 1. Tools —— 通用工具集（核心亮点）
`Assets/KFramework/Rumtime/Tools` 下沉淀了 30+ 个高频工具类，是框架使用最频繁的部分：

| 模块 | 代表文件 | 能力说明 |
| --- | --- | --- |
| 单例 | `Singleton.cs` | 普通类单例 `Singleton<T>` 与 MonoBehaviour 单例 `SingleTonMonoBehaviour<T>`（自动挂到 `KFramework~` 常驻节点） |
| 序列化 | `CommonResSerialization.cs` | 在 Inspector 中序列化 Prefab / SpriteAtlas / Sprite / Texture / Audio / Shader / Material 等资源，提供 `FindPrefab` / `FindSprite` 等查找接口 |
| Json | `JsonTool.cs` | 基于 Newtonsoft.Json 的 `FromJson<T>` / `ToJson` 封装 |
| 屏幕适配 | `CanvasScreenAdapter.cs` / `SafeAreaFit.cs` / `BGAdapter.cs` | UI 画布缩放、刘海屏安全区、背景适配 |
| 协程/线程 | `CoroutineTool.cs` / `ThreadTool.cs` | 协程与多线程辅助 |
| 文件/IO | `FileIOTool.cs` / `WWWTools.cs` | 文件读写、网络请求、下载进度 `DownloadProgressInfo` |
| 工具方法 | `GameTools.cs` / `RandomTool.cs` / `TimeTool.cs` / `ColorTool.cs` / `HashTools.cs` / `VersionTool.cs` | 世界坐标↔UI 坐标转换、随机、时间、颜色、哈希、版本号等 |
| 调试 | `PrintTool.cs` / `FPSDisplay.cs` | 日志输出、帧率显示 |
| 移动端 | `AndroidVibrateHelper.cs` | Android 振动反馈 |
| 其它 | `FindComponent.cs` / `EventClickTriggerAllListener.cs` / `AnimationEventHub.cs` / `UnityEngineObjectExtention.cs` | 组件查找、点击监听、动画事件、Unity 对象扩展 |

### 2. Encryption —— 内容加解密
`ContentEncryption.cs` 提供 `Encode` / `Decode`，通过 **Base64 + 二进制流** 双重编码，用于本地存档、配置文件等内容的轻量混淆保护。

### 3. KTween —— 缓动动画系统
MIT 风格的轻量 Tween 替代方案（对标 DOTween），`KTweenExtensions.cs` 提供：
- `DOText`：打字机文本效果
- `To`：Vector2 值动画
- `DOColor`：Image 颜色渐变
- `DOMovePath`：沿路径点移动

### 4. KUISystem —— UI 框架
基于 DLL 封装的 UI 系统，提供统一的界面管理与生命周期，降低 UI 开发样板代码。

### 5. AKNet —— 网络层
- `AKNet.Common.dll`：通用网络通信基础
- `AKNet.WebSocket.dll`：WebSocket 协议支持

### 6. NoLogo —— 启动去 Logo
`NoLogo.cs` + `使用说明.txt`：拖入工程即可隐藏 Unity 启动 Logo（需注意对应授权合规）。

### 7. Editor —— 编辑器提效工具链
- **一键打包**：`BuildAPK/BuildAndroidEditor.cs`、`BuildIOSEditor.cs`、`BuildApkEditor.cs` —— 支持 Android / iOS 自动化构建。
- **宏定义管理**：`SymbolDefinitionEditor.cs`。
- **资源依赖查找**：`FindAssetDependenciesEditor.cs` —— 分析资源引用关系，便于清理冗余。
- **缓存清理 / 序列化面板**：`ClearCacheEditor.cs`、`CommonResSerializationEditor.cs` 等。

---

## 四、快速上手

1. 将 `Assets/KFramework` 整个文件夹复制到你的 Unity 工程 `Assets` 目录下。
2. 直接在代码中引用命名空间与工具类，例如：

```csharp
// 单例
public class GameManager : SingleTonMonoBehaviour<GameManager> { }

// Json 序列化
var json = JsonTool.ToJson(playerData);
var data = JsonTool.FromJson<PlayerData>(json);

// 缓动
text.DOText("Hello KFramework", 1.5f);
```

3. 编辑器菜单中使用 Build / 资源序列化 / 依赖查找等扩展功能。

---

## 五、技术亮点（面试关注点）

- **模块化拆分**：运行时与编辑器职责分离，按需裁剪，零强制依赖。
- **工程化思维**：内置打包、宏、依赖分析等编辑器工具，体现完整开发闭环意识。
- **动画/网络/加解密** 等通用能力自封装，降低对第三方插件（如 DOTween）的强绑定。
- **代码可读性**：工具类职责单一、命名清晰，便于阅读与维护，适合作为技术评审讲解材料。

---

## 六、说明

- 框架以源码 + 部分 DLL 形式提供，适合学习与二次开发。
- `Rumtime` 为运行时目录（项目原始命名）。
- 部分网络、UI 模块以 DLL 提供，如需源码定制可自行替换。

> 本仓库作为 **个人面试作品** 维护，欢迎在技术交流中作为架构与编码能力的参考示例。
