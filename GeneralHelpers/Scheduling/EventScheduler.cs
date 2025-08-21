using Crestron.SimplSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralHelpers.EventScheduling
{
    public class EventScheduler
    {
        #region Fields


        private string fileName;
        //private string filepath;
        private string dir = "\\user\\Schedule\\";
        private string[] dayArray = new string[7] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        private int schedID;

        private bool registered = false;

        private JTimer eventTimer;
        private List<EventData> eventList = new List<EventData>();
        FileChangeMonitor monitor;

        FileOperations fileOps;

         #endregion

        #region properties

        /// <summary>
        /// Sets what Schedule you want to edit, or Gets the current schedule you are editing 0 based
        /// </summary>
        public int ScheduleID
        {

            get
            {
                try
                {
                    return schedID;
                }
                catch (Exception e)
                {
                    SendDebug("Error getting schedule ID is: " + e);
                    return schedID;
                }

            }
            set
            {
                try
                {
                    try
                    {
                        if (value >= 0 && value < eventList.Count)
                        {
                            schedID = eventList.FindIndex(id => id.EventID == value);
                            eventList.Clear();
                            eventList = DeserializeObject();
                            EventData data = eventList[schedID];
                            onScheduleEdit(data);
                            SetFormattedTimeFeedback();
                        }
                        else
                            SendDebug("Value is higher than there are schedules");
                    }
                    catch (Exception e)
                    {
                        SendDebug($"EventScheduler Error in ScheduleID is {e}");
                    }

                }
                catch (Exception e)
                {
                    SendDebug("public int ScheduleID setting value error is: " + e);
                }

            }
        }

        /// <summary>
        /// Set [ScheduleID] event name
        /// or get [ScheduleID] event name
        /// </summary>
        public string ScheduleName
        {
            get
            {
                return eventList[schedID].EventName;
            }
            set
            {
                try
                {
                    eventList[schedID].EventName = value;
                    onNameChange(eventList[schedID].EventName);
                    SaveSchedule();
                }
                catch (Exception e)
                {
                    SendDebug("string ScheduleName SET name is: " + e);
                }

            }
        }

        /// <summary>
        /// Set [ScheduleID] event days
        /// or get [ScheduleID] event days
        /// </summary>
        public bool[] Days
        {
            get
            {
                return eventList[schedID].Days;
            }
            set
            {
                try
                {
                    eventList[schedID].Days = value;
                    onDaysChange(eventList[schedID].Days);
                    SaveSchedule();
                }
                catch (Exception e)
                {
                    SendDebug("bool[] Days set error is: " + e);
                }

            }
        }

        /// <summary>
        /// Set [ScheduleID] event Hour
        /// or get [ScheduleID] event Hour
        /// </summary>
        public int Hour
        {
            get
            {
                return eventList[schedID].Hour;
            }
            set
            {
                try
                {
                    if (value < 24 && value >= 0)
                    {
                        eventList[ScheduleID].Hour = value;
                        onHoursChange(eventList[ScheduleID].Hour);
                        SaveSchedule();
                        SetFormattedTimeFeedback();
                    }
                }
                catch (Exception e)
                {
                    SendDebug("int Hour setting schedule hour is: " + e);
                }


            }
        }

        /// <summary>
        /// Set [ScheduleID] event minutes
        /// or get [ScheduleID] event minutes
        /// </summary>
        public int Minute
        {
            get
            {
                return eventList[schedID].Minute;
            }
            set
            {
                try
                {
                    if (value < 60 && value >= 0)
                    {
                        eventList[schedID].Minute = value;
                        onMinutesChange(eventList[schedID].Minute);
                        SaveSchedule();
                        SetFormattedTimeFeedback();

                    }
                }
                catch (Exception e)
                {
                    SendDebug("int Minute error is: " + e);
                }


            }
        }

        /// <summary>
        /// Set [ScheduleID] event active state
        /// or get [ScheduleID] event active state
        /// </summary>
        public bool ScheduleActive
        {
            get
            {
                return eventList[schedID].Active;
            }
            set
            {
                try
                {
                    eventList[schedID].Active = value;
                    onActiveChange(eventList[schedID].Active);
                    SaveSchedule();
                }
                catch (Exception e)
                {
                    SendDebug("bool ScheduleActive setting active error is: " + e);
                }

            }
        }

        public string FileLocation
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }

        public static bool Debug { get; set; }

        #endregion

        #region Delegates

        public delegate void ScheduleMatchEventHandler(int eventIndex, string eventName, bool isMatch);

        public delegate void ScheduleChangeEventHandler(EventData data);

        public delegate void ScheduleSavingEventHandler(bool state);

        public delegate void ScheduleHoursChange(int hours);

        public delegate void ScheduleMinutesChange(int minutes);

        public delegate void ScheduleDaysChange(bool[] days);

        public delegate void ScheduleNameChange(string name);

        public delegate void ScheduleActiveChange(bool active);

        public delegate void FormattedScheduler12Hours(string formattedTime);

        #endregion

        #region Events

        public event ScheduleMatchEventHandler onDateMatch;

        public event ScheduleChangeEventHandler onScheduleEdit;

        public event ScheduleSavingEventHandler onScheduleSave;

        public event ScheduleHoursChange onHoursChange;

        public event ScheduleMinutesChange onMinutesChange;

        public event ScheduleDaysChange onDaysChange;

        public event ScheduleNameChange onNameChange;

        public event ScheduleActiveChange onActiveChange;

        public event FormattedScheduler12Hours onFormattedTime;

        #endregion

        #region Constructors

        /// <summary>
        /// If json file exists, will read to populate info, otherwise it will create the file. 
        /// Max Events is 50
        /// If default file is made, all values will be either 0 or empty
        /// </summary>
        /// <param name="FileName">name of scheduler file</param>
        public EventScheduler(string FileName)
        {
            try
            {
                if (!registered)
                {
                    fileName = FileName.Contains(".json") ? FileName : FileName + ".json";




                    FileOperations.Debug = true;
                    fileOps = new FileOperations(fileName,"Schedule");
                    fileOps.onFileChange += FileOps_onFileChange;


                    if (fileOps.FileExists(fileName))
                    {
                        CrestronConsole.PrintLine("EventScheduler File Exists");
                        eventList = DeserializeObject();
                    }
                    else
                    {
                        // Create the scheduler
                        for (int i = 0; i < 20; i++)
                        {
                            eventList.Add(new EventData());
                            eventList[i].Active = false;
                            eventList[i].Days = new bool[] { false, false, false, false, false, false, false };
                            eventList[i].DaysString = dayArray;
                            eventList[i].EventID = i + 1;
                            eventList[i].EventName = "";
                            eventList[i].Hour = 0;
                            eventList[i].Minute = 0;
                        }


                        SerializeObject(eventList);
                    }

                    if (eventTimer != null)
                    {
                        eventTimer.Stop();
                        eventTimer.Dispose();
                    }

                    eventTimer = new JTimer(EventCallback);
                    schedID = 0;

                    registered = true;
                }

            }
            catch (Exception e)
            {
                SendDebug("\nError in EventScheduler COnstructor is message: " + e);
            }
        }



        #endregion

        #region Internal Methods

        private void EventCallback(object obj)
        {

            try
            {
                int index = Convert.ToInt32(obj);
                var jsonList = DeserializeObject();
                // Active must be true
                //Hour Must match
                //Minute muts match
                // Day must be active
                var matches = jsonList.Where(m => 
                                             m.Active  && 
                                             m.Days[(int)DateTime.Now.DayOfWeek] &&
                                             m.Hour == DateTime.Now.Hour && 
                                             m.Minute == DateTime.Now.Minute);

                var noMatches = jsonList.Except(matches).ToList();



                if (matches != null)
                {
                    foreach (var item in matches)
                        onDateMatch(item.EventID, item.EventName, true);
                }

                if(noMatches != null)
                {
                    foreach (var item in noMatches)
                        onDateMatch(item.EventID, item.EventName, false);
                }



            }
            catch (Exception e)
            {
                SendDebug("\nCheckDayCallBAck error is: " + e);
            }
     
        }

        private void ScheduleEditEventTrigger(EventData data)
        {
            onScheduleEdit(data);
        }


        internal void SendDebug(string data)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine("\nEventScheduler error at: " + data);
                ErrorLog.Error("\nEventScheduler error at: " + data);
            }

        }

        private List<EventData> DeserializeObject()
        {
            try
            {
                string jsonString = "";
                if (fileOps.ReadFromFile(out jsonString))
                {
                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    var list = JsonConvert.DeserializeObject<List<EventData>>(jsonString, jsonSettings);


                    return list;
                }

                else
                    return new List<EventData>();
                


            }
            catch (Exception e)
            {
                SendDebug("\nDeserializeObject error is: " + e);
                return new List<EventData>(); ;
            }

        }

        private void SerializeObject(List<EventData> eventList)
        {
            try
            {
                var settings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                if (fileOps.WriteToFile(JsonConvert.SerializeObject(eventList, settings),  true))
                {
                    SendDebug("EventScheduler Wrote to file");
                    //onScheduleSave(true);
                    SendDebug("EventScheduler calling on schedule Save");

                }
                else
                {
                    SendDebug("EventScheduler Did not write to file");
                }
                
            }
            catch (Exception e)
            {
                SendDebug("\nError in SerializeObject is: " + e);
            }

        }

        private void Monitor_onFileChanged()
        {
            eventList.Clear();
            eventList = DeserializeObject();
        }

        private void FileOps_onFileChange()
        {
            eventList.Clear();
            eventList = DeserializeObject();
        }

        private void SetFormattedTimeFeedback()
        {
            string fillData = "";
            if (Hour <= 11)
            {

                fillData = Hour == 0 ? "12" : Hour.ToString("D2");
            }
            else
            {
                fillData = Hour > 12 ? (Hour - 12).ToString("D2") : Hour.ToString();
            }

            string data = string.Format("{0}:{1} {2}", fillData, Minute.ToString("D2"), Hour > 11 ? "PM" : "AM");
            onFormattedTime(data);

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// If you do not save the schedule, it will not run. You need to save and Start it
        /// </summary>
        public void SaveSchedule()
        {
            try
            {

                SerializeObject(eventList);

            }
            catch (Exception e)
            {
                SendDebug("\nEventScheduler Save Schedule Error is: " + e.Message);
            }

        }

        /// <summary>
        /// Set every event to on
        /// </summary>
        public void StartSchedulerGlobal()
        {
            try
            {
                for (int i = 0; i < eventList.Count; i++)
                    eventList[i].Active = true;
              }
            catch (Exception e)
            {
                SendDebug("StartSchedulerGlobal() Error Starting All Schedules is: " + e);
            }
        }

        /// <summary>
        /// Set every event to off
        /// </summary>
        public void StopSchedulerGlobal()
        {
            try
            {
                for (int i = 0; i < eventList.Count; i++)
                    eventList[i].Active = false;
            }
            catch (Exception e)
            {
                SendDebug("StopSchedulerGlobal() Error Stopping All Schedules is: " + e);
            }
        }

        /// <summary>
        /// Will add an event to the collection
        /// </summary>
        /// <param name="data"></param>
        /// <param name="eventID"></param>
        public void AddEvent(EventData data, int eventID)
        {
            try
            {
                if (eventList.Count < 50)
                {
                    eventList.Add(data);
                    eventList[eventList.Count - 1].EventID = eventID;
                    SaveSchedule();
                }

            }
            catch (Exception e)
            {
                SendDebug("\nEventScheduler error is AddEvent is: " + e);
            }

        }

        /// <summary>
        /// Remove event based on ID
        /// </summary>
        /// <param name="eventID"></param>
        public void DeleteEvent(int eventID)
        {
            try
            {
                eventList.RemoveAt(eventList.FindIndex(id => id.EventID == eventID));
                SaveSchedule();
            }
            catch (Exception e)
            {
                SendDebug("\nEventScheduler error is DeleteEvent(int EventID) is: " + e);
            }
        }

        /// <summary>
        /// Remove event based on event Name
        /// </summary>
        /// <param name="eventName"></param>
        public void DeleteEvent(string eventName)
        {

            try
            {
                eventList.RemoveAll(n => n.EventName == eventName);
                SaveSchedule();
            }
            catch (Exception e)
            {

                SendDebug("\nEventScheduler error is DeleteEvent(string eventName) is: " + e);
            }
        }

        /// <summary>
        /// Increment the hours by 1
        /// </summary>
        public void IncrementHours()
        {
            try
            {
                if (Hour < 23)
                {
                    Hour++;
                }
                else
                {
                    Hour = 0;
                }

            }
            catch (Exception e)
            {
                SendDebug($"EventScheduler error IncrememntHours is {e}");
            }

        }

        public void DecrementHours()
        {
            try
            {
                if (Hour > 0)
                {
                    Hour--;
                }
                else
                {
                    Hour = 23;
                }
            }
            catch (Exception e)
            {
                SendDebug($"EventScheduler error Decrement hours is {e}");
            }

        }

        public void IncrementMinutes()
        {

            try
            {
                if (Minute == 59)
                {
                    Minute = 0;
                    Hour++;
                }
                else
                {
                    Minute++;
                }
            }
            catch (Exception e)
            {
                SendDebug($"EventScheduler error IncrememntMinutes is {e}");
            }
        }

        public void DecrementMinutes()
        {

            try
            {
                if (Minute == 0)
                {
                    Minute = 59;
                    Hour--;
                }
                else
                {
                    Minute--;
                }
            }
            catch (Exception e)
            {
                SendDebug($"EventScheduler error DecrementMinutes is {e}");
            }
        }

        #endregion*/
    }
}
