using Microsoft.Xna.Framework;

namespace Platformer2D.Core
{
    /// <summary>
    /// Represents a 2D circle.
    /// </summary>
    struct Circle
    {
        /// <summary>
        /// Center position of the circle.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// Radius of the circle.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Constructs a new circle.
        /// </summary>
        /// <param name="position">The Vector2 position which will the center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        public Circle(Vector2 position, float radius)
        {
            Center = position;
            Radius = radius;
        }

        /// <summary>
        /// Determines if this circle intersects with the specified rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to test for intersection.</param>
        /// <returns>
        /// <c>true</c> if the circle and rectangle overlap; otherwise, <c>false</c>.
        /// </returns>
        public bool Intersects(Rectangle rectangle)
        {
            // Find the closest point on the rectangle to the circle's center.
            // This ensures the point lies within the rectangle's bounds.
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right),
                MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom)
            );

            // Calculate the vector from the circle's center to the closest point
            Vector2 direction = Center - closestPoint;

            // Calculate the squared distance (avoids the costlier square root operation)
            float distanceSquared = direction.LengthSquared();

            // The circle and rectangle intersect if this distance is less than the circle's radius squared.
            return ((distanceSquared > 0) && (distanceSquared < Radius * Radius));
        }
    }
}