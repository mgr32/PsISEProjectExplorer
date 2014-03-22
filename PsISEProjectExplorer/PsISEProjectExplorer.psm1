function Register-PsISEProjectExplorer() {
	Add-Type -Path (Join-Path $PSScriptRoot 'PsISEProjectExplorer.dll')
	$psISE.CurrentPowerShellTab.VerticalAddOnTools.Add('Project Explorer', [PsISEProjectExplorer.ProjectExplorerWindow], $true) | Out-Null
}

function Register-PsISEProjectExplorerMenus() {
	$psISE.CurrentPowershellTab.AddOnsMenu.Submenus.Add("Go To Definition", { (Get-PSISEProjectExplorerControlHandle).GoToDefinition() }, "F12")
	$psISE.CurrentPowershellTab.AddOnsMenu.Submenus.Add("Find All Occurrences", { (Get-PSISEProjectExplorerControlHandle).FindAllOccurrences() }, "SHIFT+F12")
	$psISE.CurrentPowershellTab.AddOnsMenu.Submenus.Add("Locate Current File", { (Get-PSISEProjectExplorerControlHandle).LocateFileInTree() }, "SHIFT+ALT+L")
}

function Get-PSISEProjectExplorerControlHandle() {
	return ($psISE.CurrentPowerShellTab.VerticalAddOnTools | where { $_.Name -eq 'Project Explorer' }).Control
}


if ($host.Name -ne 'Windows PowerShell ISE Host') {
	Write-Warning "PsISEProjectExplorer module only runs inside PowerShell ISE"
	return
}

Register-PsISEProjectExplorer
Register-PsISEProjectExplorerMenus