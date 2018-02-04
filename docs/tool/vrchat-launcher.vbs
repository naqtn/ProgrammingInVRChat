' VRChat client launcher
'
' * Wrapper for local test
'   *  (When staring from VRChat SDK integrated in Unity)
'   * Test with non VR mode
' * Launch menu
'   *  (When staring from Windows Explorer.  ie. with no argument)
'   * You can select where to go before starting VRChat client.
'
' This software is licensed under the MIT License
' Contact: https://github.com/naqtn, https://twitter.com/naqtn

'========================================
' Configure:

'----------
' VRChat installation folder if you changed from default.
vrchatInstallFolder = "C:\Program Files\Steam (x86)\steamapps\common\VRChat"
' example: I set steam library folder as "D:\SteamLibrary" , then
' vrchatInstallFolder = "D:\SteamLibrary\steamapps\common\VRChat"

'----------
' Confirm mode
'  true: shows confirm step before starting VRChat when used from VRChat SDK
confirmMode = false

'----------
' Default use of VR
'  false: start VRChat in desktop mode
startWithVR = false

'----------
' World selection menu items
' Shows if invoke directrly from Windows Explorer
worldsInfo = Array( _
  "Help Videos", "wrld_48cf80e6-15dd-4c17-8667-c5dc01baa5cb", _
  "Avatar Testing!", "wrld_8ef393c0-a985-4d7e-90f0-33ab10d41ee3", _
  "The HUB", "wrld_eb7a5096-9c93-41db-a9d7-7b349a5d4815", _
  "The Great Pug", "wrld_6caf5200-70e1-46c2-b043-e3c4abe69e0f", _
  "--ENDMARK--", "wrld_00000000-0000-0000-0000-000000000000")


'========================================
' program: 

Function StartVRChat(vrmode, args)
  Set objShell = CreateObject("WScript.Shell")
  objShell.CurrentDirectory = vrchatInstallFolder

  cmdline = "VRChat.exe"
  If Not vrmode Then
    cmdline = cmdline & " --no-vr"
  End If
  cmdline = cmdline & " " & args
  
  ' WScript.Echo "cmdline=", cmdline
  Set objExec = objShell.Exec(cmdline)
End Function


Function RecombineArtuments()
  RecombineArtuments = ""
  For i = 0 to WScript.Arguments.Count - 1
    RecombineArtuments = RecombineArtuments & " """ & WScript.Arguments(i) & """"
  Next
End Function


Function IsRunningWithWScript()
  Set reg = new regexp 
  reg.Pattern = "wscript\.exe$"
  reg.IgnoreCase = true
  IsRunningWithWScript = reg.Test(WScript.FullName)
End Function


Function BooleanToStr(a)
  If a Then
    BooleanToStr = "true"
  Else
    BooleanToStr = "false"
  End If
End Function



Set objShell = CreateObject("WScript.Shell")

If WScript.Arguments.Count = 0 Then
  If IsRunningWithWScript() Then
    ' re-run myself with CScript for getting console
    cmdline = "CScript """ & WScript.ScriptFullName & """ //Nologo"
    objShell.Run(cmdline)
    WScript.Quit(0)

  Else
    ' Selection menu mode
    WScript.Echo "[VRChat launcher (menu)]"

    WScript.Echo "startWithVR:", BooleanToStr(startWithVR)

    WScript.Echo ""
    WScript.Echo "Worlds:"
    i = 0
    Do While i <= Ubound(worldsInfo)-2
      WScript.Echo " ", (i/2+1), ") ", worldsInfo(i)
      i = i + 2
    Loop
    WScript.Echo "Operation:"
    WScript.Echo "  0 ) toggle VR Mode"
    
    Set regNum = new regexp 
    regNum.Pattern = "^0|[1-9][0-9]*$"

    Do While true
      WScript.Echo "> Select item number:"
      input = WScript.StdIn.ReadLine
      ' WScript.Echo "Select input:", input

      If regNum.Test(input) Then
        idx = (input-1)*2
        If input = 0 Then
          ' Zero to toggle startWithVR
          startWithVR = Not startWithVR
          WScript.Echo "startWithVR:", BooleanToStr(startWithVR)
        ElseIf idx <= Ubound(worldsInfo)-2 Then
          worldid = worldsInfo(idx + 1)
          ' WScript.Echo " world id:", worldid
          StartVRChat startWithVR, "vrchat://launch?id=" & worldid
          WScript.Quit(0)
        End If
      Else
        ' invalid input
      End If
    Loop
    
  End If

Else
  If confirmMode Then
    If IsRunningWithWScript() Then
      ' re-run myself with CScript for getting console
      cmdline = "CScript """ & WScript.ScriptFullName & """ //Nologo" & RecombineArtuments()
      objShell.Run(cmdline)
      WScript.Quit(0)
    Else

      WScript.Echo "[VRChat launcher (confirm)]"
      ' WScript.Echo "  I will start >>>", RecombineArtuments(), "<<<"
      WScript.Echo ""
      WScript.Echo "  0 ) toggle VR Mode"
      WScript.Echo "  1 ) start"

      '---------- copy&paste base :( see also above
      Set regNum = new regexp 
      regNum.Pattern = "^0|[1-9][0-9]*$"

      Do While true
        WScript.Echo "> Select item number:"
        input = WScript.StdIn.ReadLine
        ' WScript.Echo "Select input:", input

        If regNum.Test(input) Then
          If input = 0 Then
            ' Zero to toggle startWithVR
            startWithVR = Not startWithVR
            WScript.Echo "startWithVR:", BooleanToStr(startWithVR)
          ElseIf input = 1 Then
            StartVRChat startWithVR, RecombineArtuments()
            WScript.Quit(0)
          End If
        Else
          ' other input
        End If
      Loop
      '----------
      
    End If
  Else
    StartVRChat startWithVR, RecombineArtuments()
    WScript.Quit(0)
  End If

End If

