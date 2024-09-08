@echo off
set str=%~dp0
call set str=%%str:\=/%%
call set str=%str:~3,-1%
dnsproxy.exe -u https://1.1.1.1/dns-query --hosts-files="%str%/hosts"