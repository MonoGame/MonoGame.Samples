using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameLibrary.Input;

public class TouchInfo
{
    /// <summary>
    /// Gets the state of touch input during the previous update cycle.
    /// </summary>
    public TouchCollection PreviousState { get; private set; }

    /// <summary>
    /// Gets the state of touch input during the current update cycle.
    /// </summary>
    public TouchCollection CurrentState { get; private set; }

    /// <summary>
    /// Gets a value that indicates if any touch input is currently active.
    /// </summary>
    public bool HasTouches => CurrentState.Count > 0;

    /// <summary>
    /// Gets the number of current active touches.
    /// </summary>
    public int TouchCount => CurrentState.Count;

    /// <summary>
    /// Gets the primary touch location, or Vector2.Zero if no touches are active.
    /// </summary>
    public Vector2 PrimaryTouchPosition => HasTouches ? CurrentState[0].Position : Vector2.Zero;

    /// <summary>
    /// Creates a new TouchInfo.
    /// </summary>
    public TouchInfo()
    {
        PreviousState = new TouchCollection();
        CurrentState = TouchPanel.GetState();
    }

    /// <summary>
    /// Updates the state information about touch input.
    /// </summary>
    public void Update()
    {
        PreviousState = CurrentState;
        CurrentState = TouchPanel.GetState();
    }

    /// <summary>
    /// Returns a value that indicates if a touch was just started on the current frame.
    /// </summary>
    /// <returns>true if a touch was just started on the current frame; otherwise, false.</returns>
    public bool WasTouchJustPressed()
    {
        return CurrentState.Any(t => t.State == TouchLocationState.Pressed) &&
               !PreviousState.Any(t => t.State == TouchLocationState.Pressed || t.State == TouchLocationState.Moved);
    }

    /// <summary>
    /// Returns a value that indicates if a touch was just released on the current frame.
    /// </summary>
    /// <returns>true if a touch was just released on the current frame; otherwise, false.</returns>
    public bool WasTouchJustReleased()
    {
        return CurrentState.Any(t => t.State == TouchLocationState.Released) ||
               (PreviousState.Count > 0 && CurrentState.Count == 0);
    }

    /// <summary>
    /// Returns a value that indicates if any touch is currently active (pressed or moved).
    /// </summary>
    /// <returns>true if any touch is currently active; otherwise, false.</returns>
    public bool IsTouchDown()
    {
        return CurrentState.Any(t => t.State == TouchLocationState.Pressed || t.State == TouchLocationState.Moved);
    }

    /// <summary>
    /// Gets all current touch locations.
    /// </summary>
    /// <returns>An enumerable of all current touch locations.</returns>
    public IEnumerable<TouchLocation> GetTouchLocations()
    {
        return CurrentState;
    }

    /// <summary>
    /// Gets a specific touch by its ID.
    /// </summary>
    /// <param name="id">The ID of the touch to find.</param>
    /// <returns>The touch location if found; otherwise, a default TouchLocation.</returns>
    public TouchLocation GetTouchById(int id)
    {
        return CurrentState.FirstOrDefault(t => t.Id == id);
    }

    /// <summary>
    /// Returns a value that indicates if a touch moved between the previous and current frames.
    /// </summary>
    /// <returns>true if any touch moved; otherwise, false.</returns>
    public bool WasTouchMoved()
    {
        return CurrentState.Any(t => t.State == TouchLocationState.Moved);
    }

    /// <summary>
    /// Gets the delta movement of the primary touch between frames.
    /// </summary>
    /// <returns>The movement delta, or Vector2.Zero if no primary touch or movement.</returns>
    public Vector2 GetPrimaryTouchDelta()
    {
        if (!HasTouches) return Vector2.Zero;

        var currentTouch = CurrentState[0];
        
        // Try to find the corresponding touch in the previous state
        foreach (var prevTouch in PreviousState)
        {
            if (prevTouch.Id == currentTouch.Id)
            {
                return currentTouch.Position - prevTouch.Position;
            }
        }
        
        return Vector2.Zero;
    }
}