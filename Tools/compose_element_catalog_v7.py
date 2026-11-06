from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import textwrap

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Research' / 'element_catalog_v7' / 'element_families_original.png'
ASSETS = ROOT / 'Research' / 'element_catalog_v7'
W, H = 1540, 1040
BG = (248, 246, 239)
CARD = (255, 255, 253)
INK = (46, 48, 54)
MUTED = (94, 100, 112)
LINE = (218, 222, 226)
BLUE = (56, 108, 166)
ORANGE = (167, 91, 40)
try:
    font_path = 'C:/Windows/Fonts/segoeui.ttf'
    title_font = ImageFont.truetype(font_path, 38)
    name_font = ImageFont.truetype(font_path, 27)
    desc_font = ImageFont.truetype(font_path, 19)
    note_font = ImageFont.truetype(font_path, 17)
except OSError:
    title_font = name_font = desc_font = note_font = ImageFont.load_default()

def wrap_pixels(draw, text, font, max_width):
    words = text.split()
    lines, current = [], ''
    for word in words:
        candidate = word if not current else current + ' ' + word
        if draw.textlength(candidate, font=font) <= max_width:
            current = candidate
        else:
            if current:
                lines.append(current)
            current = word
    if current:
        lines.append(current)
    return '\n'.join(lines)

cards = [
    ('Vòng xoay', 'Xoay vòng để căn khe và tháo gỡ.', 'native_ring.png', None),
    ('Thanh trượt', 'Kéo dọc trục giới hạn để giải phóng.', 'native_pull.png', None),
    ('Chốt cố định', 'Vật bắt cố định, tạo điểm giữ cho vòng.', 'native_fixed_catch.png', None),
    ('Bướm khóa', 'Thanh bướm với hai khe bắt cần được tháo.', 'native_butterfly.png', None),
    ('Vật mang bom', 'Bom gắn trên một mảnh; va chạm có thể kích nổ.', 'native_bomb_carrier.png', None),
    ('Khóa xích', 'Vòng bị khóa bằng xích; thao tác cần gỡ khóa.', 'native_chain_lock.png', None),
    ('Khe khóa màu', 'Cơ chế có trong code; không thấy trong corpus 507 màn.', None, 'RingPaletteLockGapObject'),
    ('Chìa khóa màu', 'Cơ chế có trong code; không thấy trong corpus 507 màn.', None, 'GameplayPartData.IsPaletteLockKey'),
]
canvas = Image.new('RGB', (W, H), BG)
d = ImageDraw.Draw(canvas)
d.text((44, 26), '8 NHÓM CƠ CHẾ TRONG ROTATE RINGS 1.2.0', font=title_font, fill=INK)
d.text((47, 80), 'Ảnh vật thể lấy từ nguồn hình học và asset gốc trong APK; hai cơ chế màu vắng mặt trong corpus mẫu.', font=desc_font, fill=MUTED)
margin, gap, cardw, cardh = 40, 20, 355, 435
start_y = 125
for idx, (name, desc, image_name, source) in enumerate(cards):
    col, row = idx % 4, idx // 4
    x, y = margin + col * (cardw + gap), start_y + row * (cardh + gap)
    d.rounded_rectangle((x, y, x + cardw, y + cardh), radius=18, fill=CARD, outline=LINE, width=2)
    if image_name:
        p = ASSETS / image_name
        if not p.exists():
            raise FileNotFoundError(f'Missing original thumbnail: {p}')
        im = Image.open(p).convert('RGBA')
        im.thumbnail((cardw - 36, 286))
        ix, iy = x + (cardw - im.width) // 2, y + 20 + (286 - im.height) // 2
        canvas.paste(im, (ix, iy), im)
        desc_color = MUTED
    else:
        # Deliberately no artwork: no source-500 example supports an illustrative rendering.
        box = (x + 18, y + 24, x + cardw - 18, y + 306)
        d.rounded_rectangle(box, radius=14, fill=(244, 242, 236), outline=(224, 219, 208), width=2)
        label = 'CHƯA CÓ MẪU\nTRONG CORPUS'
        bbox = d.multiline_textbbox((0, 0), label, font=name_font, spacing=8, align='center')
        tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
        d.multiline_text((x + (cardw - tw) / 2, y + 126), label, font=name_font, fill=ORANGE, spacing=8, align='center')
        source_wrapped = wrap_pixels(d, source, note_font, cardw - 40)
        d.multiline_text((x + 20, y + 260), source_wrapped, font=note_font, fill=MUTED, spacing=3)
        desc_color = MUTED
    d.text((x + 20, y + 320), name, font=name_font, fill=INK)
    desc_wrapped = wrap_pixels(d, desc, desc_font, cardw - 40)
    d.multiline_text((x + 20, y + 360), desc_wrapped, font=desc_font, fill=desc_color, spacing=5)
canvas.save(OUT)
print(OUT)
