-- =============================================================================
-- Схема БД «Балтика» (таблицы и связи)
-- Предварительно создайте базу: create_database.sql, затем выполняйте этот файл,
-- подключившись к БД baltika (пользователь с правом CREATE).
-- Кодировка: UTF-8.
-- =============================================================================

-- ============================================
-- Справочники
-- ============================================

-- 1. Типы судов
CREATE TABLE ship_types (
    type_id      SERIAL PRIMARY KEY,
    type_name    VARCHAR(60) UNIQUE NOT NULL
);

-- 2. Верфи-производители
CREATE TABLE dockyards (
    dockyard_id  SERIAL PRIMARY KEY,
    name         VARCHAR(100) NOT NULL,
    country      VARCHAR(60)
);

-- 3. Единицы измерения грузов
CREATE TABLE units (
    unit_id      SERIAL PRIMARY KEY,
    unit_name    VARCHAR(30)  NOT NULL,   -- кг, т, м3, шт, л
    unit_code    VARCHAR(10)  NOT NULL    -- условное обозначение (kg, t, m3, pcs, l)
);

-- 4. Банки
CREATE TABLE banks (
    bank_id      SERIAL PRIMARY KEY,
    bank_name    VARCHAR(150) NOT NULL
);

-- 5. Адреса (атомарные компоненты адреса)
CREATE TABLE addresses (
    address_id       SERIAL PRIMARY KEY,
    country          VARCHAR(60)  NOT NULL,
    region           VARCHAR(60),
    city             VARCHAR(60)  NOT NULL,
    street           VARCHAR(100) NOT NULL,
    building_number  VARCHAR(20)  NOT NULL
);

-- 6. Капитаны
CREATE TABLE captains (
    captain_id   SERIAL PRIMARY KEY,
    full_name    VARCHAR(150) NOT NULL,
    experience   INTEGER,
    created_at   TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW(),
    updated_at   TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW()
);

-- 7. Порты
CREATE TABLE ports (
    port_id      SERIAL PRIMARY KEY,
    port_name    VARCHAR(150) NOT NULL,
    address_id   INTEGER REFERENCES addresses(address_id),
    created_at   TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW(),
    updated_at   TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW()
);

-- ============================================
-- Основные сущности
-- ============================================

-- 8. Суда
CREATE TABLE ships (
    ship_id        SERIAL PRIMARY KEY,
    reg_number     VARCHAR(10)  UNIQUE NOT NULL,
    name           VARCHAR(60)  NOT NULL,
    captain_id     INTEGER UNIQUE REFERENCES captains(captain_id),
    type_id        INTEGER       NOT NULL REFERENCES ship_types(type_id),
    dockyard_id    INTEGER REFERENCES dockyards(dockyard_id),
    capacity       INTEGER,
    year_built     INTEGER,
    customs_value  NUMERIC(10,2),
    picture        BYTEA,
    home_port_id   INTEGER REFERENCES ports(port_id),
    created_at     TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW(),
    updated_at     TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW()
);

-- 9. Отправители
CREATE TABLE senders (
    sender_id      SERIAL PRIMARY KEY,
    sender_name    VARCHAR(150) NOT NULL,
    inn_sender     VARCHAR(10)  UNIQUE NOT NULL,
    bank_id        INTEGER REFERENCES banks(bank_id),
    address_id     INTEGER REFERENCES addresses(address_id),
    created_at     TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW(),
    updated_at     TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW()
);

-- 10. Получатели
CREATE TABLE consignees (
    consignee_id   SERIAL PRIMARY KEY,
    consignee_name VARCHAR(150) NOT NULL,
    inn_consignee  VARCHAR(10)  UNIQUE NOT NULL,
    bank_id        INTEGER REFERENCES banks(bank_id),
    address_id     INTEGER REFERENCES addresses(address_id),
    created_at     TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW(),
    updated_at     TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW()
);

-- 11. Рейсы (партии грузов)
CREATE TABLE shipments (
    shipment_id         SERIAL PRIMARY KEY,
    ship_id             INTEGER NOT NULL REFERENCES ships(ship_id),
    origin_port_id      INTEGER NOT NULL REFERENCES ports(port_id),
    destination_port_id INTEGER NOT NULL REFERENCES ports(port_id),
    departure_date      DATE    NOT NULL,
    arrive_date         DATE,
    customs_value       NUMERIC(10,2),
    custom_clearance    BOOLEAN DEFAULT FALSE,
    created_at          TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW(),
    updated_at          TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW()
);

-- 12. Отдельные грузы
CREATE TABLE cargo (
    cargo_id        SERIAL PRIMARY KEY,
    shipment_id     INTEGER NOT NULL REFERENCES shipments(shipment_id),
    sender_id       INTEGER NOT NULL REFERENCES senders(sender_id),
    consignee_id    INTEGER NOT NULL REFERENCES consignees(consignee_id),
    cargo_number    INTEGER       NOT NULL,        -- номер в партии
    cargo_name      VARCHAR(150)  NOT NULL,
    unit_id         INTEGER       NOT NULL REFERENCES units(unit_id),
    declared_value  NUMERIC(10,2),
    insured_value   NUMERIC(10,2),
    custom_value    NUMERIC(10,2),
    quantity        NUMERIC(10,2),
    comment         TEXT,
    created_at      TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW(),
    updated_at      TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW()
);
