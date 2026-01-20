Write-Host "============================================="
Write-Host " Calculator - Demo Script (Single Console)"
Write-Host " Algoritmos y Estructuras de Datos I"
Write-Host "============================================="
Write-Host ""

# Moverse a la carpeta del script (soporta espacios)
Set-Location "$PSScriptRoot"

# 1. Verificar version de dotnet
Write-Host "[1/4] Verificando version de .NET..."
dotnet --version
Write-Host ""

# 2. Build completo
Write-Host "[2/4] Compilando la solucion completa..."
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo la compilacion. Abortando demo."
    exit 1
}
Write-Host "Build exitoso."
Write-Host ""

# 3. Iniciar servidor en background
Write-Host "[3/4] Iniciando servidor TCP en background..."
$serverProcess = Start-Process dotnet `
    -ArgumentList "run --project src/Calculator.Server/Calculator.Server.csproj" `
    -PassThru `
    -NoNewWindow

Start-Sleep -Seconds 2
Write-Host "Servidor iniciado (PID $($serverProcess.Id))."
Write-Host ""

# 4. Iniciar cliente (bloqueante)
Write-Host "[4/4] Iniciando cliente WinForms..."
dotnet run --project src/Calculator.Client/Calculator.Client.csproj

# 5. Al cerrar el cliente, detener el servidor
Write-Host ""
Write-Host "Cerrando servidor..."
if ($serverProcess -and !$serverProcess.HasExited) {
    Stop-Process -Id $serverProcess.Id -Force
    Write-Host "Servidor detenido."
}

Write-Host ""
Write-Host "============================================="
Write-Host " DEMO FINALIZADA"
Write-Host " Para correr pruebas:"
Write-Host " dotnet test"
Write-Host "============================================="
