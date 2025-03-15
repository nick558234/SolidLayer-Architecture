@echo off
echo Running model cleanup script...
powershell -ExecutionPolicy Bypass -File .\cleanup-models.ps1
pause
