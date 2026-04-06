param(
    [string]$PgHost = "localhost",
    [int]$Port = 5432,
    [string]$User = "postgres",
    [string]$Password,
    [string]$DumpPath,
    [switch]$DropExisting
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $DumpPath) {
    $DumpPath = Join-Path $here "baltika_backup.dump"
}
if (-not (Test-Path $DumpPath)) {
    throw "Dump not found: $DumpPath"
}

function Find-PgTool([string]$exeName) {
    $cmd = Get-Command $exeName -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $found = Get-ChildItem "C:\Program Files\PostgreSQL" -Recurse -Filter $exeName -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($found) { return $found.FullName }
    return $null
}

$pgRestore = Find-PgTool "pg_restore.exe"
$psql = Find-PgTool "psql.exe"
if (-not $pgRestore) {
    throw "pg_restore.exe not found. Install PostgreSQL (client tools) or add bin to PATH."
}

if ([string]::IsNullOrWhiteSpace($Password)) {
    $Password = Read-Host "Password for PostgreSQL user '$User'"
}

$env:PGHOST = $PgHost
$env:PGPORT = "$Port"
$env:PGUSER = $User
$env:PGPASSWORD = $Password

if ($DropExisting) {
    if (-not $psql) {
        throw "psql.exe not found (needed for -DropExisting). Install PostgreSQL client tools."
    }
    Write-Host "Dropping existing database baltika (if any)..." -ForegroundColor Yellow
    & $psql -d postgres -v ON_ERROR_STOP=1 -c "DROP DATABASE IF EXISTS baltika WITH (FORCE);"
    if ($LASTEXITCODE -ne 0) { throw "psql failed to drop database." }
}

Write-Host "Restoring from $DumpPath ..." -ForegroundColor Cyan
& $pgRestore --no-owner --no-acl -v -C -d postgres $DumpPath
if ($LASTEXITCODE -ne 0) { throw "pg_restore failed with exit code $LASTEXITCODE" }

Write-Host "Done: database baltika is restored." -ForegroundColor Green
Write-Host "App connections: baltika_reader / 123, baltika_writer / 12345 (see sql/deploy/README.md)." -ForegroundColor Gray
