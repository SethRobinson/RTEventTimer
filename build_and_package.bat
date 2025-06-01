@echo off
setlocal

echo RTEventTimer - Build and Package Script
echo ======================================
echo.

:: Set variables
set PROJECT_NAME=RTEventTimer
set OUTPUT_DIR=publish
set ZIP_NAME=RTEventTimerLatest.zip

:: Clean previous builds
echo Cleaning previous builds...
if exist %OUTPUT_DIR% rmdir /s /q %OUTPUT_DIR%
if exist %ZIP_NAME% del %ZIP_NAME%

:: Clean the project first
echo.
echo Cleaning project...
dotnet clean -c Release

:: Build the project as a single file executable
echo.
echo Building single file executable...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none -p:DebugSymbols=false -p:EnableCompressionInSingleFile=true -o %OUTPUT_DIR%

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

:: Sign the executable
echo.
echo Signing the executable...
call %RT_PROJECTS%\Signing\sign.bat "%OUTPUT_DIR%\%PROJECT_NAME%.exe" "%PROJECT_NAME%"

if %ERRORLEVEL% neq 0 (
    echo.
    echo WARNING: Signing failed or was skipped
    echo Continuing with packaging...
)

:: Remove any PDB files if they exist
echo.
echo Removing any debug files...
if exist %OUTPUT_DIR%\*.pdb (
    echo Found PDB files, removing...
    del /q %OUTPUT_DIR%\*.pdb
)
if exist %OUTPUT_DIR%\%PROJECT_NAME%.pdb del /q %OUTPUT_DIR%\%PROJECT_NAME%.pdb

:: List files to be packaged (for verification)
echo.
echo Files to be packaged:
dir /b %OUTPUT_DIR%
echo.

:: Create README for distribution
echo.
echo Creating distribution README...
echo RTEventTimer - Real-Time Event Timer > %OUTPUT_DIR%\README.txt
echo ===================================== >> %OUTPUT_DIR%\README.txt
echo. >> %OUTPUT_DIR%\README.txt
echo To run the timer: >> %OUTPUT_DIR%\README.txt
echo 1. Double-click RTEventTimer.exe >> %OUTPUT_DIR%\README.txt
echo 2. The timer will appear as an overlay on your screen >> %OUTPUT_DIR%\README.txt
echo. >> %OUTPUT_DIR%\README.txt
echo Controls: >> %OUTPUT_DIR%\README.txt
echo - Click and drag anywhere to move the timer >> %OUTPUT_DIR%\README.txt
echo - Click the gear icon to open settings >> %OUTPUT_DIR%\README.txt
echo - Click the X button to close >> %OUTPUT_DIR%\README.txt
echo. >> %OUTPUT_DIR%\README.txt
echo Note: You can customize the sounds and background by replacing files in the Assets folder: >> %OUTPUT_DIR%\README.txt
echo - background.png: Timer background image (1920x270 pixels with transparency) >> %OUTPUT_DIR%\README.txt
echo - timer_finished.wav: Sound that plays when timer reaches zero >> %OUTPUT_DIR%\README.txt
echo - button1.wav: Sound that plays when buttons are clicked >> %OUTPUT_DIR%\README.txt

:: Copy Assets folder
echo.
echo Copying Assets folder...
xcopy /E /I /Y Assets %OUTPUT_DIR%\Assets

:: Copy config.txt if it exists
echo.
echo Copying config.txt...
if exist config.txt (
    copy /Y config.txt %OUTPUT_DIR%\
    echo config.txt copied.
) else (
    echo config.txt not found, skipping...
)

:: Package into ZIP
echo.
echo Creating ZIP package...
%RT_PROJECTS%\proton\shared\win\utils\7za.exe a -tzip %ZIP_NAME% .\%OUTPUT_DIR%\* -r

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Failed to create ZIP file!
    pause
    exit /b 1
)

:: Clean up publish directory
echo.
echo Cleaning up temporary files...
rmdir /s /q %OUTPUT_DIR%

:: Done
echo.
echo ======================================
echo Build and packaging completed!
echo Output: %ZIP_NAME%
echo ======================================
echo.

pause 