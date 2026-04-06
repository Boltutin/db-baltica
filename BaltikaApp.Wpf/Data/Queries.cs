namespace BaltikaApp.Data
{
    /// <summary>
    /// SQL для KPI на главном окне. Границы периода задаются параметрами <c>@start</c> и <c>@end</c> (см. <c>MainWindow.RefreshKpi</c>).
    /// </summary>
    internal static class KpiQueries
    {
        public const string MainDashboardStats = @"
SELECT
    COUNT(*) FILTER (
        WHERE ""Дата отправления"" >= @start::date AND ""Дата отправления"" <= @end::date
    ) AS shipments_last_month,
    (
        SELECT COUNT(*)
        FROM cargo_full_info c
        WHERE c.""Дата отправления"" >= @start::date
          AND c.""Дата отправления"" <= @end::date
    ) AS cargo_last_month
FROM shipments_full_info;";
    }

    /// <summary>
    /// Запросы к таблице и связанным данным по судам.
    /// </summary>
    internal static class ShipQueries
    {
        public const string ListShipsExtended = @"
SELECT
    s.ship_id,
    s.reg_number          AS ""Регистрационный номер"",
    s.name                AS ""Название судна"",
    c.full_name           AS ""Капитан"",
    c.experience          AS ""Опыт капитана"",
    st.type_name          AS ""Тип судна"",
    s.capacity            AS ""Грузоподъемность (т)"",
    s.year_built          AS ""Год постройки"",
    d.name                AS ""Верфь"",
    s.customs_value       AS ""Таможенная стоимость"",
    p.port_name           AS ""Порт приписки"",
    (a.country || ', ' ||
     COALESCE(a.region || ', ', '') ||
     a.city || ', ' ||
     a.street || ' ' || a.building_number) AS ""Адрес порта приписки""
FROM ships s
LEFT JOIN captains   c  ON s.captain_id  = c.captain_id
LEFT JOIN ship_types st ON s.type_id     = st.type_id
LEFT JOIN dockyards  d  ON s.dockyard_id = d.dockyard_id
LEFT JOIN ports      p  ON s.home_port_id = p.port_id
LEFT JOIN addresses  a  ON p.address_id  = a.address_id
ORDER BY s.reg_number;";

        public const string InsertShip = @"
INSERT INTO ships
  (reg_number, name, captain_id, type_id, dockyard_id, capacity, year_built, customs_value, home_port_id)
  VALUES
  (@reg, @name, @captain, @type, @dockyard, @capacity, @year_built, @customs_value, @home_port_id);";

        public const string UpdateShip = @"
UPDATE ships
  SET reg_number = @reg,
      name = @name,
      captain_id = @captain,
      type_id = @type,
      dockyard_id = @dockyard,
      capacity = @capacity,
      year_built = @year_built,
      customs_value = @customs_value,
      home_port_id = @home_port_id
  WHERE ship_id = @ship_id;";

        public const string DeleteShip = "DELETE FROM ships WHERE ship_id=@id;";
    }

    /// <summary>
    /// Рейсы: выборка из представления <c>shipments_full_info</c> с фильтром по датам и опционально по судну; CRUD по таблице <c>shipments</c>.
    /// </summary>
    internal static class ShipmentQueries
    {
        public const string ShipsForFilterCombo = "SELECT ship_id, reg_number, name FROM ships ORDER BY reg_number;";

        public const string ShipmentsFullInfoPrefix = "SELECT * FROM shipments_full_info";

        public const string ShipmentsOrderByDeparture = " ORDER BY \"Дата отправления\"";

        public const string InsertShipment = @"
INSERT INTO shipments
  (ship_id, origin_port_id, destination_port_id, departure_date, arrive_date, customs_value, custom_clearance)
  VALUES
  (@ship_id, @origin_port_id, @destination_port_id, @departure_date, @arrive_date, @customs_value, @custom_clearance);";

        public const string UpdateShipment = @"
UPDATE shipments
  SET ship_id = @ship_id,
      origin_port_id = @origin_port_id,
      destination_port_id = @destination_port_id,
      departure_date = @departure_date,
      arrive_date = @arrive_date,
      customs_value = @customs_value,
      custom_clearance = @custom_clearance
  WHERE shipment_id = @shipment_id;";

        public const string DeleteShipment = "DELETE FROM shipments WHERE shipment_id=@id;";
    }

    /// <summary>
    /// Запросы к отправителям, получателям и сводной активности клиентов.
    /// </summary>
    internal static class ClientsQueries
    {
        public const string SendersGrid = @"
                    SELECT
                        s.sender_id,
                        s.sender_name AS ""Название"",
                        s.inn_sender  AS ""ИНН"",
                        b.bank_name   AS ""Банк"",
                        (a.country || ', ' ||
                         COALESCE(a.region || ', ', '') ||
                         a.city || ', ' ||
                         a.street || ' ' || a.building_number) AS ""Адрес""
                    FROM senders s
                    LEFT JOIN banks b ON s.bank_id = b.bank_id
                    LEFT JOIN addresses a ON s.address_id = a.address_id
                    ORDER BY s.sender_name;";

        public const string ConsigneesGrid = @"
                    SELECT
                        c.consignee_id,
                        c.consignee_name AS ""Название"",
                        c.inn_consignee  AS ""ИНН"",
                        b.bank_name      AS ""Банк"",
                        (a.country || ', ' ||
                         COALESCE(a.region || ', ', '') ||
                         a.city || ', ' ||
                         a.street || ' ' || a.building_number) AS ""Адрес""
                    FROM consignees c
                    LEFT JOIN banks b ON c.bank_id = b.bank_id
                    LEFT JOIN addresses a ON c.address_id = a.address_id
                    ORDER BY c.consignee_name;";

        public const string ClientsActivity = @"
                    SELECT
                        'Отправитель'        AS ""Тип клиента"",
                        s.sender_name        AS ""Название"",
                        s.inn_sender         AS ""ИНН"",
                        (a.country || ', ' ||
                         COALESCE(a.region || ', ', '') ||
                         a.city || ', ' ||
                         a.street || ' ' || a.building_number) AS ""Адрес"",
                        COUNT(c.cargo_id)    AS ""Количество грузов"",
                        SUM(c.declared_value) AS ""Общая стоимость""
                    FROM senders s
                    LEFT JOIN addresses a ON s.address_id = a.address_id
                    LEFT JOIN cargo c ON s.sender_id = c.sender_id
                    GROUP BY
                        s.sender_id,
                        s.sender_name,
                        s.inn_sender,
                        a.address_id,
                        a.country, a.region, a.city, a.street, a.building_number

                    UNION ALL

                    SELECT
                        'Получатель'         AS ""Тип клиента"",
                        con.consignee_name   AS ""Название"",
                        con.inn_consignee    AS ""ИНН"",
                        (a.country || ', ' ||
                         COALESCE(a.region || ', ', '') ||
                         a.city || ', ' ||
                         a.street || ' ' || a.building_number) AS ""Адрес"",
                        COUNT(c.cargo_id)    AS ""Количество грузов"",
                        SUM(c.declared_value) AS ""Общая стоимость""
                    FROM consignees con
                    LEFT JOIN addresses a ON con.address_id = a.address_id
                    LEFT JOIN cargo c ON con.consignee_id = c.consignee_id
                    GROUP BY
                        con.consignee_id,
                        con.consignee_name,
                        con.inn_consignee,
                        a.address_id,
                        a.country, a.region, a.city, a.street, a.building_number

                    ORDER BY ""Общая стоимость"" DESC NULLS LAST;";

        public const string InsertSender = @"
INSERT INTO senders (sender_name, inn_sender, bank_id, address_id)
VALUES (@name, @inn, @bank_id, @address_id);";

        public const string InsertConsignee = @"
INSERT INTO consignees (consignee_name, inn_consignee, bank_id, address_id)
VALUES (@name, @inn, @bank_id, @address_id);";

        public const string UpdateSender = @"
UPDATE senders
SET sender_name=@name,
    inn_sender=@inn,
    bank_id=@bank_id,
    address_id=@address_id
WHERE sender_id=@id;";

        public const string UpdateConsignee = @"
UPDATE consignees
SET consignee_name=@name,
    inn_consignee=@inn,
    bank_id=@bank_id,
    address_id=@address_id
WHERE consignee_id=@id;";

        public const string DeleteSender = "DELETE FROM senders WHERE sender_id=@id;";
        public const string DeleteConsignee = "DELETE FROM consignees WHERE consignee_id=@id;";
    }

    /// <summary>
    /// Запросы к грузам: выборка, вставка, изменение, удаление.
    /// </summary>
    internal static class CargoQueries
    {
        public const string ShipmentsForFilterCombo = @"
SELECT shipment_id,
       (""Рег. номер"" || ' | ' || ""Дата отправления""::text) AS caption
FROM shipments_full_info
ORDER BY ""Дата отправления"" DESC;";

        public const string AllCargo = "SELECT * FROM cargo_full_info;";

        public const string CargoByShipment = "SELECT * FROM cargo_full_info WHERE shipment_id=@shipment_id;";

        public const string InsertCargo = @"
INSERT INTO cargo
  (shipment_id, sender_id, consignee_id, cargo_number, cargo_name, unit_id,
   declared_value, insured_value, custom_value, quantity, comment)
VALUES
  (@shipment_id, @sender_id, @consignee_id, @cargo_number, @cargo_name, @unit_id,
   @declared_value, @insured_value, @custom_value, @quantity, @comment);";

        public const string UpdateCargo = @"
UPDATE cargo
SET sender_id=@sender_id, consignee_id=@consignee_id,
    cargo_number=@cargo_number, cargo_name=@cargo_name, unit_id=@unit_id,
    declared_value=@declared_value, insured_value=@insured_value,
    custom_value=@custom_value, quantity=@quantity, comment=@comment
WHERE cargo_id=@cargo_id;";

        public const string DeleteCargo = "DELETE FROM cargo WHERE cargo_id=@id;";
    }

    /// <summary>
    /// Тексты выборок для формы аналитических отчётов и справочников.
    /// </summary>
    internal static class ReportQueries
    {
        public const string ShipsByType = "SELECT * FROM ships_by_type;";
        public const string PortsStatistics = "SELECT * FROM ports_statistics;";
        public const string ShipsActivity = "SELECT * FROM ships_activity;";
        public const string CargoFinancialSummary = "SELECT * FROM cargo_financial_summary;";
        /// <summary>Представление <c>clients_activity</c> (дублирует по смыслу запрос <c>ClientsQueries.ClientsActivity</c> для окна клиентов).</summary>
        public const string ClientsActivityView = "SELECT * FROM clients_activity;";
        public const string ShipmentsByMonth = "SELECT * FROM shipments_by_month;";
        public const string RefCaptains = "SELECT captain_id AS \"ID\", full_name AS \"ФИО\", experience AS \"Стаж (лет)\", created_at AS \"Создан\", updated_at AS \"Изменён\" FROM captains ORDER BY full_name;";
        public const string RefAddresses = "SELECT address_id AS \"ID\", country AS \"Страна\", region AS \"Регион\", city AS \"Город\", street AS \"Улица\", building_number AS \"Дом\" FROM addresses ORDER BY country, city, street, building_number;";
        public const string RefPorts = "SELECT p.port_id AS \"ID\", p.port_name AS \"Порт\", (a.country || ', ' || COALESCE(a.region || ', ', '') || a.city || ', ' || a.street || ' ' || a.building_number) AS \"Адрес\" FROM ports p LEFT JOIN addresses a ON p.address_id = a.address_id ORDER BY p.port_name;";
        public const string RefShipTypes = "SELECT type_id AS \"ID\", type_name AS \"Тип судна\" FROM ship_types ORDER BY type_name;";
        public const string RefDockyards = "SELECT dockyard_id AS \"ID\", name AS \"Верфь\", country AS \"Страна\" FROM dockyards ORDER BY name;";
        public const string RefUnits = "SELECT unit_id AS \"ID\", unit_name AS \"Единица\", unit_code AS \"Код\" FROM units ORDER BY unit_name;";
        public const string RefBanks = "SELECT bank_id AS \"ID\", bank_name AS \"Банк\" FROM banks ORDER BY bank_name;";
    }
}
