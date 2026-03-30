using System;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Scenes;

public class SceneTransition
{
    public DateTimeOffset StartTime;
    public TimeSpan Duration;
    
    /// <summary>
    /// true when the transition is progressing from 0 to 1.
    /// false when the transition is progressing from 1 to 0.
    /// </summary>
    public bool IsForwards;
    
    /// <summary>
    /// The index into the <see cref="Core.SceneTransitionTextures"/>
    /// </summary>
    public int TextureIndex;
    
    /// <summary>
    /// The 0 to 1 value representing the progress of the transition. 
    /// </summary>
    public float ProgressRatio => MathHelper.Clamp((float)(EndTime - DateTimeOffset.Now).TotalMilliseconds / (float)Duration.TotalMilliseconds, 0, 1);
    
    public float DirectionalRatio => IsForwards ? 1 - ProgressRatio : ProgressRatio;
    
    public DateTimeOffset EndTime => StartTime + Duration;
    public bool IsComplete => DateTimeOffset.Now >= EndTime;
    

    /// <summary>
    /// Create a new transition
    /// </summary>
    /// <param name="durationMs">
    ///     how long will the transition last in milliseconds?
    /// </param>
    /// <param name="isForwards">
    ///     should the transition be animating the Progress parameter from 0 to 1, or 1 to 0?
    /// </param>
    /// <returns></returns>
    public static SceneTransition Create(int durationMs, bool isForwards)
    {
        return new SceneTransition
        {
            Duration = TimeSpan.FromMilliseconds(durationMs),
            StartTime = DateTimeOffset.Now,
            TextureIndex = Random.Shared.Next(),
            IsForwards = isForwards
        };
    }

    public static SceneTransition Open(int durationMs) => Create(durationMs, true);
    public static SceneTransition Close(int durationMs) => Create(durationMs, false);
}