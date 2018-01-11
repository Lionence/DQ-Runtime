# DQ-Runtime v0.1.0 (BETA)

This is an open-source project  for developers from DevQuarter developers.
Please visit our website: https://www.devquarter.com

This is a Module System based on .NET Reflection.

The idea is based on changing software components and updating frameworks quicker than before. Because of automatic module detection and updates this system does not requires many user interactions. You can easily develop your own module and use it just by 'dropping' it to a specified folder.

Features:
  - ModuleLoader implementations (methods like Loading, Reloading, Unloading etc.)
  - ModuleTemplate interface (a template for your to develop yourown modules)
  - ModuleWatcher events implemented: derived from FileSystemWatcher and extended with new functions.
  - Example module: DevQuarter Extension Module
  - Example code (ModuleHandler.cs): an example of how could be the system used
 
 Planned developments for next release (v0.2.0):
  - ModuleTemplate interface improvement: Interaction and communication layout for modules
  - Editable configuration file for switching on/off some of the features
  - Logging which could be turned on/off
  - User Interface (console): prompt feature
