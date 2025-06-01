@echo off
echo Starting RTEventTimer...
echo ======================
echo.

:: Run the application (dotnet run automatically builds if needed)
dotnet run

:: Keep window open if there was an error
if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Failed to run RTEventTimer
    pause
) 