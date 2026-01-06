using System;
using Microsoft.Xna.Framework;

namespace Platformer2D.Core
{
    /// <summary>
    /// A set of helpful methods for working with rectangles.
    /// </summary>
    public static class RectangleExtensions
    {
        /// <summary>
        /// Calculates the signed depth of intersection between two rectangles.
        /// </summary>
        /// <param name="rectA">The first rectangle.</param>
        /// <param name="rectB">The second rectangle.</param>
        /// <returns>
        /// A <see cref="Vector2"/> representing the depth of the intersection.
        /// Positive values indicate that <paramref name="rectA"/> is to the right or below <paramref name="rectB"/>.
        /// Negative values indicate that <paramref name="rectA"/> is to the left or above.
        /// If the rectangles are not intersecting, returns <see cref="Vector2.Zero"/>.
        /// </returns>
        public static Vector2 GetIntersectionDepth(this Rectangle rectA, Rectangle rectB)
        {
            // Calculate half sizes for both rectangles (helps with center-based calculations).
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            // Calculate the centers of each rectangle.
            Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            // Calculate the current distance between the rectangle centers.
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;

            // Calculate the minimum distance required for the rectangles to *not* be intersecting.
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            // If the rectangles are not overlapping, return zero depth.
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;

            // Calculate intersection depths.
            float depthX = distanceX > 0
                ? minDistanceX - distanceX   // Positive depth (rectA is on the right side)
                : -minDistanceX - distanceX; // Negative depth (rectA is on the left side)

            float depthY = distanceY > 0
                ? minDistanceY - distanceY   // Positive depth (rectA is below)
                : -minDistanceY - distanceY; // Negative depth (rectA is above)

            return new Vector2(depthX, depthY);
        }

        /// <summary>
        /// Gets the position of the center of the bottom edge of the rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to calculate from.</param>
        /// <returns>
        /// A <see cref="Vector2"/> representing the bottom-center point of the rectangle.
        /// </returns>
        public static Vector2 GetBottomCenter(this Rectangle rect)
        {
            return new Vector2(rect.X + rect.Width / 2.0f, rect.Bottom);
        }
    }
}