# valueedge-visual-studio-plugin
## Visual Studio IDE Integration for ValueEdge

This plugin adds tool windows to Visual Studio to allow users to see the My Work items inside Visual Studio.

After installing the plugin, open ValueEdge tool window by selecting `View -> Other Windows -> ValueEdge`.

### Supported Visual Studio Version

The plugin is supported on Visual Studio 2015 (14) and 2017 (15).

Connection to the server is done using the [C# REST SDK for ValueEdge](https://github.com/MicroFocus/alm-octane-csharp-rest-sdk)

### How to build:

Building this project requires MSBuild 15 and to run `msbuild /p:Configuration=Release /t:rebuild`.  
The output file is `OctaneVSPlugin\bin\Release\valueedge-visual-studio-plugin.vsix`

VSIX project template was using external `vssdk\Microsoft.VsSDK.targets` file.  
We've changed it to use Nuget package from `..\packages\Microsoft.VSSDK.BuildTools.15.1.192\tools\vssdk\Microsoft.VsSDK.targets`.  
Note that you'll need to change this reference manually when updating the Nuget package `Microsoft.VSSDK.BuildTools`.

### How to debug: 

Open `octane-visual-studio-plugin.sln` with Visual Studio, go to the `Debug` tab under the project properties.
Set `Start external program` to the path of the Visual Studio executable and add `/rootsuffix Exp` to `Command line arguments`.
Selecting `Debug -> Start Debugging`, Visual Studio will run another instance of Visual Studio and inject the plugin.