![alt text][logo]
==========
 
What's LINQBridge?
-----------------

LINQBridge is a Visual Studio Extension for Visual Studio 2010 and Visual Studio 2012.
It is a bridge between a Visual Studio debugging session and [LINQPad] (http://www.linqpad.net).
When enabled on a selection of VB/C# projects, it creates a Custom Debugger Visualizer 
mapping to it all the complex types and making the magnifier glass then 
available for all of them. By clicking on the magnifier glass the types will be trasmitted over a
bus and read back by LINQPad.

## Getting Started

Once the LINQBridge Extension is installed a new menu will be available in the Visual Studio Menu Bar, only visible
when a solution is loaded and a project selected. There are two options available:

*  Enable LINQBridge
*  Disable LINQBridge

Enable LINQBridge on a selected projet will do the magic and that's it! 

Compile your solution, run it, set a breakpoint and hover the pointer
on whatever object instance and you will see the magnifier glass appear. 

Clicking the magnifier will export the instance to [LINQPad](http://www.linqpad.net). 

[logo]: https://raw.github.com/nbasakuragi/LINQBridge/master/VSExtension/Resources/LINQBridgeLogo.png "LINQBridge"

