# Node-Based Application

This project is a **node-based application** designed to visualize and manipulate interconnected data structures through an interactive interface. Built with **C# Blazor**, it provides seamless integration of both **C# plugins** and **JavaScript plugins**, offering unparalleled flexibility and extendability.

## Version
Currently no releases are available, you need to build the project youself. The project is far away from stable!

## Features

- **Blazor-Powered UI**: Use Blazor to create rich and interactive configuration interfaces effortlessly.  
- **Plugin Support**: Supports custom plugins in C# and JavaScript for tailored functionality.  
- **Node API for .NET**: JavaScript can be used as usual thanks to the Node API for .NET framework.   
- **Node Creation**: Intuitive UI for adding and naming nodes.  
- **Link Management**: Connect nodes with edges and manage the relationships between them.  
- **Interactive Interface**: Drag-and-drop functionality for nodes and links.  
- **Real-Time Updates**: Any changes to the network are reflected instantly, ensuring a seamless user experience.  

## Use Cases

- **Data Visualization**: Represent hierarchical or graph-based data structures visually.  
- **Process Mapping**: Build workflows, decision trees, or dependency diagrams.  
- **Educational Tools**: Teach and understand graph theory, networks, or algorithmic concepts.  

## Building: 

For Node API for .NET a Shared DLL has to be compiled. More info here: https://microsoft.github.io/node-api-dotnet/scenarios/dotnet-js.html

The Plugins need to be located in **NovusNodi/plugins/<AssemblyName>**. The contect of a donet publish needs to be copied to this location. More Infos will follow! 

## Plugins
In order to create a plugin the **NovusNodoPluginLibrary.dll** has to be referenced in a Razor Component Project. The Plugin class needs to inherit from the **PluginBase** class and the Plugin UI (for configuration) needs to inherit from the **NovusUIPluginBase**, implement the abstract methods and your own functionality.  
The Plugin needs to adapt the following attribute: `[NovusPlugin("7BA6BE2A-19A1-44FF-878D-3E408CA17366", "JS Function", "#ea899a")]`
Check the "NovusNodoUIPLugin" project for a sample implementation.

### Important:
These restrictions are not permanent, please do not mention them as bugs, I'm well aware of them. 
- The **NovusUIPluginBase** has to be derived from in the .razor class (compiler will throw an error!)
- Icons need to be placed in the folder **./wwwroot/icons** with no subdirectories
- Icon name for start and end icon need to be set the coresponding properties in PluginBase (StarterIconName / EndIconName)
- CSS Can only be used directly in the .razor file, either inline or with a style tag. 

## Technologies Used

- **Framework**: C# Blazor for frontend and backend integration.  
- **Plugin System**: Enables dynamic extension with C# and JavaScript.  
- **Canvas/Graphics Library**: Used for rendering the graph.  

## Acknowledgments
This project leverages the following fantastic libraries:
- **MudBlazor: A modern Blazor component library for building beautiful and functional user interfaces.
- **Node API for .NET: Enables advanced interoperability between .NET and JavaScript in the same process.
- **CodeMirror for easy code highlight and online edit.
- **JsonEditor to enable beautiful visualisation of JSON objects.
- **FontAwesome free icon sets.