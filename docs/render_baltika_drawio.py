# -*- coding: utf-8 -*-
"""
Генерирует docs/Baltika_conceptual_ER.drawio — концептуальная ER-модель (этап конструирования БД):
нотация Чена: сущности (прямоугольники), связи (ромбы), кардинальности на рёбрах.

Предметная область согласована с sql/deploy/02_schema.sql; без имён таблиц, типов SQL и служебных полей.

Открытие: app.diagrams.net → File → Open. Пересборка: python docs/render_baltika_drawio.py
"""
from __future__ import annotations

import html
from pathlib import Path
from xml.sax.saxutils import escape as xml_escape


def esc_attr(s: str) -> str:
    return xml_escape(s, {'"': '&quot;', "'": '&apos;'})


def entity_value(title: str, attrs: list[str]) -> str:
    lines = [f"<b>{html.escape(title)}</b>"]
    for a in attrs:
        lines.append(f'<font size="1" color="#4a5568">{html.escape(a)}</font>')
    return "<div style='text-align:center;line-height:1.2;'>" + "<br/>".join(lines) + "</div>"


STYLE_ENTITY = (
    "rounded=1;whiteSpace=wrap;html=1;align=center;verticalAlign=middle;"
    "fillColor=#dae8fc;strokeColor=#6c8ebf;strokeWidth=2;fontFamily=Segoe UI;fontSize=11;"
)
STYLE_REL = (
    "shape=rhombus;perimeter=rhombusPerimeter;whiteSpace=wrap;html=1;align=center;"
    "verticalAlign=middle;fillColor=#fff2cc;strokeColor=#d6b656;strokeWidth=1.5;"
    "fontFamily=Segoe UI;fontSize=10;"
)
STYLE_LEGEND = (
    "rounded=0;whiteSpace=wrap;html=1;align=left;verticalAlign=top;fillColor=#f8fafc;"
    "strokeColor=#cbd5e0;fontSize=10;spacing=8;"
)

STYLE_EDGE = (
    "edgeStyle=orthogonalEdgeStyle;rounded=1;orthogonalLoop=1;jettySize=auto;html=1;"
    "strokeWidth=1.5;strokeColor=#334155;endArrow=none;startArrow=none;"
)
STYLE_EDGE_MAIN = (
    "edgeStyle=orthogonalEdgeStyle;rounded=1;orthogonalLoop=1;jettySize=auto;html=1;"
    "strokeWidth=2.5;strokeColor=#0f4c81;endArrow=none;startArrow=none;"
)


def cell_vertex(cid: str, x: float, y: float, w: float, h: float, value: str, style: str) -> str:
    return (
        f'        <mxCell id="{cid}" value="{esc_attr(value)}" style="{esc_attr(style)}" '
        f'vertex="1" parent="1">\n'
        f'          <mxGeometry x="{x}" y="{y}" width="{w}" height="{h}" as="geometry"/>\n'
        f"        </mxCell>"
    )


def cell_edge(eid: str, src: str, tgt: str, label: str, style: str) -> str:
    lab = esc_attr(label) if label else ""
    return (
        f'        <mxCell id="{eid}" value="{lab}" style="{esc_attr(style)}" edge="1" parent="1" '
        f'source="{src}" target="{tgt}">\n'
        f'          <mxGeometry relative="1" as="geometry"/>\n'
        f"        </mxCell>"
    )


def main() -> None:
    cells: list[str] = []
    edges: list[str] = []
    nid = 2

    def vid() -> str:
        nonlocal nid
        s = str(nid)
        nid += 1
        return s

    V: dict[str, str] = {}

    def ent(key: str, x: float, y: float, w: float, h: float, title: str, attrs: list[str]) -> None:
        nonlocal cells
        i = vid()
        V[key] = i
        cells.append(cell_vertex(i, x, y, w, h, entity_value(title, attrs), STYLE_ENTITY))

    def rel(key: str, x: float, y: float, w: float, h: float, name: str) -> None:
        nonlocal cells
        i = vid()
        V[key] = i
        cells.append(cell_vertex(i, x, y, w, h, f"<b>{html.escape(name)}</b>", STYLE_REL))

    def edge(a: str, b: str, label: str = "", *, main: bool = False) -> None:
        nonlocal edges
        st = STYLE_EDGE_MAIN if main else STYLE_EDGE
        edges.append(cell_edge(vid(), V[a], V[b], label, st))

    # --- Координаты: центральная колонка — ядро; слева справочники и отправитель; справа получатель ---
    ent("type", 30, 40, 128, 70, "Тип судна", ["наименование типа"])
    rel("r_type", 188, 47, 96, 58, "относится\nк типу")
    ent("ship", 310, 32, 208, 100, "Судно", ["рег. номер", "название", "вместимость, год, …"])

    ent("dock", 30, 150, 128, 70, "Верфь", ["название", "страна"])
    rel("r_dock", 188, 157, 96, 58, "построено\nна верфи")

    ent("cap", 30, 260, 128, 76, "Капитан", ["ФИО", "стаж"])
    rel("r_cap", 188, 267, 96, 58, "назначен\nна судно")

    rel("r_perf", 360, 168, 108, 62, "выполняет\nрейс")
    ent("shipment", 310, 280, 208, 100, "Рейс", ["даты следования", "таможня", "оформление"])

    rel("r_inc", 360, 420, 108, 62, "включает\nгруз")
    ent("cargo", 310, 520, 208, 104, "Груз", ["наименование партии", "стоимости", "количество"])

    ent("sender", 30, 530, 144, 86, "Отправитель", ["наименование", "ИНН"])
    rel("r_snd", 200, 542, 92, 56, "отправляет")
    ent("consignee", 654, 530, 144, 86, "Получатель", ["наименование", "ИНН"])
    rel("r_rcv", 586, 542, 92, 56, "получает")

    ent("port", 310, 680, 208, 78, "Порт", ["название"])
    rel("r_dep", 90, 600, 108, 58, "отправление\nиз порта")
    rel("r_arr", 630, 600, 108, 58, "прибытие\nв порт")
    rel("r_home", 360, 580, 108, 58, "приписка\nк порту")

    ent("addr", 310, 820, 208, 68, "Адрес", ["территория", "населённый пункт", "улица, дом"])

    ent("bank", 30, 660, 128, 68, "Банк", ["наименование"])
    rel("r_bk_s", 200, 668, 88, 50, "обслуживает")
    rel("r_bk_c", 710, 668, 88, 50, "обслуживает")

    rel("r_as", 200, 760, 88, 50, "юр. адрес")
    rel("r_ac", 710, 760, 88, 50, "юр. адрес")

    ent("unit", 654, 660, 128, 68, "Ед. изм.", ["код", "условное обозначение"])
    rel("r_unit", 586, 672, 96, 56, "измеряется\nв ед.")

    rel("r_loc", 360, 780, 108, 58, "расположен\nпо адресу")

    # --- Рёбра и кардинальности (у ромба: сторона сущности «многие» — N, сторона «один» — 1) ---
    edge("ship", "r_type", "N")
    edge("r_type", "type", "1")

    edge("ship", "r_dock", "N")
    edge("r_dock", "dock", "0..1")

    edge("ship", "r_cap", "1")
    edge("r_cap", "cap", "0..1")

    edge("ship", "r_perf", "1", main=True)
    edge("r_perf", "shipment", "N", main=True)

    edge("shipment", "r_inc", "1", main=True)
    edge("r_inc", "cargo", "N", main=True)

    edge("cargo", "r_snd", "N")
    edge("r_snd", "sender", "1")
    edge("cargo", "r_rcv", "N")
    edge("r_rcv", "consignee", "1")

    edge("shipment", "r_dep", "N")
    edge("r_dep", "port", "1")
    edge("shipment", "r_arr", "N")
    edge("r_arr", "port", "1")

    edge("ship", "r_home", "N")
    edge("r_home", "port", "1")

    edge("port", "r_loc", "N")
    edge("r_loc", "addr", "1")

    edge("sender", "r_bk_s", "N")
    edge("r_bk_s", "bank", "1")
    edge("consignee", "r_bk_c", "N")
    edge("r_bk_c", "bank", "1")

    edge("sender", "r_as", "N")
    edge("r_as", "addr", "1")
    edge("consignee", "r_ac", "N")
    edge("r_ac", "addr", "1")

    edge("cargo", "r_unit", "N")
    edge("r_unit", "unit", "1")

    legend = (
        "<div style='text-align:left;line-height:1.35;'>"
        "<b>Концептуальная ER-модель (нотация Чена)</b><br/>"
        "<b>Прямоугольник</b> — сущность предметной области; <b>ромб</b> — связь (глагол или краткое имя роли).<br/>"
        "Подписи <b>1</b>, <b>N</b>, <b>0..1</b> на линиях — кардинальность и обязательность участия (классический ER).<br/>"
        "Жирные линии — основная цепочка «Судно → Рейс → Груз». Источник смысла полей: sql/deploy/02_schema.sql.<br/>"
        "<font color='#64748b'>Файл сгенерирован: docs/render_baltika_drawio.py</font>"
        "</div>"
    )
    cells.append(
        cell_vertex(
            vid(),
            24,
            930,
            920,
            120,
            legend,
            STYLE_LEGEND,
        )
    )

    inner = "\n".join(cells + edges)
    page_w = 1100
    page_h = 1080

    out = (
        '<?xml version="1.0" encoding="UTF-8"?>\n'
        '<mxfile host="app.diagrams.net" agent="BaltikaApp/render_baltika_drawio" version="22.1.0" type="device">\n'
        '  <diagram id="baltika-conceptual-er" name="Балтика — концептуальная ER">\n'
        f'    <mxGraphModel dx="1400" dy="900" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" '
        f'fold="1" page="1" pageScale="1" pageWidth="{page_w}" pageHeight="{page_h}" math="0" shadow="0">\n'
        "      <root>\n"
        "        <mxCell id=\"0\"/>\n"
        "        <mxCell id=\"1\" parent=\"0\"/>\n"
        f"{inner}\n"
        "      </root>\n"
        "    </mxGraphModel>\n"
        "  </diagram>\n"
        "</mxfile>\n"
    )

    path = Path(__file__).with_name("Baltika_conceptual_ER.drawio")
    path.write_text(out, encoding="utf-8", newline="\n")
    assert "Судно" in out and "выполняет" in out
    print("OK", path, path.stat().st_size)


if __name__ == "__main__":
    main()
