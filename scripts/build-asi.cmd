@echo off
rem Builds the in-game plugin (GtavxrPtbr.asi), its test and the stand-in tables the test runs against in the CI
rem (fake_tables.dll as GTAVXR 1.0.2 lays them out, fake_wider.dll with an extra column that must be refused),
rem with the Microsoft C++ x64 tools. Usage: scripts\build-asi.cmd <vcvars64.bat> <version> [out dir]
setlocal
if "%~2"=="" (echo usage: build-asi.cmd ^<vcvars64.bat^> ^<version^> [out dir] & exit /b 2)
set "src=%~dp0..\src\GtavxrPtbrAsi"
set "out=%~3"
if "%out%"=="" set "out=%~dp0..\work\asi"
call "%~1" >nul || exit /b 1
if not exist "%out%" mkdir "%out%" || exit /b 1
pushd "%out%" || exit /b 1
lib /nologo /def:"%src%\ScriptHookV.def" /machine:x64 /out:ScriptHookV.lib || exit /b 1
cl /nologo /LD /O2 /MT /W4 /WX /EHsc /std:c++17 /Brepro /DGTAVXR_PTBR_VERSION=\"%~2\" "%src%\main.cpp" ScriptHookV.lib shell32.lib ole32.lib /link /Brepro /OUT:GtavxrPtbr.asi /NOIMPLIB /NOEXP || exit /b 1
cl /nologo /O2 /MT /W4 /WX /EHsc /std:c++17 "%src%\test_patch.cpp" /Fe:test_patch.exe || exit /b 1
cl /nologo /LD /O2 /MT /W4 /WX "%src%\fake_tables.cpp" /Fe:fake_tables.dll /link /NOIMPLIB /NOEXP || exit /b 1
cl /nologo /LD /O2 /MT /W4 /WX /DFAKE_WIDER "%src%\fake_tables.cpp" /Fo:fake_wider.obj /Fe:fake_wider.dll /link /NOIMPLIB /NOEXP || exit /b 1
del /q *.obj *.lib *.exp 2>nul
popd
exit /b 0
