if ($host.Name -ne "Windows PowerShell ISE Host")
{
    Write-Warning "PsISEProjectExplorer module only runs inside PowerShell ISE"
    return
}

Add-Type -Path (Join-Path $PSScriptRoot "PsISEProjectExplorer.dll")
$psISE.CurrentPowerShellTab.VerticalAddOnTools.Add('Project Explorer', [PsISEProjectExplorer.ProjectExplorerWindow], $true) | Out-Null