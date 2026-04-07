# -*- coding: utf-8 -*-
"""
Генерирует docs/Baltika_IDEF1X.drawio — черновик логической схемы для diagrams.net (Draw.io).
Источник полей: sql/deploy/02_schema.sql. Открытие: app.diagrams.net → File → Open → выбрать файл.

После открытия можно: выделить всё → Arrange → Layout → … или вручную развести связи.
"""
from __future__ import annotations

import html
from pathlib import Path
from xml.sax.saxutils import escape as xml_escape


def esc_attr(s: str) -> str:
    return xml_escape(s, {'"': '&quot;', "'": '&apos;'})


def entity_label(
    title_ru: str,
    table: str,
    pk: list[str],
    fk: list[str],
    data: list[str],
) -> str:
    lines = [
        f"<b>{html.escape(title_ru)}</b>",
        f'<font color="#64748b">{html.escape(table)}</font>',
        "<hr size='1'/>",
        '<font color="#0f4c81"><b>Ключ</b></font>',
    ]
    for p in pk:
        lines.append(f"PK {html.escape(p)}")
    lines.append('<font color="#334155"><b>Атрибуты / FK</b></font>')
    for f in fk:
        lines.append(f'<font color="#7c3aed">FK {html.escape(f)}</font>')
    for d in data:
        lines.append(html.escape(d))
    return "<div style='text-align:left;font-size:10px;line-height:1.25;'>" + "<br/>".join(lines) + "</div>"


def cell_style(*, dependent: bool) -> str:
    base = (
        "whiteSpace=wrap;html=1;align=left;verticalAlign=top;"
        "fillColor=#ffffff;strokeColor=#1a365d;strokeWidth=2;"
        "spacingLeft=6;spacingTop=4;spacingRight=4;spacingBottom=4;"
        "fontFamily=Segoe UI;fontSize=10;"
    )
    if dependent:
        return base + "rounded=1;arcSize=12;"
    return base + "rounded=0;"


def edge_style(*, solid: bool) -> str:
    if solid:
        return (
            "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;"
            "jettySize=auto;html=1;strokeWidth=2;strokeColor=#0f4c81;"
            "endArrow=block;endFill=1;"
        )
    return (
        "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;"
        "jettySize=auto;html=1;strokeWidth=1;dashed=1;dashPattern=5 4;"
        "strokeColor=#64748b;endArrow=block;endFill=0;"
    )


def main() -> None:
    # id -> (x, y, w, h, dependent, label_html)
    entities: dict[str, tuple[float, float, float, float, bool, str]] = {}

    def add(
        eid: str,
        x: float,
        y: float,
        w: float,
        h: float,
        dependent: bool,
        title: str,
        table: str,
        pk: list[str],
        fk: list[str],
        data: list[str],
    ) -> None:
        entities[eid] = (x, y, w, h, dependent, entity_label(title, table, pk, fk, data))

    # --- справочники (верх) ---
    y0 = 40
    w0 = 168
    gap = 18
    xs = [40 + i * (w0 + gap) for i in range(6)]
    h_ref = 200
    add("addresses", xs[0], y0, w0, h_ref, False, "Адрес", "addresses", ["address_id"], [], ["country", "region", "city", "street", "building_number"])
    add("banks", xs[1], y0, w0, h_ref, False, "Банк", "banks", ["bank_id"], [], ["bank_name"])
    add("ship_types", xs[2], y0, w0, h_ref, False, "Тип судна", "ship_types", ["type_id"], [], ["type_name"])
    add("dockyards", xs[3], y0, w0, h_ref, False, "Верфь", "dockyards", ["dockyard_id"], [], ["name", "country"])
    add("units", xs[4], y0, w0, h_ref, False, "Ед. изм.", "units", ["unit_id"], [], ["unit_name", "unit_code"])
    add("captains", xs[5], y0, w0, h_ref, False, "Капитан", "captains", ["captain_id"], [], ["full_name", "experience", "created_at, updated_at"])

    y1 = y0 + h_ref + 40
    w_port = w0 * 2 + gap
    add("ports", xs[2], y1, w_port, 160, True, "Порт", "ports", ["port_id"], ["address_id"], ["port_name", "created_at, updated_at"])

    y2 = y1 + 160 + 36
    h_mid = 200
    add("senders", xs[1], y2, w0, h_mid, True, "Отправитель", "senders", ["sender_id"], ["bank_id", "address_id"], ["sender_name", "inn_sender", "created_at, updated_at"])
    add("consignees", xs[3], y2, w0, h_mid, True, "Получатель", "consignees", ["consignee_id"], ["bank_id", "address_id"], ["consignee_name", "inn_consignee", "created_at, updated_at"])

    y3 = y2 + h_mid + 40
    w_ship = 440
    x_ship = 40 + (w0 + gap) * 2
    add(
        "ships",
        x_ship,
        y3,
        w_ship,
        240,
        True,
        "Судно",
        "ships",
        ["ship_id"],
        ["captain_id", "type_id", "dockyard_id", "home_port_id"],
        ["reg_number", "name", "capacity", "year_built", "customs_value", "picture", "created_at, updated_at"],
    )

    y4 = y3 + 240 + 36
    add(
        "shipments",
        x_ship,
        y4,
        w_ship,
        220,
        True,
        "Рейс",
        "shipments",
        ["shipment_id"],
        ["ship_id", "origin_port_id", "destination_port_id"],
        ["departure_date", "arrive_date", "customs_value", "custom_clearance", "created_at, updated_at"],
    )

    y5 = y4 + 220 + 36
    add(
        "cargo",
        x_ship,
        y5,
        w_ship,
        260,
        True,
        "Груз",
        "cargo",
        ["cargo_id"],
        ["shipment_id", "sender_id", "consignee_id", "unit_id"],
        [
            "cargo_number",
            "cargo_name",
            "declared_value",
            "insured_value",
            "custom_value",
            "quantity",
            "comment",
            "created_at, updated_at",
        ],
    )

    legend_y = y5 + 260 + 30
    legend_h = 100
    legend_w = 900
    legend_x = 40

    cells_xml: list[str] = []
    nid = 2
    id_map: dict[str, str] = {}

    for eid, (x, y, w, h, dep, label) in entities.items():
        cid = str(nid)
        nid += 1
        id_map[eid] = cid
        val = esc_attr(label)
        st = esc_attr(cell_style(dependent=dep))
        cells_xml.append(
            f'        <mxCell id="{cid}" value="{val}" style="{st}" vertex="1" parent="1">\n'
            f'          <mxGeometry x="{x}" y="{y}" width="{w}" height="{h}" as="geometry"/>\n'
            f"        </mxCell>"
        )

    legend_cid = str(nid)
    nid += 1
    legend_text = (
        "<div style='text-align:left;font-size:10px;'>"
        "<b>IDEF1X (черновик для правки в Draw.io)</b><br/>"
        "Прямые углы — независимые сущности; скругление — есть FK в атрибутах.<br/>"
        "Пунктир — ненидентифицирующие связи; сплошная жирная — Судно → Рейс → Груз.<br/>"
        "<font color='#64748b'>Источник: sql/deploy/02_schema.sql · генератор: docs/render_baltika_drawio.py</font>"
        "</div>"
    )
    cells_xml.append(
        f'        <mxCell id="{legend_cid}" value="{esc_attr(legend_text)}" '
        f'style="{esc_attr("rounded=0;whiteSpace=wrap;html=1;fillColor=#f8fafc;strokeColor=#cbd5e0;align=left;verticalAlign=top;spacing=8;")}" '
        f'vertex="1" parent="1">\n'
        f'          <mxGeometry x="{legend_x}" y="{legend_y}" width="{legend_w}" height="{legend_h}" as="geometry"/>\n'
        f"        </mxCell>"
    )

    # (source, target, solid, optional label)
    edges: list[tuple[str, str, bool, str]] = [
        ("addresses", "ports", False, ""),
        ("banks", "senders", False, ""),
        ("banks", "consignees", False, ""),
        ("addresses", "senders", False, ""),
        ("addresses", "consignees", False, ""),
        ("ship_types", "ships", False, ""),
        ("dockyards", "ships", False, ""),
        ("captains", "ships", False, ""),
        ("ports", "ships", False, "home_port_id"),
        ("ports", "shipments", False, "origin / destination"),
        ("ships", "shipments", True, ""),
        ("shipments", "cargo", True, ""),
        ("senders", "cargo", False, ""),
        ("consignees", "cargo", False, ""),
        ("units", "cargo", False, ""),
    ]

    for i, (a, b, solid, elab) in enumerate(edges):
        eid = str(nid)
        nid += 1
        src = id_map[a]
        tgt = id_map[b]
        est = esc_attr(edge_style(solid=solid))
        lbl = esc_attr(elab) if elab else ""
        cells_xml.append(
            f'        <mxCell id="{eid}" value="{lbl}" style="{est}" edge="1" parent="1" source="{src}" target="{tgt}">\n'
            f'          <mxGeometry relative="1" as="geometry"/>\n'
            f"        </mxCell>"
        )

    inner = "\n".join(cells_xml)
    page_h = legend_y + legend_h + 60
    pw = int(page_h * 0.6 + 400)
    ph = int(page_h)

    # Без textwrap.dedent: иначе общий префикс пробелов «ломает» отступы у <mxGeometry> внутри ячеек.
    out = (
        '<?xml version="1.0" encoding="UTF-8"?>\n'
        '<mxfile host="app.diagrams.net" agent="BaltikaApp/render_baltika_drawio" version="22.1.0" type="device">\n'
        '  <diagram id="baltika-idef1x" name="Балтика IDEF1X">\n'
        f'    <mxGraphModel dx="1400" dy="900" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" '
        f'fold="1" page="1" pageScale="1" pageWidth="{pw}" pageHeight="{ph}" math="0" shadow="0">\n'
        '      <root>\n'
        '        <mxCell id="0"/>\n'
        '        <mxCell id="1" parent="0"/>\n'
        f"{inner}\n"
        "      </root>\n"
        "    </mxGraphModel>\n"
        "  </diagram>\n"
        "</mxfile>\n"
    )

    path = Path(__file__).with_name("Baltika_IDEF1X.drawio")
    path.write_text(out, encoding="utf-8", newline="\n")
    print("OK", path, path.stat().st_size)


if __name__ == "__main__":
    main()
