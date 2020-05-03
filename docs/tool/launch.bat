@echo off

REM VRChat "alternative launch.bat" with VR or Desktop mode selection feature
REM
REM Ordinary launch.bat is installed by VRChat.
REM It is used to launch VRChat client from Web browser to get in specific world.
REM This alternative "launch.bat" additionally provides mode selection before starting VRChat.
REM 
REM INSTALL:
REM 1. Open steam client, and show VRChat 
REM 2. On VRChat entry, right click to open context menu ,
REM    Select "Properties" > "LOCAL FILES" tab > "BROWSE LOCAL FILES..." button
REM 3. Move original launch.bat in this folder to other place (or just rename)
REM 4. Copy this file to this folder
REM 5. Modify setting if you want (see bellow)
REM
REM This software is licensed under the MIT License
REM (Contact: https://github.com/naqtn, https://twitter.com/naqtn)


REM ==== BEGIN Settings section ====

REM Waiting time (in seconds) for mode choice
set timeout=10

REM Default choice: 'd' for Desktop mode, 'v' for VR mode
set default=d

REM close window: 't' for close window after VRChat started
set close_window=t

REM verbose mode: 't' for showing more info
set verbose=f

REM 't' for one key mode:
REM   not need Enter key, timeout supported, beep sound when unexpected key typed.
REM others: need Enter key, no timeout, silent.
set one_key_mode=t

REM default profile number:
set profile_no=0

REM Default logging option ('on' or 'off')
set log_sw=off

REM ==== Settings section END ====

setlocal EnableDelayedExpansion

echo Starting VRChat...
if %verbose%==t (
  echo.
  echo dir=%1
  echo world=%2
)

if not %one_key_mode%==t goto :enter_key_mode

echo Type 'd' to launch in Desktop mode , 'v' to in VR mode
echo.
echo Additional options:
echo   '0' to use profile 0,  '1', '2' ... '3' use profile 3
echo   'l' for toggle logging feature

:one_key_mode_loop
echo.
REM  /M "Type d,v,0,1,2,3, or l"
choice /C dv0123l /D %default% /T %timeout%
set mode_choice=!ERRORLEVEL!

if "!mode_choice!"=="3" (
  set profile_no=0
  echo profile "!profile_no!" selected
  goto :one_key_mode_loop
) else if "!mode_choice!"=="4" (
  set profile_no=1
  echo profile "!profile_no!" selected
  goto :one_key_mode_loop
) else if "!mode_choice!"=="5" (
  set profile_no=2
  echo profile "!profile_no!" selected
  goto :one_key_mode_loop
) else if "!mode_choice!"=="6" (
  set profile_no=3
  echo profile "!profile_no!" selected
  goto :one_key_mode_loop
) else if "!mode_choice!"=="7" (
  if "!log_sw!"=="on" (
    set log_sw=off
  ) else (
    set log_sw=on
  )
  echo logging will be "!log_sw!"
  goto :one_key_mode_loop
)

goto :end_user_choice

:enter_key_mode

echo.
echo Select mode:
echo   'd' and Enter for Desktop mode,
echo   'v' and Enter for VR mode, 
echo   just Enter for default '%default%'
echo.
echo Additional option:
echo   '0' to use profile 0,  '1', '2' ... '3' use profile 3
echo   'l' for toggle logging feature

:enter_key_mode_loop
set /p mode_choice=

if "!mode_choice!"=="" (
  set mode_choice=%default%
)
if "!mode_choice!"=="d" (
  set mode_choice=1
) else if "!mode_choice!"=="v" (
  set mode_choice=2
) else if "!mode_choice!"=="0" (
  set profile_no=0
  echo profile "!profile_no!" selected
  goto :enter_key_mode_loop
) else if "!mode_choice!"=="1" (
  set profile_no=1
  echo profile "!profile_no!" selected
  goto :enter_key_mode_loop
) else if "!mode_choice!"=="2" (
  set profile_no=2
  echo profile "!profile_no!" selected
  goto :enter_key_mode_loop
) else if "!mode_choice!"=="3" (
  set profile_no=3
  echo profile "!profile_no!" selected
  goto :enter_key_mode_loop
) else if "!mode_choice!"=="l" (
  if "!log_sw!"=="on" (
    set log_sw=off
  ) else (
    set log_sw=on
  )
  echo logging will be "!log_sw!"
  goto :enter_key_mode_loop
) else if "!mode_choice!"=="?" (
  goto :enter_key_mode
) else (
  echo invalid input
  goto :enter_key_mode_loop
)

:end_user_choice

if %verbose%==t (
  echo.
  echo Selected options:
  echo   mode_choice=%mode_choice%
  echo   profile_no="%profile_no%"
  echo   log_sw=%log_sw%
)

set opt_start=
if %close_window%==t (
  set opt_start=start
)

echo.
if "%mode_choice%"=="1" (
  echo Starting in Desktop mode
  set opt_no_vr=--no-vr
) else if "%mode_choice%"=="2" (
  echo Starting in VR mode
  set opt_no_vr=
) else (
  echo launch.bat error.
  pause
  endlocal
  exit
)


set opt_log=
if "%log_sw%"=="on" (
  set opt_log=--enable-debug-gui --enable-sdk-log-levels --enable-udon-debug-logging
)

if not "%~1"=="" (
  cd /d %1
)
%opt_start% VRChat.exe %opt_no_vr% %opt_log% --profile=%profile_no% %2

endlocal
