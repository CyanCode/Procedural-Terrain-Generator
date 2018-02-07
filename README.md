## Overview
---
This is a procedural terrain generator for the Unity 3D engine. It features a node generator construction  system, material splatting, and a programmable generation pipeline API.

<a href="https://drive.google.com/open?id=12-6kPdBn5LssFr5xOF1vJnG4od9EWgj0">Documentation</a>

##Key Features:
- A node/graph editor for easy--- programming-free generation
- Uses a Mesh rather than Unity Terrain for lower level customization and optimization
- Heightmap-free generation
- Node graphs are saved in a json file and can be shared/imported across different computers
- Fully programmable generation pipeline API
- A material editor that stamps textures based on height and angle
- An event system that reports different stages of execution allowing insertion of custom code
- A chunk-based infinite terrain system that caches previously visited chunks 
- Compatibility with procedural material shaders (such as MegaSplat’s) 
- In-editor terrain previewing 
- Deferred collider generation for chunks that are outside a set radius 

##Images
![Image of node editor](https://github.com/CyanCode/Procedural-Terrain-Generator/blob/master/AssetStore/sc-5.png?raw=true)

![Image of editor previewing](https://github.com/CyanCode/Procedural-Terrain-Generator/blob/master/AssetStore/sc-2.png?raw=true)

![image of resulting terrain](https://github.com/CyanCode/Procedural-Terrain-Generator/blob/master/AssetStore/sc-3.png?raw=true)
