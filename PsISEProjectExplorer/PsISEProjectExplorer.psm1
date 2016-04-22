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
	Register-PsISEProjectExplorerMenu -name 'Go To Definition' -scriptblock { (Get-PsISEProjectExplorerControlHandle).GoToDefinition() } -hotkey "F12"
	Register-PsISEProjectExplorerMenu -name 'Find All Occurrences' -scriptblock { (Get-PsISEProjectExplorerControlHandle).FindAllOccurrences() } -hotkey "SHIFT+F12"
	Register-PsISEProjectExplorerMenu -name 'Locate Current File' -scriptblock { (Get-PsISEProjectExplorerControlHandle).LocateFileInTree() } -hotkey "ALT+SHIFT+L"
	Register-PsISEProjectExplorerMenu -name 'Find In Files' -scriptblock { (Get-PsISEProjectExplorerControlHandle).FindInFiles() } -hotkey "CTRL+SHIFT+F"
	Register-PsISEProjectExplorerMenu -name 'Close All But This' -scriptblock { (Get-PsISEProjectExplorerControlHandle).CloseAllButThis() } -hotkey "CTRL+ALT+W"
}

function Get-PsISEProjectExplorerControlHandle() {
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

# this is for backward compatibility, to be removed in future
function Copy-PsISEOldConfigFile {
	$OldFile = Join-Path -Path $PSScriptRoot -ChildPath 'PsISEProjectExplorer.config'
	$NewPath = Join-Path -Path $Env:LOCALAPPDATA -ChildPath 'PsISEProjectExplorer'
	$NewFile = Join-Path -Path $Env:LOCALAPPDATA -ChildPath 'PsISEProjectExplorer\PsISEProjectExplorer.config'

	if ((Test-Path -Path $OldFile) -and -not (Test-Path -Path $NewFile))
	{
		New-Item -Path $NewPath -ItemType Directory -Force | Out-Null
		Copy-Item -Path $OldFile -Destination $NewFile
	}
}

function Add-PsISEProjectExplorerToIseProfile {
	$docDir = [Environment]::GetFolderPath("mydocuments")
	$profileFile = "$docDir\WindowsPowerShell\Microsoft.PowerShellISE_profile.ps1"

	if (!(Test-Path $profileFile)) {
		Write-Host -Object "Creating file '$profileFile'..." -NoNewline
		[void](New-Item -Path $profileFile -ItemType File)
		Write-Host -Object 'OK'
		$content = ''
	} else {
		Write-Host "Reading file '$profileFile'..." -NoNewLine
		$contents = Get-Content -Path $profileFile | Out-String
        Write-Host -Object 'OK'
	}

	$importModule = "Import-Module PsISEProjectExplorer"

	if ($contents -inotmatch $importModule) {
		Write-Host "Adding '$importModule'..." -NoNewLine
		Add-Content -Path $profileFile -Value $importModule | Out-Null
        Write-Host 'OK'
	} else {
		Write-Host 'Import command for PsISEProjectExplorer already exists in profile file.'
	}
}


if ($host.Name -ne 'Windows PowerShell ISE Host') {
	Write-Warning "PsISEProjectExplorer module only runs inside PowerShell ISE"
	return
}

if ($PSVersionTable.PSVersion.Major -lt 3) {
	Write-Warning "PsISEProjectExplorer requires Powershell 3.0 or above"
	return
}

Copy-PsISEOldConfigFile

Register-PsISEProjectExplorer
Register-PsISEProjectExplorerMenus