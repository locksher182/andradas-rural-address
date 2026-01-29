# Configurações
$BackupDir = "C:\Users\Prefeitura\Google Drive\Backups_RuralAddress" # Ajuste para o caminho do seu Google Drive
$MaxBackups = 2
$DbName = "RuralAddressDb"
$DbUser = "postgres"
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupFile = "$BackupDir\RuralAddress_$Timestamp.sql"

# 1. Cria a pasta se não existir
if (!(Test-Path -Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir | Out-Null
    Write-Host "Pasta de backup criada: $BackupDir"
}

# 2. Realiza o Backup (pg_dump)
# Assumindo que o pg_dump está no PATH. Se não estiver, precisaremos do caminho completo.
Write-Host "Iniciando backup de $DbName..."
$env:PGPASSWORD = "Sushivivo182"
$PgDumpPath = "C:\Program Files\PostgreSQL\17\bin\pg_dump.exe"
$DumpCommand = "& ""$PgDumpPath"" -U $DbUser -h 147.93.68.244 -F c -b -v -f ""$BackupFile"" $DbName"

try {
    # Executa o comando. Em ambientes reais, pode ser necessário configurar a senha no pgpass.conf
    Invoke-Expression $DumpCommand
    Write-Host "Backup realizado com sucesso: $BackupFile"
}
catch {
    Write-Error "Erro ao realizar backup: $_"
    exit
}

# 3. Rotação (Manter apenas os últimos X)
$Files = Get-ChildItem -Path $BackupDir -Filter "RuralAddress_*.sql" | Sort-Object CreationTimeDescending

if ($Files.Count -gt $MaxBackups) {
    $FilesToDelete = $Files | Select-Object -Skip $MaxBackups
    foreach ($File in $FilesToDelete) {
        Remove-Item $File.FullName
        Write-Host "Backup antigo removido: $($File.Name)"
    }
}
else {
    Write-Host "Quantidade de backups dentro do limite ($($Files.Count)/$MaxBackups)."
}

Write-Host "Processo concluído."
