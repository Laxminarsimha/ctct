using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Windows.Forms;

namespace GVK_Patch_Updates
{
    internal class AppPatchUpdates
    {
        #region Private members

        /// <summary>
        /// Returns name of the config file.
        /// </summary>
        private static readonly string configFileName = "Config.xml";
        /// <summary>
        /// Returns name of the application config folder name.
        /// </summary>
        private static readonly string configFolderName = "ApplicationConfig";

        #endregion

        #region Properties

        private static string ApplicationStartupPath
        {
            get
            {
                return Application.StartupPath;
            }
        }

        /// <summary>
        /// Returns config directory from where the application running.
        /// </summary>
        private static string ApplicationConfigDirectory
        {
            get
            {
                return Path.Combine(ApplicationStartupPath, configFolderName);
            }
        }

        /// <summary>
        /// Returns config file full path where application is running.
        /// </summary>
        private static string ApplicationConfigFilePath
        {
            get
            {
                return Path.Combine(ApplicationConfigDirectory, configFileName);
            }
        }

        /// <summary>
        /// Gets current application patch update files shared folder directory.
        /// </summary>
        private static string ServerPatchFilesDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings["PatchUpdatesServerPath"];
            }
        }

        /// <summary>
        /// Gets config file directory in patch updates machine.
        /// </summary>
        private static string ServerConfigDirectory
        {
            get
            {
                return Path.Combine(ServerPatchFilesDirectory, configFolderName);
            }
        }

        /// <summary>
        /// Gets config file full path in patch updates machine.
        /// </summary>
        private static string ServerConfigFilePath
        {
            get
            {
                return Path.Combine(ServerConfigDirectory, configFileName);
            }
        }

        /// <summary>
        /// Gets the oracle connection string for patch updates (i.e. dll published) server.
        /// </summary>
        public string OracleConnectionString
        {
            get
            {
                string hostName = ConfigurationManager.AppSettings["PatchUpdatesHost"];
                string port = ConfigurationManager.AppSettings["PatchUpdatesPortNumber"];
                string database = ConfigurationManager.AppSettings["PatchUpdatesServiceName"];
                string userName = ConfigurationManager.AppSettings["PatchUpdatesUserName"];
                string password = ConfigurationManager.AppSettings["PatchUpdatesPassword"];

                return string.Format(@"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))
                (CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={2})));User Id={3};Password={4}; Connection Timeout=60;Validate Connection = true;",
                       hostName, port, database, userName, password);
            }
        }

        #endregion

        #region Database Methods

        #region Oracle connections

        /// <summary>
        /// Gets oracle connection string specified in the Config.xml file.
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            string connString = "";
            string appPath = ApplicationConfigFilePath;
            XDocument configFile;

            if (File.Exists(appPath))
            {
                configFile = XDocument.Load(appPath);
            }
            else
            {
                configFile = XDocument.Load(ServerConfigFilePath);
            }

            var connectionStringName = from connStringName in configFile.Descendants("add")
                                       where (string)connStringName.Attribute("key") == "AppConnectionString"
                                       select connStringName;

            foreach (var item in connectionStringName)
            {
                connString = (string)item.Attribute("value").Value;
            }

            var connectionStrings = from connectionString in configFile.Descendants("add")
                                    where (string)connectionString.Attribute("name") == connString
                                    select connectionString;

            foreach (var item in connectionStrings)
            {
                connString = (string)item.Attribute("connectionString").Value;
            }
            return connString;
        }

        #endregion

        /// <summary>
        /// Gets latest released files or assembly list.
        /// </summary>
        /// <param name="setupNumber">Setupnumber for which to get assemblys or files.</param>
        /// <param name="patchNumber">Patch or build number for which to get assemblys or files.</param>
        /// <param name="maxPatchNumber">This out parameter returns the latest build number.</param>
        /// <returns>List of <c>PatchFile</c> instances.</returns>
        public static IList<PatchFile> GetPublishedFiles(int setupNumber, int patchNumber, out int maxPatchNumber, out int maxSetupNumber)
        {
            OracleDecimal oracleDecimal;
            IList<PatchFile> patchFiles = null;
            string strAppName = "";

            try
            {
                if (ConfigurationSettings.AppSettings["ApplicationName"] == null)
                {
                    strAppName = "";
                }
                else
                {
                    strAppName = (ConfigurationSettings.AppSettings["ApplicationName"].ToString());
                    strAppName = strAppName.ToUpper();
                }

                maxPatchNumber = 1;
                maxSetupNumber = 1;

                using (OracleConnection oracleConnection = new OracleConnection(GetConnectionString()))
                {
                    using (OracleCommand command = oracleConnection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = @"PATCH_UPDATES";
                        command.Parameters.Add(new OracleParameter("PIN_SETUP_NO", OracleDbType.Int32, setupNumber, ParameterDirection.Input));
                        command.Parameters.Add(new OracleParameter("PIN_PATCH_NO", OracleDbType.Int32, patchNumber, ParameterDirection.Input));
                        command.Parameters.Add(new OracleParameter("PIN_APP_NAME", OracleDbType.Varchar2, strAppName, ParameterDirection.Input));
                        command.Parameters.Add(new OracleParameter("PO_MAX_PATCH_NO", OracleDbType.Int32, ParameterDirection.Output));
                        command.Parameters.Add(new OracleParameter("PO_MAX_SETUP_NO", OracleDbType.Int32, ParameterDirection.Output));
                        command.Parameters.Add(new OracleParameter("POC_REFCURSOR", OracleDbType.RefCursor, ParameterDirection.Output));

                        if (oracleConnection.State != ConnectionState.Open)
                        {
                            oracleConnection.Open();
                        }
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                patchFiles = new List<PatchFile>(5);
                                while (reader.Read())
                                {
                                    patchFiles.Add(FillDataRecord(reader));
                                }
                            }
                            if (command.Parameters["PO_MAX_PATCH_NO"] != null && command.Parameters["PO_MAX_PATCH_NO"].Value != null)
                            {
                                oracleDecimal = (OracleDecimal)command.Parameters["PO_MAX_PATCH_NO"].Value;
                                if (oracleDecimal.IsInt)
                                {
                                    maxPatchNumber = oracleDecimal.ToInt32();
                                }
                            }
                            if (command.Parameters["PO_MAX_SETUP_NO"] != null && command.Parameters["PO_MAX_SETUP_NO"].Value != null)
                            {
                                oracleDecimal = (OracleDecimal)command.Parameters["PO_MAX_SETUP_NO"].Value;
                                if (oracleDecimal.IsInt)
                                {
                                    maxSetupNumber = oracleDecimal.ToInt32();
                                }
                            }
                        }
                    }
                }
                return patchFiles;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        

        /// <summary>
        /// Reads data from <c>OracleDataReader</c> instance and returns <c>PatchFile</c> instance.
        /// </summary>
        /// <param name="record"><c>OracleDataReader</c> instance to read.</param>
        /// <returns><c>PatchFile</c> instance.</returns>
        private static PatchFile FillDataRecord(IDataRecord record)
        {
            PatchFile patchFile = new PatchFile();

            if (!record.IsDBNull(record.GetOrdinal("SETUP_NO")))
            {
                patchFile.SetupNumber = (int)record.GetDecimal(record.GetOrdinal("SETUP_NO"));
            }
            patchFile.PatchId = (int)record.GetDecimal(record.GetOrdinal("PATCH_ID"));
            patchFile.PatchFileId = (int)record.GetDecimal(record.GetOrdinal("PATCH_FILE_ID"));
            if (!record.IsDBNull(record.GetOrdinal("FILE_NAME")))
            {
                patchFile.FileName = record.GetString(record.GetOrdinal("FILE_NAME"));
            }
            if (!record.IsDBNull(record.GetOrdinal("FILE_VERSION")))
            {
                patchFile.Version = record.GetString(record.GetOrdinal("FILE_VERSION"));
            }
            if (!record.IsDBNull(record.GetOrdinal("FILE_PATH")))
            {
                patchFile.FilePath = record.GetString(record.GetOrdinal("FILE_PATH"));
            }
            return patchFile;
        }

        #endregion

        #region File Download Methods

        /// <summary>
        /// if Config.xml file modifies in the server, then it copys to application path.
        /// </summary>
        /// <returns></returns>
        public static bool DownloadConfigFile()
        {
            string serverPath = ServerConfigFilePath;
            string appPath = ApplicationConfigFilePath;
            DateTime lastModifiedInServer = DateTime.MinValue;
            DateTime lastModifiedInApp = DateTime.MinValue;
            if (File.Exists(serverPath))
            {
                lastModifiedInServer = File.GetLastWriteTimeUtc(serverPath);
            }
            if (File.Exists(appPath))
            {
                lastModifiedInApp = File.GetLastWriteTimeUtc(appPath);
            }
            else if (!Directory.Exists(ApplicationConfigDirectory))
            {
                Directory.CreateDirectory(ApplicationConfigDirectory);
            }
            int result = lastModifiedInServer.CompareTo(lastModifiedInApp);
            AppPatchUpdates.WriteLog("Config file modifies in the server, then it copys to application path. Result: " + result);
            if (result > 0)
            {
                try
                {
                    File.Copy(serverPath, appPath, true);
                    AppPatchUpdates.WriteLog("Download from :" + serverPath + " to " + appPath + " is ended!");
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return true;
        }

        /// <summary>
        /// Copies specified list of patch files from published directory to application directory.
        /// </summary>
        /// <param name="patchFiles">List of <c>PatchFile</c> instances to copy.</param>
        /// <returns>True if copy succeeded.</returns>
        public static bool Download(IList<PatchFile> patchFiles)
        {
            if (patchFiles == null || patchFiles.Count == 0)
            {
                AppPatchUpdates.WriteLog("Copies specified list of patch files:  " + patchFiles.Count);
                return false;
            }
            foreach (var patchFile in patchFiles)
            {
                AppPatchUpdates.WriteLog("Copies specified list of patch files:  " + patchFile);
                Download(patchFile);
            }
            return true;
        }

        /// <summary>
        /// Copies specified patch file from published directory to application directory.
        /// </summary>
        /// <param name="patchFile"></param>
        /// <returns></returns>
        public static bool Download(PatchFile patchFile)
        {
            if (patchFile == null)
            {
                return false;
            }

            string serverPath = null;
            string applicationPath = null;
            string applicationDirectory = ApplicationStartupPath;

            AppPatchUpdates.WriteLog("Download method started!");
            serverPath = Path.Combine(patchFile.FilePath, patchFile.FileName);
            AppPatchUpdates.WriteLog("serverPath  -: " + serverPath);
            if (File.Exists(serverPath))
            {
                applicationPath = Path.Combine(applicationDirectory, patchFile.FileName);
                if (File.Exists(applicationPath))
                {
                    FileInfo fileInfo = new FileInfo(applicationPath);
                    if (fileInfo.IsReadOnly)
                    {
                        fileInfo.IsReadOnly = false;
                        fileInfo.Refresh();
                    }
                }
                File.Copy(serverPath, applicationPath, true);
                AppPatchUpdates.WriteLog("Download from :"+ serverPath+" to "+ applicationPath+" is ended!");
            }
            AppPatchUpdates.WriteLog("Download method  ended!");
            return true;

        }

        /// <summary>
        /// Gets Patch release info from application directory (i.e. setupNumber and build number).
        /// </summary>
        /// <param name="setupNumber"></param>
        /// <param name="buildNumber"></param>
        public static void GetPatchReleaseInfo(out int setupNumber, out int buildNumber)
        {
            setupNumber = 1;
            buildNumber = 1;

            char[] splitChar = { ':' };
            string line = null;
            string patchReleasePath = Path.Combine(ApplicationConfigDirectory, "PatchReleaseInfo.txt");
            string patchReleaseSeverPath = Path.Combine(ServerConfigDirectory, "PatchReleaseInfo.txt");

            AppPatchUpdates.WriteLog("GetPatchReleaseInfo started!");
            AppPatchUpdates.WriteLog("Patch release path :  " + patchReleasePath);
            if (File.Exists(patchReleasePath))
            {
                using (StreamReader reader = new StreamReader(patchReleasePath))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("Setup"))
                        {
                            setupNumber = Convert.ToInt32(line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries)[1]);
                        }
                        if (line.StartsWith("Build"))
                        {
                            buildNumber = Convert.ToInt32(line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries)[1]);
                        }
                    }
                }
            }
            else
            {
                File.Copy(patchReleaseSeverPath, patchReleasePath, true);
                AppPatchUpdates.WriteLog("Download from :" + patchReleaseSeverPath + " to " + patchReleasePath + " is ended!");
                AppPatchUpdates.GetPatchReleaseInfo(out setupNumber, out buildNumber);
            }
        }

        /// <summary>
        /// Saves the setup number and build number to patch release info file in the application directory.
        /// </summary>
        /// <param name="setupNumber"></param>
        /// <param name="buildNumber"></param>
        public static void UpdatePatchReleaseInfo(int setupNumber, int buildNumber)
        {
            string patchReleasePath = Path.Combine(ApplicationConfigDirectory, "PatchReleaseInfo.txt");
            AppPatchUpdates.WriteLog("Patch release path :  " + patchReleasePath);

            StringBuilder text = new StringBuilder();
            text.AppendLine(string.Format("Setup #:{0}", setupNumber));
            text.AppendLine(string.Format("Build/Patch Number #:{0}", buildNumber));

            //string text = string.Format("Setup #:{0}\nBuild #:{1}", setupNumber, buildNumber);

            using (StreamWriter outfile = new StreamWriter(patchReleasePath, false))
            {
                outfile.Write(text.ToString());
            }
        }

        #endregion

        #region Errorlog

        /// <summary>
        /// Trace Log
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLog(string message)
        {
            string traceFolder = "TRACE";
            string traceFolderPath = ApplicationStartupPath + "\\" + traceFolder;

            if (!Directory.Exists(traceFolderPath))
            {
                Directory.CreateDirectory(traceFolderPath);
            }

            FileTarget target = new FileTarget();

            target.Layout = "${longdate} ${logger} ${message}";
            target.FileName = "${basedir}/Trace/Log.txt";
            target.KeepFileOpen = false;
            target.Encoding = "iso-8859-2";

            AsyncTargetWrapper wrapper = new AsyncTargetWrapper();
            wrapper.WrappedTarget = target;
            wrapper.QueueLimit = 5000;
            wrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Discard;

            SimpleConfigurator.ConfigureForTargetLogging(wrapper, LogLevel.Trace);

            Logger logger = LogManager.GetLogger("TRACE LOG");
            logger.Trace(message);
        }

        #endregion
    }
}
