## Powershell ISE Addon - Project Explorer 1.3.2

[PsGallery](https://www.powershellgallery.com/packages/PsISEProjectExplorer) or [direct download](https://github.com/mgr32/PsISEProjectExplorer/releases/latest)

[What's new](https://github.com/mgr32/PsISEProjectExplorer/wiki/What's-new)

#### Description

Provides a tree view that enables to index and explore whole directory structure containing Powershell scripts. It has following features:

* Visualize directory structure (also files not loaded to ISE yet) in a tree view.
* Show functions, classes and DSL nodes (e.g. Pester / psake / custom) in leaves of the tree view and jump to the function definition (F12, similarly to some available Function Explorer plugins).
* Search the tree view (file names, function names, optionally file contents) - using full-text search or regex.
* Show parse errors in tree view.
* File operations in tree view (context menu - add / rename / delete, exclude, drag&drop).
* Find all occurrences of the text under the cursor (SHIFT+F12).
* Locate current file in the tree view (ALT+SHIFT+L).
* Close All But This tab (CTRL+ALT+W).
* Automatic reindex on file system change.
* Ask user to reload files on file system change (editor functionality missing in ISE).


Requires Powershell 3.0 or above.

If you find it useful, see any bugs or have any suggestions for improvements feel free to add an <a href="https://github.com/mgr32/PsISEProjectExplorer/issues">issue</a>.

#### Screenshots
![ScreenShot](./PsISEProjectExplorer_screen.png?raw=true)
![ScreenShot](./PsISEProjectExplorer_screen_dsl.png?raw=true)

#### Installation

If you have Powershell 5 or PowerShellGet, run following commands in Powershell ISE:
```
Install-Module PsISEProjectExplorer
Import-Module PsISEProjectExplorer
Add-PsISEProjectExplorerToIseProfile
```

If you don't have PsGet, [download latest package](https://github.com/mgr32/PsISEProjectExplorer/releases/latest) and either:
* Install it automatically - by running `Install_to_UserModules.bat`, or
* Install it manually:
 * Ensure all the files are unblocked (properties of the file / General)
 * Copy PSISEProjectExplorer to `$env:USERPROFILE\Documents\WindowsPowerShell\Modules`.
 * Launch PowerShell ISE.
 * Run `Import-Module PsISEProjectExplorer`.
 * If you want it to be loaded automatically when ISE starts, add the line above to your ISE profile (see `$profile`).

#### Usage

When you open a Powershell file in ISE, Project Explorer will automatically set its project root directory to the last parent directory of the opened file where any .ps*1 file resides. 

You can also select the root directory manually (by clicking 'Change' button), which will prevent automatic root directory changes (you can enable it again by enabling 'Auto-update root dir').

#### Documentation

Please see [wiki](https://github.com/mgr32/PsISEProjectExplorer/wiki).
