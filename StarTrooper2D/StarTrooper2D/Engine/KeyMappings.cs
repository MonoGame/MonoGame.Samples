
using Microsoft.Xna.Framework.Input;

namespace StarTrooper2D
{

    public struct InputMappings
    {
        public bool SettingsSaved;

        public Keys MoveUp;
        public Keys MoveDown;
        public Keys MoveLeft;
        public Keys MoveRight;
        public Keys Fire;

        public MovementMethod AltMoveMethod; //switch to use analogue or digital control

        public Buttons AltMoveControl; //button or stick to use for control
        public Buttons AltMoveUp;
        public Buttons AltMoveDown;
        public Buttons AltMoveLeft;
        public Buttons AltMoveRight;
        public Buttons AltFire;

        public Keys ChangeTrooperFireButton;
        public Keys SaveSettings;
               
        public Buttons AltChangeTrooperFireButton;
        public Buttons AltSaveSettings;


        public bool InvertYLeftStick;
        public bool InvertYRightStick;

    }

    public enum MovementMethod
    {
        Digital,
        Analogue
    }
}
