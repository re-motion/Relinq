@echo off

set keyfile=remotion.snk

echo Current directory: %CD%
echo Checking whether %keyfile% needs to be created...
echo.
if not exist %keyfile% goto notfound

echo %keyfile% already exists.

goto end

:notfound

echo %keyfile% does not exist, generating...
echo.
sn.exe -k %keyfile%
echo.

if %ERRORLEVEL%==9009 goto nosn
if not %ERRORLEVEL%==0 goto snerror

echo Note that this newly generated key file will not match the original key used by rubicon to sign re-motion assemblies.

goto end

:nosn

echo 'sn.exe', which is a part of the .NET/Windows SDK, could not be found in the PATH. Please set the PATH to include the .NET/Windows SDK or run this script from a Visual Studio command prompt.

exit /b 1

:snerror

echo There was an error executing 'sn.exe'.

exit /b 2

:end

exit /b 0