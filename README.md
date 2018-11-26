## Overview
This is a procedural terrain generator for the Unity 3D game engine. It features a node generator construction system, biomes, material splatting, and a programmable generation pipeline API.

<a href="https://drive.google.com/open?id=12-6kPdBn5LssFr5xOF1vJnG4od9EWgj0">Documentation</a>

## Key Features
- A node/graph editor for easy-- programming-free generation.
- Level of detail system that changes the resolution of generated heightmaps based on a Tile&#39;s distance from the camera.
- A biome system that determines biomes through moisture, temperature, and height maps.
- Node graphs that are stored in Unity asset files for sharing across different installations.
- Fully customizeable generation pipeline API.
- A material editor that stamps textures based on height and angle.
- A procedural object placement system.
- An event system that reports different stages of execution allowing for easy insertion of custom code.
- A chunk-based infinite terrain system that caches previously visited chunks.
- Compatibility with procedural material shaders (such as MegaSplat).
- In-editor terrain previewing.
- Multithreaded generation.

## Images

#### Terrain Configuration
| ![Image of editor previewing](https://imgur.com/download/QCFSGRi)  | ![image of resulting terrain](https://imgur.com/download/F0pAJAZ)  |
| :------------: | :------------: |
| ![Image of node editor](https://imgur.com/download/SuEtbDE)  | ![Image of node editor](https://imgur.com/download/i0IU0ig)  |


#### Terrain & LOD Previews
![Image of node editor](https://imgur.com/download/x1HhQCY)
![Image of node editor](https://imgur.com/download/AHSjdXl)

#### Heightmap Node Editor
![Image of node editor](https://imgur.com/download/Mas3PJ2)



