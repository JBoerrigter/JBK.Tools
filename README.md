# JBK.Tools

A collection of small utilities for working with game assets.

## Tools

- **MapExtractor** – Windows Forms application for inspecting `.kcm` client map files and exporting height, color, object, and texture maps.
- **ModelLoader** – Console tool for loading `.gb` model files, combining meshes and animations, and exporting them as `glb`.
- **OplReader** – Windows Forms viewer for `.opl` object placement lists that displays positions, rotation, and scale information.

## Building

The projects target .NET 9.0. Build the entire solution with:

```bash
 dotnet build JBK.Tools.sln
```

Run a specific tool by pointing `dotnet run` at the project:

```bash
 dotnet run --project JBK.Tools.MapExtractor
```

For more details, explore the source of each project.

