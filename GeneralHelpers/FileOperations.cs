using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace GeneralHelpers
{

    public class FileOperations
    {
        #region Fields

        private  string dir = "\\user\\";

        string fileLocation;

        FileChangeMonitor monitor;

        #endregion

        #region Properties


        public static bool Debug 
        { 
            get; 
            set; 
        }
        #endregion

        #region Delegates


        #endregion


        #region Events

        public event Action onFileChange;

        #endregion



        #region Constructors

        /// <summary>
        /// Just send the name of the file, and the sub directory, do not add any slashes
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="subDirectory"></param>
        public FileOperations(string filename, string subDirectory)
        {
            try
            {
          

                if(CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance)
                {

                    if (!dir.Contains(subDirectory))
                    {
                        dir += subDirectory;
                    }
                     

                     if (!Directory.Exists(dir))
                         Directory.CreateDirectory(dir);

                     if (subDirectory.EndsWith("\\"))
                     {
                        fileLocation = dir + filename;
                     }
                     else
                     {
                         fileLocation = dir + "\\" + filename;
                     }
                     
                     CrestronConsole.PrintLine("The directory and filepath to the file is: " + fileLocation); 

                }
                else
                {
                    string rootDir = Crestron.SimplSharp.CrestronIO.Directory.GetApplicationRootDirectory();
                    if (filename.Contains("\\"))
                    {
                        filename = filename.Replace("\\", "");
                    }

                    fileLocation = Path.Combine(rootDir, "user", filename);

                }

            }
            catch (Exception e)
            {
                SendDebug("FileOperations Error in constructor is: " + e);
            }
        }

        private void Monitor_onFileChanged()
        {
            onFileChange();
        }

        #endregion

        #region Internal Methods

        internal static void SendDebug(string data)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine("\nError File Operations is: " + data);
                ErrorLog.Error("\nError File Operations is: " + data);
            }
        }

        internal void InstantiateFileMonitor()
        {
            try
            {


                Task.Run(async () =>
               {
                   await Task.Delay(1000);

                   if (monitor == null && FileExists(fileLocation))
                   {
                       monitor = new FileChangeMonitor(fileLocation, 1000);
                       monitor.onFileChanged += Monitor_onFileChanged;
                       monitor.Start();
                   }
               });


            }
            catch (Exception e)
            {
                SendDebug($"Error FileOperations InstantiateFileMonitor() is {e}");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns if the file currently exists Use this method to either write or read
        /// </summary>
        /// <returns></returns>
        public bool FileExists(string fileName)
        {
            return File.Exists(fileLocation); 
        }

        /// <summary>
        /// Write to file 
        /// If it exists leave Overwrite false and it Will append
        /// Overwirte is true: it will delete and overwrite the file
        /// If file does not exist it will auto write it
        /// </summary>
        /// <param name="fileData">Data for the file</param>
        /// <param name="overwriteFile">Do you want to overwrite if file is existing</param>
        /// <returns></returns>
        public bool WriteToFile(string fileData, bool overwriteFile)
        {
            try
            {

                // File exists and we want to overwrite it
                if (FileExists(fileLocation) && overwriteFile)
                {

                    File.Delete(fileLocation);

                    using (FileStream fs = new FileStream(fileLocation, FileMode.Create, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(fileData);
                    }

                    InstantiateFileMonitor();
                    return true;

                }
                // File exists and Do Not Overwrite it
                else if (FileExists(fileLocation) && !overwriteFile)
                {
                    using (FileStream fs = new FileStream(fileLocation, FileMode.Append, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(fileData);
                    }


                    return true;
                }
                //File doesnt exist
                else if(!FileExists(fileLocation))
                {

                    using (FileStream fs = new FileStream(fileLocation, FileMode.Create, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(fileData);
                    }
                    InstantiateFileMonitor();
                    return true;

                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                SendDebug("WriteToFile error is: " + e.Message);
                return false;
            }
        }


        /// <summary>
        /// Overload Method Write to File Write List String
        /// </summary>
        /// <param name="fileData">List<string> data</string></param>
        /// <param name="fileLocation">filelocation</param>
        /// <param name="overwriteFile">Ignored</param>
        /// <returns></returns>
        public bool WriteToFile(List<string> fileData, bool overwriteFile)
        {
            try
            {
                // File exists and we want to overwrite it
                if (FileExists(fileLocation) && overwriteFile)
                {

                    File.Delete(fileLocation);

                    using (FileStream fs = new FileStream(fileLocation, FileMode.Create, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        foreach (var line in fileData)
                            sw.WriteLine(line);
                    }

                    InstantiateFileMonitor();
                    return true;

                }
                // File exists and Do Not Overwrite it
                else if (FileExists(fileLocation) && !overwriteFile)
                {
                    InstantiateFileMonitor();
                    return true;
                }
                //File doesnt exist
                else if (!FileExists(fileLocation))
                {
                    using (FileStream fs = new FileStream(fileLocation, FileMode.Create, FileAccess.Write))
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        foreach (var line in fileData)
                            sw.WriteLine(line);
                    }
                    InstantiateFileMonitor();


                    return true;

                }
                else
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                SendDebug("WriteToFile error is: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Basic text return as a list
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public  bool ReadFromFile(out List<string> fileData)
        {
            try
            {

                if (FileExists(fileLocation))
                {
                    var lines = File.ReadAllLines(fileLocation);
                    fileData = lines.ToList();
                    return true;
                }
                else
                {
                    fileData = new List<string>();
                    return false;
                }
                    

            }
            catch (Exception e)
            {
                SendDebug("Error Read From File isL " + e.Message);
                fileData = new List<string>();
                return false;
            }
        }
        /// <summary>
        /// Read From File
        /// </summary>
        /// <param name="fileData">The File Data</param>
        /// <param name="filePath">The File Path</param>
        /// <returns></returns>
        public bool ReadFromFile(out string fileData)
        {
            try
            {
                if (FileExists(fileLocation))
                {
                    fileData = File.ReadAllText(fileLocation);
                    return true;
                }
                else
                {
                    fileData = "";
                    return false;
                }
            }
            catch (Exception e)
            {
                SendDebug("Error Read From File is " + e.Message);
                fileData = "";
                return false;
            }
        }


        /// <summary>
        /// If file was deleted will return a 1, if file does not exist, it will be a 0, if file was not deleted or error, will be -1 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public int DeleteFile()
        {
            try
            {
                int val;

                if (FileExists(fileLocation))
                {
                    File.Delete(fileLocation);
                    val = FileExists(fileLocation) ? -1 : 1;
                    monitor.Stop();
                    monitor.Dispose();
                }
                else
                    val = 0;

                return val;
            }
            catch (Exception e)
            {
                SendDebug("Error at Delet File is: " + e);
                return -1;
            }
        }


        #endregion
    }
}
