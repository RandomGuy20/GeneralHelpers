using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;
using GeneralHelpers.Debugging;
using GeneralHelpers.EventScheduling;

namespace GeneralHelpers.BasicSecurity
{
    /// <summary>
    /// Will Write to a new folder on the machine called userData
    /// will go to a file called minilogs if logins if it is there it will completely overwrite it
    /// The data is going to be ecrypted for future recall
    /// 
    /// </summary>
    internal class BasicPasswordWithBackdoor
    {
        #region Fields

        private JTimer eventTimer;
        private List<EventData> eventList = new List<EventData>();
        FileChangeMonitor monitor;

        FileOperations fileOps;

        private string dir = "\\user\\UIOperationsUsers\\";


        private CrestronDataStore dataStore;


        DebugErrorTracker debug = new DebugErrorTracker();


        #endregion

        #region Properties


        #endregion

        #region Delegates

        public delegate void AddedUser(bool state);

        public delegate void RemovedUser(bool state);

        public delegate void SuccesfulLogin(bool state);

        #endregion

        #region Events

        #endregion

        #region Constructors

        public BasicPasswordWithBackdoor()
        {
            try
            {

                CrestronDataStoreStatic.InitCrestronDataStore(); 
                CrestronDataStoreStatic.GlobalAccess = CrestronDataStore.CSDAFLAGS.OWNERREADWRITE & CrestronDataStore.CSDAFLAGS.OTHERREADWRITE;


                // Add console command to read data
                //add console command to add a user
                // add console command to remove a user
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


        public void AddUsernameAndPassword(string username, string password)
        {
            try
            {


            }
            catch (Exception e)
            {
                debug.SendDebug($"Error Adding USername and Password is: {e}");
            }
        }

        public void RemoveUsername()
        {

        }

        public void LoginAttempt(string username, string password)
        {
        }

        #endregion
    }
}
