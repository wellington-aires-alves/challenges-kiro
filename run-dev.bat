@echo off
REM ============================================
REM TaskFlow - Modo Desenvolvimento (Hot Reload)
REM ============================================

echo.
echo ========================================
echo   TaskFlow - Modo Desenvolvimento
echo ========================================
echo.
echo Hot Reload ativado - alteracoes serao
echo aplicadas automaticamente!
echo.
echo Acesse a aplicacao em:
echo   - https://localhost:5001
echo   - http://localhost:5000
echo.
echo Pressione Ctrl+C para parar
echo ========================================
echo.

dotnet watch run --project TaskFlow.API

if %errorlevel% neq 0 (
    echo.
    echo [ERRO] Falha ao iniciar em modo desenvolvimento!
    pause
    exit /b 1
)
