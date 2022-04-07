#RequireAdmin


Func _GetHwndFromPID($PID)
	local $hWnd = 0
	$stPID = DllStructCreate("int")
	Do
		$winlist2 = WinList()
		For $i = 1 To $winlist2[0][0]
			If $winlist2[$i][0] <> "" Then
				DllCall("user32.dll", "int", "GetWindowThreadProcessId", "hwnd", $winlist2[$i][1], "ptr", DllStructGetPtr($stPID))
				If DllStructGetData($stPID, 1) = $PID Then
					$hWnd = $winlist2[$i][1]
					ExitLoop
				EndIf
			EndIf
		Next
		Sleep(100)
	Until $hWnd <> 0
	Return $hWnd
EndFunc   ;==>_GetHwndFromPID

Local $HWnd = _GetHwndFromPID(32980)
Local $p = WinGetPos($hWnd)
WinMove($HWnd, Default, $p[0], $p[1], 1100,800)