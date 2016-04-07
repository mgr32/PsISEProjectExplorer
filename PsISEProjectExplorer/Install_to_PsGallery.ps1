$Global:ErrorActionPreference = 'Stop'

$publishParams = @{
    Path = "$PSScriptRoot\.\bin\Release\PsISEProjectExplorer"
    NuGetApiKey = 'private' 
}

Publish-Module @publishParams