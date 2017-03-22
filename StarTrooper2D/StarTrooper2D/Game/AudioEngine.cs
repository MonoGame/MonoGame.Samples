using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace StarTrooper2D
{
    public static class AudioEngine
    {

        //Ship Audio Effects
        private static SoundEffect shoot;
        private static SoundEffect die;
        private static SoundEffect music;
        private static SoundEffectInstance backgroundMusic;


        public static void LoadContent(ContentManager contentManager)
        {
            shoot = contentManager.Load<SoundEffect>(@"Sounds\shoot");
            die = contentManager.Load<SoundEffect>(@"Sounds\die");


            music = contentManager.Load<SoundEffect>(@"Music\music");
            backgroundMusic = music.CreateInstance();
            backgroundMusic.IsLooped = true;
        }

        public static void PlayShoot()
        {
            shoot.Play();
        }

        public static void PlayDie()
        {
            die.Play();
        }

        public static void PlayMusic()
        {
            if (MediaPlayer.GameHasControl)
            {
                backgroundMusic.Play();
            }
        }
        public static void StopMusic()
        {
            if (backgroundMusic.State != SoundState.Stopped)
            {
                backgroundMusic.Stop();
            }
        }
    }
}
