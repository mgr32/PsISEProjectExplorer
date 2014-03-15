$curDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
Add-Type -Path "$curDir\bin\Debug\ProjectExplorer.dll"
$psISE.CurrentPowerShellTab.VerticalAddOnTools.Add('Project Explorer', [ProjectExplorer.ProjectExplorerWindow], $true)