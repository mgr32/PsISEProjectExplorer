$curDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
Add-Type -Path "$curDir\bin\Debug\PsISEProjectExplorer.dll"
$psISE.CurrentPowerShellTab.VerticalAddOnTools.Add('Project Explorer', [PsISEProjectExplorer.ProjectExplorerWindow], $true)