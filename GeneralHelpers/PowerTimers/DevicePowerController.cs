using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralHelpers.PowerTimers
{
        /*
     * 1. Get current Power State
     * Have a power state request change - Async or task based
     * report if device is warming or cooling
     * parameter for length of cooling and warming
     * event fires to send a power on or a power off command - 
     * keep firing this off until the power state match the last request
     * when device is warming or cooling will ignore any negated state change
     * */

    /// <summary>
    /// enum to track which command needs to be fire off
    /// </summary>
    public enum eDisplayCommandToFire
    {
        PowerOn = 1,
        PowerOff = 0
    }
    /// <summary>
    /// This is used to request power on/off commands
    /// device will fire events when you can send the power on and power off request
    /// </summary>
    public class DevicePowerController
    {
        #region Fields

        bool isPower;

        int warmingTime;
        int coolingTime;

        eDisplayCommandToFire cmd;

        CancellationTokenSource token;

        JTimer warmingTimer;
        JTimer coolingTimer;


        #endregion

        #region Properties

        /// <summary>
        /// Getter to see if Device is Cooling
        /// </summary>
        public bool IsWarming { get; private set; }

        /// <summary>
        /// Getter to see if Device is Cooling
        /// </summary>
        public bool IsCooling { get; private set; }


        #endregion

        #region Delegates

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isCooling"></param>
        /// <param name="isWarming"></param>
        public delegate void DisplayTimerState(bool isCooling, bool isWarming);

        /// <summary>
        /// Return an Enum telling the parent class which command to be sent
        /// </summary>
        /// <param name="cmd"></param>
        public delegate void DisplayCommandToSend(eDisplayCommandToFire cmd);

        #endregion

        #region Events

        /// <summary>
        /// Subscribe to this Event to get when the Device is currently Cooling
        /// When it is currently warming and when it is neither
        /// </summary>
        public event DisplayTimerState OnDisplayTimerChange;


        /// <summary>
        /// Event that reports what command was last requested
        /// </summary>
        public event DisplayCommandToSend OnCommandToSend;

        #endregion

        #region Constructors

        /// <summary>
        /// Base Constructor
        /// </summary>
        /// <param name="warmupTime">in MS</param>
        /// <param name="cooldownTime">in MS</param>
        public DevicePowerController(int warmupTime, int cooldownTime)
        {
            warmingTime = warmupTime;
            coolingTime = cooldownTime;
            IsWarming = false;
            IsCooling = false;
            isPower = false;
        }

        #endregion

        #region Internal Methods

        private async Task ChangePower(bool pwr)
        {
            try
            {
                bool setState = pwr;
                if (token != null && !token.Token.IsCancellationRequested)
                {
                    token.Cancel();
                }

                token = new CancellationTokenSource();

                if (pwr)
                {
                    await Task.Run(() =>
                    {
                        while (IsCooling )
                        {

                        }
                        if (!token.Token.IsCancellationRequested && setState)
                            OnCommandToSend(cmd);

                    });

                }
                else
                {
                    await Task.Run(() =>
                    {
                        while (IsWarming)
                        {

                        }
                        if (!token.Token.IsCancellationRequested && !setState)
                            OnCommandToSend(cmd);
                    });

                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\n Error DevicePowerController Task ChangePower(bool pw) is: " + e);
            }


        }

        private void WarmingCallbackTimer(object obj)
        {
            IsWarming = false;
            OnDisplayTimerChange(IsWarming, IsCooling);

            warmingTimer.Stop();
            warmingTimer.Dispose();
        }

        private void CoolingCallbackTimer(object obj)
        {
            IsCooling = false;
            OnDisplayTimerChange(IsWarming, IsCooling);

            coolingTimer.Stop();
            coolingTimer.Dispose();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Call this Method when the power state of the device changes
        /// The library needs this to properly function as a helper method
        /// </summary>
        /// <param name="state"></param>
        public void PowerStateChange(bool state)
        {
            try
            {
                if(isPower != state)
                {
                    isPower = state;
                    if (isPower)
                    {
                        if (warmingTimer != null)
                        {
                            warmingTimer.Stop();
                            warmingTimer.Dispose();
                        }

                        warmingTimer = new JTimer(WarmingCallbackTimer, 0, warmingTime);
                        IsWarming = true;
                        isPower = true;

                    }
                    else
                    {
                        if (coolingTimer != null)
                        {
                            coolingTimer.Stop();
                            coolingTimer.Dispose();
                        }

                        coolingTimer = new JTimer(CoolingCallbackTimer, 0, coolingTime);
                        IsCooling = true;
                        isPower = false;

                    }

                    OnDisplayTimerChange(IsWarming, IsCooling);
                }


            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\n DevicePowerController PowerStateChange(bool state) error is: " + e);
            }

        }

        /// <summary>
        /// Call this method when you want to change the power state of the device. 
        /// </summary>
        /// <param name="state"></param>
        public async void PowerStateRequest(bool state)
        {
            try
            {
                if (state != isPower)
                {
                    cmd = (eDisplayCommandToFire)Convert.ToInt32(state);
                    await ChangePower(state);
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\n Error DevicePowerController PowerStateRequest(bool state) is: " + e);

            }


        }


        /// <summary>
        /// Cancel the current Power state request
        /// </summary>
        public void CancelPowerRequest()
        {
            try
            {
                token.Cancel();
                token.Dispose();
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\n Error DevicePowerController CancelPowerRequest() is: " + e);
            }

        }

        #endregion
    }
}
