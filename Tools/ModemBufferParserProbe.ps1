$ErrorActionPreference = 'Stop'
$assemblyPath = Join-Path (Get-Location) 'bin\Debug\GSM.Core.dll'
if (-not (Test-Path $assemblyPath)) { throw "Assembly not found: $assemblyPath" }
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$parserType = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath)).GetType('GSM.Core.Source.StringAnalyzer.StringAnalyzer+ModemBufferParser', $true)
$parseMethod = $parserType.GetMethod('AppendAndParse')

function New-Parser {
	return [Activator]::CreateInstance($parserType)
}

function Invoke-Parse($Parser, $Data) {
	$arguments = [object[]]@($Data, $null)
	$accepted = [bool]$parseMethod.Invoke($Parser, $arguments)
	$names = @()
	if ($null -ne $arguments[1]) {
		foreach ($action in $arguments[1]) { $names += $action.ToString() }
	}
	return [pscustomobject]@{ Accepted = $accepted; Actions = [string[]]$names }
}

$script:passed = 0
$script:failed = 0
function Assert-Result($Name, $Result, [bool]$ExpectedAccepted, [string]$ExpectedActions) {
	$actualActions = [string]::Join(',', [string[]]$Result.Actions)
	if ($Result.Accepted -ne $ExpectedAccepted -or $actualActions -cne $ExpectedActions) {
		$script:failed++
		Write-Output "FAIL $Name : accepted=$($Result.Accepted); actions=[$actualActions]; expected accepted=$ExpectedAccepted; actions=[$ExpectedActions]"
	}
	else {
		$script:passed++
		Write-Output "PASS $Name : accepted=$($Result.Accepted); actions=[$actualActions]"
	}
}

$p = New-Parser
Assert-Result 'null buffer' (Invoke-Parse $p $null) $true 'ACTION_NULL'
Assert-Result 'empty buffer' (Invoke-Parse $p '') $true 'ACTION_NULL'
Assert-Result 'whitespace buffer' (Invoke-Parse $p " `t`r`n") $true 'ACTION_NULL'
Assert-Result 'control-only buffer' (Invoke-Parse $p ([string][char]0x12)) $true 'ACTION_NULL'

$p = New-Parser
Assert-Result 'partial CSQ first chunk' (Invoke-Parse $p '+CSQ: 25,') $false ''
Assert-Result 'empty chunk preserves partial CSQ' (Invoke-Parse $p '') $true 'ACTION_NULL'
Assert-Result 'whitespace chunk preserves partial CSQ' (Invoke-Parse $p '  ') $true 'ACTION_NULL'
Assert-Result 'CSQ completed by later chunk' (Invoke-Parse $p "0`r`n") $true 'SignalStrengthResult'

$p = New-Parser
Assert-Result 'startup and ready' (Invoke-Parse $p "MODEM:STARTUP`r`n+PBREADY`r`n") $true 'ModemStartup,ModemReady'
Assert-Result 'control-wrapped OK, CSQ, and OK' (Invoke-Parse $p "`u0010OK`u0006`r`nAT+CSQ`r`n+CSQ: 25,0`r`nOK`r`n") $true 'Ok,SignalStrengthResult,Ok'

$p = New-Parser
Assert-Result 'CMGS first prompt' (Invoke-Parse $p "AT+CMGS=30`r`n> ") $true 'SmsSendPrompt'
Assert-Result 'CMGS duplicate prompt suppressed' (Invoke-Parse $p "`u0012OK`r`n06918919015000B1000C9189194519210000080B10062A0633`r`n062A00200628062706410631`u001A`r`n> ") $true 'Ok'
Assert-Result 'CMGS final result' (Invoke-Parse $p "+CMGS: 191`r`nOK`r`n") $true 'SmsSendSucceeded,Ok'

$p = New-Parser
$pdu = '06918919015000068A0B819020200806F7424042515363414240425153834100'
Assert-Result 'CMGR sample PDU and OK' (Invoke-Parse $p "AT+CMGR=1`r`n+CMGR: 1,,25`r`n$pdu`r`nOK`r`n") $true 'SmsReadDeliveryReport,Ok'

$p = New-Parser
Assert-Result 'truncated PDU reproduction' (Invoke-Parse $p "+CMGR: 0,,1`r`n0002`r`n") $true 'SmsReadDeliveryReport'

$p = New-Parser
Assert-Result 'CMGD error' (Invoke-Parse $p "AT+CMGD=1,255`r`nERROR`r`n") $true 'AtError'
$p = New-Parser
Assert-Result 'CMGD delete all' (Invoke-Parse $p "AT+CMGD=1,4`r`nOK`r`n") $true 'Ok,SmsDeleteAllAccepted'

Write-Output "SUMMARY passed=$script:passed failed=$script:failed"
if ($script:failed -gt 0) { exit 1 }
