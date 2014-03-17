$curDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
Import-Module "$curDir\bin\Debug\PsISEProjectExplorer\PsISEProjectExplorer.psm1" -Force
