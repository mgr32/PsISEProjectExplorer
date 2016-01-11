## Powershell ISE Addon - Project Explorer

<a href="https://github.com/mgrzywa/PsISEProjectExplorer/releases/latest">Download</a>

##### Description

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

##### Screenshots
![ScreenShot](./PsISEProjectExplorer_screen.png?raw=true)
![ScreenShot](./PsISEProjectExplorer_screen_dsl.png?raw=true)

##### Installation

* Automatic - run Install_to_UserModules.bat
* Manual:
 * Ensure all the files are unblocked (properties of the file / General)
 * Copy PSISEProjectExplorer to $env:USERPROFILE\Documents\WindowsPowerShell\Modules.
 * Launch PowerShell ISE.
 * Run 'Import-Module PsISEProjectExplorer'.
 * If you want it to be loaded automatically when ISE starts, add the line above to your ISE profile (see $profile).

##### Usage

When you open a Powershell file in ISE, Project Explorer will automatically set its project root directory to the last parent directory of the opened file where any .ps*1 file resides. 

You can also select the root directory manually (by clicking 'Change' button), which will prevent automatic root directory changes (you can enable it again by enabling 'Auto-update root dir').

##### Why?

Because I work on complex Powershell modules with lots of functions, and navigating between them in ISE is painful. I wasn't able to find an ISE plugin that could search through whole directory structure, without requiring the user to load the files into the ISE first. Also, I was missing 'Go to Definition' and 'Find all references' features from Visual Studio and 'Locate in Solution Explorer' from Resharper.

##### Implementation details

Written in C#, .NET 4.5, WPF using Microsoft Visual Studio 2015.

Uses three background threads:
* One for indexing directory structure and file contents. Indexes are stored in RAM only (not stored on disk), so they need full refresh after closing Powershell ISE.
* Second one for searching the indexes.
* Third one for listening on file system changes (checking for accumulated changes each 100 ms).

Uses a configuration file PsISEProjectExplorer.config stored next to PsISEProjectExplorer.dll ($env:LOCALAPPDATA\PsISEProjectExplorer). It contains last state of the UI, plus following entries which can be modified manually:
* `<add key="MaxNumOfWorkspaceDirectories" value="5" />` - maximum number of remembered workspace directories.
* `<add key="DslAutoDiscovery" value="True" />` - whether automatic recognition of DSL elements (like `Describe` or `It` should be enabled (lines starting with constant string and ending with scriptblock).
* `<add key="DslCustomDictionary" value="task,serverrole,serverconnection,step" />` - additional dictionary of DSL elements (useful for ones that does not necessarily end with scriptblock).


To modify keyboard shortcuts, edit PsISEProjectExplorer.psm1 file.

##### How to build

* Open PsISEProjectExplorer.sln.
* Select 'Release' configuration.
* Build Solution (F7).

It will create output in bin\Release directory.

##### How to debug

* Ensure you don't have PsISEProjectExplorer module in your profile directory (see Debug_LoadToIse.bat).
* Open PsISEProjectExplorer.sln.
* Select 'Debug' configuration.
* Start Debugging (F5). It will run Powershell ISE with Debug_LoadToIse.ps1 opened.
* Run Debug_LoadToIse.ps1 in Powershell ISE (F5). It will start PsISEProjectExplorer in debug mode.

##### Third party libraries
* <a href="https://lucenenet.apache.org">Apache Lucene .Net 3.0.3</a> (<a href="http://www.apache.org/licenses/LICENSE-2.0">Apache License, Version 2.0</a>)
* <a href="https://github.com/apache/lucenenet/tree/master/src/contrib/Regex">Apache Lucene .Net Contrib.Regex</a> (<a href="http://www.apache.org/licenses/LICENSE-2.0">Apache License, Version 2.0</a>)
* <a href="http://nlog-project.org">NLog</a> (<a href="https://github.com/NLog/NLog/blob/master/LICENSE.txt">BSD license</a>)
* <a href="http://www.ookii.org/software/dialogs">Ookii.Dialogs</a> (<a href="PsISEProjectExplorer/UI/Ookii.Dialogs.Wpf/license.txt">License</a>)
* All icons come from <a href="http://www.famfamfam.com/lab/icons/silk">Silk icons by Mark James</a>
