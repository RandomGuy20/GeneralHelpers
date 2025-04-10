using GeneralHelpers.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralHelpers.PowerTimers
{
    internal class ShutdownTimer
    {
        #region Fields

        private int countdownTime;

        bool isPause = false;
        bool isRunning = false;

        CancellationTokenSource cts;
        DebugErrorTracker debug = new DebugErrorTracker();


        #endregion

        #region Properties


        #endregion

        #region Delegates

        internal delegate void SecondRemainingEventHandler(string secondsRemaining);

        internal delegate void TimerStateChangedEventHandler(bool running, bool paused, bool ended);



        #endregion

        #region Events

        public event SecondRemainingEventHandler onSecondsRemainingChangeEvent;

        public event TimerStateChangedEventHandler onTimerStateChangedEvent;

        #endregion

        #region Constructors
        /// <summary>
        /// Send time in base seconds i.e NOT Milliseconds 1 = 1 second 
        /// </summary>
        /// <param name="time"></param>
        public ShutdownTimer(int time)
        {
            countdownTime = time ;
        }

        #endregion

        #region Internal Methods


        internal async Task InitiateCountDownTimer()
        {
            try
            {

                if (isRunning)
                    return;

                if (cts != null && !cts.Token.IsCancellationRequested)
                {
                    cts.Cancel();
                }
                isRunning = true;

                cts = new CancellationTokenSource();



                await Task.Run(() => CoundownTimerWorker(cts.Token), cts.Token);
            }
            catch (Exception e)
            {
                debug.SendDebug($"ERrror in Task InitiateCountDownTimer() is {e}");
            }
            finally
            {
                cts?.Dispose();
                isRunning = false;
            }
        }

        internal async void CoundownTimerWorker(CancellationToken token)
        {
            int timer = countdownTime;

            onTimerStateChangedEvent?.Invoke(true, isPause, false);
            while (!token.IsCancellationRequested && timer > 0)
            {

                if (!isPause)
                {
                    timer--;
                    onSecondsRemainingChangeEvent?.Invoke(timer.ToString());

                   await Task.Delay(1000,token);
                }

                
            }
            onTimerStateChangedEvent?.Invoke(false, isPause, true);

            return;
        }

        #endregion

        #region Public Methods

        public async void StartTimer()
        {
            await InitiateCountDownTimer();
        }

        public void EndTimer()
        {

            if (!isRunning)
                return;

            cts?.Cancel();
            isRunning = false;
            isPause = false;

            onTimerStateChangedEvent?.Invoke(isRunning, isPause, true);
        }

        public void CancelTimer()
        {

            if (!isRunning)
                return;

            cts?.Cancel();
            isRunning = false;
            isPause = false;

            onTimerStateChangedEvent?.Invoke(isRunning, isPause, false);
        }

        public void PauseTimer()
        {

            if (!isRunning)
                return;

            isPause = !isPause;
            isRunning = isPause;
            onTimerStateChangedEvent?.Invoke(isRunning, isPause, false);


        }

        #endregion
    }
}
