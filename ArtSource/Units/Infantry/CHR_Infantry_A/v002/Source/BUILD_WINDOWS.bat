@echo off
setlocal
set "BLENDER=blender.exe"
if not "%~1"=="" set "BLENDER=%~1"
"%BLENDER%" --background --factory-startup --python "%~dp0build_unit03_l3_blender.py" -- --package-root "%~dp0.."
if errorlevel 1 exit /b %errorlevel%
echo Build complete.
endlocal
