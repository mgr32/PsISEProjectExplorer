## Powershell ISE Addon - Project Explorer

<a href="https://github.com/mgrzywa/PsISEProjectExplorer/releases/latest">Download</a>

##### Description

Provides a tree view that enables to index and explore whole directory structure containing Powershell scripts. It has following features:

* Visualize directory structure (also files not loaded to ISE yet) in a Solution Explorer-like tree view.
* Show functions in leafs of the tree view and jump to the function definition (F12, similarly to some available Function Explorer plugins).
* Search the tree view (file names, function names, optionally file contents).
* File operations in tree view (context menu - add / rename / delete, drag&drop).
* Find all occurrences of the text under the cursor (SHIFT+F12).
* Locate current file in the tree view (ALT+SHIFT+L).
* Automatic reindex on file system change.

Requires Powershell 3.0 or above.

If you find it useful, see any bugs or have any suggestions for improvements feel free to add an issue or comment at the <b><a href="http://mgr32.github.io/PsISEProjectExplorer/">project home page</a></b>.

##### Screenshots
![ScreenShot](./PsISEExplorer_screen.png?raw=true)

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

You can also select the root directory manually (by clicking 'Change' button) and ensure it doesn't update automatically (by unselecting 'Auto-update root dir' checkbox).

##### Why?

Because I work on complex Powershell modules with lots of functions, and navigating between them in ISE is painful. I wasn't able to find an ISE plugin that could search through whole directory structure, without requiring the user to load the files into the ISE first. Also, I was missing 'Go to Definition' and 'Find all references' features from Visual Studio and 'Locate in Solution Explorer' from Resharper.

##### Implementation details

Written in C#, .NET 4.5, WPF using Microsoft Visual Studio Express 2012 for Desktop.

Uses three background threads:
* One for indexing directory structure and file contents. Indexes are stored in RAM only (not stored on disk), so they need full refresh after closing Powershell ISE.
* Second one for searching the indexes.
* Third one for listening on file system changes (checking for accumulated changes each 100 ms).

Uses a configuration file PsISEProjectExplorer.config stored next to PsISEProjectExplorer.dll ($env:USERPROFILE\Documents\WindowsPowerShell\Modules\PsISEProjectExplorer). Currently it contains only the last state of the UI.

##### Third party libraries
* <a href="https://lucenenet.apache.org">Apache Lucene .Net 3.0.3</a> (<a href="http://www.apache.org/licenses/LICENSE-2.0">Apache License, Version 2.0</a>)
* <a href="http://nlog-project.org">NLog 2.1</a> (<a href="https://github.com/NLog/NLog/blob/master/LICENSE.txt">BSD license</a>)
* <a href="http://www.ookii.org/software/dialogs">Ookii.Dialogs</a> (<a href="PsISEProjectExplorer/UI/Ookii.Dialogs.Wpf/license.txt">License</a>)
* All icons come from <a href="http://www.famfamfam.com/lab/icons/silk">Silk icons by Mark James</a>
