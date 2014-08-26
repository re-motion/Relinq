@echo off
pushd %~dp0
set msbuild="C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
if not exist remotion.snk goto nosnk

if not [%1]==[] goto %1
	
echo Welcome to the re-motion build tool!
echo.
echo Choose your desired build:
echo [1] ... Test build ^(x86-debug^)
echo [2] ... Full build ^(x86-debug/release, x64-debug/release^)
echo [3] ... Docs build ^(x86-debug if not present, docs^)
echo           Requires Sandcastle Help File Builder to be installed!
echo [4] ... Package ^(create zip package from pre-existent build^)
echo [5] ... Run DependDB
echo [6] ... Oops, nothing please - exit.
echo.

choice /c:123456 /n /m "Your choice: "

if %ERRORLEVEL%==1 goto run_test_build
if %ERRORLEVEL%==2 goto run_full_build
if %ERRORLEVEL%==3 goto run_docs_build
if %ERRORLEVEL%==4 goto run_pkg_build
if %ERRORLEVEL%==5 goto run_dependdb
if %ERRORLEVEL%==6 goto run_exit
goto build_succeeded

:run_test_build
mkdir build\BuildOutput\log
%msbuild% build\Remotion.Local.build /t:TestBuild /maxcpucount /verbosity:normal /flp:verbosity=normal;logfile=build\BuildOutput\log\build.log
if not %ERRORLEVEL%==0 goto build_failed
goto build_succeeded

:run_full_build
mkdir build\BuildOutput\log
%msbuild% build\Remotion.Local.build /t:FullBuild /maxcpucount /verbosity:normal /flp:verbosity=normal;logfile=build\BuildOutput\log\build.log
if not %ERRORLEVEL%==0 goto build_failed
goto build_succeeded

:run_docs_build
mkdir build\BuildOutput\log
%msbuild% build\Remotion.Local.build /t:DocumentationTestBuild /maxcpucount /verbosity:minimal /flp:verbosity=normal;logfile=build\BuildOutput\log\build.log
if not %ERRORLEVEL%==0 goto build_failed
goto build_succeeded

:run_pkg_build
mkdir build\BuildOutput\log
%msbuild% build\Remotion.Local.build /t:ZipPackageAll /maxcpucount /verbosity:minimal /flp:verbosity=normal;logfile=build\BuildOutput\log\build.log "/p:Version=0.0.0.0"
if not %ERRORLEVEL%==0 goto build_failed
goto build_succeeded

:run_dependdb
mkdir build\BuildOutput\log
%msbuild% build\Remotion.build /t:DependDB /maxcpucount /verbosity:normal /flp:verbosity=detailed;logfile=build\BuildOutput\log\build.log "/p:Version=0.0.0.0;DependDBProjectBranch=Trunk;DependDBProjectVersion=0.0.0.0;DependDBProjectVCSUrlTemplate=https://svn.re-motion.org:443/svn/Remotion/!svn/bc/$(DependDBProjectRevision)/trunk/{0};DependDBProjectImportNotificationMailAddress=%USERNAME%@rubicon.eu;DependDBUploadPath=C:\Temp\LocalDependDBUploadDirectory"
if not %ERRORLEVEL%==0 goto build_failed
goto build_succeeded

:run_exit
exit /b 0


:build_failed
echo.
echo Building re-motion has failed.
start build\BuildOutput\log\build.log
pause
popd
exit /b 1

:build_succeeded
echo.
pause
popd
exit /b 0

:nosnk
echo remotion.snk does not exist. Please run Generate-Snk.cmd from a Visual Studio Command Prompt.
pause
popd
exit /b 2
