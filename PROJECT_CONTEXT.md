# Project Context

## 项目名称

feiling

## 当前阶段

陪伴型桌宠本体打磨阶段

## 当前工作目录

- `C:\Users\Administrator\Desktop\feiling`

## 项目职责

这个项目当前只负责：

- 绯铃桌宠本体
- WPF 桌宠壳
- 待机动画
- 菜单与移动行为
- 桌宠交互与陪伴体验

## 当前不负责

以下内容不在这个项目里继续做：

- 本地文件检索
- 搜索窗口
- 对话搜索
- LLM 接入
- Agent 能力实验

这些已经留在：

- `C:\Users\Administrator\Desktop\agent_study`

## 当前目标

先把绯铃做成一个顺手、自然、安静的标准陪伴型桌宠。

重点不是功能堆叠，而是：

- 待机自然
- 拖拽顺手
- 菜单清楚
- 移动有分寸
- 对桌面遮挡尽量小

## 当前技术路线

- 桌宠壳：WPF
- 当前待机：精灵帧循环
- 当前行为模式：
  - `固定位置`
  - `闲逛`
  - `追逐鼠标`

## 当前能力

- 透明无边框桌宠窗口
- 绯铃本体显示
- 基于精灵帧的待机循环
- 移动时切换 `walk_loop`
- 左右移动朝向自动镜像
- 全身拖拽
- 右侧菜单
- 菜单项：
  - `打个招呼`
  - `开始闲逛`
  - `追逐鼠标`
  - `固定位置`
  - `退出绯铃`

## 当前素材路线

- 正式运行时素材：
  - `assets/characters/feiling`
- 当前待机精灵帧来源：
  - `assets/characters/feiling/references/source_exports/feiling_idle_loop_sheet_source_v1.png`
- 构建脚本：
  - `scripts/build_feiling_idle_loop.py`
- 运行时帧目录：
  - `assets/characters/feiling/animations/idle_loop`

## 当前文档入口

- `README.md`
- `LEARNING_JOURNAL.md`
- `docs\OC_PROFILE.md`
- `docs\FEILING_DIALOGUE_GUIDE.md`
- `FEILING_MOVEMENT_PLAN.md`
- `WALK_LOOP_BRIEF.md`
- `WALK_LOOP_RUNTIME_PLAN.md`
- `BLINK_SPRITESHEET_BRIEF.md`
- `IDLE_BREATH_SPRITESHEET_BRIEF.md`
- `LIVE2D_SPRITE_HYBRID_BRIEF.md`

## 当前规则

- 先做桌宠本体，再考虑能力扩展
- 移动模式互斥
- 手动拖拽优先级最高
- 拖拽后默认回到 `固定位置`
- 不把实验性方案直接当正式路线

## 当前下一步

- 把待机循环继续收顺
- 继续收口 `walk_loop`、闲逛与追鼠标手感
- 评估是否补 `idle -> walk / walk -> idle` 过渡
- 再决定是否继续引入更复杂表现方案

## 备注

如果以后要重新把搜索或 Agent 能力赋回绯铃，那是下一阶段的决定，不属于当前项目范围。
