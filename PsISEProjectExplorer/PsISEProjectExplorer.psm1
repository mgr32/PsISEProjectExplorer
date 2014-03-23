function Register-PsISEProjectExplorer() {
	Add-Type -Path (Join-Path $PSScriptRoot 'PsISEProjectExplorer.dll')
	$psISE.CurrentPowerShellTab.VerticalAddOnTools.Add('Project Explorer', [PsISEProjectExplorer.ProjectExplorerWindow], $true) | Out-Null
}

function Register-PsISEProjectExplorerMenus() {
	Register-PsISEProjectExplorerMenu -name "Go To Definition" -scriptblock { (Get-PSISEProjectExplorerControlHandle).GoToDefinition() } -hotkey "F12"
	Register-PsISEProjectExplorerMenu -name "Find All Occurrences" -scriptblock { (Get-PSISEProjectExplorerControlHandle).FindAllOccurrences() } -hotkey "SHIFT+F12"
	Register-PsISEProjectExplorerMenu -name "Locate Current File" -scriptblock { (Get-PSISEProjectExplorerControlHandle).LocateFileInTree() } -hotkey "ALT+SHIFT+L"
}

function Get-PSISEProjectExplorerControlHandle() {
	return ($psISE.CurrentPowerShellTab.VerticalAddOnTools | where { $_.Name -eq 'Project Explorer' }).Control
}

function Register-PsISEProjectExplorerMenu($name, $scriptblock, $hotkey) {
	$submenus = $psISE.CurrentPowershellTab.AddOnsMenu.Submenus
	$submenu = ($submenus | where { $_.DisplayName -eq $name })
	if (!$submenu) {
		$submenus.Add($name, $scriptblock, $hotkey)
	}
}


if ($host.Name -ne 'Windows PowerShell ISE Host') {
	Write-Warning "PsISEProjectExplorer module only runs inside PowerShell ISE"
	return
}

Register-PsISEProjectExplorer
Register-PsISEProjectExplorerMenus