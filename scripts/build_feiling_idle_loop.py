from pathlib import Path
from PIL import Image


ALPHA_THRESHOLD = 8
COLUMNS = 8
ROWS = 8


def has_visible_pixels(img: Image.Image) -> bool:
    alpha = img.getchannel("A")
    bbox = alpha.point(lambda a: 255 if a > ALPHA_THRESHOLD else 0).getbbox()
    return bbox is not None


def visible_bbox(img: Image.Image):
    alpha = img.getchannel("A")
    return alpha.point(lambda a: 255 if a > ALPHA_THRESHOLD else 0).getbbox()


def main():
    project_root = Path(__file__).resolve().parent.parent
    source_path = project_root / "assets" / "characters" / "feiling" / "references" / "source_exports" / "feiling_idle_loop_sheet_source_v1.png"
    target_dir = project_root / "assets" / "characters" / "feiling" / "animations" / "idle_loop"
    manifest_path = target_dir / "manifest.txt"

    sheet = Image.open(source_path).convert("RGBA")
    cell_w = sheet.width // COLUMNS
    cell_h = sheet.height // ROWS

    frames = []
    union_bbox = None
    for row in range(ROWS):
        for col in range(COLUMNS):
            left = col * cell_w
            top = row * cell_h
            frame = sheet.crop((left, top, left + cell_w, top + cell_h))
            if not has_visible_pixels(frame):
                continue

            bbox = visible_bbox(frame)
            frames.append((row, col, frame))
            if union_bbox is None:
                union_bbox = list(bbox)
            else:
                union_bbox[0] = min(union_bbox[0], bbox[0])
                union_bbox[1] = min(union_bbox[1], bbox[1])
                union_bbox[2] = max(union_bbox[2], bbox[2])
                union_bbox[3] = max(union_bbox[3], bbox[3])

    if not frames or union_bbox is None:
        raise SystemExit("No visible frames found in sprite sheet.")

    target_dir.mkdir(parents=True, exist_ok=True)
    for old in target_dir.glob("feiling_idle_loop_*.png"):
        old.unlink()

    crop_box = tuple(union_bbox)
    manifest_lines = [
        f"source={source_path}",
        f"cell_size={cell_w}x{cell_h}",
        f"crop_box={crop_box}",
        f"frame_count={len(frames)}",
    ]

    for index, (row, col, frame) in enumerate(frames):
        cropped = frame.crop(crop_box)
        out_path = target_dir / f"feiling_idle_loop_{index:03d}.png"
        cropped.save(out_path)
        manifest_lines.append(f"{index:03d}=row{row}_col{col}:{out_path.name}")

    manifest_path.write_text("\n".join(manifest_lines), encoding="utf-8")
    print(f"Built {len(frames)} idle loop frames in {target_dir}")


if __name__ == "__main__":
    main()
