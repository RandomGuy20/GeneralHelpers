using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Newtonsoft.Json.Linq;

namespace GeneralHelpers.CustomSmartObjectJson
{
    /// <summary>
    /// This will be used with SIMPL+ to read specific parameters from a JSON STRING.
    ///
    /// Will need to take in the string, find the needed parameters and then return the values for them.
    ///
    /// Make a DIGITAL, Analog, and Serial Version for this
    ///
    ///
    /// The required parameters need to come in in a comma separated string. 
    /// </summary>
    internal class JSONReader
    {

        #region Fields
        
        private string[] _valuesToGet ;
        
        private List<ushort> _valuesToReturn = new List<ushort>();

        #endregion

        #region properties

        #endregion

        #region Delegates

        public delegate void JSONReadCompleteEventHandler(ushort[] values);

        #endregion

        #region Events

        public JSONReadCompleteEventHandler JSONReadComplete { get; set; }

        #endregion


        #region Constructors

        public void Initialize(SimplSharpString valuesToGet)
        {
            try
            {
                _valuesToGet = valuesToGet.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < 10; i++)
                {
                    _valuesToReturn.Add(0);
                }
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

        public void TryGetValues(SimplSharpString dataIn)
        {
            try
            {
                JObject jsonObject = JObject.Parse(dataIn.ToString());


                for (int i = 0; i < _valuesToGet.Length; i++)
                {
                    foreach (var obj in jsonObject)
                    {
                        if (string.CompareOrdinal(_valuesToGet[i],obj.Key) == 0)
                        {
                            _valuesToReturn[i] = Convert.ToUInt16(obj.Value);
                        }
                    }
                }


            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine($"Error in JSONReader tryGetvalues is {e}");
            }
        }



        #endregion
    }
}
