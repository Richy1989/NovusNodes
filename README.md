# Node-Based Application

This project is a **node-based application** designed to visualize and manipulate interconnected data structures through an interactive interface. Built with **C# Blazor**, it provides seamless integration of both **C# plugins** and **JavaScript plugins**, offering unparalleled flexibility and extendability.

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

## Note: 

For Node API for .NET a Shared DLL has to be compiled. More info here: https://microsoft.github.io/node-api-dotnet/scenarios/dotnet-js.html

## Plugins
In order to create a plugin the **NovusNodoPluginLibrary.dll** has to be referenced in a Razor Component Project. The Plugin class needs to inherit from the **PluginBase** class and the Plugin UI (for configuration) needs to inherit from the **NovusUIPluginBase**, implement the abstract methods and your own functionality.  
The Plugin needs to adapt the following attribute: `[NovusPlugin("7BA6BE2A-19A1-44FF-878D-3E408CA17366", "JS Function", "#ea899a")]`
Check the "NovusNodoUIPLugin" project for a sample implementation.

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