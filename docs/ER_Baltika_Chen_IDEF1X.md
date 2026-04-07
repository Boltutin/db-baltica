# ER-модель БД «Балтика» (нотация Чена и IDEF1X)

Соответствует скрипту `sql/deploy/02_schema.sql`.

**Отличие от упрощённой схемы «судно → груз»:** в проекте **груз** (`cargo`) относится к **рейсу** (`shipments`), а уже **рейс** — к **судну** (`ships`). Порты отправления/назначения и даты хранятся в **рейсе**, а не в строке груза.

---

## 1. Нотация Чена (логический уровень)

В классической нотации Чена: **сущности** — прямоугольники, **связи** — ромбы, **атрибуты** — овалы. Ниже — эквивалент в виде диаграммы ER (Mermaid на GitHub / VS Code отображает её как связи с кардинальностями).

### 1.1. Ядро предметной области (фрагмент, как на учебном рисунке, но по вашей БД)

Смысл связей:

| Ромб (связь)   | Сторона 1 | Сторона M | Пояснение |
|----------------|-----------|-----------|-----------|
| **отправляет** | Отправитель (`senders`) | Груз (`cargo`) | У одного отправителя много грузов; у груза один отправитель. |
| **получает**   | Получатель (`consignees`) | Груз (`cargo`) | Аналогично. |
| **включает**   | Рейс (`shipments`) | Груз (`cargo`) | Рейс — партия; в ней много записей груза. |
| **выполняет**  | Судно (`ships`) | Рейс (`shipments`) | Одно судно совершает много рейсов. |

```mermaid
erDiagram
    senders ||--o{ cargo : "отправляет"
    consignees ||--o{ cargo : "получает"
    shipments ||--o{ cargo : "включает"
    ships ||--o{ shipments : "выполняет"
```

### 1.2. Атрибуты ядра (как ключевые поля таблиц)

```mermaid
erDiagram
    senders {
        int sender_id PK
        string sender_name
        string inn_sender
        int bank_id FK
        int address_id FK
    }
    consignees {
        int consignee_id PK
        string consignee_name
        string inn_consignee
        int bank_id FK
        int address_id FK
    }
    ships {
        int ship_id PK
        string reg_number
        string name
        int captain_id FK
        int type_id FK
        int dockyard_id FK
        int capacity
        int year_built
        bytea picture
        int home_port_id FK
    }
    shipments {
        int shipment_id PK
        int ship_id FK
        int origin_port_id FK
        int destination_port_id FK
        date departure_date
        date arrive_date
        numeric customs_value
        bool custom_clearance
    }
    cargo {
        int cargo_id PK
        int shipment_id FK
        int sender_id FK
        int consignee_id FK
        int cargo_number
        string cargo_name
        int unit_id FK
        numeric declared_value
        numeric insured_value
        numeric custom_value
        numeric quantity
        text comment
    }
    senders ||--o{ cargo : "sender_id"
    consignees ||--o{ cargo : "consignee_id"
    shipments ||--o{ cargo : "shipment_id"
    ships ||--o{ shipments : "ship_id"
```

### 1.3. Полная схема (все таблицы `02_schema.sql`)

```mermaid
erDiagram
    addresses ||--o{ ports : "address_id"
    addresses ||--o{ senders : "address_id"
    addresses ||--o{ consignees : "address_id"
    banks ||--o{ senders : "bank_id"
    banks ||--o{ consignees : "bank_id"
    ship_types ||--o{ ships : "type_id"
    dockyards ||--o{ ships : "dockyard_id"
    ships }o--|| captains : "captain_id (в БД доп. ограничение UNIQUE)"
    ports ||--o{ ships : "home_port_id"
    ports ||--o{ shipments : "origin_port_id"
    ports ||--o{ shipments : "destination_port_id"
    ships ||--o{ shipments : "ship_id"
    shipments ||--o{ cargo : "shipment_id"
    senders ||--o{ cargo : "sender_id"
    consignees ||--o{ cargo : "consignee_id"
    units ||--o{ cargo : "unit_id"
```

> **Как нарисовать «чистый» Чен с ромбами и овалами:** экспортируйте эту структуру в [draw.io](https://app.diagrams.net/), Dia, ERwin или используйте шаблон «Chen ER» — блоки из раздела 1.2 переносятся в овалы у сущностей, связи подписываются как в таблице выше.

---

## 2. IDEF1X (категория/ключи)

В IDEF1X различают **независимые** сущности (идентификатор — собственный PK) и **зависимые** (часть ключа или смысловая зависимость от родителя). В PostgreSQL все таблицы имеют **суррогатный PK** (`SERIAL`), поэтому ниже — **логическая** классификация.

### 2.1. Независимые сущности (корневые)

| Сущность     | PK (логический) | Примечание |
|--------------|-------------------|------------|
| `addresses`  | `address_id`      | Адрес. |
| `banks`      | `bank_id`         | Банк. |
| `ship_types` | `type_id`         | Тип судна. |
| `dockyards`  | `dockyard_id`     | Верфь. |
| `units`      | `unit_id`         | Единица измерения. |
| `captains`   | `captain_id`      | Капитан. |

### 2.2. Зависимые / связующие

| Сущность   | PK   | FK (родители) |
|------------|------|----------------|
| `ports`    | `port_id` | → `addresses` (опционально) |
| `senders`  | `sender_id` | → `banks`, `addresses` |
| `consignees` | `consignee_id` | → `banks`, `addresses` |
| `ships`    | `ship_id` | → `ship_types`, `captains` (1:1), `dockyards`, `ports` (порт приписки) |
| `shipments`| `shipment_id` | → `ships`, `ports` (два раза: отправление, назначение) |
| `cargo`    | `cargo_id` | → `shipments`, `senders`, `consignees`, `units` |

### 2.3. Кардинальность связей (для IDEF1X и отчётов)

- `ships` — `shipments`: **1 : N** (одно судно, много рейсов).
- `shipments` — `cargo`: **1 : N** (один рейс, много строк груза).
- `senders` — `cargo`: **1 : N**; `consignees` — `cargo`: **1 : N**.

---

## 3. Экспорт в картинку

- **Из VS Code:** расширение «Markdown Preview Mermaid Support» → экспорт PDF/печать.
- **Онлайн:** [mermaid.live](https://mermaid.live) — вставьте блок из раздела 1.2 или 1.3 → PNG/SVG.
- **Pandoc / Quarto:** рендер Markdown с Mermaid в PDF для пояснительной записки.

Если нужен **строго** ромб Чена в одном файле без Mermaid — удобнее собрать схему в draw.io по таблицам раздела 2.
