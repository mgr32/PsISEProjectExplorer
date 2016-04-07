## Powershell ISE Addon - Project Explorer 1.3.1

[PsGallery](https://www.powershellgallery.com/packages/PsISEProjectExplorer) or [Direct download](https://github.com/mgr32/PsISEProjectExplorer/releases/latest)

[What's new](https://github.com/mgr32/PsISEProjectExplorer/wiki/What's-new)

#### Description

Provides a tree view that enables to index and explore whole directory structure containing Powershell scripts. It has following features:

* Visualize directory structure (also files not loaded to ISE yet) in a Solution Explorer-like tree view.
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

#### Why?

Because I used to work on complex Powershell modules with lots of functions (like [PSCI](https://github.com/ObjectivityBSS/PSCI)), and navigating between them in ISE was painful. I wasn't able to find an ISE plugin that could search through whole directory structure, without requiring the user to load the files into the ISE first. Also, I was missing 'Go to Definition' and 'Find all references' features from Visual Studio and 'Locate in Solution Explorer' from Resharper.

#### Configuration file

PsISEProjectExplorer uses a configuration file `PsISEProjectExplorer.config` stored next to `PsISEProjectExplorer.dll` (`$env:LOCALAPPDATA\PsISEProjectExplorer`). It contains last state of the UI, plus following entries which can be modified manually:
* `<add key="MaxNumOfWorkspaceDirectories" value="5" />` - maximum number of remembered workspace directories.
* `<add key="DslAutoDiscovery" value="True" />` - whether automatic recognition of DSL elements (like `Describe` or `It` should be enabled (lines starting with constant string and ending with scriptblock).
* `<add key="DslCustomDictionary" value="task,serverrole,serverconnection,step" />` - additional dictionary of DSL elements (useful for ones that does not necessarily end with scriptblock).

To modify keyboard shortcuts, you need to edit PsISEProjectExplorer.psm1 file.

#### Links

[How to build](https://github.com/mgr32/PsISEProjectExplorer/wiki/How-to-build)

[Third party libraries](https://github.com/mgr32/PsISEProjectExplorer/wiki/Third-party-libraries)
