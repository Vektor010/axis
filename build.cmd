@echo off
chcp 65001 >nul
title Сборка WinTweaker
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1"
if %errorlevel% neq 0 (
    echo.
    echo Ошибка сборки. Нажмите любую клавишу для выхода.
    pause >nul
    exit /b %errorlevel%
)
pause
