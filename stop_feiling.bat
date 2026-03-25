@echo off
setlocal
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\stop_feiling.ps1"
if errorlevel 1 (
  echo.
  echo Feiling stop failed.
  pause
)
