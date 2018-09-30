
@echo OFF

if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" set MSBUILDDIR=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin
echo %MSBUILDDIR%

pushd .
del sdk\WebexSDKTests\app.config
copy c:\app.config sdk\WebexSDKTests\app.config
REM .\bin\nuget.exe update -self
bin\nuget.exe restore sdk\solutions\WinSDK4Desktop\WinSDK4Desktop.sln -NonInteractive
REM clear NuGet package cache
bin\nuget.exe locals all -clear

set SDKNuGetPackage=Cisco.Webex.WindowsSDK.2.0.0-EFT03
echo SDKNuGetPackage is %SDKNuGetPackage%
echo copy scf libraries to sdk\solutions\WinSDK4Desktop\packages\%SDKNuGetPackage%\lib\native\
copy /y spark-client-framework\scfLibrary\Release\*.dll sdk\solutions\WinSDK4Desktop\packages\%SDKNuGetPackage%\lib\native\
if not %errorlevel% == 0 ( 
	echo update scf libraries failed.
	goto EXIT 
)
copy /y spark-client-framework\scfLibrary\Release\spark-client-framework-dot-net.dll sdk\solutions\WinSDK4Desktop\packages\%SDKNuGetPackage%\lib\net452\
if not %errorlevel% == 0 ( 
	echo update scf libraries failed.
	goto EXIT 
)

"%MSBUILDDIR%\msbuild.exe" sdk\solutions\WinSDK4Desktop\WinSDK4Desktop.sln /t:Rebuild /p:Configuration="Debug" /p:Platform="x86"
if not %errorlevel% == 0 ( 
	echo build debug version failed!
	goto EXIT 
)

"%MSBUILDDIR%\msbuild.exe" sdk\solutions\WinSDK4Desktop\WinSDK4Desktop.sln /t:Rebuild /p:Configuration="Release" /p:Platform="x86"
if not %errorlevel% == 0 (
	echo build release version failed!
	goto EXIT
)

popd

call bin\mstest.bat

call bin\packageNuGet.bat

echo generate API Doc
"%MSBUILDDIR%\msbuild.exe" doc\WebexSDKDoc.shfbproj
if not %errorlevel% == 0 (
	echo generate API doc failed!
	goto EXIT
)

:EXIT
echo error level: %errorlevel% 
EXIT /B %errorlevel%