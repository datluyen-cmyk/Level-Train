@echo off
setlocal
set "PY=C:\Users\Admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe"
if not exist "%PY%" set "PY=python"
"%PY%" "%~dp0level_training_bot_gui.py" %*
endlocal
