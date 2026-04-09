# 绯铃素材清单

## 母版

- `base/feiling_master_v1.png`
  - 当前确认的全身中性母版
  - 后续所有状态图和动画都从这里继续扩

## 状态图

- `states/feiling_idle_v1.png`
  - 待机
- `states/feiling_happy_soft_v1.png`
  - 轻开心 / 欢迎
- `states/feiling_thinking_v1.png`
  - 思考中
- `states/feiling_confused_v1.png`
  - 困惑 / 没明白
- `states/feiling_smug_v1.png`
  - 轻微傲娇的小得意

## 动画图

- `animations/feiling_idle_blink_half_v1.png`
- `animations/feiling_idle_blink_closed_v1.png`
- `animations/feiling_idle_blink_half_overlay_v2.png`
- `animations/feiling_idle_blink_closed_overlay_v2.png`
- `animations/idle_loop/feiling_idle_loop_000.png` ~ `feiling_idle_loop_060.png`
- `animations/idle_loop/manifest.txt`
- `animations/walk_loop/feiling_walk_loop_000.png` ~ `feiling_walk_loop_011.png`
- `animations/walk_loop/manifest.txt`

说明：

- 旧 blink overlay 素材仍保留
- 当前正式待机主方案已切到 `idle_loop` 精灵帧目录
- 第一版 `walk_loop` 运行时目录已固定，素材来自 `video_extract_20260327/selected_70_spread_fixed`

## 原始导出图

- `references/source_exports/feiling_master_source_v1.png`
- `references/source_exports/feiling_state_idle_source_v1.png`
- `references/source_exports/feiling_state_happy_source_v1.png`
- `references/source_exports/feiling_state_thinking_source_v1.png`
- `references/source_exports/feiling_state_confused_source_v1.png`
- `references/source_exports/feiling_state_smug_source_v1.png`
- `references/source_exports/feiling_idle_blink_half_source_v1.png`
- `references/source_exports/feiling_idle_blink_closed_source_v1.png`
- `references/source_exports/feiling_idle_loop_sheet_source_v1.png`

## 动作候选素材

- `animations/video_extract_20260327/selected_70_spread_fixed/`
  - 第一版 `walk_loop` 正式运行时源
- `animations/video_extract_20260327/selected_70_idle_bridge_fixed/`
  - 未来 `idle -> walk / walk -> idle` 过渡素材储备

## 历史归档

- `references/legacy_raster_exports/`
  - 旧 JPG 导出
  - 不参与当前运行时引用

## 当前接入优先级

1. `idle`
2. `happy_soft`
3. `thinking`
4. `confused`
5. `smug`
6. `idle loop`
7. `walk loop`
