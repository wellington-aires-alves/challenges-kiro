@echo off
REM ============================================
REM TaskFlow - Script de Execução Local
REM ============================================

echo.
echo ========================================
echo   TaskFlow - Iniciando Aplicacao
echo ========================================
echo.

REM Verificar se o .NET está instalado
echo [1/4] Verificando instalacao do .NET...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERRO] .NET nao encontrado!
    echo Por favor, instale o .NET 9 SDK em: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo [OK] .NET encontrado: 
dotnet --version
echo.

REM Restaurar dependências
echo [2/4] Restaurando dependencias...
dotnet restore
if %errorlevel% neq 0 (
    echo [ERRO] Falha ao restaurar dependencias!
    pause
    exit /b 1
)
echo [OK] Dependencias restauradas com sucesso!
echo.

REM Compilar o projeto
echo [3/4] Compilando o projeto...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo [ERRO] Falha ao compilar o projeto!
    pause
    exit /b 1
)
echo [OK] Projeto compilado com sucesso!
echo.

REM Executar a aplicação
echo [4/4] Iniciando a aplicacao...
echo.
echo ========================================
echo   Aplicacao rodando!
echo ========================================
echo.
echo Acesse a aplicacao em:
echo   - http://localhost:5251
echo.
echo Pressione Ctrl+C para parar o servidor
echo ========================================
echo.

dotnet run --project TaskFlow.API --configuration Release

REM Se o dotnet run falhar
if %errorlevel% neq 0 (
    echo.
    echo [ERRO] Falha ao iniciar a aplicacao!
    pause
    exit /b 1
)
