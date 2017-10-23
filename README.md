# octane-visual-studio-plugin
## Visual Studio IDE Integration for ALM Octane

This plugin add tool windows to Visual Studio to allow users to see the My Work items inside Visual Studio.

After installing the plugin open ALM Octane tool window by selecting `View -> Other Windows -> ALM Octane`.

### Supported Visaul Studio Version

The plugin supported on Visual Studio 2015 (14) and 2017 (15).

Connection to the server is done using the [C# REST SDK for ALM Octane](https://github.com/HPSoftware/octane-visual-studio-plugin)

### How to build:

Building this project require MSBuild 15 and to run `msbuild /p:Configuration=Release /t:rebuild`.  
The output file is `OctaneVSPlugin\bin\Release\octane-visual-studio-plugin.vsix`

VSIX project template was using external `vssdk\Microsoft.VsSDK.targets` file.  
We've changed it to use Nuget package from `..\packages\Microsoft.VSSDK.BuildTools.15.1.192\tools\vssdk\Microsoft.VsSDK.targets`.  
Note that you'll need to change this reference manually when updating the Nuget package `Microsoft.VSSDK.BuildTools`.

### How to debug: 

Open `octane-visual-studio-plugin.sln` with Visual Studio and select `Debug -> Start Debugging`. Visual Studio will run another instance of Visual Studio in `/rootsuffix Exp` and inject the plugin.