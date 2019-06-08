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

REM ==== Settings section END ====

setlocal EnableDelayedExpansion

echo Starting VRChat...
if %verbose%==t (
  echo.
  echo dir=%1
  echo world=%2
)

if %one_key_mode%==t (
  echo.
  choice /C dv /D %default% /T %timeout% /M "Type 'd' for Desktop mode , 'v' for VR mode "
  set mode_choice=!ERRORLEVEL!

) else (
  echo Select mode:
  echo   'd' and Enter for Desktop mode,
  echo   'v' and Enter for VR mode, 
  echo   just Enter for default '%default%'
  set /p mode_choice=

  if not "!mode_choice!"=="d" if not "!mode_choice!"=="v" (
    set mode_choice=%default%
  )

  if "!mode_choice!"=="d" (
    set mode_choice=1
  ) else if "!mode_choice!"=="v" (
    set mode_choice=2
  )
)

if %verbose%==t (
  echo.
  echo mode_choice=%mode_choice%
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
  endlocal
  exit
)

cd /d %1
%opt_start% VRChat.exe %opt_no_vr% %2

endlocal
