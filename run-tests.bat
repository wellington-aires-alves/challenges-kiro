@echo off
REM ============================================
REM TaskFlow - Executar Testes
REM ============================================

echo.
echo ========================================
echo   TaskFlow - Executando Testes
echo ========================================
echo.

REM Executar todos os testes
dotnet test --verbosity normal

if %errorlevel% neq 0 (
    echo.
    echo [ERRO] Alguns testes falharam!
    pause
    exit /b 1
) else (
    echo.
    echo ========================================
    echo   Todos os testes passaram!
    echo ========================================
    echo.
)

pause
