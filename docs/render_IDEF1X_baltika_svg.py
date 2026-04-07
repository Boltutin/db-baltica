# -*- coding: utf-8 -*-
"""
Генерирует IDEF1X_baltika.svg — логическая модель в нотации IDEF1X (блоки с PK/атрибутами).
Соответствует sql/deploy/02_schema.sql. Запуск: python render_IDEF1X_baltika_svg.py
"""
from __future__ import annotations

from pathlib import Path


def esc(s: str) -> str:
    return (
        s.replace("&", "&amp;")
        .replace("<", "&lt;")
        .replace(">", "&gt;")
        .replace('"', "&quot;")
    )


def box(
    x: float,
    y: float,
    w: float,
    title_ru: str,
    table: str,
    pk: list[str],
    fk: list[str],
    data: list[str],
    *,
    dependent: bool = False,
) -> str:
    """Прямоугольник сущности IDEF1X: верх — имя; линия; зона ключа (PK); зона атрибутов (FK + данные)."""
    rx = 14 if dependent else 4
    line_h = 13
    key_h = 8 + line_h * max(1, len(pk))
    attr_lines = fk + data
    attr_h = 6 + line_h * max(1, len(attr_lines))
    h = 26 + key_h + attr_h
    parts: list[str] = [
        f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="#ffffff" '
        f'stroke="#1a365d" stroke-width="1.6"/>',
        f'<text x="{x + w/2}" y="{y + 17}" text-anchor="middle" font-size="11" font-weight="700" fill="#0f172a">{esc(title_ru)}</text>',
        f'<text x="{x + w/2}" y="{y + 30}" text-anchor="middle" font-size="7.5" fill="#64748b">{esc(table)}</text>',
        f'<line x1="{x}" y1="{y + 36}" x2="{x + w}" y2="{y + 36}" stroke="#1a365d" stroke-width="1.2"/>',
        f'<text x="{x + 6}" y="{y + 48}" font-size="8" font-weight="600" fill="#0f4c81">Ключ</text>',
    ]
    yy = y + 52
    for p in pk:
        parts.append(
            f'<text x="{x + 6}" y="{yy}" font-size="8.5" fill="#0f172a">PK {esc(p)}</text>'
        )
        yy += line_h
    yy += 4
    parts.append(f'<line x1="{x}" y1="{yy - 2}" x2="{x + w}" y2="{yy - 2}" stroke="#94a3b8" stroke-width="0.8"/>')
    parts.append(f'<text x="{x + 6}" y="{yy + 8}" font-size="8" font-weight="600" fill="#334155">Атрибуты / FK</text>')
    yy += 14
    for a in fk:
        parts.append(
            f'<text x="{x + 6}" y="{yy}" font-size="8" fill="#7c3aed">FK {esc(a)}</text>'
        )
        yy += line_h
    for a in data:
        parts.append(f'<text x="{x + 6}" y="{yy}" font-size="8" fill="#475569">{esc(a)}</text>')
        yy += line_h
    return "\n  ".join(parts), h


def main() -> None:
    # Координаты подобраны вручную (читаемая сетка)
    pieces: list[str] = []
    bw = 168

    # --- Независимые справочники (верхний ряд) ---
    y0 = 36
    xs = [24, 210, 396, 582, 768, 954]
    refs = [
        ("Адрес", "addresses", ["address_id"], [], ["country", "region", "city", "street", "building_number"]),
        ("Банк", "banks", ["bank_id"], [], ["bank_name"]),
        ("Тип судна", "ship_types", ["type_id"], [], ["type_name"]),
        ("Верфь", "dockyards", ["dockyard_id"], [], ["name", "country"]),
        ("Ед. изм.", "units", ["unit_id"], [], ["unit_name", "unit_code"]),
        ("Капитан", "captains", ["captain_id"], [], ["full_name", "experience", "..."]),
    ]
    heights: list[float] = []
    for i, (ru, tb, pk, fk, dt) in enumerate(refs):
        svg, h = box(xs[i], y0, bw, ru, tb, pk, fk, dt, dependent=False)
        pieces.append(svg)
        heights.append(h)

    # --- Порт (зависим от адреса — скругление) ---
    y1 = y0 + max(heights) + 28
    svg, h_port = box(396, y1, bw * 2 + 18, "Порт", "ports", ["port_id"], ["address_id"], ["port_name", "..."], dependent=True)
    pieces.append(svg)

    # --- Отправитель / Получатель ---
    y2 = y1 + h_port + 24
    svg, h_s = box(210, y2, bw, "Отправитель", "senders", ["sender_id"], ["bank_id", "address_id"], ["sender_name", "inn_sender", "..."], dependent=True)
    pieces.append(svg)
    svg, h_c = box(582, y2, bw, "Получатель", "consignees", ["consignee_id"], ["bank_id", "address_id"], ["consignee_name", "inn_consignee", "..."], dependent=True)
    pieces.append(svg)

    # --- Судно ---
    y3 = y2 + max(h_s, h_c) + 24
    svg, h_ship = box(
        340,
        y3,
        440,
        "Судно",
        "ships",
        ["ship_id"],
        ["captain_id", "type_id", "dockyard_id", "home_port_id"],
        ["reg_number", "name", "capacity", "year_built", "customs_value", "picture", "..."],
        dependent=True,
    )
    pieces.append(svg)

    # --- Рейс ---
    y4 = y3 + h_ship + 24
    svg, h_sh = box(
        340,
        y4,
        440,
        "Рейс",
        "shipments",
        ["shipment_id"],
        ["ship_id", "origin_port_id", "destination_port_id"],
        ["departure_date", "arrive_date", "customs_value", "custom_clearance", "..."],
        dependent=True,
    )
    pieces.append(svg)

    # --- Груз ---
    y5 = y4 + h_sh + 24
    svg, h_cg = box(
        340,
        y5,
        440,
        "Груз",
        "cargo",
        ["cargo_id"],
        ["shipment_id", "sender_id", "consignee_id", "unit_id"],
        ["cargo_number", "cargo_name", "declared_value", "insured_value", "custom_value", "quantity", "comment", "..."],
        dependent=True,
    )
    pieces.append(svg)

    # --- Связи: пунктир = ненидентифицирующие FK; сплошная = судно → рейс → груз ---
    line_svg = [
        # address -> port: from bottom of addresses box
        f'<path d="M {24 + bw/2} {y0 + max(heights)} L {24 + bw/2} {y1 + 8} L {500} {y1 + 8} L {500} {y1}" fill="none" stroke="#64748b" stroke-width="1" stroke-dasharray="4,3"/>',
        # bank -> sender
        f'<path d="M {210 + bw/2} {y0 + max(heights)} L {210 + bw/2} {y2}" fill="none" stroke="#64748b" stroke-width="1" stroke-dasharray="4,3"/>',
        # bank -> consignee
        f'<path d="M {210 + bw/2} {y0 + max(heights)} L {210 + bw/2} {y2 + h_s/2} L {582} {y2 + h_c/2}" fill="none" stroke="#94a3b8" stroke-width="1" stroke-dasharray="4,3"/>',
        # address -> sender / consignee (one branch)
        f'<path d="M {24 + bw} {y0 + max(heights) - 20} L {180} {y0 + max(heights) - 20} L {180} {y2 + 25} L {210} {y2 + 25}" fill="none" stroke="#64748b" stroke-width="1" stroke-dasharray="4,3"/>',
        f'<path d="M {180} {y2 + 25} L {582} {y2 + 25}" fill="none" stroke="#94a3b8" stroke-width="0.8" stroke-dasharray="3,3"/>',
        # ship_types -> ships
        f'<path d="M {396 + bw} {y0 + max(heights)} L {500} {y0 + max(heights)} L {500} {y3}" fill="none" stroke="#64748b" stroke-width="1" stroke-dasharray="4,3"/>',
        # dockyards -> ships
        f'<path d="M {582 + bw/2} {y0 + max(heights)} L {582 + bw/2} {y3 - 40} L {500} {y3 - 40} L {500} {y3}" fill="none" stroke="#94a3b8" stroke-width="1" stroke-dasharray="4,3"/>',
        # captains -> ships
        f'<path d="M {954 + bw/2} {y0 + max(heights)} L {954 + bw/2} {y3 - 60} L {620} {y3 - 60} L {620} {y3}" fill="none" stroke="#94a3b8" stroke-width="1" stroke-dasharray="4,3"/>',
        # port -> ship home
        f'<path d="M {500} {y1 + h_port} L {500} {y3}" fill="none" stroke="#64748b" stroke-width="1.2" stroke-dasharray="5,4"/>',
        # port -> shipment (origin/dest) simplified vertical through ship column
        f'<path d="M {460} {y1 + h_port/2} L {340} {y1 + h_port/2} L {340} {y4 + 20} L {500} {y4 + 20}" fill="none" stroke="#94a3b8" stroke-width="0.9" stroke-dasharray="3,3"/>',
        f'<path d="M {540} {y1 + h_port/2} L {780} {y1 + h_port/2} L {780} {y4 + 35} L {560} {y4 + 35}" fill="none" stroke="#94a3b8" stroke-width="0.9" stroke-dasharray="3,3"/>',
        # ship -> shipment -> cargo (сплошная, акцент)
        f'<path d="M {560} {y3 + h_ship} L {560} {y4}" fill="none" stroke="#0f4c81" stroke-width="2"/>',
        f'<path d="M {560} {y4 + h_sh} L {560} {y5}" fill="none" stroke="#0f4c81" stroke-width="2"/>',
        # units -> cargo
        f'<path d="M {768 + bw/2} {y0 + max(heights)} L {768 + bw/2} {y5 - 30} L {560} {y5 - 30} L {560} {y5}" fill="none" stroke="#64748b" stroke-width="1" stroke-dasharray="4,3"/>',
        # senders -> cargo
        f'<path d="M {210 + bw} {y2 + h_s/2} L {340} {y2 + h_s/2} L {340} {y5 + 40} L {400} {y5 + 40}" fill="none" stroke="#64748b" stroke-width="1" stroke-dasharray="4,3"/>',
        # consignees -> cargo
        f'<path d="M {582} {y2 + h_c/2} L {520} {y2 + h_c/2} L {520} {y5 + 55} L {480} {y5 + 55}" fill="none" stroke="#64748b" stroke-width="1" stroke-dasharray="4,3"/>',
    ]

    legend_y = y5 + h_cg + 20
    legend = f"""
  <rect x="24" y="{legend_y}" width="900" height="92" rx="4" fill="#f8fafc" stroke="#cbd5e0" stroke-width="1"/>
  <text x="36" y="{legend_y + 20}" font-size="10.5" font-weight="600" fill="#1e293b">IDEF1X: условные обозначения</text>
  <text x="36" y="{legend_y + 38}" font-size="8.5" fill="#475569">Прямоугольник с прямыми углами — независимая сущность (ключи только свои); скруглённые углы — сущность с внешними FK в зоне атрибутов.</text>
  <text x="36" y="{legend_y + 54}" font-size="8.5" fill="#475569">Связь ненидентифицирующая (пунктир): внешний ключ не входит в первичный ключ (суррогатные SERIAL везде).</text>
  <text x="36" y="{legend_y + 70}" font-size="8.5" fill="#475569">Сплошная линия: основная цепочка «Судно → Рейс → Груз». Источник: sql/deploy/02_schema.sql.</text>
  <text x="36" y="{legend_y + 86}" font-size="8" fill="#64748b">«...» — служебные поля created_at / updated_at для краткости.</text>
"""

    vb_h = legend_y + 120
    header = f"""<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1120 {vb_h}" width="1120" height="{vb_h}" font-family="Segoe UI, Arial, Helvetica, sans-serif">
"""
    out = (
        header
        + "  <rect width=\"100%\" height=\"100%\" fill=\"#fafcff\"/>\n  "
        + "\n  ".join(pieces)
        + "\n  "
        + "\n  ".join(line_svg)
        + legend
        + "\n</svg>\n"
    )

    path = Path(__file__).with_name("IDEF1X_baltika.svg")
    path.write_text(out, encoding="utf-8", newline="\n")
    assert "Судно" in out and "shipments" in out
    print("OK", path, path.stat().st_size)


if __name__ == "__main__":
    main()
