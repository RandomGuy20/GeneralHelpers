using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralHelpers
{
    /// <summary>
    /// Class that takes commands from Simpl+
    /// </summary>
    public class ReadWriteFileSIMPL
    {
        #region Fields

        public static bool debug;
        public bool isInit = false;
        private string fileLocation;

        FileChangeMonitor monitor;
        List<string> data = new List<string>();

        FileOperations fileOps;
        #endregion

        #region properties

        #endregion

        #region Delegates

        public delegate void SerialOutputChange(ushort val, SimplSharpString data);

        #endregion

        #region Events

        public SerialOutputChange onSerialChange { get; set; }

        #endregion

        #region Constructors

        public void CheckIfDataMatches(string fileData)
        {

            try
            {
                string path = fileLocation;
                string newData;
                bool dataChanged = false;

                data = fileData.Split('\n').ToList();

                Task.Run(() =>
                {
                    if (fileOps.ReadFromFile(out newData))
                    {
                        List<string> newLines = newData.Split('\n').ToList();

                        for (int i = 0; i < newLines.Count; i++)
                        {
                            if (data[i] != newLines[i])
                            {
                                newLines[i] = data[i];
                                dataChanged = true;
                            }

                        }

                        if (!dataChanged)
                            return;

                        WriteToFile(newLines, fileLocation);
                        ReadFromFile(fileLocation);


                    }
                    else
                    {
                        SendDebug("\nSIMPL Read From File - Could not write");
                    }
                });
            }
            catch (Exception e)
            {
                SendDebug("ReadWriteFileSimpl error in CheckDataMatches() error is: " + e);
            }

        }

        /// <summary>
        /// Initialize Method to get From SIMPL+
        /// </summary>
        /// <param name="FileLocation"></param>
        /// <param name="fileData"></param>
        public void Initialize(string FileLocation,SimplSharpString fileData)
        {
            try
            {

                Task.Run(() =>
                {
                    fileLocation = FileLocation;

                    fileOps = new FileOperations(fileLocation, "User");
                    fileOps.onFileChange += FileOps_onFileChange;
                    Task.Delay(5000).Wait();
                    if (fileOps.FileExists(fileOps.FullFilePath))
                    {
                        CrestronConsole.PrintLine("\nSIMPL File Exists, Reading from file: " + FileLocation);
                        ReadFromFile(FileLocation);
                    }
                    else
                    {
                        CrestronConsole.PrintLine("\nSIMPL File does not exist, creating file: " + FileLocation);
                        data = fileData.ToString().Split('\n').Select(line => line.Replace("\n", "")).ToList();
                        if (data[data.Count - 1].Equals("\n"))
                            data.RemoveAt(data.Count - 1);

                        WriteToFile(data, fileLocation);
                        Thread.Sleep(2000);
                        ReadFromFile(FileLocation);
                    }
                });


                isInit = true;                

            }
            catch (Exception e)
            {
                SendDebug("\n ReadWriteSIMPl error in constructor is: " + e.Message);
            }

        }




        #endregion

        #region Internal Methods

        internal void SetSerialOutputs(string[] lines)
        {
            for (ushort i = 0; i < lines.Length; i++)
            {
                onSerialChange(i, (SimplSharpString)lines[i]);
            }

        }

        internal void SendDebug(string data)
        {
            if (debug)
            {
                CrestronConsole.PrintLine("\nError ReadnWriteSIMPL File is: " + data);
                ErrorLog.Error("\nError ReadnWriteSIMPL File is: " + data);
            }
        }

        private void Monitor_onFileChanged()
        {
            
        }

        private void FileOps_onFileChange()
        {
            ReadFromFile(fileLocation);
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Write to gfile
        /// </summary>
        /// <param name="fileData">the data</param>
        /// <param name="filePath">where the file is</param>
        public void WriteToFile(SimplSharpString fileData,SimplSharpString filePath)
        {

            try
            {
                string _data = fileData.ToString();
                string path = filePath.ToString();

                Task.Run(() =>
                {
                    if (fileOps.WriteToFile(_data,  true))
                    {
                        SendDebug("\nSIMPl Write to file - success!");
                        ReadFromFile(filePath);
                    }

                    else
                        SendDebug("\nSIMPL Write To File - Could not write");
                });

            }
            catch (Exception e)
            {
                SendDebug("\nSIMPL Error Write To File: " + e.Message);
            }

        }

        /// <summary>
        /// Write List to file
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="filePath"></param>
        public void WriteToFile(List<string>fileData,string filePath)
        {
            try
            {
                var _data = fileData;
                if (fileOps.WriteToFile(_data,  true))
                {
                    SendDebug("\nSIMPl Write to file - success!");
                    ReadFromFile(filePath);
                }
                else
                    SendDebug("\nSIMPL Write To File - Could not write");




            }
            catch (Exception e)
            {
                SendDebug("\nSIMPL Error Write To File: " + e.Message);
            }
        }

        /// <summary>
        /// Read Data from file
        /// </summary>
        /// <param name="filePath"></param>
        public void ReadFromFile(SimplSharpString filePath)
        {
            try
            {

                string path = filePath.ToString();

                Task.Run(() =>
                {
                    if (fileOps.ReadFromFile(out data))
                    {
                        SetSerialOutputs(data.ToArray());
                        SendDebug("\nSIMPl Read From file - success!");
                    }
                    else
                    {
                        SendDebug("\nSIMPL Read From File - Could not write");
                    }
                });


            }
            catch (Exception e)
            {
                SendDebug("\nSIMPL Error Read From File: " + e.Message);
            }
        }

        public void ReadFromFile(string filePath)
        {
            try
            {

                string path = filePath.ToString();

                Task.Run(() =>
                {
                    if (fileOps.ReadFromFile(out data))
                    {
                        SetSerialOutputs(data.ToArray());
                        SendDebug("\nSIMPl Read From file - success!");
                    }
                    else
                    {
                        SendDebug("\nSIMPL Read From File - Could not Read");
                    }
                });


            }
            catch (Exception e)
            {
                SendDebug("\nSIMPL Error Read From File: " + e.Message);
            }
        }

        /// <summary>
        /// Change a line item
        /// </summary>
        /// <param name="index">1 based index from Simpl Windows</param>
        /// <param name="newData">New Line to be added</param>
        public void ChangeLineItem(ushort index, SimplSharpString newData)
        {
            try
            {

                if (data.Count > 0)
                    data.Clear();

                Task.Run(() =>
                {
                    if (fileOps.ReadFromFile(out data))
                    {

                        SendDebug("FILEOPsSIMPL ChangeLineItem ReadFrom Filer Succesfully.");

                        data[index - 1] = newData.ToString();
                        fileOps.WriteToFile(data,  true);
                    }
                });



            }
            catch (Exception e)
            {
                SendDebug("ReadWriteSIMPL Error ChangeLineItem() is: " + e);
            }
        }

        /// <summary>
        /// Debug SIMPL
        /// </summary>
        /// <param name="val"></param>
        public void SetDebugSIMPL(ushort val)
        {
            debug = Convert.ToBoolean(val);
        }

        /// <summary>
        /// DebugReadWriteHelper
        /// </summary>
        /// <param name="val"></param>
        public void SetDebugReadWrite(ushort val)
        {
            FileOperations.Debug = Convert.ToBoolean(val);
        }

        #endregion
    }
}
