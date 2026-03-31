# BaltikaApp

Настольное WPF-приложение для учёта деятельности судоходной компании: суда, рейсы, грузы, контрагенты, разграничение режимов доступа к PostgreSQL и аналитическая отчётность.

---

## 1. Реализованная функциональность

- полный цикл CRUD для основных сущностей (`ships`, `shipments`, `cargo`, `senders`, `consignees`);
- CRUD для ключевых справочников (`captains`, `ports`, `addresses`) через отдельное окно «Справочники»;
- запуск в режиме только чтения с переключением в режим редактирования после ввода пароля;
- навигация по разделам через команды WPF (ICommand / RelayCommand);
- централизованные стили интерфейса через `ResourceDictionary` (`AppTheme.xaml`);
- единый сервис сообщений (`WpfMessageService`), хелпер CRUD-кнопок (`CrudWindowHelper`), утилиты DataGrid (`DataGridHelper`);
- просмотр детальных атрибутов строки по двойному щелчку;
- отчёты с экспортом в CSV и XLSX, печать через `PrintDialog` и `FlowDocument`;
- оперативные показатели на главном окне (рейсы и грузы за последний месяц);
- централизованное хранение SQL-запросов в классах `Queries.cs`;
- доступ к данным через `NpgsqlDataSource` с пересозданием пулов при смене параметров подключения;
- унифицированные пользовательские статусы в окнах данных через `StatusUiHelper` (информационные, успешные, предупреждения, ошибки);
- централизованный каталог UI-текстов `UiText` для единообразия формулировок;
- параметры подключения: переменные окружения, локальный файл `%LocalAppData%\BaltikaApp\connection.json`, диалог настроек (доступен в режиме редактирования);
- запуск локальной службы или portable-экземпляра PostgreSQL из диалога настроек.

---

## 2. Технологии и зависимости

| Компонент | Версия / примечание |
|-----------|---------------------|
| Платформа | .NET 8 (`net8.0-windows`) |
| Язык | C# |
| Интерфейс | WPF (Windows Presentation Foundation) |
| СУБД | PostgreSQL |
| Драйвер | Npgsql 10.x |
| Экспорт таблиц | ClosedXML (XLSX) |

Внутренние модули (пространство имён `BaltikaApp.Data`):

- `DatabaseConnectionSettings` — параметры подключения (окружение, файл, значения по умолчанию);
- `ConnectionManager` — текущий режим доступа (чтение / редактирование);
- `Db` — выполнение запросов и команд, пересоздание источников данных;
- `Queries.cs` — централизованные тексты SQL.

Инфраструктура WPF (пространство имён `BaltikaApp.Wpf`):

- `RelayCommand` — универсальная реализация `ICommand` на делегатах;
- `AppState` — модель состояния главного окна (`INotifyPropertyChanged`);
- `CrudWindowHelper` — управление доступностью CRUD-кнопок по режиму доступа;
- `DataGridHelper` — извлечение ID строки и отображение деталей записи;
- `StatusUiHelper` — единообразная установка статусных сообщений с типом состояния;
- `UiText` — централизованный каталог пользовательских текстов интерфейса;
- `WpfMessageService` — обёртка над `MessageBox` для информационных сообщений, ошибок и подтверждений;
- `ValidationService` — проверка пользовательского ввода.

---

## 3. Структура проекта

```
BaltikaApp/
├── BaltikaApp.sln                     # Solution
├── BaltikaApp.Wpf/                    # Единственный проект (WPF)
│   ├── Commands/
│   │   └── RelayCommand.cs            # Реализация ICommand
│   ├── Data/
│   │   ├── ConnectionConfig.cs        # DTO параметров подключения
│   │   ├── ConnectionConfigStore.cs   # Чтение/запись connection.json
│   │   ├── ConnectionManager.cs       # Режим доступа (Reader/Writer)
│   │   ├── DatabaseConnectionSettings.cs  # Параметры PostgreSQL
│   │   ├── Db.cs                      # Выполнение SQL (Query, Execute)
│   │   └── Queries.cs                 # Централизованные SQL-тексты
│   ├── Assets/
│   │   └── AppIcon.ico                 # Иконка приложения (окна и exe)
│   ├── Dialogs/
│   │   ├── ShipEditDialog.xaml(.cs)
│   │   ├── ShipmentEditDialog.xaml(.cs)
│   │   ├── CargoEditDialog.xaml(.cs)
│   │   ├── SenderEditDialog.xaml(.cs)
│   │   ├── ConsigneeEditDialog.xaml(.cs)
│   │   ├── CaptainEditDialog.xaml(.cs)
│   │   ├── PortEditDialog.xaml(.cs)
│   │   └── AddressEditDialog.xaml(.cs)
│   ├── Helpers/
│   │   ├── CrudWindowHelper.cs        # Управление CRUD-кнопками
│   │   ├── DataGridHelper.cs          # Утилиты DataGrid
│   │   └── StatusUiHelper.cs          # Унифицированные статусы UI
│   ├── Models/
│   │   └── EditValues.cs              # Структуры данных диалогов
│   ├── Themes/
│   │   └── AppTheme.xaml              # Централизованные стили
│   ├── App.xaml(.cs)                  # Точка запуска
│   ├── AppState.cs                    # Модель состояния (MVVM)
│   ├── MainWindow.xaml(.cs)           # Главное окно
│   ├── ReferenceDataWindow.xaml(.cs)  # Справочники (капитаны, порты, адреса)
│   ├── ShipsWindow.xaml(.cs)          # Суда
│   ├── ShipmentsWindow.xaml(.cs)      # Рейсы
│   ├── CargoWindow.xaml(.cs)          # Грузы
│   ├── ClientsWindow.xaml(.cs)        # Клиенты
│   ├── ReportsWindow.xaml(.cs)        # Отчёты и справочники
│   ├── ShipmentsByPeriodWindow.xaml(.cs)  # Рейсы за период
│   ├── ConnectionSettingsWindow.xaml(.cs) # Настройки подключения
│   ├── PasswordPromptDialog.xaml(.cs) # Ввод пароля
│   ├── ThemeManager.cs                # Переключение и сохранение темы
│   ├── UiText.cs                      # Централизованные UI-тексты
│   ├── WpfMessageService.cs           # Сервис сообщений
│   └── ValidationService.cs           # Валидация ввода
├── create_database.sql                # Создание базы baltika
├── schema_baltika.sql                 # Таблицы и ограничения
├── data_baltika.sql                   # Начальное наполнение
├── reports_baltika.sql                # Представления и функции
├── users.sql                          # Роли и привилегии
├── export-baltika.bat / .ps1          # Экспорт дампа БД
├── import-baltika.bat / .ps1          # Импорт дампа БД
├── USERGUIDE.md                       # Руководство пользователя
└── README.md                          # Техническая документация
```

---

## 4. Развёртывание базы данных

Порядок выполнения скриптов (от суперпользователя PostgreSQL):

1. `create_database.sql` — создание базы `baltika` (UTF-8).
2. Подключиться к базе `baltika`.
3. `schema_baltika.sql` — таблицы и ограничения целостности.
4. `data_baltika.sql` — начальные данные.
5. `reports_baltika.sql` — представления и функция `get_shipments_by_period`.
6. `users.sql` — роли `baltika_reader` / `baltika_writer` и привилегии.

---

## 5. Параметры подключения к PostgreSQL

Приоритет применения:

1. **Переменные окружения** (необязательно): `BALTIKA_HOST`, `BALTIKA_PORT`, `BALTIKA_DB`, `BALTIKA_ADMIN_USER`, `BALTIKA_ADMIN_PASSWORD`.
2. **Файл** `%LocalAppData%\BaltikaApp\connection.json` — хост, порт и имя базы, сохраняемые из приложения между сеансами. При наличии файла его значения перекрывают переменные окружения.
3. **Диалог настроек** (кнопка ⚙ на главном окне, доступна в режиме редактирования) — позволяет изменить параметры, проверить подключение и запустить локальный PostgreSQL.

После сохранения пересоздаются пулы подключений Npgsql; открытые окна необходимо обновить вручную.

---

## 6. Сборка и запуск

```powershell
dotnet build BaltikaApp.sln -c Release
dotnet run --project BaltikaApp.Wpf\BaltikaApp.Wpf.csproj
```

Однофайловая публикация:

```powershell
dotnet publish BaltikaApp.Wpf\BaltikaApp.Wpf.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

При запуске выполняется проверка доступности PostgreSQL (подключение от `baltika_reader`). Если сервер недоступен — выводится сообщение и приложение завершается.

---

## 7. Окна приложения

| Окно | Назначение |
|------|------------|
| `MainWindow` | Навигация, режим доступа, настройки подключения, KPI |
| `ReferenceDataWindow` | Справочники: капитаны, порты, адреса (CRUD) |
| `ShipsWindow` | Суда (CRUD, детали по двойному щелчку) |
| `ShipmentsWindow` | Рейсы (фильтры по судну и датам, сброс фильтров, CRUD) |
| `CargoWindow` | Грузы (фильтр по рейсу, сброс фильтра, CRUD) |
| `ClientsWindow` | Отправители, получатели, активность клиентов |
| `ReportsWindow` | Аналитика, справочники, экспорт CSV/XLSX, печать |
| `ShipmentsByPeriodWindow` | Рейсы за интервал дат (функция `get_shipments_by_period`) |
| `ConnectionSettingsWindow` | Хост, порт, база данных, запуск локального PostgreSQL |
| `PasswordPromptDialog` | Ввод пароля для режима редактирования |

---

## 8. Объекты базы данных

### 8.1 Основные таблицы

`ships`, `shipments`, `cargo`, `senders`, `consignees` — основные сущности; справочники (`captains`, `ship_types`, `dockyards`, `ports`, `units`, `banks`, `addresses`) используются в диалогах ввода и в отчётах.

### 8.2 Представления и функции

- `ships_full_info`, `shipments_full_info`, `cargo_full_info` — источники данных для табличного просмотра;
- аналитические представления (`ships_by_type`, `ports_statistics`, `ships_activity`, `cargo_financial_summary`, `clients_activity`, `shipments_by_month`) — окно отчётов;
- `get_shipments_by_period(date, date)` — окно «Рейсы по периоду».

---

## 9. Экспорт и печать

В `ReportsWindow`:

- **CSV** — с экранированием полей (точка с запятой как разделитель);
- **XLSX** — через ClosedXML с автоподбором ширин колонок;
- **Печать** — через `FlowDocument` с заголовками, границами ячеек и корректной пагинацией.

---

## 10. Архитектура

- **Data Layer** (`BaltikaApp.Data`) — доступ к данным, конфигурация, SQL-запросы. Не зависит от WPF;
- **UI Layer** (`BaltikaApp.Wpf`) — окна, диалоги, стили, команды. Использует MVVM-паттерны (привязка данных, `ICommand`, `INotifyPropertyChanged`);
- **Helpers** — `CrudWindowHelper` (управление CRUD-кнопками), `DataGridHelper` (работа с выделением и деталями строки), `StatusUiHelper` (единый UX статусов);
- **Themes** — единый `ResourceDictionary` со стилями кнопок, DataGrid и общими ресурсами.

---

## 11. UX-поведение интерфейса

- в окнах данных отображается строка статуса выполнения операций (загрузка, успех, предупреждение, ошибка);
- при отсутствии строк показываются подсказки с рекомендуемым действием;
- в окнах с фильтрами доступны кнопки сброса фильтра (возврат к базовым значениям);
- в настройках подключения раздельно показываются:
  - результат последнего действия пользователя;
  - текущий статус локального PostgreSQL;
- переключение светлой/тёмной темы выполняется сразу и сохраняется между запусками.

---

## 12. Частые вопросы

**Почему после запуска недоступно редактирование?**
По умолчанию устанавливается режим чтения для снижения риска случайных изменений.

**Где редактировать справочники (капитаны, порты, адреса)?**
В главном окне: блок «Режим доступа и состояние» → кнопка «Справочники» (доступно в режиме редактирования).

**Почему в заголовке окна или у exe может не сразу обновиться иконка?**
Windows может использовать кэш значков. После пересборки/публикации рекомендуется перезапустить приложение и обновить ярлык.

**Пустой результат в «Рейсы по периоду»?**
Проверить диапазон дат и наличие рейсов в базе.

**Как перенести базу на другой компьютер?**
Использовать скрипты `export-baltika` / `import-baltika` в корне репозитория.

---

## 13. Направления развития

- журналирование действий пользователя в режиме записи;
- расширение модели безопасности (хранение паролей вне кода приложения);
- автоматизированные интеграционные тесты;
- дополнительные фильтры и поисковые возможности в окнах данных.
