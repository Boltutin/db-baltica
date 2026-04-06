# Развёртывание БД «Балтика» на тестовом стенде (с нуля)

Набор скриптов повторяет структуру и данные из репозитория BaltikaApp и позволяет поднять ту же базу на чистом PostgreSQL.

## Требования

- PostgreSQL 12+ (рекомендуется 14+).
- Учётная запись с правами суперпользователя (`postgres`) для создания БД и ролей.
- Кодировка UTF-8.

## Порядок выполнения

Подключайтесь к серверу с нужным хостом и портом. Примеры для `localhost`:

| Шаг | Файл | Подключение к БД | Назначение |
|-----|------|------------------|------------|
| 0 (опционально) | `00_drop_database_test_only.sql` | `postgres` | Удалить существующую `baltika` (только тест) |
| 1 | `01_create_database.sql` | `postgres` | `CREATE DATABASE baltika` |
| 2 | `02_schema.sql` | `baltika` | Таблицы, FK |
| 3 | `03_data.sql` | `baltika` | Тестовые данные |
| 4 | `04_reports_and_functions.sql` | `baltika` | Представления и функция `get_shipments_by_period` |
| 5 | `05_roles_and_grants.sql` | `baltika` | Роли `baltika_reader` / `baltika_writer`, GRANT |

После шага 5 приложение BaltikaApp может подключаться:

- режим чтения: пользователь `baltika_reader`, пароль `123`;
- режим записи: пользователь `baltika_writer`, пароль `12345`.

Пароли заданы в скрипте и совпадают с кодом приложения; на продакшене их нужно сменить и обновить в `users.sql` / настройках приложения.

## Автоматический запуск (Windows)

Из каталога `sql/deploy`.

### `deploy.cmd` (только cmd.exe + psql, без PowerShell)

Подходит для ПК, где PowerShell отключён или недоступен. Нужны:

- `psql.exe` в `PATH`, **или** переменная `PSQL_EXE` с полным путём к `psql.exe`, **или** скрипт сам запросит путь, если `psql` не найден.

```cmd
deploy.cmd
```

По умолчанию: пользователь `postgres`, пароль `12345` (можно нажать Enter).

### `deploy.ps1` (PowerShell)

Если PowerShell разрешён:

```powershell
.\deploy.ps1 -PgHost localhost -Port 5432 -User postgres -Password 12345
```

или интерактивно:

```powershell
.\deploy.ps1 -AskCredentials
```

### Поведение при ошибках

И в `deploy.cmd`, и в `deploy.ps1` при сбое шага предлагается:

- `R` — повторить шаг;
- `C` — изменить параметры подключения и повторить;
- `Q` — выйти.

В конце окно не закрывается сразу (ожидание нажатия клавиши).

## Ручной запуск (psql)

```text
psql -U postgres -h localhost -d postgres -f 01_create_database.sql
psql -U postgres -h localhost -d baltika   -f 02_schema.sql
psql -U postgres -h localhost -d baltika   -f 03_data.sql
psql -U postgres -h localhost -d baltika   -f 04_reports_and_functions.sql
psql -U postgres -h localhost -d baltika   -f 05_roles_and_grants.sql
```

## Полная переустановка на тесте

```text
psql -U postgres -h localhost -d postgres -f 00_drop_database_test_only.sql
```

Затем снова шаги 1–5 или `deploy.ps1`.

## Связь с корнем репозитория

Файлы в `sql/deploy/` — копии с тем же содержимым, что:

- `create_database.sql`
- `schema_baltika.sql`
- `data_baltika.sql`
- `reports_baltika.sql`
- `users.sql`

При изменении схемы или данных обновляйте исходники в корне и синхронизируйте копии в `sql/deploy/` (или наоборот — ведите правки здесь и копируйте в корень).

## Альтернатива: дамп с работающего сервера

Если нужна побитовая копия уже запущенной БД:

```text
pg_dump -U postgres -h HOST -d baltika -F p -f baltika_full.sql
```

Восстановление: создать пустую БД и `psql -d baltika -f baltika_full.sql`.
