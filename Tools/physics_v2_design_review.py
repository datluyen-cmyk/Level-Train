#!/usr/bin/env python3
"""Create a review contact sheet, index, and playtest log for a physics-v2 batch.

The preview draws the exact PolygonCollider2D paths in each referenced source
template. It does not synthesize pin poses or claim that a generated layout has
source-level provenance, human parity, or accepted physical replay.
"""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import math
import re
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[1]
DEFAULT_CATALOG = ROOT / "Research" / "physics_v2" / "templates.json"
DEFAULT_CORPUS = ROOT / "Research" / "normalized_corpus.json"
DEFAULT_OUTPUT = ROOT / "RotateRings Test" / "LevelTraining_PhysicsV2"
PALETTE = ("#32AFDD", "#EC8F2E", "#984BE0", "#DD433E", "#6AC53E", "#4E77DD", "#23A468")


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def level_parts(level: dict[str, Any]) -> list[dict[str, Any]]:
    state = level.get("state")
    if isinstance(state, dict):
        parts = state.get("parts")
        if isinstance(parts, list):
            return parts
    parts = level.get("parts")
    return parts if isinstance(parts, list) else []


def level_links(level: dict[str, Any]) -> list[dict[str, Any]]:
    state = level.get("state")
    if isinstance(state, dict) and isinstance(state.get("links"), list):
        return state["links"]
    links = level.get("links")
    return links if isinstance(links, list) else []


def read_json(path: Path) -> dict[str, Any]:
    with path.open("r", encoding="utf-8-sig") as stream:
        data = json.load(stream)
    if not isinstance(data, dict):
        raise ValueError(f"Expected a JSON object: {path}")
    return data


def candidate_score(path: Path) -> tuple[int, int]:
    try:
        data = read_json(path)
        levels = data.get("levels") or []
        if not levels:
            return (-1, 0)
        first_parts = level_parts(levels[0])
        has_templates = bool(first_parts) and all(p.get("templateId") for p in first_parts)
        schema2 = data.get("schemaVersion") == 2
        return (int(has_templates) + int(schema2), len(levels))
    except Exception:
        return (-1, 0)


def resolve_batch(argument: Path) -> tuple[Path, dict[str, Any]]:
    if argument.is_file():
        path = argument
    elif argument.is_dir():
        names = ("unity_levels.json", "physics_v2_pack.json", "generated_levels.json", "pack.json", "levels.json")
        candidates = [p for name in names for p in argument.rglob(name)]
        if not candidates:
            raise FileNotFoundError(f"No pack JSON found beneath {argument}")
        path = max(candidates, key=lambda p: candidate_score(p))
    else:
        raise FileNotFoundError(argument)
    data = read_json(path)
    levels = data.get("levels")
    if not isinstance(levels, list) or not levels:
        raise ValueError(f"Pack has no levels array: {path}")
    missing = [str(level.get("id", "?")) for level in levels if not level_parts(level) or
               any(not p.get("templateId") for p in level_parts(level))]
    if missing:
        raise ValueError("Pack is not the final template-bound v2 format; missing templateId in level(s): " +
                         ", ".join(missing[:12]))
    return path, data


def source_position_map(corpus_path: Path) -> tuple[dict[str, dict[str, Any]], dict[str, dict[str, Any]]]:
    data = read_json(corpus_path)
    by_id: dict[str, dict[str, Any]] = {}
    by_alias: dict[str, dict[str, Any]] = {}
    for level in data.get("levels", []):
        level_id = str(level.get("id", ""))
        by_id[level_id] = level
        by_alias[level_id] = level
        by_alias[f"original-{level_id}"] = level
    return by_id, by_alias


def get_reference(level: dict[str, Any], by_id: dict[str, dict[str, Any]],
                  by_alias: dict[str, dict[str, Any]]) -> tuple[str, dict[str, Any] | None]:
    ref = str(level.get("referenceId", level.get("reference_id", "")) or "")
    candidate = by_alias.get(ref) or by_id.get(ref)
    if candidate is None:
        match = re.search(r"(?:original[-_:])?(\d+)", ref)
        if match:
            candidate = by_id.get(match.group(1))
    return ref, candidate


def nested_value(data: dict[str, Any], keys: tuple[str, ...]) -> Any:
    """Read a value only if supplied by the input metadata; never infer a score."""
    containers = [data]
    for name in ("features", "acceptance", "scores", "metrics", "proxy", "difficultyEstimate"):
        value = data.get(name)
        if isinstance(value, dict):
            containers.append(value)
    for container in containers:
        for key in keys:
            if key in container and container[key] is not None:
                return container[key]
    return None


def graph_metrics(parts: list[dict[str, Any]], links: list[dict[str, Any]]) -> tuple[int, int, str]:
    ids = {p.get("id") for p in parts}
    out: dict[Any, list[Any]] = {node: [] for node in ids}
    indegree: dict[Any, int] = {node: 0 for node in ids}
    for link in links:
        source, target = link.get("source"), link.get("target")
        if source in ids and target in ids:
            out[source].append(target)
            indegree[target] += 1
    branching = sum(1 for values in out.values() if len(values) > 1)
    max_out = max((len(values) for values in out.values()), default=0)
    # Kahn's algorithm gives a reproducible depth for DAGs. Cycles are reported
    # as such instead of receiving a made-up path length.
    queue = [node for node, degree in indegree.items() if degree == 0]
    depth = {node: 0 for node in queue}
    seen = 0
    while queue:
        node = queue.pop(0)
        seen += 1
        for target in out[node]:
            depth[target] = max(depth.get(target, 0), depth[node] + 1)
            indegree[target] -= 1
            if indegree[target] == 0:
                queue.append(target)
    return branching, max_out, str(max(depth.values(), default=0)) if seen == len(ids) else "cycle"


def index_row(level: dict[str, Any], level_number: int, by_id: dict[str, dict[str, Any]],
              by_alias: dict[str, dict[str, Any]]) -> dict[str, Any]:
    parts = level_parts(level)
    links = level_links(level)
    ref_id, reference = get_reference(level, by_id, by_alias)
    ref_state = reference.get("state", {}) if reference else {}
    reference_parts = ref_state.get("parts", []) if isinstance(ref_state, dict) else []
    branching, max_out, depth = graph_metrics(parts, links)
    novelty = nested_value(level, ("noveltyScore", "novelty_score", "noveltyDistance", "minOriginalDistance"))
    proxy = nested_value(level, ("proxyScore", "proxy_score", "difficultyScore", "proxyDifficulty", "difficulty"))
    kinds: dict[str, int] = {}
    for part in parts:
        kind = str(part.get("kind", "unknown"))
        kinds[kind] = kinds.get(kind, 0) + 1
    position = level.get("referencePosition", level.get("reference_position"))
    if position is None and reference:
        position = reference.get("position")
    if position is None and isinstance(level.get("proposalDonor"), dict):
        position = level["proposalDonor"].get("position")
    return {
        "level_id": level.get("id", level_number),
        "reference_id": ref_id,
        "reference_position": position if position is not None else "",
        "reference_part_count": len(reference_parts) if reference else "",
        "actual_part_count": len(parts),
        "ring_count": kinds.get("ring", 0),
        "slider_count": kinds.get("slider", 0),
        "link_count": len(links),
        "branching_parts": branching,
        "max_out_degree": max_out,
        "graph_depth": depth,
        "novelty_score": novelty if novelty is not None else "",
        "proxy_score": proxy if proxy is not None else "",
        "proxy_score_field": "provided metadata" if proxy is not None else "",
        "template_ids": ";".join(str(p.get("templateId", "")) for p in parts),
    }


def point_xy(point: Any) -> tuple[float, float]:
    if isinstance(point, dict):
        return float(point.get("x", 0.0)), float(point.get("y", 0.0))
    if isinstance(point, (list, tuple)) and len(point) >= 2:
        return float(point[0]), float(point[1])
    raise ValueError(f"Unrecognized source collider point: {point!r}")


def world_polygon(points: list[Any], part: dict[str, Any]) -> list[tuple[float, float]]:
    x0, y0 = float(part.get("x", 0.0)), float(part.get("y", 0.0))
    angle = math.radians(float(part.get("angle", 0.0)))
    c, s = math.cos(angle), math.sin(angle)
    result = []
    for point in points:
        x, y = point_xy(point)
        result.append((x0 + c * x - s * y, y0 + s * x + c * y))
    return result


def template_paths(template: dict[str, Any]) -> list[tuple[str, int, bool, list[Any]]]:
    result = []
    for shape in template.get("physicalShapes") or []:
        paths = shape.get("paths") or []
        if paths:
            for path in paths:
                points = path.get("points", []) if isinstance(path, dict) else path
                if points:
                    result.append((str(shape.get("role", "body")), int(shape.get("attachmentId", -1)),
                                   bool(shape.get("trigger", False)), points))
        elif shape.get("points"):
            result.append((str(shape.get("role", "body")), int(shape.get("attachmentId", -1)),
                           bool(shape.get("trigger", False)), shape["points"]))
    return result


def render_preview(levels: list[dict[str, Any]], rows: list[dict[str, Any]],
                   templates: dict[str, dict[str, Any]], out_path: Path) -> list[int]:
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    from matplotlib.patches import Polygon

    n = len(levels)
    positions = list(range(min(30, n)))
    representatives = [max(0, round((n - 1) * fraction)) for fraction in (0.165, 0.33, 0.5, 0.665, 0.83, 0.995)]
    selected = list(dict.fromkeys(positions + representatives))
    cols = 6
    rows_n = math.ceil(len(selected) / cols)
    fig, axes = plt.subplots(rows_n, cols, figsize=(cols * 3.0, rows_n * 3.15), dpi=150)
    flat_axes = list(axes.flat) if hasattr(axes, "flat") else [axes]
    role_style = {
        "body": (0.20, 0.32),
        "stem": (0.75, 0.7),
        "head_trigger": (0.95, 0.9),
        "head_wall": (0.78, 0.85),
        "wall": (0.7, 0.8),
    }
    for cell, ax in enumerate(flat_axes):
        ax.set_axis_off()
        if cell >= len(selected):
            continue
        idx = selected[cell]
        level = levels[idx]
        metric = rows[idx]
        all_x: list[float] = []
        all_y: list[float] = []
        part_items = level_parts(level)
        for part in part_items:
            template = templates[str(part.get("templateId"))]
            color = PALETTE[abs(int(part.get("color", 0))) % len(PALETTE)]
            paths = template_paths(template)
            if not paths:
                raise ValueError(f"Template {template.get('id')} contains no physical shapes")
            for role, attachment_id, trigger, local_points in paths:
                polygon = world_polygon(local_points, part)
                all_x.extend(p[0] for p in polygon)
                all_y.extend(p[1] for p in polygon)
                alpha, line_alpha = role_style.get(role, (0.45, 0.65))
                if role == "body":
                    face = color
                    edge = color
                elif role == "stem":
                    face = color
                    edge = "#442C23"
                elif role == "head_trigger":
                    face = "#FFE7A6"
                    edge = "#735A31"
                elif role == "head_wall":
                    face = "#DAD2C5"
                    edge = "#554A40"
                else:
                    face = color
                    edge = "#554A40"
                ax.add_patch(Polygon(polygon, closed=True, facecolor=face,
                                     edgecolor=edge, linewidth=.45, alpha=alpha,
                                     zorder=2 if role == "body" else 3))
        if all_x:
            span = max(max(all_x) - min(all_x), max(all_y) - min(all_y), 1.0)
            margin = span * .08
            ax.set_xlim(min(all_x) - margin, max(all_x) + margin)
            ax.set_ylim(min(all_y) - margin, max(all_y) + margin)
            ax.set_aspect("equal", adjustable="box")
        ax.set_title(f"Level {metric['level_id']}  |  ref {metric['reference_id']}\n"
                     f"{metric['actual_part_count']} pieces  ·  {metric['link_count']} links",
                     fontsize=8, pad=3)
    fig.suptitle("Physics v2 — source-template collider preview\nExact catalog paths at generated level positions; not a playtest or parity verdict",
                 fontsize=12, y=.997)
    fig.tight_layout(rect=(0, 0, 1, .98), h_pad=.55, w_pad=.25)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    fig.savefig(out_path, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    return [int(levels[i].get("id", i + 1)) if str(levels[i].get("id", i + 1)).isdigit() else i + 1 for i in selected]


def write_csv(path: Path, fields: list[str], rows: list[dict[str, Any]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", newline="", encoding="utf-8-sig") as stream:
        writer = csv.DictWriter(stream, fieldnames=fields, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(rows)


def write_playtest_log(path: Path, levels: list[dict[str, Any]], rows: list[dict[str, Any]]) -> None:
    fields = ["level_id", "reference_id", "tester", "test_date", "attempted", "solved",
              "time_minutes", "moves_used", "first_impression", "confusing_moment",
              "felt_fair", "visual_readability", "enjoyment_1_to_5", "notes", "capture_path"]
    entries = [{"level_id": row["level_id"], "reference_id": row["reference_id"]} for row in rows]
    write_csv(path, fields, entries)


def write_notes(path: Path, batch_path: Path, catalog_path: Path, level_count: int,
                preview_ids: list[int], batch_hash: str, catalog_hash: str,
                catalog_status: str, eligible_count: Any, source_count: Any) -> None:
    preview_text = ", ".join(map(str, preview_ids))
    content = f"""# Physics v2 — ghi chú rà soát thiết kế

- Batch: `{batch_path}`
- SHA256 batch: `{batch_hash}`
- Catalog mẫu nguồn: `{catalog_path}`
- SHA256 catalog: `{catalog_hash}`
- Trạng thái catalog: `{catalog_status}` ({eligible_count}/{source_count} nguồn đủ điều kiện theo metadata catalog)
- Số màn trong index: {level_count}
- Preview hiển thị 30 màn đầu và các vị trí đại diện; ID được chọn: {preview_text}
- Hình preview vẽ các đường PolygonCollider2D nguyên bản theo `templateId` của từng quân cờ, đặt tại vị trí/góc của batch. Tọa độ pin lấy nguyên từ template; script không suy ra pin từ link và không co giãn riêng pin.
- Bố cục trong batch vẫn là bố cục sinh mới. Việc dùng từng mảnh nguồn nguyên bản không chứng minh toàn bộ bố cục từng xuất hiện trong game gốc.
- Chỉ số nhánh/độ sâu được tính từ đồ thị link của batch. Điểm novelty/proxy chỉ được sao chép nếu batch có trường đó; trường thiếu để trống. Chúng không phải đánh giá của người chơi.
- Tài liệu này không khẳng định 300 màn đã được xác nhận vật lý, có độ khó tương đương, vui, công bằng hay có human parity. Hãy ghi trải nghiệm thực tế trong `Playtest Log.csv`.

## Cách đọc

- `Level Design Preview.png`: contact sheet để kiểm tra nhanh các đường viền vật lý theo vị trí.
- `Level Design Index.csv`: một dòng cho mỗi màn; `reference_position/count` là tham chiếu gốc nếu ánh xạ được, `actual_part_count` là số quân thật trong batch.
- `Playtest Log.csv`: 300 dòng trống để ghi nhận buổi chơi thủ công.
"""
    path.write_text(content, encoding="utf-8")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("batch", type=Path, help="Final v2 pack JSON or its batch directory")
    parser.add_argument("--catalog", type=Path, default=DEFAULT_CATALOG)
    parser.add_argument("--source-corpus", type=Path, default=DEFAULT_CORPUS)
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()

    batch_path, pack = resolve_batch(args.batch)
    if len(pack["levels"]) != 300:
        raise ValueError(f"Expected the final 300-level batch, received {len(pack['levels'])}")
    catalog_data = read_json(args.catalog)
    templates_list = catalog_data.get("templates")
    if not isinstance(templates_list, list):
        raise ValueError(f"Catalog has no templates array: {args.catalog}")
    templates = {str(t["id"]): t for t in templates_list}
    levels = pack["levels"]
    for level in levels:
        for part in level_parts(level):
            template_id = str(part.get("templateId", ""))
            if template_id not in templates:
                raise ValueError(f"Level {level.get('id')} uses missing templateId {template_id!r}")
            if not template_paths(templates[template_id]):
                raise ValueError(f"Template {template_id} has no physical collider paths")

    by_id, by_alias = source_position_map(args.source_corpus)
    rows = [index_row(level, i + 1, by_id, by_alias) for i, level in enumerate(levels)]
    index_path = args.output / "Level Design Index.csv"
    write_csv(index_path, list(rows[0].keys()), rows)
    preview_path = args.output / "Level Design Preview.png"
    selected_ids = render_preview(levels, rows, templates, preview_path)
    write_playtest_log(args.output / "Playtest Log.csv", levels, rows)
    write_notes(args.output / "README_vi.md", batch_path, args.catalog, len(levels), selected_ids,
                sha256(batch_path), sha256(args.catalog), str(catalog_data.get("ruleStatus", "unspecified")),
                catalog_data.get("eligibleSourceCount", "unspecified"), catalog_data.get("sourceCount", "unspecified"))
    print(f"Wrote review artifacts to {args.output}")
    print(f"Levels indexed: {len(levels)}; preview samples: {len(selected_ids)}")
    print("No solvability, novelty, difficulty, or human-parity claim was generated.")


if __name__ == "__main__":
    main()
