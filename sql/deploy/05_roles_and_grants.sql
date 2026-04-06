-- =============================================================================
-- sql/deploy/05_roles_and_grants.sql — шаг 5 из 5
-- Роли baltika_reader / baltika_writer и GRANT. Выполнять от postgres, БД baltika.
-- =============================================================================
-- Роли и права для BaltikaApp (пароли ролей согласованы с DatabaseConnectionSettings и ConnectionManager в приложении)
-- Выполнять под суперпользователем (postgres), подключившись к БД baltika
-- после: schema_baltika.sql, data_baltika.sql, reports_baltika.sql
-- =============================================================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'baltika_reader') THEN
        CREATE ROLE baltika_reader LOGIN PASSWORD '123';
    END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'baltika_writer') THEN
        CREATE ROLE baltika_writer LOGIN PASSWORD '12345';
    END IF;
END
$$;

-- Пароли как в приложении (при повторном запуске выравнивают значения)
ALTER ROLE baltika_reader WITH PASSWORD '123';
ALTER ROLE baltika_writer WITH PASSWORD '12345';

GRANT CONNECT ON DATABASE baltika TO baltika_reader;
GRANT CONNECT ON DATABASE baltika TO baltika_writer;

GRANT USAGE ON SCHEMA public TO baltika_reader;
GRANT USAGE ON SCHEMA public TO baltika_writer;

-- Reader: только чтение таблиц и представлений, вызов функций отчётов
GRANT SELECT ON ALL TABLES IN SCHEMA public TO baltika_reader;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO baltika_reader;

-- Writer: полный CRUD по таблицам, последовательности для SERIAL, вызов функций
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO baltika_writer;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO baltika_writer;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO baltika_writer;
