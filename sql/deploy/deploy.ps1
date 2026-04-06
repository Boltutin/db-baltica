param(
    [string]$PgHost = "localhost",
    [int]$Port = 5432,
    [string]$User = "postgres",
    [string]$Psql = "psql",
    [string]$Password,
    [switch]$AskCredentials
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path

function Prompt-Value([string]$Caption, [string]$DefaultValue) {
    $value = Read-Host "$Caption [$DefaultValue]"
    if ([string]::IsNullOrWhiteSpace($value)) { return $DefaultValue }
    return $value.Trim()
}

function Configure-Connection {
    param([switch]$Interactive)

    if ($Interactive) {
        Write-Host ""
        Write-Host "=== PostgreSQL connection settings ===" -ForegroundColor Yellow
        $script:PgHost = Prompt-Value "Host" $script:PgHost

        $portInput = Prompt-Value "Port" "$script:Port"
        [int]$parsed = 0
        if ([int]::TryParse($portInput, [ref]$parsed)) {
            $script:Port = $parsed
        }

        $script:User = Prompt-Value "User" $script:User

        $defaultPassText = if ([string]::IsNullOrEmpty($script:Password)) { "12345" } else { $script:Password }
        $passInput = Read-Host "Password (Enter = $defaultPassText)"
        if ([string]::IsNullOrWhiteSpace($passInput)) {
            $script:Password = $defaultPassText
        } else {
            $script:Password = $passInput
        }
    } elseif ([string]::IsNullOrWhiteSpace($script:Password)) {
        $passInput = Read-Host "Password for user '$script:User' (Enter = 12345)"
        if ([string]::IsNullOrWhiteSpace($passInput)) {
            $script:Password = "12345"
        } else {
            $script:Password = $passInput
        }
    }

    $env:PGHOST = $script:PgHost
    $env:PGPORT = "$script:Port"
    $env:PGUSER = $script:User
    $env:PGPASSWORD = $script:Password
}

function Wait-Exit {
    Write-Host ""
    Read-Host "Press Enter to exit"
}

$steps = @(
    @{ Db = "postgres"; File = "01_create_database.sql" },
    @{ Db = "baltika";  File = "02_schema.sql" },
    @{ Db = "baltika";  File = "03_data.sql" },
    @{ Db = "baltika";  File = "04_reports_and_functions.sql" },
    @{ Db = "baltika";  File = "05_roles_and_grants.sql" }
)

try {
    Configure-Connection -Interactive:$AskCredentials

    foreach ($s in $steps) {
        $path = Join-Path $here $s.File
        if (-not (Test-Path $path)) {
            throw "File not found: $path"
        }

        $stepDone = $false
        while (-not $stepDone) {
            Write-Host ""
            Write-Host ">>> $($s.File) (database $($s.Db))" -ForegroundColor Cyan
            Write-Host "    host=$PgHost port=$Port user=$User"

            & $Psql -d $s.Db -v ON_ERROR_STOP=1 -f $path
            $code = $LASTEXITCODE

            if ($code -eq 0) {
                $stepDone = $true
                continue
            }

            Write-Host ""
            Write-Host "psql failed with code $code on file $($s.File)." -ForegroundColor Red
            Write-Host "Choose action:"
            Write-Host "  [R] Retry this step"
            Write-Host "  [C] Change connection settings and retry"
            Write-Host "  [Q] Quit"
            $action = (Read-Host "Enter R/C/Q").Trim().ToUpperInvariant()

            switch ($action) {
                "R" { }
                "C" { Configure-Connection -Interactive }
                "Q" { throw "Interrupted by user." }
                default {
                    Write-Host "Unknown option. Retrying step..." -ForegroundColor Yellow
                }
            }
        }
    }

    Write-Host ""
    Write-Host "Done: database baltika deployed." -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "Finished with error: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    Wait-Exit
}
