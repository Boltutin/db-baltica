@echo off
setlocal EnableExtensions EnableDelayedExpansion
chcp 65001 >nul 2>&1

set "SCRIPT_DIR=%~dp0"

REM Optional: set PSQL_EXE=C:\Program Files\PostgreSQL\16\bin\psql.exe before run
if defined PSQL_EXE goto :have_psql

where psql >nul 2>&1
if errorlevel 1 goto :psql_not_in_path
for /f "delims=" %%A in ('where psql 2^>nul') do (
  set "PSQL_EXE=%%A"
  goto :have_psql
)

:psql_not_in_path
echo.
echo [ERROR] psql.exe not found in PATH.
echo Add PostgreSQL "bin" folder to PATH, or run:
echo   set PSQL_EXE=C:\Program Files\PostgreSQL\16\bin\psql.exe
echo   deploy.cmd
echo.
set /p "PSQL_EXE=Full path to psql.exe: "
if not defined PSQL_EXE (
  echo Cancelled.
  goto :end_pause
)

:have_psql
echo !PSQL_EXE! | findstr /i "\\" >nul 2>&1
if not errorlevel 1 if not exist "!PSQL_EXE!" (
  echo [ERROR] File not found: !PSQL_EXE!
  goto :end_pause
)

echo ============================================
echo Baltika DB deploy (cmd only, no PowerShell)
echo ============================================
echo.

call :prompt_connection

call :run_step postgres 01_create_database.sql
if errorlevel 2 goto :end_pause
if errorlevel 1 goto :failed

call :run_step baltika 02_schema.sql
if errorlevel 2 goto :end_pause
if errorlevel 1 goto :failed

call :run_step baltika 03_data.sql
if errorlevel 2 goto :end_pause
if errorlevel 1 goto :failed

call :run_step baltika 04_reports_and_functions.sql
if errorlevel 2 goto :end_pause
if errorlevel 1 goto :failed

call :run_step baltika 05_roles_and_grants.sql
if errorlevel 2 goto :end_pause
if errorlevel 1 goto :failed

echo.
echo [OK] Database baltika deployed.
goto :end_pause

:failed
echo.
echo [ERROR] Deploy stopped with error.
goto :end_pause

:end_pause
echo.
pause
endlocal
exit /b 0

REM ---------------------------------------------------------------------------
REM Prompt: host, port, user, password (Enter = defaults)
REM ---------------------------------------------------------------------------
:prompt_connection
echo --- Connection (Enter = default in brackets) ---
set "PGHOST=localhost"
set /p "PGHOST=Host [localhost]: "
if "!PGHOST!"=="" set "PGHOST=localhost"

set "PGPORT=5432"
set /p "PGPORT=Port [5432]: "
if "!PGPORT!"=="" set "PGPORT=5432"

set "PGUSER=postgres"
set /p "PGUSER=User [postgres]: "
if "!PGUSER!"=="" set "PGUSER=postgres"

set "PGPASSWORD=12345"
set /p "PGPASSWORD=Password [12345]: "
if "!PGPASSWORD!"=="" set "PGPASSWORD=12345"

set "PGCLIENTENCODING=UTF8"
echo.
exit /b 0

REM ---------------------------------------------------------------------------
REM Args: %1 = database name, %2 = sql file in this folder
REM Returns: 0 ok, 1 fatal error after step, 2 user quit
REM ---------------------------------------------------------------------------
:run_step
set "_DB=%~1"
set "_SQL=%~2"
set "_FILE=!SCRIPT_DIR!!_SQL!"

if not exist "!_FILE!" (
  echo [ERROR] SQL file not found: !_FILE!
  exit /b 1
)

:run_step_retry
echo.
echo ^>^>^> !_SQL!  (database=!_DB!)
echo     host=!PGHOST! port=!PGPORT! user=!PGUSER!

"!PSQL_EXE!" -h "!PGHOST!" -p "!PGPORT!" -U "!PGUSER!" -d "!_DB!" -v ON_ERROR_STOP=1 -f "!_FILE!"
set "_EC=!ERRORLEVEL!"
if "!_EC!"=="0" exit /b 0

echo.
echo psql failed (code=!_EC!) on file: !_SQL!
echo   [R] Retry this step
echo   [C] Change connection and retry
echo   [Q] Quit
set "_ACT="
set /p "_ACT=Enter R / C / Q: "
if /i "!_ACT!"=="R" goto :run_step_retry
if /i "!_ACT!"=="C" (
  call :prompt_connection
  goto :run_step_retry
)
if /i "!_ACT!"=="Q" exit /b 2

echo Unknown choice, retrying step...
goto :run_step_retry
