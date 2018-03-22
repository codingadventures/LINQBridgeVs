![alt text][logo]
==========

[![Join the chat at https://gitter.im/nbasakuragi/LINQBridgeVs](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/nbasakuragi/LINQBridgeVs?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Build status](https://ci.appveyor.com/api/projects/status/ibasbqbycm33plpi?svg=true)](https://ci.appveyor.com/project/nbasakuragi/linqbridgevs)
 
What's LINQBridgeVs?
-----------------

LINQBridgeVs is a Visual Studio Extension compatible with Visual Studio 2012/2013/2015/2017.
It is a bridge between a Visual Studio debugging session and [LINQPad] (http://www.linqpad.net).
When enabled on a solution containing VB/C# projects, it creates at build time Custom Debugger Visualizer 
mapping to all the public complex types and making the magnifier glass 
available for all of them. By clicking on the magnifier glass on an object instance, it will be transmitted over a
bus and read back by LINQPad.

## Getting Started

Once the extension is installed a new menu will be available in the Visual Studio Menu Bar, only visible
when a solution is loaded, called "BridgeVs". There are two options available:

*  Bridge Solution
*  Un-Bridge Solution

Bridging a solution will enable all of the public types of every projects to be exported to [LINQPad](http://www.linqpad.net) at runtime. 

Rebuild your solution, run it, set a breakpoint and hover the mouse pointer
on any object instance and the magnifier glass will appear. 

No need to mark any of your classes as Serialized.

[LINQPad] (http://www.linqpad.net) it is to be installed beforehand.

## Compatibility

LINQBridgeVs is compatible for Visual Studio 2012/2013/2015/2015/2017. It works **for .NET Framework 4.0 - 4.6.2**. There is no support for .NET Framework 2.0 to 3.5 

Either LINQPad 4 and LINQPad 5 are supported.

[logo]: https://raw.github.com/codingadventures/LINQBridgeVs/master/Src/VsExtension/LINQBridgeLogo.png "LINQBridge"


