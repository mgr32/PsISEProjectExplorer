## Powershell ISE Addon - Project Explorer

##### Description

Project not released yet - you can compile it manually or wait for first official release.

Provides a tree view that enables to index and explore whole directory structure containing Powershell scripts. It has following features:

* Visualize directory structure (also files not loaded to ISE yet) in a Solution Explorer-like tree view.
* Show functions in leafs of the tree view and jump to the function definition (F12, similarly to some available Function Explorer plugins).
* Search the tree view (file names, function names, optionally file contents).
* Find all occurrences of the text under the cursor (SHIFT+F12).
* Locate current file in the tree view (ALT+SHIFT+L).
* Automatic reindex on file system change.

##### Screenshots
![Image](./PsISEExplorer_screen1.png?raw=true)

##### Installation

* Run Install_to_UserModules.bat or copy PSISEProjectExplorer folder manually to $env:USERPROFILE\Documents\WindowsPowerShell\Modules.
* Launch PowerShell ISE.
* Run 'Import-Module PsISEProjectExplorer'.
* If you want it to be loaded automatically, add the line above to your ISE profile (see $profile).

##### Usage

When you open a Powershell file in ISE, Project Explorer will automatically set its project root directory to the first parent directory of the opened file where a .psm1 file resides. If there's no .psm1 file in any parent directory, it will take the last parent containing .ps1 files.

You can also select the root directory manually (by clicking 'Change' button) and ensure it doesn't update automatically (by selecting 'Freeze root dir' checkbox).

##### Why?

Because I work on complex Powershell modules with lots of functions, and navigating between them in ISE is painful. I wasn't able to find an ISE plugin that could search through whole directory structure, without requiring the user to load the files into the ISE first. Also, I was missing 'Go to Definition' and 'Find all references' features from Visual Studio and 'Locate in Solution Explorer' from Resharper.

##### Implementation details

Written in C#, .NET 4.5, WPF using Microsoft Visual Studio Express 2012 for Desktop.

Uses two background threads:
* One for indexing directory structure and file contents. Indexes are stored in RAM only (not stored on disk), so they need full refresh after closing Powershell ISE.
* Second one for searching the indexes.

##### Third party libraries
* Apache Lucene .Net 3.0.3 - https://lucenenet.apache.org/ 
* NLog 2.1 - http://nlog-project.org/
* Ookii.Dialogs - http://www.ookii.org/software/dialogs/
* Silk icon set 1.3 by Mark James - http://www.famfamfam.com/lab/icons/silk/
