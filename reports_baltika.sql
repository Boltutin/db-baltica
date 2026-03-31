-- ============================================
-- reports_baltika.sql
-- Отчётные представления и функция для БД "Балтика"
-- Структура БД соответствует schema_baltika.sql
-- Выполнять после schema_baltika.sql, до или после data_baltika.sql (представления не зависят от данных).
-- Колонки представлений сверяются с --introspect-db в Program.cs.
-- ============================================

-- 1. Представление: Полная информация о судах
------------------------------------------------

CREATE OR REPLACE VIEW ships_full_info AS
SELECT
    s.ship_id,
    s.reg_number          AS "Регистрационный номер",
    s.name                AS "Название судна",
    c.full_name           AS "Капитан",
    st.type_name          AS "Тип судна",
    s.capacity            AS "Грузоподъемность (т)",
    s.year_built          AS "Год постройки",
    d.name                AS "Верфь",
    s.customs_value       AS "Таможенная стоимость",
    p.port_name           AS "Порт приписки"
FROM ships s
LEFT JOIN captains   c  ON s.captain_id  = c.captain_id
LEFT JOIN ship_types st ON s.type_id     = st.type_id
LEFT JOIN dockyards  d  ON s.dockyard_id = d.dockyard_id
LEFT JOIN ports      p  ON s.home_port_id = p.port_id;


-- 2. Представление: Полная информация о рейсах
----------------------------------------------

CREATE OR REPLACE VIEW shipments_full_info AS
SELECT
    sh.shipment_id,
    s.name                AS "Судно",
    s.reg_number          AS "Рег. номер",
    sh.departure_date     AS "Дата отправления",
    sh.arrive_date        AS "Дата прибытия",
    p1.port_name          AS "Порт отправления",
    p2.port_name          AS "Порт назначения",
    CASE 
        WHEN sh.custom_clearance THEN 'Да' 
        ELSE 'Нет' 
    END                   AS "Таможенное оформление",
    (sh.arrive_date - sh.departure_date) AS "Продолжительность рейса (дней)",
    sh.customs_value      AS "Таможенная стоимость партии"
FROM shipments sh
JOIN ships s   ON sh.ship_id        = s.ship_id
JOIN ports p1  ON sh.origin_port_id = p1.port_id
JOIN ports p2  ON sh.destination_port_id = p2.port_id;


-- 3. Представление: Полная информация о грузах
----------------------------------------------

CREATE OR REPLACE VIEW cargo_full_info AS
SELECT
    c.cargo_id,
    sh.shipment_id,
    s.name                AS "Судно",
    s.reg_number          AS "Рег. номер судна",
    c.cargo_number        AS "Номер груза в партии",
    c.cargo_name          AS "Наименование груза",
    c.declared_value      AS "Заявленная стоимость",
    u.unit_name           AS "Единица измерения",
    c.insured_value       AS "Страховая стоимость",
    c.custom_value        AS "Таможенная стоимость груза",
    c.quantity            AS "Количество",
    sen.sender_name       AS "Отправитель",
    con.consignee_name    AS "Получатель",
    p1.port_name          AS "Порт отправления",
    p2.port_name          AS "Порт назначения",
    sh.departure_date     AS "Дата отправления",
    sh.arrive_date        AS "Дата прибытия",
    c.comment             AS "Примечания"
FROM cargo c
JOIN shipments sh   ON c.shipment_id   = sh.shipment_id
JOIN ships s        ON sh.ship_id      = s.ship_id
JOIN senders sen    ON c.sender_id     = sen.sender_id
JOIN consignees con ON c.consignee_id  = con.consignee_id
JOIN ports p1       ON sh.origin_port_id      = p1.port_id
JOIN ports p2       ON sh.destination_port_id = p2.port_id
LEFT JOIN units u   ON c.unit_id       = u.unit_id;


-- 4. Представление: Суда по типам (агрегированная статистика)
--------------------------------------------------------------

CREATE OR REPLACE VIEW ships_by_type AS
SELECT
    st.type_name                  AS "Тип судна",
    COUNT(*)                      AS "Количество судов",
    ROUND(AVG(s.capacity), 2)     AS "Средняя грузоподъемность",
    MIN(s.year_built)             AS "Самое старое судно (год)",
    MAX(s.year_built)             AS "Самое новое судно (год)"
FROM ships s
JOIN ship_types st ON s.type_id = st.type_id
GROUP BY st.type_name, st.type_id
ORDER BY COUNT(*) DESC;


-- 5. Представление: Статистика по портам
----------------------------------------

CREATE OR REPLACE VIEW ports_statistics AS
SELECT
    p.port_name AS "Порт",
    COUNT(DISTINCT sh1.shipment_id) AS "Отправлений",
    COUNT(DISTINCT sh2.shipment_id) AS "Прибытий",
    COUNT(DISTINCT sh1.shipment_id) 
      + COUNT(DISTINCT sh2.shipment_id) AS "Всего операций"
FROM ports p
LEFT JOIN shipments sh1 ON p.port_id = sh1.origin_port_id
LEFT JOIN shipments sh2 ON p.port_id = sh2.destination_port_id
GROUP BY p.port_id, p.port_name
HAVING COUNT(DISTINCT sh1.shipment_id) 
     + COUNT(DISTINCT sh2.shipment_id) > 0
ORDER BY "Всего операций" DESC;


-- 6. Представление: Финансовая сводка по грузам
-----------------------------------------------

CREATE OR REPLACE VIEW cargo_financial_summary AS
SELECT
    c.cargo_name               AS "Наименование груза",
    COUNT(*)                   AS "Количество записей",
    SUM(c.declared_value)      AS "Общая заявленная стоимость",
    SUM(c.insured_value)       AS "Общая страховая стоимость",
    ROUND(AVG(c.declared_value), 2) AS "Средняя заявленная стоимость"
FROM cargo c
GROUP BY c.cargo_name
ORDER BY SUM(c.declared_value) DESC;


-- 7. Представление: Активность судов
-------------------------------------

CREATE OR REPLACE VIEW ships_activity AS
SELECT
    s.name                AS "Судно",
    s.reg_number          AS "Рег. номер",
    COUNT(sh.shipment_id) AS "Количество рейсов",
    MIN(sh.departure_date) AS "Первый рейс",
    MAX(sh.arrive_date)    AS "Последний рейс",
    SUM(c.declared_value)  AS "Общая стоимость перевезенных грузов"
FROM ships s
LEFT JOIN shipments sh ON s.ship_id = sh.ship_id
LEFT JOIN cargo c      ON sh.shipment_id = c.shipment_id
GROUP BY s.ship_id, s.name, s.reg_number
ORDER BY COUNT(sh.shipment_id) DESC;


-- 8. Представление: Активность клиентов (отправители и получатели)
-------------------------------------------------------------------

CREATE OR REPLACE VIEW clients_activity AS
SELECT
    'Отправитель'        AS "Тип клиента",
    s.sender_name        AS "Название",
    s.inn_sender         AS "ИНН",
    COUNT(c.cargo_id)    AS "Количество грузов",
    SUM(c.declared_value) AS "Общая стоимость"
FROM senders s
LEFT JOIN cargo c ON s.sender_id = c.sender_id
GROUP BY s.sender_id, s.sender_name, s.inn_sender

UNION ALL

SELECT
    'Получатель'         AS "Тип клиента",
    con.consignee_name   AS "Название",
    con.inn_consignee    AS "ИНН",
    COUNT(c.cargo_id)    AS "Количество грузов",
    SUM(c.declared_value) AS "Общая стоимость"
FROM consignees con
LEFT JOIN cargo c ON con.consignee_id = c.consignee_id
GROUP BY con.consignee_id, con.consignee_name, con.inn_consignee

ORDER BY "Общая стоимость" DESC NULLS LAST;


-- 9. Представление: Рейсы по месяцам
-------------------------------------

CREATE OR REPLACE VIEW shipments_by_month AS
SELECT
    EXTRACT(YEAR  FROM sh.departure_date) AS "Год",
    EXTRACT(MONTH FROM sh.departure_date) AS "Месяц",
    COUNT(*)                              AS "Количество рейсов",
    COUNT(DISTINCT sh.ship_id)            AS "Количество судов",
    SUM(c.declared_value)                 AS "Общая стоимость грузов"
FROM shipments sh
LEFT JOIN cargo c ON sh.shipment_id = c.shipment_id
GROUP BY EXTRACT(YEAR  FROM sh.departure_date),
         EXTRACT(MONTH FROM sh.departure_date)
ORDER BY "Год", "Месяц";


-- 10. Функция: Поиск рейсов по периоду
--------------------------------------

CREATE OR REPLACE FUNCTION get_shipments_by_period(
    start_date DATE,
    end_date   DATE
)
RETURNS TABLE (
    "Судно"             VARCHAR(60),
    "Рег. номер"        VARCHAR(10),
    "Дата отправления"  DATE,
    "Дата прибытия"     DATE,
    "Маршрут"           TEXT,
    "Количество грузов" BIGINT,
    "Общая стоимость"   NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        s.name,
        s.reg_number,
        sh.departure_date,
        sh.arrive_date,
        CONCAT(p1.port_name, ' → ', p2.port_name) AS route,
        COUNT(c.cargo_id),
        COALESCE(SUM(c.declared_value), 0)
    FROM shipments sh
    JOIN ships s  ON sh.ship_id        = s.ship_id
    JOIN ports p1 ON sh.origin_port_id = p1.port_id
    JOIN ports p2 ON sh.destination_port_id = p2.port_id
    LEFT JOIN cargo c ON sh.shipment_id = c.shipment_id
    WHERE sh.departure_date >= start_date
      AND sh.departure_date <= end_date
    GROUP BY s.name, s.reg_number,
             sh.departure_date, sh.arrive_date,
             p1.port_name, p2.port_name
    ORDER BY sh.departure_date;
END;
$$ LANGUAGE plpgsql;
