# LINQBridgeVs [![Tweet](https://img.shields.io/twitter/url/http/shields.io.svg?style=social)](https://twitter.com/intent/tweet?text=LINQBridgeVs&url=https://github.com/codingadventures/LINQBridgeVs&hashtags=VisualStudio,LINQPad,csharp,developers)

[![Join the chat at https://gitter.im/nbasakuragi/LINQBridgeVs](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/nbasakuragi/LINQBridgeVs?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Build status](https://ci.appveyor.com/api/projects/status/ibasbqbycm33plpi?svg=true)](https://ci.appveyor.com/project/nbasakuragi/linqbridgevs)
[![GitHubIssues](https://img.shields.io/github/issues/codingadventures/LINQBridgevs.svg)](https://github.com/codingadventures/LINQBridgeVs/issues)

[![](https://img.shields.io/github/downloads/codingadventures/LINQBridgeVs/total.svg)](https://github.com/codingadventures/LINQBridgeVs/releases)

LINQBridgeVs is a Visual Studio Extension compatible with Visual Studio 2012/2013/2015/2017.
It is a bridge between a Visual Studio debugging session and [LINQPad](http://www.linqpad.net).
When enabled on a solution containing C#, VB, ASP.NET or MVC projects, it creates at compile time a Custom Debugger Visualizer assembly
mapping to all the public complex types and making the magnifier glass available for all of them. By clicking on the magnifier glass on an object instance, this will be transmitted over a
bus and 'Dumped' out by LINQPad.
<br><br>
## Getting Started

Once the extension is installed a new menu will be available in the Visual Studio Menu Bar, only visible
when a solution is loaded, called "BridgeVs". There are two options available:

*  *Bridge Solution*
*  *Un-Bridge Solution*

Bridging a solution will extend the MsBuild process to create a custom debugger visualizer assembly for each project. These assemblies are created or updated when you build your projects. 
Private and internal classes _are not_ included, magnifier glass will not be available for them. 
Run the solution, set a breakpoint and hover the mouse pointer on any object instance and the magnifier glass will appear. 
If the magnifier glass does not appear, after you bridged and rebuilt your solution, make sure that in this folder _"C:\Users\youruser\Documents\Visual Studio 201**x**\Visualizers"_ there is/are assemblies with this name template "***AssemblyName**.Visualizer.V1**x**.dll*" (where x is the version number of Visual Studio). 
If the folder does not contain any assembly then please check the log files here: *C:\Users\youruser\AppData\Local\LINQBridgeVs* and open [a new bug](https://github.com/codingadventures/LINQBridgeVs/issues/new).

Do not keep two instances of two different version of LINQPad running (e.g. LINQPad 4 and LINQPad 5) at the same time. If you do so, the data will be automatically sent to the lower version of LINQPad. 
<br><br>
<p align="center">
    <img src="https://github.com/codingadventures/LINQBridgeVs/blob/master/Docs/Example.gif?raw=true" width="1000" />
</p>

## Configuration

Unfortunately an installer is not available yet, therefore Visual Studio needs to be configured in order to run the extension. This is done only once by the extension itself. 
Only for the first time, Visual Studio must be run with Administrator privileges, if not a form will appear asking to restart Visual Studio: 
<br><br>
<p align="center">
    <img src="https://github.com/codingadventures/LINQBridgeVs/blob/master/Docs/VsRestart.PNG?raw=true" width="700" />
</p>

Once Visual Studio is restarted as Administrator, the configuration will complete and the form will never appear again. 

During this process two custom MsBuild Targets [Custom.After.Microsoft.Common.targets](https://github.com/codingadventures/LINQBridgeVs/blob/master/Src/VsExtension/Targets/Custom.After.Microsoft.Common.targets) and [Custom.Before.Microsoft.Common.targets](https://github.com/codingadventures/LINQBridgeVs/blob/master/Src/VsExtension/Targets/Custom.Before.Microsoft.Common.targets), are needed to extend the MsBuild process. They are copied into specific Visual Studio version and edition's folder:
* Visual Studio 2017 - C:\Program Files (x86)\Microsoft Visual Studio\2017\{Edition}\MSBuild\v15.0
* Visual Studio 2015 - C:\Program Files (x86)\MSBuild\v14.0
* Visual Studio 2013 - C:\Program Files (x86)\MSBuild\v12.0
* Visual Studio 2012 - C:\Program Files (x86)\MSBuild\v4.0

You can skip this process but you will not be able to use the extension until you complete the configuration.

## Compatibility

LINQBridgeVs is compatible with any Visual Studio edition from 2012 to 2017. It works only for **.NET Framework 4.0 to 4.6.2**. There is no support for .NET Framework 3.5 downwards. LINQPad 4 and 5 are either supported.

## How it works

LINQBridgeVs uses a technique called [Aspect Oriented Programming](https://wikipedia.AOP). AOP is a programming paradigm that aims to increase modularity by allowing separations of cross-cutting concerns (read behaviors) without modifying the code itself. In general this is achieved by adding an extra step in the build process, usually after the assembly is created. Specifically in Visual Studio the [MsBuild](https://en.wikipedia.org/wiki/MSBuild), which is the Microsoft Build Engine, is to be extended to use AOP. This can be done by either copying a [custom Target file](https://msdn.microsoft.com/en-us/library/ms164312.aspx) to the MsBuild installation folder or by adding ***Target*** tags in a project file like the following:
```xml
<!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets. -->
 <Target Name="BeforeBuild">
  </Target>
  <Target Name="AfterBuild">
  </Target>
```

This extension uses custom build Targets in order to avoid the modification of each individual project.

When a solution is *Bridged*, every projects in it is essentially flagged. Configuration values are stored in the Windows Registry at this location ***\HKEY_CURRENT_USER\Software\LINQBridgeVs***

<p align="center">
    <img src="https://github.com/codingadventures/LINQBridgeVs/blob/master/Docs/Registry.PNG?raw=true" width="700" />
</p>

 For each flagged project the custom tasks that extend the MsBuild engine mentioned earlier, perform some operation on the corresponding generated assembly. These tasks are: 

* **Mapper Task** - This task creates a Custom Debugger Visualizer assembly for each project.
* **SInjection Task** - SInjection stands for "Serializable Injection". This task reads the built assembly and mark every public class and struct as Serializable. By doing so it's then possible to use binary serialization through the [BinaryFormatter](https://msdn.microsoft.com/en-us/library/system.runtime.serialization.formatters.binary.binaryformatter(v=vs.110).aspx) which is the standard binary serializer available in the .NET framework. Binary serialization is pretty powerful, and also fast. I prefer it over Json serialization, as it proved to be more suitable for complex object graphs.
* **Clean Task** - This taks deletes the custom visualizers assemblies. It is invoked either by rebuilding or cleaning the solution.

## Architecture of a Debugger Visualizer

From the [MSDN documentation](https://docs.microsoft.com/en-us/visualstudio/debugger/create-custom-visualizers-of-data): 
   >Visualizers are components of the Visual Studio debugger user interface. A visualizer creates a dialog box or another interface to display a variable or object in a manner that is appropriate to its data type. For example, an HTML visualizer interprets an HTML string and displays the result as it would appear in a browser window; a bitmap visualizer interprets a bitmap structure and displays the graphic it represents. The Visual Studio debugger includes six standard visualizers.
   >
The process of creating custom visualizers to extend the debugger is not too complicated. However it requires a lot of setup, either for mapping a class to custom visualizer but also to prepare the class for serialization. 

The architecture of a debugger visualizer has two parts:

* The debugger side runs within the Visual Studio debugger. 
* The debugger-side code creates and displays the user interface for your visualizer.

The debuggee side runs within the process Visual Studio is debugging (the debuggee). The debuggee side has to send that data object to the debugger side, which can then display it using a user interface you create. In order to send that data, the object needs to be serialized. Unless you use a serializer, like JSON.NET, that requires no markup on neither the class itself nor on the properties, the class must be marked as Serialized, or with other specific attributes depending on the serializer.

Below is an example of what it is needed to map a class to a custom debugger visualizer:
```csharp
using DebuggerVisualizerExample;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Microsoft.Windows.Form;

[assembly: System.Diagnostics.DebuggerVisualizer(
    typeof(DebuggerSide),
    typeof(VisualizerObjectSource),
    Target = typeof(Class1),
    Description = "My First Visualizer")]
[assembly: System.Diagnostics.DebuggerVisualizer(
    typeof(DebuggerSide),
    typeof(VisualizerObjectSource),
    Target = typeof(Class2),
    Description = "My First Visualizer")]
[assembly: System.Diagnostics.DebuggerVisualizer(
    typeof(DebuggerSide),
    typeof(VisualizerObjectSource),
    Target = typeof(Class3),
    Description = "My First Visualizer")]
namespace DebuggerVisualizerExample
{
    [Serializable]
    public class Class1 { }

    [Serializable]
    public class Class2 { }

    [Serializable]
    public class Class3 { }

    public class DebuggerSide : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            MessageBox.Show(objectProvider.GetObject()); //obviously MessageBox can't show any object we pass to it
        }
    }
}
```

For each class, *Class1, Class2, Class3* it is needed a global assembly attribute that maps the type to a debugger visualizer. This extension does all of this automatically. Internally it uses the .NET BinaryFormatter to send the data from the debuggee to the debugger. In case the BinaryFormatter fails to serialize the data, JSON.NET is used instead.

## Acknowledgments

I have used several community open source projects to make this extension. So if you like LINQBridgeVs I recommend checking out the following:

- [JSON.NET](https://github.com/JamesNK/Newtonsoft.Json) is an awesome serializer. It has become part of the .NET Framework and it is used by millions worlwide, now it has become the standard. LINQBridgeVs mainly uses the BinaryFormatter but in case it fails it falls back on JSON.NET.
- [VsRestart](https://github.com/ilmax/vs-restart) is an extension that restart Visual Studio under Administrator privileges. Unfortunately the project seems abandoned and the extension works only with Visual Studio 2013. However but code proved to be working with any version really.
- [MahApps](https://github.com/MahApps/MahApps.Metro) which was the first open source project to make WPF truly modern.
- [LINQPad](http://www.linqpad.net) super useful  scratch pad for C#, VB, F#, SQL. Without it this extension wouldn't exist.
- [Mono.Cecil](http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cecil/) very famous and useful AOP library designed, written and maintaned by JB Evain to generate and inspect programs and libraries in the ECMA CIL format.
- [ILMerge](https://github.com/Microsoft/ILMerge) is a utility that merges multiple .NET assemblies into a single assembly.

[logo]: https://raw.github.com/codingadventures/LINQBridgeVs/master/Src/VsExtension/LINQBridgeLogo.png "LINQBridge"