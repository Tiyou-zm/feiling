# 绯铃 walk_loop 运行时接入方案

## 目标

把 2026-03-27 新整理出的动作素材，收敛成一套可以直接被 WPF 运行时加载的 `walk_loop` 目录约定。

这一步先解决三件事：

- 正式运行时该引用哪组动作素材
- 正式运行时目录和命名如何固定
- 代码接入时，`idle_loop` 和 `walk_loop` 怎么切

## 当前判断

`video_extract_20260327` 里有两组已经整理过的位置统一素材：

1. `selected_70_spread_fixed`
2. `selected_70_idle_bridge_fixed`

其中：

- `selected_70_spread_fixed`
  - 是按整段动作均匀铺开的 70 帧
  - 全部是连续动作帧
  - 更适合作为第一版 `walk_loop` 的正式源
- `selected_70_idle_bridge_fixed`
  - 包含 `idle_ref`、`intro_blend`、`outro_blend`
  - 更适合作为以后补“待机切走路 / 走路切回待机”的过渡素材储备
  - 不适合直接作为纯 `walk_loop` 目录

## 正式目录约定

第一版正式运行时目录固定为：

- `assets/characters/feiling/animations/walk_loop`

目录内文件固定为：

- `feiling_walk_loop_000.png`
- `feiling_walk_loop_001.png`
- `...`
- `manifest.txt`

这套约定与当前 `idle_loop` 保持一致，后续 WPF 运行时只需要按目录顺序加载。

## 第一版素材收敛规则

第一版不直接把 70 帧全量接进正式 `walk_loop`，而是先收敛成 12 帧运行时序列：

- 帧数控制在任务单允许的平滑范围内
- 避免运行时同时常驻 `idle_loop + walk_loop` 时占用过多内存
- 先得到一套“可运行、可调手感”的 walk loop，再决定是否补更多帧

具体规则：

- 源目录：`assets/characters/feiling/animations/video_extract_20260327/selected_70_spread_fixed/frames`
- 源 manifest：`feiling_video_motion_bridge_spread_fixed_70_manifest.json`
- 采样方式：`uniform_spread_no_end_duplicate`
- 目标帧数：`12`
- 保留统一后的画布尺寸和角色摆位，不再二次裁切

## 构建脚本

正式构建脚本：

- `scripts/build_feiling_walk_loop.py`

作用：

- 从 `selected_70_spread_fixed/frames` 中均匀抽取 12 帧
- 输出到 `animations/walk_loop`
- 生成 `manifest.txt`
- 在 manifest 中保留“运行时帧 -> 候选帧 -> 原视频帧”的映射

## 运行时切换规则

第一版代码接入时已经按下面方式落地：

### 1. `Pinned`

- 永远播放 `idle_loop`

### 2. `Wander`

- 有移动目标且本 tick 发生位移时：播放 `walk_loop`
- 没有目标或已经进入停顿期时：播放 `idle_loop`

### 3. `FollowMouse`

- 鼠标在 dead zone 外，且本 tick 发生位移时：播放 `walk_loop`
- 鼠标进入 dead zone 或本 tick 没有位移时：播放 `idle_loop`
- 跟随使用 `engage / release` 两段距离阈值，避免靠近鼠标时反复“追一下又停一下”
- 移动目标保持一段固定跟随距离，不直接贴到鼠标点上
- 跟随朝向优先按“目标方向”的水平分量判断，不按极小的瞬时位移频繁反转

## 步调设置

- `idle_loop` 维持当前偏轻的播放速度
- `walk_loop` 使用更慢的单独帧间隔
- 动画模式切换时不重置到第一帧，优先减少“刚切走路就抽一下”的感觉

## 左右朝向规则

为避免出现“向左走但动作还是向右”的问题，第一版不额外维护左右两套 PNG，而是直接在运行时按水平位移方向做镜像：

- 向右移动：保持原始朝向
- 向左移动：水平镜像当前帧
- 停下后：保留最后一次移动时的朝向

当前镜像使用的是 `PetDragSurface` 上已有的 `PetScaleTransform`，因此会同时作用于角色本体和 overlay 层。

当前 `walk_loop` 源帧的天然朝向按“朝左”处理：

- 向左移动：使用源帧原始朝向
- 向右移动：对源帧做水平镜像

如果后续观察到源素材的默认朝向与实际相反，可以只调整：

- `desktop/FeilingPetShell/MainWindow.xaml.cs`
  - `SourceSpriteFacingScaleX`

## 最小代码改动点

主入口仍是：

- `desktop/FeilingPetShell/MainWindow.xaml.cs`

当前代码已经按下面方式接入：

1. 增加 `walk_loop` 资源加载
2. 把“当前播放动画”从“只有 idle”改成“idle / walk”两态
3. 在 `MoveTowards(...)` 后判断本 tick 是否真的发生位移
4. 移动时切 `walk_loop`，停下时切回 `idle_loop`
5. 先不引入 `intro/outro` 过渡，先把基本手感跑通

## 暂不进入第一版的内容

以下内容先不做，避免把这一步做复杂：

- `idle -> walk` 混合过渡
- `walk -> idle` 混合过渡
- 根据速度切不同步频
- 根据模式切不同走路素材

这些以后如果要补，优先复用：

- `assets/characters/feiling/animations/video_extract_20260327/selected_70_idle_bridge_fixed`

## 当前结论

当前已经可以把新动作素材收敛成下面这套稳定结构：

- `selected_70_spread_fixed` 作为第一版 `walk_loop` 正式源
- `selected_70_idle_bridge_fixed` 作为未来过渡素材储备
- `animations/walk_loop` 作为正式运行时目录
- `build_feiling_walk_loop.py` 作为正式构建入口
- `desktop/FeilingPetShell/MainWindow.xaml.cs` 已在移动时切 `walk_loop`，停下时切回 `idle_loop`
- 左右移动已按水平位移方向自动镜像
