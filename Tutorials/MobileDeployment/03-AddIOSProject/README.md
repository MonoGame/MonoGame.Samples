# Chapter 3: Adding IOS Project

This chapter demonstrates how to convert a Windows-only MonoGame project to support iOS platforms.

The chapter covers:

* Converting a single-platform project to multi-platform structure.
* Creating platform-specific project shells for Windows, and iOS.
* Configuring conditional package references for each platform.
* Understanding cross-platform project architecture and naming conventions.
* Updating third-party libraries for cross-platform compatibility.

## Project Structure

This sample includes:

* **DungeonSlime** - Windows desktop project shell
* **DungeonSlime.iOS** - iOS mobile project shell  

## Prerequisites

* Completed the MonoGame 2D tutorial
* Development environment set up
* For iOS: Mac with Xcode and Apple Developer account

## Key Features Demonstrated

* Multi-targeting framework configuration (`net8.0;net8.0-ios`)
* Platform-specific MonoGame package references
* Modern .NET project management with Central Package Management
