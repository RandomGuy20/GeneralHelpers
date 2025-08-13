using Crestron.SimplSharp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GeneralHelpers.CustomSmartObjectJson
{
    public class StaticMicJSONObject
    {
        #region Fields

        private string name;

        private MicControlData currMic;

        private ushort[] volJoins = new ushort[] { 0, 0, 0 };

        private ushort deviceGaugeLevel = 0;

        private bool isMute = false;


        #endregion

        #region properties

        #endregion

        #region Delegates
        
        public delegate void StaticMicVolChangeEventHandler(ushort volUp, ushort volDn, ushort mute);

        public delegate void StaticMicCommandToSendChangeEventhandler(SimplSharpString data);
        




        #endregion

        #region Events

        public StaticMicVolChangeEventHandler StaticMicJSONObjectEvent { get; set; }
        
        public StaticMicCommandToSendChangeEventhandler StaticMicCommandToSendChangeEvent { get; set; }



        #endregion

        #region Constructors

        public void Initialize(SimplSharpString deviceName)
        {
            try
            {
                name = deviceName.ToString();
                
                currMic = new MicControlData
                {
                    Name = name,
                    ActionName = "",
                    state = false,
                    joinNumber = 0,
                    GaugeJoin = 0,
                    gaugeLevel = "0"
                };
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine($"Error in JSONRead initialize is {e}");

            }
        }

        #endregion

        #region Internal Methods




        #endregion

        #region Public Methods

        /// <summary>
        /// 1. When Value come sin assign all the joins I need
        /// </summary>
        /// <param name="dataIn"></param>
        public void TryGetValues(SimplSharpString dataIn)
        {
            try
            {
                string temp = dataIn.ToString();

                if (temp.StartsWith("Get data"))
                {
                    var newName = temp.Split();

                    if (string.CompareOrdinal(name, newName[2]) == 0)
                    {

                        MicControlData data = new MicControlData
                        {
                            Name = name,
                            ActionName = "",
                            joinNumber = Convert.ToUInt16(ushort.Parse(newName[3]) + 2),
                            GaugeJoin = Convert.ToUInt16(newName[3]),
                            gaugeLevel = currMic.gaugeLevel,
                            state = isMute
                        };

                        StaticMicCommandToSendChangeEvent?.Invoke(JsonConvert.SerializeObject(data));
                    }
                    
                }
                else
                {
                    var response = JsonConvert.DeserializeObject<MicControlData>(dataIn.ToString());

                    if (response == null || !string.Equals(response.Name,name,StringComparison.OrdinalIgnoreCase))
                        return;


                    currMic.GaugeJoin = response.GaugeJoin;


                    ushort state = Convert.ToUInt16(response.state);
                    if (response.ActionName == "mute")
                    {
                        StaticMicJSONObjectEvent?.Invoke(0, 0, state);
                        volJoins[0] = response.joinNumber;
                    }
                    else if (response.ActionName == "up")
                    {
                        StaticMicJSONObjectEvent?.Invoke(state, 0, 0);
                        volJoins[1] = response.joinNumber;
                    }
                    else if (response.ActionName == "down")
                    {
                        StaticMicJSONObjectEvent?.Invoke(0, state, 0);
                        volJoins[2] = response.joinNumber;
                    }


                }

                
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine($"Error in JSONReader tryGetvalues is {e}");
            }
        }




        public void SetVolMuteFeedback(ushort state)
        {
            try
            {
               
                currMic.joinNumber = volJoins[0];
                isMute = Convert.ToBoolean(state);
                currMic.ActionName = "mute";
                currMic.state = isMute;




                StaticMicCommandToSendChangeEvent?.Invoke(JsonConvert.SerializeObject(currMic));

            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine($"Error in JSONReader SetUIFeedback is {e}");
            }
        }

        /// <summary>
        /// Return the whole object to the UI
        /// </summary>
        /// <param name="volUp"></param>
        /// <param name="volDn"></param>
        /// <param name="mute"></param>
        /// <param name="Level"></param>
        public void SetVolUpFeedback(ushort state)
        {
            try
            {
                currMic.joinNumber = volJoins[1];
                currMic.state = Convert.ToBoolean(state);
                currMic.ActionName = "up";




                StaticMicCommandToSendChangeEvent?.Invoke(JsonConvert.SerializeObject(currMic));
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine($"Error in JSONReader SetUIFeedback is {e}");
            }
        }

        public void SetVolDNFeedback(ushort state)
        {
            try
            {
                currMic.joinNumber = volJoins[2];
                currMic.state = Convert.ToBoolean(state);
                currMic.ActionName = "down";




                StaticMicCommandToSendChangeEvent?.Invoke(JsonConvert.SerializeObject(currMic));
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine($"Error in JSONReader SetUIFeedback is {e}");
            }
        }



        public void SetVolGaugeFeedback(ushort Level)
        {
            try
            {
                currMic.gaugeLevel = Level.ToString();
                currMic.ActionName = "gauge";




                StaticMicCommandToSendChangeEvent?.Invoke(JsonConvert.SerializeObject(currMic));
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine($"Error in JSONReader SetUIFeedback is {e}");
            }
        }



        #endregion
    }
}
