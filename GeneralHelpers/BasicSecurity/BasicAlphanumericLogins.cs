using Crestron.SimplSharp.CrestronDataStore;
using GeneralHelpers.Debugging;
using GeneralHelpers.EventScheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.BasicSecurity
{
    internal class BasicAlphanumericLogins
    {
        #region Fields

        private JTimer eventTimer;
        //private List<string> pinList = new List<string>();

        private HashSet<string> pinList = new HashSet<string>();

        FileChangeMonitor monitor;
        FileOperations fileOps;

        private string fileName = "Userpins.txt";


        bool isRegistered = false;

        DebugErrorTracker debug = new DebugErrorTracker();


        #endregion

        #region Properties


        #endregion

        #region Delegates

        public delegate void AddedPin(bool state);

        public delegate void RemovedPin(bool state);

        public delegate void LoginSuccess(bool state);

        public delegate void LoginFailed(bool state);

        #endregion

        #region Events

        public event AddedPin onAddedPin;

        public event RemovedPin onRemovedPin;

        public event LoginSuccess onLoginSuccess;

        public event LoginFailed onLoginFail;

        #endregion

        #region Constructors

        public BasicAlphanumericLogins()
        {
            try
            {
                //Console Command to add a Pin -A
                //Console Command to Remove a pin -D
                //Console COmmand to Read the file  -R
                //Look for the File, if it is not there add a backdoor of 2262


                if (!isRegistered)
                {
                    FileOperations.Debug = true;
                    fileOps = new FileOperations(fileName, "UIPinLogins");


                    if (fileOps.FileExists(fileName))
                    {

                        if (fileOps.ReadFromFile(out List<string> myList))
                        {
                            pinList = myList.ToHashSet();
                            pinList.Add("2262");
                        }
                        
                    }
                    else
                    {
                        fileOps.WriteToFile("2262",true);

                        if (fileOps.ReadFromFile(out List<string> myList))
                        {
                            pinList = myList.ToHashSet();
                        }
                    }

                    isRegistered = true;
                }
            }
            catch (Exception e)
            {
                debug.SendDebug($"Error in Password with Backdoor is: {e}");
            }
        }



        #endregion

        #region Internal Methods








        #endregion

        #region Public Methods


        public void AddPin(string pin)
        {
            try
            {
                pinList?.Add(pin);

                onAddedPin(pinList.Any(d => d == pin));

            }
            catch (Exception e)
            {
                debug.SendDebug($"Error Adding Pin is: {e}");
            }
        }

        public void RemovePin(string pin)
        {
            try
            {
                pinList?.Remove(pin);

                onAddedPin(!pinList.Any(d => d == pin));

            }
            catch (Exception e)
            {
                debug.SendDebug($"Error Removing Pin is: {e}");
            }
        }

        public void LoginAttempt(string pin)
        {
            try
            {
                onLoginSuccess(pinList.Any(d => d == pin));
                onLoginFail(!pinList.Any(d => d == pin));

            }
            catch (Exception e)
            {
                debug.SendDebug($"Error LoggingIn is: {e}");
            }
        }

        #endregion
    }
}
