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

## 2026-04-10

### 进展梳理

- 回看当前 `feiling` 项目文档、代码和最近提交，确认主线仍然是桌宠本体打磨。
- 追查 `walk_loop` 当前状态，确认代码侧尚未接入，但 2026-03-27 已新增一批可作为走路候选的动作抽帧素材。
- 把这批新动作素材正式梳理成可接入运行时的 `walk_loop` 方案。

### 当前判断

- `desktop/FeilingPetShell/MainWindow.xaml.cs` 当前只有 `idle_loop` 加载和播放逻辑，还没有 `walk_loop` 资源加载与状态切换。
- `assets/characters/feiling/animations/video_extract_20260327/selected_70_spread_fixed` 更适合作为第一版纯 `walk_loop` 正式源。
- `assets/characters/feiling/animations/video_extract_20260327/selected_70_idle_bridge_fixed` 包含 `idle_ref / intro_blend / outro_blend`，更适合保留为以后补过渡动画的储备。

### 本步处理

- 新增 `scripts/build_feiling_walk_loop.py`，把 `selected_70_spread_fixed` 均匀抽样为 12 帧正式 `walk_loop` 运行时序列。
- 固定正式运行时目录为 `assets/characters/feiling/animations/walk_loop`。
- 新增 `WALK_LOOP_RUNTIME_PLAN.md`，明确素材来源、构建方式、运行时切换规则和后续代码接入点。
- 同步 `README.md`、`PROJECT_CONTEXT.md`、`assets/characters/feiling/README.md`、`assets/characters/feiling/ASSET_MANIFEST.md`。
- 在 `desktop/FeilingPetShell/MainWindow.xaml.cs` 中正式接入 `walk_loop` 播放逻辑。
- 移动时切 `walk_loop`，停下时切回 `idle_loop`。
- 按水平位移方向自动镜像当前动画，解决左右行走朝向问题。
- 本地重新编译并启动桌宠，确认代码可正常构建运行。
- 收紧 `追逐鼠标` 的跟随滞后区间，避免“走几步就停一下”的抽搐感。
- 把 `walk_loop` 动画单独放慢，并避免切换时反复重置到首帧。
- 复查 `walk_loop` 素材后，确认“整段均匀抽样”会混入起步和回正帧，已改为只取稳定步行动作窗口 `032 ~ 043` 作为正式运行时循环。
- 在继续观察后，确认连续窗口方案体感偏慢，已进一步改成手工挑选的 stride 帧集合 `024, 026, 028, 030, 032, 034, 036, 038, 040`。

### 当前缺口

- `idle -> walk` 和 `walk -> idle` 过渡逻辑仍未实现。
- 按速度调步频、移动中的更细致角色感，仍属于后续阶段。
