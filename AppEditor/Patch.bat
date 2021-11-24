@echo off
REM This batch depends on an environment variable called webserver that should point to the path of your webserver
echo Patch Batch . . .

set servicePath=\\%webserver%\www\webos-appcatalog\
set catalogPath=\\%webserver%\media\webOS\
set sourcePath=%1
set sourcePath=%sourcePath:"=%
echo.
echo Source: %sourcePath%
echo App: %2
echo.
if [%2] NEQ [] (
	copy "%sourcePath%\AppMetadata\%2" "%catalogPath%AppMetadata\" /y
)
copy "%sourcePath%\masterAppData.json" "%catalogPath%" /y
copy "%sourcePath%\extantAppData.json" "%servicePath%" /y
echo.
cd "%sourcePath%\AppMetadata"
git add .
git commit -m "update metadata"
git push
echo Done!
powershell sleep -s 2