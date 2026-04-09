from __future__ import annotations

import json
import shutil
from pathlib import Path


SOURCE_SET_NAME = "selected_70_spread_fixed"
WALK_LOOP_SOURCE_FRAMES_1BASED = [24, 26, 28, 30, 32, 34, 36, 38, 40]


def main() -> None:
    project_root = Path(__file__).resolve().parent.parent
    source_root = (
        project_root
        / "assets"
        / "characters"
        / "feiling"
        / "animations"
        / "video_extract_20260327"
        / SOURCE_SET_NAME
    )
    source_frames_dir = source_root / "frames"
    source_manifest_path = source_root / "feiling_video_motion_bridge_spread_fixed_70_manifest.json"
    target_dir = (
        project_root / "assets" / "characters" / "feiling" / "animations" / "walk_loop"
    )
    target_manifest_path = target_dir / "manifest.txt"

    if not source_frames_dir.exists():
        raise SystemExit(f"Missing source frames directory: {source_frames_dir}")

    if not source_manifest_path.exists():
        raise SystemExit(f"Missing source manifest: {source_manifest_path}")

    source_manifest = json.loads(source_manifest_path.read_text(encoding="utf-8"))
    source_frames = sorted(source_frames_dir.glob("*.png"))
    selected_indices = [frame_number - 1 for frame_number in WALK_LOOP_SOURCE_FRAMES_1BASED]
    target_frame_count = len(selected_indices)

    if max(selected_indices) >= len(source_frames):
        raise SystemExit(
            f"Walk loop selection exceeds available frames: last={max(selected_indices) + 1}, available={len(source_frames)}"
        )

    target_dir.mkdir(parents=True, exist_ok=True)
    for existing in target_dir.glob("feiling_walk_loop_*.png"):
        existing.unlink()

    manifest_lines = [
        f"source_dir={source_frames_dir}",
        f"source_manifest={source_manifest_path}",
        f"source_set={SOURCE_SET_NAME}",
        f"source_frame_count={len(source_frames)}",
        f"target_frame_count={target_frame_count}",
        "selection_frames_1based=" + ",".join(str(frame_number) for frame_number in WALK_LOOP_SOURCE_FRAMES_1BASED),
        "selection_mode=curated_stride_frames",
    ]

    source_indices_1based = source_manifest.get("source_frame_indices_1based", [])

    for output_index, source_index in enumerate(selected_indices):
        source_path = source_frames[source_index]
        out_path = target_dir / f"feiling_walk_loop_{output_index:03d}.png"
        shutil.copy2(source_path, out_path)

        manifest_line = (
            f"{output_index:03d}=src_pos_{source_index + 1:03d}:{source_path.name}"
        )
        if source_indices_1based and source_index < len(source_indices_1based):
            manifest_line += f":video_src_{source_indices_1based[source_index]:03d}"
        manifest_line += f":{out_path.name}"
        manifest_lines.append(manifest_line)

    target_manifest_path.write_text("\n".join(manifest_lines), encoding="utf-8")
    print(f"Built {target_frame_count} walk loop frames in {target_dir}")


if __name__ == "__main__":
    main()
