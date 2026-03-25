@echo off
setlocal
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\start_feiling.ps1"
if errorlevel 1 (
  echo.
  echo Feiling start failed.
  pause
)
