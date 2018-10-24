' Based on https://stackoverflow.com/questions/20498004/how-to-save-sapi-text-to-speech-to-an-audio-file-in-vbscript

' cscript make-voice.vbs

'-----
' https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms720595%28v%3dvs.85%29
Const SAFT48kHz16BitStereo = 39
Const SAFT44kHz16BitMono = 34
Const SSFMCreateForWrite = 3 ' Creates file even if file exists and so destroys or overwrites the existing file

Dim oFileStream, oVoice

Set oFileStream = CreateObject("SAPI.SpFileStream")
oFileStream.Format.Type = SAFT44kHz16BitMono
oFileStream.Open "output.wav", SSFMCreateForWrite

Set oVoice = CreateObject("SAPI.SpVoice")
Set oVoice.AudioOutputStream = oFileStream


'oVoice.Speak oVoice.Voice.GetDescription
' Speak method additional flags
' https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ms720892%28v%3dvs.85%29


Set oVoice.Voice = oVoice.GetVoices.Item(3)
oVoice.Speak "zero"
Set oVoice.Voice = oVoice.GetVoices.Item(0)
oVoice.Speak "ÇOÅB"
'ÇOÇPÇQÇRÇSÇTÇUÇVÇWÇX


oVoice.WaitUntilDone(10000)

oFileStream.Close
