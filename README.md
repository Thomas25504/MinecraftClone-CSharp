# OpenTK Voxel Engine - Minecraft Clone

A Minecraft-style voxel engine built in C# using OpenTK and OpenGL.

![World Generation](WorldGen.JPG)

## Features

- Infinite procedurally generated terrain using Simplex noise
- Chunk-based world management with configurable render distance
- Greedy face culling (only visible faces are rendered)
- Texture atlas support with per-face UV mapping
- Face shading / ambient lighting for visual depth
- Transparent block support (glass) with two-pass rendering
- Block placement and removal with DDA raycast
- Block outline highlight on targeted block
- First-person camera with mouse look
- Multiple block types with number key selection
- Crosshair UI
- Wireframe debug mode (F key)

## Screenshots

### Terrain
![Terrain](WorldGen.JPG)

### Wireframe View
![Wireframe](WireFrame.JPG)

## Development Journey

### Bug Fixes Along the Way

**Black faces (winding order)**
![Texture Error](TextureError.JPG)
Early build had incorrect vertex winding order on Back, Right and Left faces causing them to render black or be culled.

**Texture rotation**
![Rotation Error](RotationError.JPG)
Side face textures were rendering upside down due to OpenGL's bottom-left UV origin vs image top-left origin.

**Raycast block outline**
![Raycast Debug](RaycastIssue.JPG)
DDA raycast debugging with block outline to align break/place targeting with the crosshair.

## Controls

| Key | Action |
|-----|--------|
| W A S D | Move |
| Space | Fly up |
| Left Shift | Fly down |
| Mouse | Look around |
| Left Click | Break block |
| Right Click | Place block |
| 1-6 | Select block type |
| F | Toggle wireframe |
| R | Toggle cursor lock |
| Escape | Quit |

## Block Types

| Key | Block |
|-----|-------|
| 1 | Dirt |
| 2 | Stone |
| 3 | Glass |
| 4 | Wood |
| 5 | Sand |
| 6 | Pink Heart |

## Project Structure
```
├── Block.cs          # Block data and texture indices
├── BlockType.cs      # Block type enum
├── Camera.cs         # First-person camera
├── Chunk.cs          # Mesh generation and rendering
├── Game.cs           # Main game loop, input, raycast
├── Noise.cs          # Simplex noise
├── Terrain.cs        # Height map generation
├── TextureAtlas.cs   # Atlas loading and UV calculation
├── World.cs          # Chunk loading/unloading
├── shader.vert       # Main vertex shader
├── shader.frag       # Main fragment shader
├── outline.vert      # Block outline vertex shader
├── outline.frag      # Block outline fragment shader
├── crosshair.vert    # Crosshair vertex shader
├── crosshair.frag    # Crosshair fragment shader
└── Atlas.png         # Texture atlas
```

## Dependencies

- [OpenTK 4.9.4](https://opentk.net/) - OpenGL bindings for .NET
- [StbImageSharp 2.30.15](https://github.com/StbSharp/StbImageSharp) - Image loading
- .NET 10.0

## Building
```bash
dotnet build
dotnet run
```
