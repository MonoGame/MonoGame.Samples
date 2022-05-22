# Intermediate Platformer Features

> 3 minutes to read

Discusses intermediate classes of the Platformer Starter Kit, and offers recommendations for modifying or extending Platformer features.

In this section, we'll explore Platformer's intermediate features and the classes that implement them, such as sprite animation. We'll talk about their purpose and design, and point out features that can be modified or expanded on. For information on basic classes, see [Exploring the Basic Classes](1_basic_platformer_features.md). For advanced classes, see [Advanced Platformer Features](3_advanced_platformer_features.md).

## Exploring the Intermediate Classes

The following classes are more complex than the basic Platformer classes, and they implement some intermediate features of Platformer.

## AnimationPlayer Structure and Animation Class

The AnimationPlayer structure is used to animate the player character and the enemies of the current level. It stores a single animation sprite sheet in the Animation property, of type Animation. Animation begins with a call to the PlayAnimation method, but if the animation is already playing, the method immediately returns. This prevents the animation from being interrupted for any reason.

The animation infrastructure is intentionally simple and customized for the base version of Platformer. It is not designed to scale up or be used independently of Platformer. The only requirement is that the animation frame is square in shape.

Platformer provides five animations for the player character and three animations for each of the different enemies. However, not all are initially used in the game.

> Note
> 
> For enemies, the Die animation is not used.

The AnimationPlayer structure implementation is located in AnimationPlayer.cs. Important methods include:

* Draw</br>Advances the current time of the animation and, based on the time, draws a single frame from the related sprite sheet.

* PlayAnimation</br>Begins playing a specified animation, using a single-animation sprite sheet. If the specified animation is already running, it is not interrupted.

Properties include the sprite sheet used for animation, the current frame index, and the origin of the current frame.

The Animation class implementation is located in Animation.cs. It stores important attributes of an animation, such as frame count, texture used for animation, frame width, and so on.

## Enemy Class and FaceDirection Enumeration

The Enemy class implementation is located in Enemy.cs. Important methods include:

* LoadContent</br>Loads a specific enemy sprite strip. The enemy type is determined by the level structure file. Platformer implements four different enemy types. These types differ only in appearance. They are stored in separate directories in the appropriate content project, under the Sprites directory. Each enemy type has two animations: Run and Idle. The third animation, Die, is not used, but it is included for future development.

* Update</br>Updates the enemy location. The default enemy behavior is to pace back and forth across a platform, pausing at the platform's edge.</br>
If an enemy detects that it will move into an impassable tile (such as a wall) or walk off the edge of a platform tile, the value of the FaceDirection property is reversed, causing the enemy to begin walking in the opposite direction.

* Draw</br>Draws the enemy character using the specified animation frame.</br>
Before drawing the current animation, Draw checks the current orientation of the enemy. If the enemy is facing to the right, the current frame is flipped by specifying SpriteEffects.FlipHorizontally in the call to SpriteBatch.Draw.

Properties are similar to the Gem class: the parent Level, its position in world space, and a bounding rectangle. This differs from the gem object implementation, which used a circle. A rectangular bounding rectangle makes more sense for enemy characters (and the player character) because of their interaction with the rectangular tiles that make up the level – especially the platform tiles.

The Enemy class is a simpler version of the Player class because it cannot jump and has only a single behavior: relentless pacing upon its current platform.

## Expanding Platformer

A logical expansion for Platformer is the use of power-ups in the game. In this topic, an Invincibility power-up is implemented in the default version of the Platformer starter kit. This power-up gives the player character temporary invincibility against enemies is indicated by a red gem. For complete details on this expansion, see [Platformer: Adding a Power-Up](2_platformer_adding_a_powerup.md).

## Exploring the Platformer Starter Kit

* [Basic Platformer Features](1_basic_platformer_features.md)</br>Discusses basic features of the Platformer Starter Kit and offers recommendations for making basic modifications to the Platformer game.

* [Intermediate Platformer Features](2_intermediate_platformer_features.md)</br>Discusses intermediate classes of the Platformer Starter Kit, and offers recommendations for modifying or extending Platformer features.

* [Advanced Platformer Features](3_advanced_platformer_features.md)</br>Discusses advanced features of the Platformer Starter Kit, and offers recommendations for modifying or extending Platformer features.
