# Basic Platformer Features

> 7 minutes to read

Discusses basic features of the Platformer Starter Kit and offers recommendations for making basic modifications to the Platformer game.

* Platformer Game Design
* Starter Kit Assets
* Exploring the Basic Classes
* Basic Modifications to the Platformer Starter Kit

## Platformer Game Design

The design of Platformer should be familiar to all gamers. You must avoid the enemies and reach the level exit before time expires (collecting gems along the way). If you complete the level with time remaining, that time is converted to a point bonus, and it is added to your current score. You lose a life if you run into an enemy, fall off a ledge, or run out of time before collecting all gems and reaching the exit.

## Game World

The game world is composed of individual levels where each level is automatically constructed, using several square tile types, from an existing text file. This text file uses a set of symbols to map out the level, locating the start and exit points, enemy starting positions, gem locations and tile locations (including their type). For a complete listing of the level file format, see [Advanced Platformer Features](3_advanced_platformer_features.md).

Three tile types are implemented by Platformer and represent the ledges and impassable areas of a level.

|Name|Properties|
|-|-|
|Impassable|Players and enemies cannot pass through the tile from any direction.|
|Passable|Players and enemies can pass through the tile freely. The main purpose of this tile type is to provide decoration to the level – decoration such as jungle foliage, rocks, and other scenery.|
|Platform|Platform tiles behave like passable tiles except that players and enemies can stand on (or in the case of the player character, fall onto) the top edge of the tile without falling.</br>This is implemented by checking for collision between the character's bounding rectangle and the top edge of the platform tile. If collision occurs, the affected character's Y velocity (that is, the character's speed when falling) is forced to 0. If the character stood, or fell onto, a passable tile, no collision checking is performed. This causes the character to continue falling until the character reaches an impassable tile or passes the bottom edge of the screen. For more information on collision checking and game world physics, see [Advanced Platformer Features](3_advanced_platformer_features.md).
|||

In addition to tiles, there are several special objects that represent special locations or objects in the game world.

|Name|Properties|
|-|-|
|Level Start|The player character begins the level at this location, facing to the right.|
|Gem|An object that players collect for points and level completion. The player must collect all gems and reach the exit before time expires to complete the level.|
|Level Exit|The location the player must reach to complete the level before time expires. Failure to reach the exit (and collect all gems) forces the player to lose a life and begin at the Level Start location.|
|||

### Player Character

The player character can run and jump. The player is affected by a simple gravity implementation, and can run off platforms without jumping. When falling, the character is halted by any collision with a platform-type tile. If the player character is moving and not under direct control of the player, simple pseudo-drag is implemented. This gradually stops the player character instead of causing an instant stop.

### Enemies

Initially, Platformer implements a single enemy type. This enemy slowly walks back and forth along a platform, automatically turning around at the platform's edge. The player character cannot kill the enemy and is immediately killed by the enemy upon contact.

## Exploring the Basic Classes

The Platformer starter kit can be pretty overwhelming the first time you see it. However, it can be broken down into more manageable pieces, based on the complexity of the game object and its usage by the Platformer game. In this section, we'll explore Platformer's basic classes and structures. We'll talk about their purpose and design and point out features that can be modified or expanded on. For information on more complex classes, see [Intermediate Platformer Features](2_intermediate_platformer_features.md) and [Advanced Platformer Features](3_advanced_platformer_features.md).

## Gem Class and Circle Structure

The Gem class implementation is located in Gem.cs. Important methods include:

* LoadContent</br>Loads a grayscale gem-shaped texture.

* Update</br>Updates the gem height at regular intervals during gameplay. This method is used to cause all gems in the level to oscillate up and down in relation to each other. The motion is implemented with a sine curve over time. In addition, the X-coordinate of the gem is used to produce a nice syncopated pattern.

* Draw</br>Draws the gem using the specified color shade. The default color is yellow.

* OnCollected</br>Plays a sound indicating retrieval by the player. This is an great place to modify the default behavior of gem collection!
Properties include the parent Level, its position in world space, and a bounding circle.

The bounding circle (implemented by the Circle structure, located in Circle.cs) is used to determine if the player is colliding with the gem. If the player collides with the gem, it is removed from the gem collection of the parent Level and points are added to the player's score. The most important method of Circle is Intersects. This method checks for intersection with a rectangle (used to represent the bounding area of other game objects). This method is called by Level.UpdateGems and if it returns true, the gem is removed and the OnGemCollected method is called.

Gems are the only objects that use a bounding circle. All other objects use a bounding rectangle.

## Gem Animation
The default animation of gems is based on a sine wave. They slowly rise and fall as a group during gameplay. The Gem.Update method controls this animation.

> Tip
> 
> You can easily modify this behavior by changing the default height and speed of the animation.

## Tile Structure and TileCollision Enumeration

The Tile structure implementation is located in Tile.cs. It has a constructor, but its main purpose is storing the properties of a level tile.

Properties include the tile dimensions (width and height), the collision behavior of the tile object, and the texture used when drawing the tile object.

The collision behavior (stored in the Collision property) is the most important property of the Tile structure. This property determines what kind of collision detection is done, if any. For more information on supported tile types, see Game World.

The TileCollision enumeration lists all possible collision behaviors for a tile and is located in Tile.cs.

## Changing the Behavior of Existing Game Events

> Tip
> 
> Modifying default behaviors in the game is as easy as modifying the related OnXEvent method.

Platformer handles important events in the game using methods that follow an OnX naming convention, where X is an important event name. For instance, when the player character collects a gem, OnCollected is ultimately called. The complete list of methods called before Gem.OnCollected is as follows:

1. Level.Update

2. Level.UpdateGems

3. Level.OnCollected

4. Gem.OnCollected

When a gem is collected, the default behavior is to play a "gem collected" sound (GemCollected.wav). If you wanted to change this behavior, there are two places you should focus on: the *Level.OnCollected* and *Gem.OnCollected* methods. Add (or remove existing) code to do different things. For instance, to create a "cursed" gem (subtracts points from the player's total when collected), modify the Level.OnCollected method to subtract *Gem.PointValue* form the total instead of adding to it.

## Exploring the Platformer Starter Kit

* [Basic Platformer Features](1_basic_platformer_features.md)</br>Discusses basic features of the Platformer Starter Kit and offers recommendations for making basic modifications to the Platformer game.

* [Intermediate Platformer Features](2_intermediate_platformer_features.md)</br>Discusses intermediate classes of the Platformer Starter Kit, and offers recommendations for modifying or extending Platformer features.

* [Advanced Platformer Features](3_advanced_platformer_features.md)</br>Discusses advanced features of the Platformer Starter Kit, and offers recommendations for modifying or extending Platformer features.
