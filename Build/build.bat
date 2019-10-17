@echo off

mkdir Build
del /q /f /s .\Build
dotnet msbuild ..\GladosV3.sln /p:Configuration=Release "/p:Platform=Any CPU"
dotnet publish ..\GLaDOSV3\GladosV3.csproj -c Release --self-contained -r win-x64 -o .\Build\
mkdir .\Build\Binaries\
mkdir .\Build\Modules\
mkdir .\Build\Images\
mkdir .\Build\Dependencies\

copy ..\GLaDOSV3\bin\Release\netcoreapp3.0\Binaries\ .\Build\Binaries\
copy ..\GLaDOSV3\bin\Release\netcoreapp3.0\Modules\ .\Build\Modules\
copy ..\GLaDOSV3\bin\Release\netcoreapp3.0\Images\ .\Build\Images\
copy ..\GLaDOSV3\bin\Release\netcoreapp3.0\Dependencies\ .\Build\Dependencies\
IF %ERRORLEVEL% NEQ 0 GOTO err
del release.zip
7z a release.zip -mx9 -r .\Build\* .\Build\*
7z l release.zip
del /q /f /s .\Build
rmdir .\Build /s /q
echo Build successful!
timeout /t 10
EXIT /B 0

:err
echo Build failed!
PAUSE
EXIT /B 1