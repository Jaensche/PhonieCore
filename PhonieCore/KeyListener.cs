using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhonieCore
{
    public class KeyListener
    {
        public delegate void KeyPressedHandler(char key);
        public event KeyPressedHandler OnKeyPressed;
        public delegate void KeyReleasedHandler(char key);
        public event KeyReleasedHandler OnKeyReleased;

        IGpioPin gpio26;


        public KeyListener()
        {
            gpio26 = Pi.Gpio[BcmPin.Gpio26];
            gpio26.InputPullMode = GpioPinResistorPullMode.PullUp;
            gpio26.PinMode = GpioPinDriveMode.Input;

            Task.Run(WatchKeys);
        }   

        public void WatchKeys()
        {
            bool greenButtonState = false;

            while(true)
            {
                Thread.Sleep(500);

                if(!gpio26.Read() && !greenButtonState)
                {
                    greenButtonState = true;
                    OnKeyPressed.Invoke('g');
                }
                else
                {
                    OnKeyReleased.Invoke('g');
                    greenButtonState = false;
                }
            }
        }      
    }
}
