namespace StarTrooper2D
{
    static class GameConstants
    {
        //Audio Test value
        public static bool UserPromptedAlready = false;
        public static bool canPlay = false;

        //ScreenSize Values
        //GameConstants.BackBufferWidth = 1280;
        //    GameConstants.BackBufferHeight = 720;
        public static int BackBufferWidth = 1280; // 1024; // 480;
        public static int BackBufferHeight = 720; // 768; // 800;

        //Managers
        public static ParticleManager ParticleManager;

        //Game Constants
        private static int m_TargetFrameRate = 30;
        private const int m_ScreenBuffer = 10;

        public static int ScreenBuffer { get { return m_ScreenBuffer; } }

        //Game Assets
        public static Condor Condor;
        public static Fire Fire;


        public static Text2D ScoreText;

        public static Text2D ShotsText;

        //camera constants
        public const float CameraHeight = 250.0f;
        public const float PlayfieldSizeX = 70f;
        public const float PlayfieldSizeY = 115f;
		//ship constants
		public const float ShipSpeedAdjustment = 0.1f;
		public const float ShipTurnSpeed = 0.1f;
		public const float ShipCollisionSize = 10f;
		public const float ShipSpawnTime = 50f;
		public const float ShipScale = 0.009f;
		//asteroid constants
        public const int NumAsteroids = 5;
        public const float AsteroidMinSpeed = 10f;
        public const float AsteroidMaxSpeed = 30.0f;
        public const float AsteroidSpeedAdjustment = 0.5f;
		public const float AsteroidCollisionSize = 10f;
		public const float AsteroidScale = 0.009f;
        //bullet constants
        public const int NumBullets = 5;
        public const float BulletSpeedAdjustment = 5.0f;
		public const float BulletCollisionSize = 2f;
		public const float BulletScale = 0.02f;
		//player stats
		public const int PlayerLives = 3;
		public const int MaxLevel = 3;
    }

}
