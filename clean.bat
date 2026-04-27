@echo off
REM ============================================
REM TaskFlow - Limpar Build
REM ============================================

echo.
echo ========================================
echo   TaskFlow - Limpando arquivos de build
echo ========================================
echo.

echo Limpando pastas bin e obj...
dotnet clean

if %errorlevel% neq 0 (
    echo [ERRO] Falha ao limpar o projeto!
    pause
    exit /b 1
)

echo.
echo [OK] Projeto limpo com sucesso!
echo.

pause
