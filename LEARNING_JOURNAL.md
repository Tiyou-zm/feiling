# Learning Journal

## 2026-03-26

### 进展梳理

- 通读当前 `feiling` 项目文档与实现，确认项目已经从 `agent_study` 独立拆出。
- 确认当前主线只做“陪伴型桌宠本体”，不再在本仓库内继续推进搜索、LLM、Agent 实验。
- 核对 WPF 壳程序、素材目录和脚本，确认文档描述与实际实现基本一致。

### 当前项目状态

- 桌宠壳已落在 `desktop/FeilingPetShell`，技术路线是 `.NET 8 + WPF`。
- 正式待机方案已经切到 `idle_loop` 精灵帧循环，不再以旧 blink overlay 方案作为主路线。
- 当前已落地交互包括：
  - 透明无边框窗口
  - 全身拖拽
  - 右侧菜单
  - `固定位置 / 闲逛 / 追逐鼠标` 三种移动模式
  - 气泡台词反馈
- 当前素材主线已经形成：
  - `base/feiling_master_v1.png`
  - 5 张基础状态图
  - `idle_loop` 61 帧序列

### 为什么这样判断

- `README.md` 与 `PROJECT_CONTEXT.md` 都明确当前阶段是“陪伴型桌宠本体打磨”。
- `FEILING_MOVEMENT_PLAN.md` 明确移动系统以可控、安静、有分寸为优先级。
- `assets/characters/feiling/README.md` 与 `ASSET_MANIFEST.md` 明确正式运行时素材已经切到 `idle_loop` 路线。
- `desktop/FeilingPetShell/MainWindow.xaml.cs` 已实现菜单、拖拽、闲逛、追鼠标、气泡与待机帧播放。

### 当前缺口

- Git 仓库已经初始化并推送到 `origin/main`，当前最新提交是 `f84894b 初始化绯铃开源仓库`。
- 后续需要按有效推进持续保持“小步同步文档 + 提交 + 推送”的节奏。
- 走路循环 `walk_loop` 还在任务单阶段，尚未接入运行时。
- 呼吸、开心弹跳、更多状态反馈仍主要停留在素材规划层。
- Live2D 仍是中长期方案，不是当前主线实现。

### 本步处理

- 补齐核心文档中的 `LEARNING_JOURNAL.md` 入口。
- 初始化 Git 仓库，并为 GitHub 公开仓库创建做整理。
- 在 GitHub 创建公开仓库并推送 `origin/main`：
  - `https://github.com/Tiyou-zm/feiling`
