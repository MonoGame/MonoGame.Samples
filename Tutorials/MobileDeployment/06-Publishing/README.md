# Chapter 6: Publishing to App Stores

This chapter demonstrates how to convert a Windows-only MonoGame project to support iOS and Android platforms using a shared codebase architecture.

The chapter covers:

* Converting a single-platform project to multi-platform structure.
* Setting up a shared common library with multi-targeting.
* Creating platform-specific project shells for Windows, iOS, and Android.
* Configuring conditional package references for each platform.
* Understanding cross-platform project architecture and naming conventions.
* Updating third-party libraries for cross-platform compatibility.
* The assets and configuration to package and deploy to the app stores.

## Project Structure

This sample includes:

* **DungeonSlime.Common** - Shared game logic library with multi-targeting support
* **DungeonSlime.Windows** - Windows desktop project shell
* **DungeonSlime.iOS** - iOS mobile project shell  
* **DungeonSlime.Android** - Android mobile project shell

## Prerequisites

* Completed the MonoGame 2D tutorial
* Development environment set up
* For iOS: Mac with Xcode and Apple Developer account
* For Android: Android SDK and development tools

## Key Features Demonstrated

* Multi-targeting framework configuration (`net8.0;net8.0-ios;net8.0-android`)
* Platform-specific MonoGame package references
* Shared code architecture for cross-platform development
* Modern .NET project management with Central Package Management
