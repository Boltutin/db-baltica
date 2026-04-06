-- =============================================================================
-- sql/deploy/00_drop_database_test_only.sql — ТОЛЬКО для тестового стенда
--
-- ВНИМАНИЕ: безвозвратно удаляет базу данных baltika и все её объекты.
-- Не выполнять на продакшене.
--
-- Выполнять под суперпользователем postgres, подключившись к БД postgres:
--   psql -U postgres -h HOST -d postgres -f 00_drop_database_test_only.sql
--
-- После этого можно заново выполнить 01–05 (или deploy.ps1).
-- =============================================================================

SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = 'baltika'
  AND pid <> pg_backend_pid();

DROP DATABASE IF EXISTS baltika;
