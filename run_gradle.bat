@echo off
set "JAVA_HOME=%~dp0.tools\jdk\jdk-17.0.10+7"
set "ANDROID_HOME=%~dp0.tools\android-sdk"
set "PATH=%JAVA_HOME%\bin;%PATH%"
cd /d "%~dp0Umacalc-Android"
call "%~dp0.tools\gradle\gradle-8.7\bin\gradle.bat" %*
