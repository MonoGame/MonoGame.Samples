//---------------------------------------------------------------------------------
// Written by Michael Hoffman
// Find the full tutorial at: http://gamedev.tutsplus.com/series/vector-shooter-xna/
//----------------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using System;

namespace NeonShooter
{
    static class MathUtil
	{
		public static Vector2 FromPolar(float angle, float magnitude)
		{
			return magnitude * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
		}
	}
}