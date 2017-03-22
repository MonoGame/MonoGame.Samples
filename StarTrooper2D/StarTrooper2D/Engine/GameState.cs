using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace StarTrooper2D
{
    public class GameState
    {
        public static int PlayerLives;
        public static Vector2 Position;
        public static List<EnemyState> EmemyStates = new List<EnemyState>();
        public static List<ShotState> ShotStates = new List<ShotState>();

        //Game management related properties
        public static List<Sprite> m_Sprites = new List<Sprite>();
        public static List<Sprite> m_ZOrderedSprites = new List<Sprite>();
        public static List<Sprite> m_DeletedSprites = new List<Sprite>();
        public static List<Sprite> m_AddedSprites = new List<Sprite>();

        public static List<Text2D> m_Text2Ds = new List<Text2D>();
        public static List<Text2D> m_DeletedText2Ds = new List<Text2D>();
        public static List<Text2D> m_AddedText2Ds = new List<Text2D>();

        public static int shots;

        public static Trooper Trooper;

        public static int score;

        public static void NewGame()
        {
            m_Sprites.Clear();
            m_ZOrderedSprites.Clear();
            m_DeletedSprites.Clear();
            m_AddedSprites.Clear();
            m_Text2Ds.Clear();
            m_DeletedText2Ds.Clear();
            m_AddedText2Ds.Clear();
            Trooper = null;
            score = 0;
            shots = 0;
            if (GameConstants.ParticleManager != null) GameConstants.ParticleManager.Initialize();
        }

        public static void SaveEmemy()
        {
            EnemyState enemyState = new EnemyState();
            enemyState.Position = Vector2.Zero;
            enemyState.Health = 10;
            EmemyStates.Add(enemyState);
        }

        public static void SaveShot()
        {
            ShotState shotState = new ShotState();
            shotState.Position = Vector2.Zero;
            ShotStates.Add(shotState);
        }
    }

    public class EnemyState
    {
        public Vector2 Position;
        public int Health;
    }

    public class ShotState
    {
        public Vector2 Position;
    }
}
