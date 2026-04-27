@echo off
chcp 65001 >nul
REM ============================================
REM TaskFlow - Menu Principal
REM ============================================

:menu
cls
echo.
echo ╔════════════════════════════════════════╗
echo ║        TaskFlow - Menu Principal       ║
echo ╚════════════════════════════════════════╝
echo.
echo  [1] Executar Aplicação (Produção)
echo  [2] Executar em Modo Desenvolvimento
echo  [3] Executar Testes
echo  [4] Limpar Build
echo  [5] Compilar Projeto
echo  [6] Restaurar Dependências
echo  [0] Sair
echo.
echo ════════════════════════════════════════
echo.

set /p opcao="Escolha uma opção: "

if "%opcao%"=="1" goto executar
if "%opcao%"=="2" goto dev
if "%opcao%"=="3" goto testes
if "%opcao%"=="4" goto limpar
if "%opcao%"=="5" goto compilar
if "%opcao%"=="6" goto restaurar
if "%opcao%"=="0" goto sair

echo.
echo [ERRO] Opção inválida!
timeout /t 2 >nul
goto menu

:executar
cls
echo.
echo ════════════════════════════════════════
echo   Executando Aplicação (Produção)
echo ════════════════════════════════════════
echo.
call run-taskflow.bat
pause
goto menu

:dev
cls
echo.
echo ════════════════════════════════════════
echo   Modo Desenvolvimento (Hot Reload)
echo ════════════════════════════════════════
echo.
call run-dev.bat
pause
goto menu

:testes
cls
echo.
echo ════════════════════════════════════════
echo   Executando Testes
echo ════════════════════════════════════════
echo.
call run-tests.bat
goto menu

:limpar
cls
echo.
echo ════════════════════════════════════════
echo   Limpando Build
echo ════════════════════════════════════════
echo.
call clean.bat
goto menu

:compilar
cls
echo.
echo ════════════════════════════════════════
echo   Compilando Projeto
echo ════════════════════════════════════════
echo.
dotnet build
if %errorlevel% neq 0 (
    echo.
    echo [ERRO] Falha ao compilar!
) else (
    echo.
    echo [OK] Compilação concluída com sucesso!
)
echo.
pause
goto menu

:restaurar
cls
echo.
echo ════════════════════════════════════════
echo   Restaurando Dependências
echo ════════════════════════════════════════
echo.
dotnet restore
if %errorlevel% neq 0 (
    echo.
    echo [ERRO] Falha ao restaurar dependências!
) else (
    echo.
    echo [OK] Dependências restauradas com sucesso!
)
echo.
pause
goto menu

:sair
cls
echo.
echo ════════════════════════════════════════
echo   Até logo!
echo ════════════════════════════════════════
echo.
timeout /t 1 >nul
exit /b 0
