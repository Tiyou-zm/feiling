# 绯铃素材目录

这个目录只服务于独立 `feiling` 项目。

当前目标：

- 把绯铃先做成一个稳定的陪伴型桌宠
- 先保证母版、状态图和基础动画素材都能持续复用
- 暂时不把素材系统和搜索、Agent 能力绑死

## 目录结构

- `base/`
  - 全身母版和中性基准图
- `states/`
  - 状态图
- `animations/`
  - 轻动画素材、overlay、序列帧
- `references/`
  - 原始导出图和历史归档

## 当前正式运行时素材

- `base/feiling_master_v1.png`
- `states/feiling_idle_v1.png`
- `states/feiling_happy_soft_v1.png`
- `states/feiling_thinking_v1.png`
- `states/feiling_confused_v1.png`
- `states/feiling_smug_v1.png`
- `animations/idle_loop/feiling_idle_loop_000.png` ~ `feiling_idle_loop_060.png`
- `animations/idle_loop/manifest.txt`

## 使用约定

- `references/source_exports/`
  - 保留原始导出图
- `references/legacy_raster_exports/`
  - 保留旧 JPG 归档，不参与运行时引用
- `base/` 和 `states/`
  - 只放标准命名后的正式接入版本
- 后续所有新状态和新动画，都优先沿用：
  - `base/feiling_master_v1.png`

## 当前动画策略

当前正式待机已经切到精灵帧路线：

- 从 `feiling_idle_loop_sheet_source_v1.png` 切分出连续 PNG 帧
- 统一按 alpha 联合边界裁切，保证对位一致
- WPF 运行时直接按顺序循环播放

旧的 `idle blink overlay` 方案保留为实验素材，不再作为当前正式待机主方案。

## 当前结论

- 这批 PNG 已经是独立 `feiling` 项目的正式桌宠素材
- 后续如果母版继续精修，仍然沿用现有标准命名直接覆盖
- 如果以后补更完整的 sprite sheet，再单独在 `animations/` 里扩目录
