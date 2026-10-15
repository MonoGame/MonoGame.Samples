# Advanced 2D Shaders in MonoGame

This tutorial series picks up where the [Building 2D Games](https://docs.monogame.net/articles/tutorials/building_2d_games/24_shaders/index.html) tutorial left off, diving deep into the world of custom shaders. The goal is to develop tooling for MonoGame to facilitate shader development, write some custom effects for _DungeonSlime_, and build intuition for how to approach shader programming.

## What We Will Build

Throughout this series, we will take our _Dungeon Slime_ game and add a whole new layer of visual polish. By the end, you will have implemented a variety of advanced shader effects, including:

- A scene transition effect,
- A color swap effect,
- A 3d effect using `SpriteBatch`,
- A lighting system using techniques from deferred rendering,
- A shadow system using stencil buffers.

## Who This Is For

This documentation is for intermediate MonoGame users. It assumes that you have a solid understanding of C# and have _already completed the entire "Building 2D Games" tutorial series._

> [!IMPORTANT]
> The concepts and code from the original tutorial, especially the [Shaders](https://docs.monogame.net/articles/tutorials/building_2d_games/24_shaders/index.html) chapter, are a mandatory prerequisite. We will be building directly on top of the final project from that series.

## Tutorial Structure

This documentation is organized to be read sequentially, as each chapter builds upon the techniques and code from the previous one.

| Chapter                                                              | Summary                                                                                                                                  | Source Files                                                                                                                                       |
| -------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Chapter 01: Introduction](https://docs.monogame.net/articles/tutorials/advanced/2d_shaders/01_introduction/)                 | An overview of the advanced shader techniques covered in this tutorial series.                                                           | [Final Chapter from Building-2D-Games Series](../learn-monogame-2d/src/27-Conclusion/) |
| [Chapter 02: Hot Reload](https://docs.monogame.net/articles/tutorials/advanced/2d_shaders/02_hot_reload/) | Set up a workflow with a "hot-reload" system to recompile and view shader changes in real-time without restarting the game.              | [02-Hot-Reload](src/02-Hot-Reload-System/)                   |
| [Chapter 03: The Material Class](https://docs.monogame.net/articles/tutorials/advanced/2d_shaders/03_the_material_class/)     | Create a `Material` class to manage shader parameters and handle the complexities of the shader compiler and our hot-reload system.      | [03-Material-Class](src/03-The-Material-Class/)                         |
| [Chapter 04: Debug UI](https://docs.monogame.net/articles/tutorials/advanced/2d_shaders/04_debug_ui/)                         | Integrate ImGui.NET to build a real-time debug UI, allowing for runtime manipulation of shader parameters.                               | [04-Debug-UI](src/04-Debug-UI/)                                         |
| [Chapter 05: Transition Effect](https://docs.monogame.net/articles/tutorials/advanced/2d_shaders/05_transition_effect/)       | Create a flexible, texture-driven screen wipe for smooth scene transitions.                                                              | [05-Transition-Effect](src/05-Transition-Effect/)                       |
| [Chapter 06: Color Swap Effect](https://docs.monogame.net/articles/tutorials/advanced/2d_shaders/06_color_swap_effect/)       | Implement a powerful color-swapping system using a 1D texture as a Look-Up Table (LUT), allowing for dynamic color palette changes.      | [06-Color-Swap-Effect](src/06-Color-Swap-Effect/)                       |
| [Chapter 07: Sprite Vertex Effect](https://docs.monogame.net/articles/tutorials/advanced/2d_shaders/07_sprite_vertex_effect/) | Dive into the vertex shader to manipulate sprite geometry, break out of the 2D plane, and give the game a dynamic 3D perspective.        | [07-Sprite-Vertex-Effect](src/07-Sprite-Vertex-Effect/)                 |
| [Chapter 08: Light Effect](https://docs.monogame.net/articles/tutorials/advanced/2d_shaders/08_light_effect/)                 | Build a 2D dynamic lighting system from scratch using a deferred rendering approach, complete with color, light, and normal map buffers. | [08-Light-Effect](src/08-Light-Effect/)                                 |
| [Chapter 09: Shadows Effect](https://docs.monogame.net/articles/tutorials/advanced/2d_shaders/09_shadows_effect/)             | Add dynamic 2D shadows to the lighting system by using a vertex shader and the stencil buffer for efficient light masking.               | [09-Shadows-Effect](src/09-Shadows-Effect/)                            |
| [Chapter 10: Conclusion & Next Steps](https://docs.monogame.net/articles/tutorials/advanced/2d_shaders/10_next_steps)        | Review the techniques learned throughout the series and preview further graphics programming topics to continue your journey.            |                                                                                                                                                    |
## Repository Structure

Each chapter's source code is contained in its own folder, allowing you to:

- See the progressive development of the game throughout the tutorial.
- Compare implementations between chapters to understand new concepts.
- Use specific chapters as starting points for your own projects.

## Getting Started

To begin using this repository:

1. Clone or download the repository.
2. Each chapter folder contains a complete, working project representing that stage of development.
3. For the full learning experience, we recommend following the tutorial chapters in sequence.

## Resources

For additional help and information:

- [MonoGame Documentation](https://docs.monogame.net/)
- [MonoGame Community Discord](https://discord.gg/monogame)
- [MonoGame GitHub Repository](https://github.com/MonoGame/MonoGame)
