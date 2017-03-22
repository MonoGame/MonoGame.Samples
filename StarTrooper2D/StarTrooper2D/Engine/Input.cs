#region Using directives

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#if ANDROID || IOS
using Microsoft.Devices.Sensors;
#endif

#endregion

namespace StarTrooper2D
{
    public sealed class Input
    {
        static InputMappings m_InputMappings = new InputMappings();
        
        static TouchCollection m_Touches = TouchPanel.GetState();
        #if ANDROID || IOS
        static Accelerometer m_accelerometer;
        #endif
        static Vector3 m_accelreading;
        static Vector3 m_Oldaccelreading;

        static float AccelerometerBuffer = 0.2f;

        static bool accelavailable = true;
        
        private Input() {}

        public static void Update()
        {
            m_Touches = TouchPanel.GetState();
            m_Oldaccelreading = m_accelreading;
#if ANDROID || IOS
#region Accelerometer section
            
            // If the accelerometer is null, it is initialized and started
            if (m_accelerometer == null & accelavailable)
            {
                // Instantiate the accelerometer sensor object
                m_accelerometer = new Accelerometer();

                if (m_accelerometer.State == SensorState.NotSupported)
                {
                    accelavailable = false;
                    m_accelerometer = null;
                    //goto AccelerometerNotImplemented;
                }
                // Add an event handler for the ReadingChanged event.
                if (m_accelerometer != null) m_accelerometer.CurrentValueChanged +=new EventHandler<SensorReadingEventArgs<AccelerometerReading>>(accelerometer_ReadingChanged);

                // The Start method could throw and exception, so use a try block
                try
                {
                    m_accelerometer.Start();
                }
                catch (AccelerometerFailedException exception)
                {
                    //throw exception;
                    Console.WriteLine("Accelerometer Not Implemented in this device, {0}", exception);
                    accelavailable = false;
                 }

            }
                 //AccelerometerNotImplemented:
                 //   Console.WriteLine("Accelerometer Not Implemented in this device");

#endregion
#endif
        }

        public static void Load_Defaults()
        {
            //Single Player settings
            m_InputMappings.MoveUp = Keys.Up;
            m_InputMappings.MoveDown = Keys.Down;
            m_InputMappings.MoveLeft = Keys.Left;
            m_InputMappings.MoveRight = Keys.Right;
            m_InputMappings.Fire = Keys.Space;

            m_InputMappings.AltMoveMethod = MovementMethod.Analogue;

            m_InputMappings.AltMoveControl = Buttons.LeftStick;
            m_InputMappings.AltMoveUp = Buttons.LeftThumbstickUp;
            m_InputMappings.AltMoveDown = Buttons.LeftThumbstickDown;
            m_InputMappings.AltMoveLeft = Buttons.LeftThumbstickLeft;
            m_InputMappings.AltMoveRight = Buttons.LeftThumbstickRight;
            m_InputMappings.AltFire = Buttons.A;

            m_InputMappings.ChangeTrooperFireButton = Keys.F;
            m_InputMappings.SaveSettings = Keys.S;

            m_InputMappings.AltChangeTrooperFireButton = Buttons.DPadDown;
            m_InputMappings.AltSaveSettings = Buttons.LeftShoulder;

            m_InputMappings.InvertYLeftStick = true;
            m_InputMappings.InvertYRightStick = false;

        }

        public static void Dispose()
        {
            // if the accelerometer is not null, call Stop
            if (accelavailable)
            {
                //try
                //{
                //    m_accelerometer.Stop();
                //    m_accelerometer = null;
                //}
                //catch (AccelerometerFailedException exception)
                //{
                //    throw exception;
                //}
            }
        }

#if ANDROID || IOS
        #region Accelerometer Event Handling
        /// <summary>
        /// The event handler for the accelerometer ReadingChanged event.
        /// BeginInvoke is used to pass this event args object to the UI thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void accelerometer_ReadingChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {

            m_accelreading.X = (float)e.SensorReading.Acceleration.X;
            m_accelreading.Y = (float)e.SensorReading.Acceleration.Y;
            m_accelreading.Z = (float)e.SensorReading.Acceleration.Z;
            StarTrooperGame.Trooper.Velocity += new Microsoft.Xna.Framework.Vector2((float)e.Value.Value.X, (float)e.Value.Value.Y);
        }

        #endregion

        #region Private Functions

        private static bool AccelerometerMoved()
        { return false; }
        #endregion
#endif

        #region Public Controls

        public static bool MoveUp()
        {
            return (m_accelreading.Y < m_Oldaccelreading.Y - AccelerometerBuffer);
        }

        public static bool MoveDown()
        {
            return (m_accelreading.Y > m_Oldaccelreading.Y + AccelerometerBuffer);
        }

        public static bool MoveLeft()
        {
            return (m_accelreading.X < m_Oldaccelreading.X - AccelerometerBuffer);
        }

        public static bool MoveRight()
        {
            return (m_accelreading.X > m_Oldaccelreading.X + AccelerometerBuffer);
        }

#endregion


#region Properties

//        public static TouchCollection touches { get {return m_Touches;} }
        public static Vector3 accelreading { get { return m_accelreading; } }
        public static InputMappings InputMappings { get { return m_InputMappings; } set { m_InputMappings = value; } }
        public static bool SettingsSaved { get { return m_InputMappings.SettingsSaved; } set { m_InputMappings.SettingsSaved = value; } }


#endregion
    }


}
