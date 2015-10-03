![alt text][logo]
==========

[![Join the chat at https://gitter.im/nbasakuragi/LINQBridgeVs](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/nbasakuragi/LINQBridgeVs?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Build status](https://ci.appveyor.com/api/projects/status/ibasbqbycm33plpi?svg=true)](https://ci.appveyor.com/project/nbasakuragi/linqbridgevs)

[![Coverage Status](https://coveralls.io/repos/nbasakuragi/LINQBridgeVs/badge.svg?branch=master&service=github)](https://coveralls.io/github/nbasakuragi/LINQBridgeVs?branch=master)
 
What's LINQBridgeVs?
-----------------

LINQBridgeVs is a Visual Studio Extension compatible with Visual Studio 2010/2012/2013.
It is a bridge between a Visual Studio debugging session and [LINQPad] (http://www.linqpad.net).
When enabled on a selection of VB/C# projects, it creates a Custom Debugger Visualizer 
mapping to all the complex types and making the magnifier glass 
available for all of them. By clicking on the magnifier glass the types will be transmitted over a
bus and read back by LINQPad.

## Getting Started

Once the LINQBridgeVs Extension is installed a new menu will be available in the Visual Studio Menu Bar, only visible
when a solution is loaded and a project selected. There are two options available:

*  Enable LINQBridge
*  Disable LINQBridge

Enabling LINQBridge on a selected project will do the magic and that's it! 

Compile your solution, run it, set a breakpoint and hover the pointer
on any object instance and the magnifier glass will appear. 

No need to mark any of your classes as Serialized.

Clicking the magnifier will export the instance to [LINQPad](http://www.linqpad.net). 

[LINQPad] (http://www.linqpad.net) it is to be installed beforehand.

##Acknowledgements

Special thanks to JetBrains who has been supporting this project with its products.

<a href="http://www.jetbrains.com/resharper/features/code_refactoring.html" alt="The best C# & VB.NET refactoring plugin for Visual Studio" title="The best C# & VB.NET refactoring plugin for Visual Studio">![alt text][resharper]</a>

## Compatibility

LINQBridgeVs is compatible for Visual Studio 2010/2012/2013. At the moment available **only for .NET Framework 4.0 onwards**


## Known Issues

***Could not load file or assembly LINQBridge.DynamicVisualizer.V11.dll or one of its dependencies.***

*Solution:* Check you have access permission on the folder CurrentUser\Documents\Visual Studio 2012\Visualizers 
for the user running the debugger (IIS_IUSRS if a WebSite). 

*Platform:* Windows 8

***Can't find the magnifying glass***

*Solution:* Make sure you are running as Administrator your Visual Studio instance. 

*Platform:* Any

*Visual Studio:* Any

***Can't find the magnifying glass - Visual Studio 2013***

*Solution:* Please update to the latest version (1.3.7) from the Visual Studio Gallery. Please always remember to run Visual Studio As Administrator.

*Platform:* Any

*Visual Studio:* 2013

[logo]: https://raw.github.com/nbasakuragi/LINQBridgeVs/master/Src/LINQBridgeVs/LinqBridgeVsExtension/Resources/LINQBridgeLogo.png "LINQBridge"

[resharper]: https://raw.github.com/nbasakuragi/LINQBridgeVs/master/Src/LINQBridgeVs/LinqBridgeVsExtension/Resources/resharperLogo.png "Resharper"

