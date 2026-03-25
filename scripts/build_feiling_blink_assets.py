from __future__ import annotations

from pathlib import Path
from PIL import Image, ImageFilter


PROJECT_ROOT = Path(__file__).resolve().parents[1]
SOURCE_DIR = PROJECT_ROOT / "assets" / "characters" / "feiling" / "references" / "source_exports"
BASE_DIR = PROJECT_ROOT / "assets" / "characters" / "feiling" / "base"
STATES_DIR = PROJECT_ROOT / "assets" / "characters" / "feiling" / "states"
ANIMATIONS_DIR = PROJECT_ROOT / "assets" / "characters" / "feiling" / "animations"

OPEN_SOURCE = SOURCE_DIR / "feiling_idle_blink_open_source_v1.png"
HALF_SOURCE = SOURCE_DIR / "feiling_idle_blink_half_source_v2.png"
CLOSED_SOURCE = SOURCE_DIR / "feiling_idle_blink_closed_source_v2.png"
HALF_RETURN_SOURCE = SOURCE_DIR / "feiling_idle_blink_half_return_source_v1.png"

VISIBLE_ALPHA_THRESHOLD = 8
# 只覆盖眼睛与极少量眼睫区域，尽量避开鼻子和嘴，方便后续复用
EYE_REGION = (900, 730, 1160, 815)
MASK_BLUR_RADIUS = 3


def threshold_alpha_bbox(image: Image.Image, threshold: int) -> tuple[int, int, int, int]:
    alpha = image.getchannel("A")
    bbox = alpha.point(lambda p: 255 if p > threshold else 0).getbbox()
    if bbox is None:
        raise RuntimeError("Could not find visible bounds in source image.")
    return bbox


def create_feathered_mask(size: tuple[int, int], blur_radius: int) -> Image.Image:
    mask = Image.new("L", size, 255)
    if blur_radius > 0:
        mask = mask.filter(ImageFilter.GaussianBlur(radius=blur_radius))
    return mask


def build_overlay_canvas(frame: Image.Image, eye_region: tuple[int, int, int, int]) -> Image.Image:
    canvas = Image.new("RGBA", frame.size, (0, 0, 0, 0))
    eye_crop = frame.crop(eye_region)
    mask = create_feathered_mask(eye_crop.size, MASK_BLUR_RADIUS)
    canvas.paste(eye_crop, eye_region[:2], mask)
    return canvas


def crop_to_bbox(image: Image.Image, bbox: tuple[int, int, int, int]) -> Image.Image:
    return image.crop(bbox)


def main() -> None:
    for path in (OPEN_SOURCE, HALF_SOURCE, CLOSED_SOURCE, HALF_RETURN_SOURCE):
        if not path.exists():
            raise FileNotFoundError(f"Missing blink source frame: {path}")

    BASE_DIR.mkdir(parents=True, exist_ok=True)
    STATES_DIR.mkdir(parents=True, exist_ok=True)
    ANIMATIONS_DIR.mkdir(parents=True, exist_ok=True)

    open_frame = Image.open(OPEN_SOURCE).convert("RGBA")
    visible_bbox = threshold_alpha_bbox(open_frame, VISIBLE_ALPHA_THRESHOLD)

    cropped_open = crop_to_bbox(open_frame, visible_bbox)
    cropped_open.save(BASE_DIR / "feiling_master_v1.png")
    cropped_open.save(STATES_DIR / "feiling_idle_v1.png")

    overlay_specs = [
        (HALF_SOURCE, ANIMATIONS_DIR / "feiling_idle_blink_half_overlay_v3.png"),
        (CLOSED_SOURCE, ANIMATIONS_DIR / "feiling_idle_blink_closed_overlay_v3.png"),
        (HALF_RETURN_SOURCE, ANIMATIONS_DIR / "feiling_idle_blink_half_return_overlay_v1.png"),
    ]

    for source_path, target_path in overlay_specs:
        frame = Image.open(source_path).convert("RGBA")
        overlay_canvas = build_overlay_canvas(frame, EYE_REGION)
        cropped_overlay = crop_to_bbox(overlay_canvas, visible_bbox)
        cropped_overlay.save(target_path)

    print("Blink assets rebuilt.")
    print(f"Visible bbox: {visible_bbox}")


if __name__ == "__main__":
    main()
