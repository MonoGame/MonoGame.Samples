using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarTrooper2D
{
    public static class GameFunctions
    {
        static Random m_Random = new Random();

        public static void Add(Sprite sprite)
        {
            GameState.m_AddedSprites.Add(sprite);
        }

        public static void Add(Text2D text)
        {
            GameState.m_AddedText2Ds.Add(text);
        }

        public static void Remove(Sprite sprite)
        {
            GameState.m_DeletedSprites.Add(sprite);
        }

        public static Random Random { get { return m_Random; } }

        //  a handy little function that gives a random float between two
        // values. This will be used in several places in the sample, in particilar in
        // ParticleSystem.InitializeParticle.
        public static float RandomBetween(float min, float max)
        {
            return min + (float)m_Random.NextDouble() * (max - min);
        }

        public static List<Sprite> GetCollidedSprites(Sprite sprite)
        {
            List<Sprite> collisionList = new List<Sprite>();
            foreach (Sprite s in GameState.m_Sprites)
            {
                if (s != sprite && s.CollidesWith(sprite))
                    collisionList.Add(s);
            }

            if (collisionList.Count != 0)
                return collisionList;
            return null;
        }

    }
}
