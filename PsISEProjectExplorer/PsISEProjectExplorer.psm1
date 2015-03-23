function Register-PsISEProjectExplorer() {
	Add-Type -Path (Join-Path $PSScriptRoot 'PsISEProjectExplorer.dll')

    $addOnName = 'Project Explorer'
    $exists = $psISE.CurrentPowerShellTab.VerticalAddOnTools | where { $_.Name -eq $addOnName }
    if ($exists) {
        $psISE.CurrentPowerShellTab.VerticalAddOnTools.Remove($exists)
    }
	$psISE.CurrentPowerShellTab.VerticalAddOnTools.Add($addOnName, [PsISEProjectExplorer.ProjectExplorerWindow], $true) | Out-Null
}

function Register-PsISEProjectExplorerMenus() {
	$root = Register-PsISEProjectExplorerMenuRoot
    if (!$root) {
        return
    }
	Register-PsISEProjectExplorerMenu -name 'Go To Definition' -scriptblock { (Get-PSISEProjectExplorerControlHandle).GoToDefinition() } -hotkey "F12"
	Register-PsISEProjectExplorerMenu -name 'Find All Occurrences' -scriptblock { (Get-PSISEProjectExplorerControlHandle).FindAllOccurrences() } -hotkey "SHIFT+F12"
	Register-PsISEProjectExplorerMenu -name 'Locate Current File' -scriptblock { (Get-PSISEProjectExplorerControlHandle).LocateFileInTree() } -hotkey "ALT+SHIFT+L"
	Register-PsISEProjectExplorerMenu -name 'Find In Files' -scriptblock { (Get-PSISEProjectExplorerControlHandle).FindInFiles() } -hotkey "CTRL+SHIFT+F"
	Register-PsISEProjectExplorerMenu -name 'Close All But This' -scriptblock { (Get-PSISEProjectExplorerControlHandle).CloseAllButThis() } -hotkey "CTRL+ALT+W"
}

function Get-PSISEProjectExplorerControlHandle() {
	return ($psISE.CurrentPowerShellTab.VerticalAddOnTools | where { $_.Name -eq 'Project Explorer' }).Control
}

function Find-PsISEProjectExplorerMenuRoot() {
    $menuName = 'PsISEProjectExplorer'
    $submenus = $psISE.CurrentPowershellTab.AddOnsMenu.SubMenus
    return $submenus | where { $_.DisplayName -eq $menuName }
}

function Register-PsISEProjectExplorerMenuRoot() {
    $exists = Find-PsISEProjectExplorerMenuRoot
    if ($exists) {
        $psISE.CurrentPowershellTab.AddOnsMenu.SubMenus.Remove($exists)
    }
	return $psISE.CurrentPowershellTab.AddOnsMenu.SubMenus.Add('PsISEProjectExplorer', $null, $null);
}

function Register-PsISEProjectExplorerMenu($root, $name, $scriptblock, $hotkey) {
    $root = Find-PsISEProjectExplorerMenuRoot
	$root.SubMenus.Add($name, $scriptblock, $hotkey)
}

if ($host.Name -ne 'Windows PowerShell ISE Host') {
	Write-Warning "PsISEProjectExplorer module only runs inside PowerShell ISE"
	return
}

if ($PSVersionTable.PSVersion.Major -lt 3) {
	Write-Warning "PsISEProjectExplorer requires Powershell 3.0 or above"
	return
}

Register-PsISEProjectExplorer
Register-PsISEProjectExplorerMenus