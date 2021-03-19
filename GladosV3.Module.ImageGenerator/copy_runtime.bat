@echo off
echo %1
setlocal enabledelayedexpansion
set newest=000000000
set dir=%userprofile%\.nuget\packages\magick.net-q16-hdri-anycpu\
PUSHD %dir%
for /f "delims=" %%G in ('dir /b /ad ^| findstr /r "^[0-9][0-9]*\.[0-9][0-9]*"') do (
    FOR /F "tokens=1-3 delims=." %%H IN ("%%~G") DO (
        SET "node1=000%%H"
        SET "node1=!node1:~-3!"
        SET "node2=000%%I"
        SET "node2=!node2:~-3!"
        SET "node3=000%%J"
        SET "node3=!node3:~-3!"
        IF 1!node1!!node2!!node3! GTR 1!newest! (
            set newest=!node1!!node2!!node3!
            set newrelease=%%G
        )
    )
)
echo %newrelease%
POPD
xcopy /s /J /I /Y "%dir%%newrelease%\runtimes" %1
exit