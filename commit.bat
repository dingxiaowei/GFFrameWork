@echo off

call update.bat

echo "================提交日志=========================="
set BatDir=%~dp0
set/p log=请输入提交日志:

git add -A .
if "%log%"=="" (git commit -m "自动提交") else (git commit -m %log%)
git push

echo "================提交成功==========================="
pause