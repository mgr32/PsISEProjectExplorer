function Register-PsISEProjectExplorer() {
	Add-Type -Path (Join-Path $PSScriptRoot 'PsISEProjectExplorer.dll')
	$psISE.CurrentPowerShellTab.VerticalAddOnTools.Add('Project Explorer', [PsISEProjectExplorer.ProjectExplorerWindow], $true) | Out-Null
}

function Register-PsISEProjectExplorerMenus() {
	$root = Register-PsISEProjectExplorerMenuRoot
	Register-PsISEProjectExplorerMenu -root $root -name "Go To Definition" -scriptblock { (Get-PSISEProjectExplorerControlHandle).GoToDefinition() } -hotkey "F12"
	Register-PsISEProjectExplorerMenu -root $root -name "Find All Occurrences" -scriptblock { (Get-PSISEProjectExplorerControlHandle).FindAllOccurrences() } -hotkey "SHIFT+F12"
	Register-PsISEProjectExplorerMenu -root $root -name "Locate Current File" -scriptblock { (Get-PSISEProjectExplorerControlHandle).LocateFileInTree() } -hotkey "ALT+SHIFT+L"
	Register-PsISEProjectExplorerMenu -root $root -name "Find In Files" -scriptblock { (Get-PSISEProjectExplorerControlHandle).FindInFiles() } -hotkey "CTRL+SHIFT+F"
}

function Get-PSISEProjectExplorerControlHandle() {
	return ($psISE.CurrentPowerShellTab.VerticalAddOnTools | where { $_.Name -eq 'Project Explorer' }).Control
}

function Register-PsISEProjectExplorerMenuRoot() {
	return $psISE.CurrentPowershellTab.AddOnsMenu.Submenus.Add("PsISEProjectExplorer", $null, $null);
}
function Register-PsISEProjectExplorerMenu($root, $name, $scriptblock, $hotkey) {
	$submenus = $root.Submenus
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