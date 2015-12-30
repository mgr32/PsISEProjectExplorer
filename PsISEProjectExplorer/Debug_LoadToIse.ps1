$curDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
Import-Module "$curDir\bin\Debug\PsISEProjectExplorer\PsISEProjectExplorer.psd1" -Force
