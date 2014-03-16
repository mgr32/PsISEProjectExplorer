## Powershell ISE Addon - Project Explorer

##### Description

Enables exploring and full-text searching whole directory structure containing Powershell scripts. It visualizes directory structure in a Solution Explorer-like tree view, but also including Powershell functions in leaf levels (thus can also be used as Function Explorer).

Project not released yet.

##### Implementation details

Written in C#, .NET 4.5, WPF using Microsoft Visual Studio Express 2012 for Desktop.

Uses two background threads:
* One for indexing directory structure and file contents. Indexes are stored in RAM only (not stored on disk), so they need full refresh after closing Powershell ISE.
* Second one for searching the indexes.

##### Third party libraries
Uses Apache Lucene .Net 3.0.3 - https://lucenenet.apache.org/ 

Uses icons created by Mark James - Silk icon set 1.3 - http://www.famfamfam.com/lab/icons/silk/

