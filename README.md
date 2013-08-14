![alt text][logo]
==========
 
What's LINQBridge?
-----------------

LINQBridge is a Visual Studio Extension for Visual Studio 2010 and Visual Studio 2012.
It is a bridge between a Visual Studio debugging session and [LINQPad] (http://www.linqpad.net).
When enabled on a selection of VB/C# projects, it creates a Custom Debugger Visualizer 
mapping to all the complex types and making the magnifier glass 
available for all of them. By clicking on the magnifier glass the types will be transmitted over a
bus and read back by LINQPad.

## Getting Started

Once the LINQBridge Extension is installed a new menu will be available in the Visual Studio Menu Bar, only visible
when a solution is loaded and a project selected. There are two options available:

*  Enable LINQBridge
*  Disable LINQBridge

Enabling LINQBridge on a selected project will do the magic and that's it! 

Compile your solution, run it, set a breakpoint and hover the pointer
on any object instance and the magnifier glass will appear. 

Clicking the magnifier will export the instance to [LINQPad](http://www.linqpad.net). 

No need to manually install LINQPad beforehand as it is built into LINQBridge

## Known Issues

***Could not load file or assembly LINQBridge.DynamicVisualizer.V11.dll or one of its dependencies.***

*Solution:* Check you have access permission on the folder CurrentUser\Documents\Visual Studio 2012\Visualizers 
for the user running the debugger (IIS_IUSRS if a WebSite). 

*Platform:* Windows 8

[logo]: https://raw.github.com/nbasakuragi/LINQBridge/master/VSExtension/Resources/LINQBridgeLogo.png "LINQBridge"

