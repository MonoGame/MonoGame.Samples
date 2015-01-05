Samples
=======

A number of simple MonoGame samples for all the supported platforms:


1. Platformer 2D (Supported on all platforms)

2. SpaceWar (experimental - Windows only)

3. NeonShooter (Currently being updated for all platforms - should work)



# Building the samples

A number of the samples use the pre-release NuGet packages in order to match the latest dev releases on the main MonoGame branch.


>if you wish you can remove the NuGet packages and reference the MonoGame source directly



When you are building the samples for the first time you will need to enable "NuGet Package Restore" which will download the latest NuGet release automatically.

To activate "NuGet package restore" just right click the solution and select "Enable NuGet package restore" or right-click the project and select "Manage NuGet Packages".



Next time you build the project it should build successfully.



#Known issues

Here is a list of the current issues in the develop branch of the samples



* SpaceWar uses an XACT (Xbox Audio) definition which the new content doesn't support.  This will be updated to use the source audio files and remove the XACT references from the project.

* SpaceWar uses some advanced shaders which need updating to a later version of DX and removal of some of the native shader code

* NeonShooter has been test across platforms but is running slow, this needs to be updated to be more performant in MonoGame

* Neon Shooter needs the rest of the platforms added to it's project.  No known issues, just time to do it.

