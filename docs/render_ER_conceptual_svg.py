# -*- coding: utf-8 -*-
"""Генерирует ER_conceptual_baltika.svg (UTF-8). Запуск: python render_ER_conceptual_svg.py"""
from pathlib import Path

SVG = r"""<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1040 680" width="1040" height="680" font-family="Segoe UI, Arial, Helvetica, sans-serif">
  <defs>
    <marker id="arr" markerWidth="8" markerHeight="8" refX="7" refY="3" orient="auto">
      <polygon points="0 0, 8 3, 0 6" fill="#1a365d"/>
    </marker>
    <marker id="arrP" markerWidth="7" markerHeight="7" refX="6" refY="3" orient="auto">
      <polygon points="0 0, 7 3, 0 6" fill="#805ad5"/>
    </marker>
  </defs>
  <rect width="100%" height="100%" fill="#fafcff"/>

  <!-- Справочники к судну -->
  <rect x="24" y="16" width="178" height="88" rx="6" fill="#fff8e6" stroke="#c05621" stroke-width="1.5"/>
  <text x="113" y="38" text-anchor="middle" font-size="11" font-weight="600" fill="#744210">Справочники</text>
  <text x="113" y="54" text-anchor="middle" font-size="9.5" fill="#4a5568">тип судна, верфь, капитан</text>
  <text x="113" y="70" text-anchor="middle" font-size="8" fill="#718096">ship_types · dockyards · captains</text>
  <path d="M 400 60 L 202 60" fill="none" stroke="#805ad5" stroke-width="1.2" stroke-dasharray="5,3" marker-end="url(#arrP)"/>
  <text x="285" y="56" font-size="9.5" fill="#553c9a" stroke="#fafcff" stroke-width="2" paint-order="stroke fill">N:1</text>

  <!-- Судно -->
  <rect x="400" y="36" width="200" height="76" rx="6" fill="#e8f4fc" stroke="#0f4c81" stroke-width="2"/>
  <text x="500" y="58" text-anchor="middle" font-size="13" font-weight="600" fill="#0f4c81">Судно</text>
  <text x="500" y="74" text-anchor="middle" font-size="9" fill="#4a5568">порт приписки → Порт</text>
  <text x="500" y="92" text-anchor="middle" font-size="8" fill="#718096">ships</text>

  <!-- Рейс: без длинной строки FK внутри — подпись вынесена -->
  <rect x="400" y="196" width="200" height="108" rx="6" fill="#e8f4fc" stroke="#0f4c81" stroke-width="2"/>
  <text x="500" y="220" text-anchor="middle" font-size="13" font-weight="600" fill="#0f4c81">Рейс</text>
  <text x="500" y="238" text-anchor="middle" font-size="9" fill="#4a5568">даты, таможня, оформление</text>
  <text x="500" y="256" text-anchor="middle" font-size="8.5" fill="#4a5568">связь с портами см. линии к «Порт»</text>
  <text x="500" y="278" text-anchor="middle" font-size="8" fill="#718096">shipments</text>

  <!-- Груз -->
  <rect x="400" y="372" width="200" height="76" rx="6" fill="#e8f4fc" stroke="#0f4c81" stroke-width="2"/>
  <text x="500" y="400" text-anchor="middle" font-size="13" font-weight="600" fill="#0f4c81">Груз</text>
  <text x="500" y="420" text-anchor="middle" font-size="8" fill="#718096">cargo</text>

  <line x1="500" y1="112" x2="500" y2="196" stroke="#1a365d" stroke-width="2" marker-end="url(#arr)"/>
  <text x="512" y="144" font-size="12" font-weight="700" fill="#c53030" stroke="#fafcff" stroke-width="2.5" paint-order="stroke fill">1</text>
  <text x="512" y="164" font-size="12" font-weight="700" fill="#c53030" stroke="#fafcff" stroke-width="2.5" paint-order="stroke fill">N</text>

  <line x1="500" y1="304" x2="500" y2="372" stroke="#1a365d" stroke-width="2" marker-end="url(#arr)"/>
  <text x="512" y="328" font-size="12" font-weight="700" fill="#c53030" stroke="#fafcff" stroke-width="2.5" paint-order="stroke fill">1</text>
  <text x="512" y="348" font-size="12" font-weight="700" fill="#c53030" stroke="#fafcff" stroke-width="2.5" paint-order="stroke fill">N</text>

  <!-- Отправитель -->
  <rect x="24" y="360" width="176" height="92" rx="6" fill="#edf7ed" stroke="#276749" stroke-width="1.5"/>
  <text x="112" y="384" text-anchor="middle" font-size="12" font-weight="600" fill="#22543d">Отправитель</text>
  <text x="112" y="402" text-anchor="middle" font-size="9" fill="#4a5568">наименование, ИНН</text>
  <text x="112" y="420" text-anchor="middle" font-size="8" fill="#718096">senders</text>
  <line x1="200" y1="406" x2="400" y2="412" stroke="#1a365d" stroke-width="1.6" marker-end="url(#arr)"/>
  <text x="248" y="400" font-size="12" font-weight="700" fill="#c53030" stroke="#fafcff" stroke-width="2.5" paint-order="stroke fill">1</text>
  <text x="340" y="408" font-size="12" font-weight="700" fill="#c53030" stroke="#fafcff" stroke-width="2.5" paint-order="stroke fill">N</text>

  <!-- Получатель -->
  <rect x="840" y="360" width="176" height="92" rx="6" fill="#edf7ed" stroke="#276749" stroke-width="1.5"/>
  <text x="928" y="384" text-anchor="middle" font-size="12" font-weight="600" fill="#22543d">Получатель</text>
  <text x="928" y="402" text-anchor="middle" font-size="9" fill="#4a5568">наименование, ИНН</text>
  <text x="928" y="420" text-anchor="middle" font-size="8" fill="#718096">consignees</text>
  <line x1="840" y1="406" x2="600" y2="412" stroke="#1a365d" stroke-width="1.6" marker-end="url(#arr)"/>
  <text x="640" y="408" font-size="12" font-weight="700" fill="#c53030" stroke="#fafcff" stroke-width="2.5" paint-order="stroke fill">N</text>
  <text x="800" y="400" font-size="12" font-weight="700" fill="#c53030" stroke="#fafcff" stroke-width="2.5" paint-order="stroke fill">1</text>

  <!-- Порт -->
  <rect x="400" y="500" width="200" height="60" rx="6" fill="#fef5f5" stroke="#9b2c2c" stroke-width="1.5"/>
  <text x="500" y="526" text-anchor="middle" font-size="12" font-weight="600" fill="#742a2a">Порт</text>
  <text x="500" y="544" text-anchor="middle" font-size="8" fill="#718096">ports</text>

  <!-- Рейс–Порт: горизонталь строго ПОД низом блока Рейс (y=304), без прохода через прямоугольник -->
  <path d="M 440 312 L 300 312 L 300 500 L 440 500" fill="none" stroke="#9b2c2c" stroke-width="1.2" stroke-dasharray="5,3"/>
  <path d="M 560 312 L 700 312 L 700 500 L 560 500" fill="none" stroke="#9b2c2c" stroke-width="1.2" stroke-dasharray="5,3"/>
  <text x="288" y="402" font-size="9" fill="#742a2a" transform="rotate(-90 288,402)">отправление</text>
  <text x="712" y="402" font-size="9" fill="#742a2a" transform="rotate(90 712,402)">назначение</text>
  <text x="308" y="308" text-anchor="middle" font-size="9.5" fill="#742a2a" stroke="#fff" stroke-width="2" paint-order="stroke fill">M:1</text>
  <text x="692" y="308" text-anchor="middle" font-size="9.5" fill="#742a2a" stroke="#fff" stroke-width="2" paint-order="stroke fill">M:1</text>

  <!-- Приписка -->
  <path d="M 420 74 L 268 74 L 268 530 L 440 530" fill="none" stroke="#9b2c2c" stroke-width="1.1" stroke-dasharray="6,4"/>
  <text x="252" y="340" font-size="9.5" fill="#742a2a" stroke="#fff" stroke-width="2" paint-order="stroke fill">N:1</text>
  <text x="252" y="356" font-size="8.5" fill="#742a2a">приписка</text>

  <!-- Адрес -->
  <rect x="400" y="580" width="200" height="42" rx="4" fill="#f3e8ff" stroke="#553c9a" stroke-width="1.3"/>
  <text x="500" y="602" text-anchor="middle" font-size="11.5" font-weight="600" fill="#44337a">Адрес</text>
  <text x="500" y="618" text-anchor="middle" font-size="8" fill="#718096">addresses</text>
  <line x1="500" y1="560" x2="500" y2="580" stroke="#553c9a" stroke-width="1.3" marker-end="url(#arr)"/>
  <text x="512" y="574" font-size="9.5" fill="#553c9a" stroke="#fafcff" stroke-width="2" paint-order="stroke fill">N:1</text>
  <text x="500" y="668" text-anchor="middle" font-size="8" fill="#718096">порт ссылается на адрес порта</text>

  <!-- К банку/ед.: маршрут y=168 — выше блока Рейс (он заканчивается y=304) -->
  <rect x="800" y="492" width="216" height="122" rx="6" fill="#fff8e6" stroke="#c05621" stroke-width="1.5"/>
  <text x="908" y="514" text-anchor="middle" font-size="11" font-weight="600" fill="#744210">Прочие справочники</text>
  <text x="908" y="532" text-anchor="middle" font-size="9" fill="#4a5568">банк — отправитель и получатель</text>
  <text x="908" y="548" text-anchor="middle" font-size="9" fill="#4a5568">ед. измерения — груз</text>
  <text x="908" y="566" text-anchor="middle" font-size="7.5" fill="#718096">banks, units (FK N:1)</text>
  <path d="M 112 406 L 112 168 L 908 168 L 908 492" fill="none" stroke="#805ad5" stroke-width="1" stroke-dasharray="4,3" marker-end="url(#arrP)"/>
  <text x="480" y="162" text-anchor="middle" font-size="8" fill="#553c9a" stroke="#fafcff" stroke-width="2" paint-order="stroke fill">N:1 (банк, отпр.)</text>
  <path d="M 928 406 L 928 168 L 908 168" fill="none" stroke="#805ad5" stroke-width="1" stroke-dasharray="4,3"/>
  <text x="918" y="162" text-anchor="end" font-size="8" fill="#553c9a" stroke="#fafcff" stroke-width="2" paint-order="stroke fill">N:1</text>
  <path d="M 600 410 L 760 410 L 800 492" fill="none" stroke="#805ad5" stroke-width="1" stroke-dasharray="4,3" marker-end="url(#arrP)"/>
  <text x="720" y="404" font-size="8.5" fill="#553c9a" stroke="#fafcff" stroke-width="2" paint-order="stroke fill">N:1 (ед.)</text>

  <!-- Легенда -->
  <rect x="24" y="488" width="248" height="118" rx="4" fill="#ffffff" stroke="#cbd5e0" stroke-width="1"/>
  <text x="36" y="508" font-size="10.5" font-weight="600" fill="#2d3748">Условные обозначения</text>
  <line x1="36" y1="524" x2="76" y2="524" stroke="#1a365d" stroke-width="1.6" marker-end="url(#arr)"/>
  <text x="82" y="528" font-size="8.5" fill="#4a5568">основная связь (1:N)</text>
  <line x1="36" y1="542" x2="76" y2="542" stroke="#805ad5" stroke-dasharray="4,2" stroke-width="1" marker-end="url(#arrP)"/>
  <text x="82" y="546" font-size="8.5" fill="#4a5568">к справочнику (N:1)</text>
  <line x1="36" y1="560" x2="76" y2="560" stroke="#9b2c2c" stroke-dasharray="4,2" stroke-width="1"/>
  <text x="82" y="564" font-size="8.5" fill="#4a5568">рейс — порт; судно — приписка</text>
  <line x1="36" y1="578" x2="76" y2="578" stroke="#553c9a" stroke-width="1.2" marker-end="url(#arr)"/>
  <text x="82" y="582" font-size="8.5" fill="#4a5568">порт — адрес</text>
</svg>
"""

def main():
    out = Path(__file__).with_name("ER_conceptual_baltika.svg")
    out.write_text(SVG, encoding="utf-8", newline="\n")
    t = out.read_text(encoding="utf-8")
    assert "Рейс" in t and "Получатель" in t
    print("OK", out, out.stat().st_size)

if __name__ == "__main__":
    main()
