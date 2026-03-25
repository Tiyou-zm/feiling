# feiling

这是独立拆出来的绯铃桌宠项目。

从当前版本开始，这个目录只做一件事：

- 把绯铃做成一个标准的陪伴型桌宠

## 不在这里做什么

下面这些能力已经和本项目分离：

- 本地文件检索
- 搜索窗口
- 对话搜索
- LLM 接入
- Agent 能力实验

这些仍然留在：

- `C:\Users\Administrator\Desktop\agent_study`

## 当前目标

- 先把绯铃做成顺手、自然、安静的桌宠
- 优先打磨桌宠本体交互和存在感
- 暂时不接复杂能力，不重新混回主仓库

## 当前技术路线

- 桌宠壳：WPF
- 当前待机：精灵帧循环
- 当前行为：固定位置 / 闲逛 / 追逐鼠标

## 当前能力

- 透明无边框桌宠窗口
- 绯铃本体显示
- 基于精灵帧的待机循环
- 全身拖拽
- 右侧菜单
- 菜单项：
  - `打个招呼`
  - `开始闲逛`
  - `追逐鼠标`
  - `固定位置`
  - `退出绯铃`

## 核心文档

- `README.md`
  - 当前仓库职责、入口索引、运行方式
- `PROJECT_CONTEXT.md`
  - 当前架构边界、阶段目标、正式路线
- `LEARNING_JOURNAL.md`
  - 每一步有效推进的记录与原因

## 目录

- `desktop/FeilingPetShell`
  - WPF 桌宠本体
- `assets/characters/feiling`
  - 绯铃素材
- `docs`
  - 绯铃设定、出图、状态图与动画任务单
- `scripts/start_feiling.ps1`
  - 启动桌宠
- `scripts/stop_feiling.ps1`
  - 停止桌宠
- `FEILING_MOVEMENT_PLAN.md`
  - 绯铃移动行为计划
- `WALK_LOOP_BRIEF.md`
  - 给素材 agent 的走路循环任务单
- `BLINK_SPRITESHEET_BRIEF.md`
  - 待机眨眼精灵图任务单
- `IDLE_BREATH_SPRITESHEET_BRIEF.md`
  - 待机呼吸精灵图任务单
- `LIVE2D_SPRITE_HYBRID_BRIEF.md`
  - Live2D 与精灵图混合路线任务单

当前已迁入 `docs` 的历史与设定文档包括：

- `OC_PROFILE.md`
- `FEILING_DIALOGUE_GUIDE.md`
- `OC_ART_MASTER.md`
- `OC_ART_PROMPT.md`
- `OC_VISUAL_LOCK.md`
- `PET_ANIMATION_PLAN.md`
- `FULLBODY_MASTER_BRIEF.md`
- `STATE_SET_BRIEF.md`
- `FIRST_ANIMATION_BRIEF.md`
- `IDLE_BLINK_BRIEF.md`

## 一键测试

直接双击根目录：

- `run_feiling.bat`
- `stop_feiling.bat`

## 命令行启动

先确保本机安装 `.NET 8 SDK`。

启动：

```powershell
.\scripts\start_feiling.ps1
```

停止：

```powershell
.\scripts\stop_feiling.ps1
```

## 当前待机方案

- 当前正式待机使用精灵帧循环
- 素材源：
  - `assets/characters/feiling/references/source_exports/feiling_idle_loop_sheet_source_v1.png`
- 构建脚本：
  - `scripts/build_feiling_idle_loop.py`
- 运行时帧目录：
  - `assets/characters/feiling/animations/idle_loop`

## 当前移动方案

- 模式只有三种：
  - `固定位置`
  - `闲逛`
  - `追逐鼠标`
- 三种模式互斥
- 手动拖拽后会自动回到 `固定位置`
- 闲逛按“走一小段 -> 停一会儿 -> 再换点”的方式工作

## 当前原则

- 先把绯铃做成稳定的陪伴型桌宠
- 先把待机、移动、菜单和交互做顺
- 如果后面要接更复杂能力，再单独评估，不默认回接主仓库
