using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Configuration;

namespace  GVK_Patch_Updates
{
    static class Program
    {
        private static string PttUserName
        {
            get;
            set;
        }

        private static int PttUserId
        {
            get;
            set;
        }

        /// <summary>
        /// The main entry point for the application.
        /// Main Method used to call the landing application and find the patch updates.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string[] param = null;
            //string pttUName = null;
            //int pttUid = 0;
            
            AppDomain.CurrentDomain.UnhandledException +=new UnhandledExceptionEventHandler(OnUnhandledExceptionPolicy);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppPatchUpdates.WriteLog("Patch Updates - Main method started");
            try
            {
                //FOR CONSOLE APPLICATION
                if (args.Length > 0)
                {
                    
                    param = args[0].Split(',');
                    PttUserName = param[0];
                    PttUserId = Convert.ToInt32(param[1]);
                    AppPatchUpdates.WriteLog("Patch Updates - Params found! User Name: " + PttUserName );  
                }
                
                GetPatchUpdateFiles();
                AppPatchUpdates.WriteLog("Patch Updates - Main method ended");
            }
            catch (Exception ex)
            {
                AppPatchUpdates.WriteLog(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Check For updates in ADOPT 
        /// This Exe will check for the updated adopt dlls and star t refering dlls.
        /// </summary>
        //static void CheckUpdates(string pttUserName,int pttUserId)
        //{
        //    #region Check For Updates
        //    string strAdoptversion = "1.0.0.";
        //    frmupdates obj2 = new frmupdates();
        //    string strapplicationpath = System.Windows.Forms.Application.StartupPath.ToString(); //Get application startup path
        //    string FilePath = "";
        //    ///Check for putty key, if not create puttt
        //    obj2.CreatePuttyKey(System.Configuration.ConfigurationManager.AppSettings["PuttyKeyPath"]);
        //    FilePath = strapplicationpath + "\\" + "CAS";
        //    obj2.Hide();
        //    obj2.Refresh();
        //    try
        //    {

        //        Hashtable htwritefilenames = new Hashtable();
        //        Hashtable htfilestobeupdated = new Hashtable();
        //        htfilestobeupdated = obj2.GetFileNamestobeUpdated();
        //        //This will return no of files to be updated
        //        string[] strdllnames = null;
        //        strdllnames = obj2.StrDllNames;
        //        if (strdllnames != null)
        //        {
        //            if (strdllnames.Length > 0)
        //            {
        //                for (int i = 0; i < strdllnames.Length; i++)
        //                {
        //                    if (!htwritefilenames.ContainsKey(strdllnames[i]))
        //                        htwritefilenames.Add(strdllnames[i], obj2.fileread(FilePath, strdllnames[i].ToString()));

        //                }

        //            }
        //        }

        //        if (htfilestobeupdated.Count > 0)
        //        {

        //            obj2.HtFilestoupdates = htfilestobeupdated;
        //            obj2.Show();
        //            if (obj2.HtFilesDownloaded != null)
        //            {
        //                if (obj2.HtFilesDownloaded.Count > 0)
        //                {
        //                    IDictionaryEnumerator myEnumerator = obj2.HtFilesDownloaded.GetEnumerator();
        //                    while (myEnumerator.MoveNext())
        //                    {
        //                        if (myEnumerator.Value.ToString().ToUpper() == "Y")
        //                        {
        //                            if (htwritefilenames.ContainsKey(myEnumerator.Key))
        //                            {
        //                                string strfileoutpath = "";
        //                                htwritefilenames[myEnumerator.Key] = myEnumerator.Key.ToString() + "#" + strAdoptversion + obj2.GetNewDLLMinorVersion(myEnumerator.Key.ToString(), out strfileoutpath);

        //                            }

        //                        }
        //                    }

        //                }
        //                //Finally updating  files
        //                IDictionaryEnumerator updatefile = htwritefilenames.GetEnumerator();
        //                string strfinalupdatedtext = "";
        //                int i = 0;
        //                while (updatefile.MoveNext())
        //                {
        //                    if (i == 0)
        //                    {
        //                        if (updatefile.Value != null)
        //                            strfinalupdatedtext = updatefile.Value.ToString();
        //                    }
        //                    else
        //                    {
        //                        if (updatefile.Value != null)
        //                            strfinalupdatedtext += "\r\n" + updatefile.Value.ToString();
        //                    }
        //                    i++;


        //                }

        //                obj2.fileupdate(FilePath, strfinalupdatedtext);

        //            }

        //        }

        //        obj2.Hide();
        //        obj2.PTTUserName = pttUserName;
        //        obj2.PTTUserId = pttUserId;
        //        Application.Run(obj2.callADopt()); //Show ADOPT Main Screen

        //    }
        //    catch (Exception ex)
        //    {
        //        obj2.WriteLog(ex.ToString());
        //        obj2.Hide();
        //        Application.Run(obj2.callADopt()); //if any error in ADOPT Update opening ADOPT Main screen with dlls updates
        //    }
        //    #endregion

        //}

        /// <summary>
        /// Catching UnHandled Exception method
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        static void OnUnhandledExceptionPolicy(Object Sender, UnhandledExceptionEventArgs e)
        {
            String InformationForLogging;
            Exception ex = e.ExceptionObject as Exception;

            if (ex != null)
            {
                // The unhandled Exception is CLS compliant
                // Extract the Information you want to know
                InformationForLogging = ex.ToString();
            }
            else
            {
                // The unhandled Exception is not CLS compliant
                // You can only handle this Exception as Object
                InformationForLogging = String.Format(
                   "Type: {0}{2}String: {1}",
                   e.ExceptionObject.GetType(),
                   e.ExceptionObject.ToString(),
                   Environment.NewLine);
            }


            if (!e.IsTerminating)
            {
                // Exception occurred in a thread pool or finalizer thread
                // Debugger launches only explicitly
            }
            else
            {
                // Exception occurred in managed thread
                // Debugger will also launch when not launched explicitly
            }
            // explicitly launch the debugger (Visual Studio)
            Debugger.Launch();
        }

        private static void GetPatchUpdateFiles()
        {
            int setupNumber = 1;
            int buildNumber = 1;
            int maxBuildNumber = 1;
            int maxSetupNumber = 1;
            IList<PatchFile> patchFiles = null;

            // Copy Config.xml file to application path.
            AppPatchUpdates.DownloadConfigFile();

            AppPatchUpdates.GetPatchReleaseInfo(out setupNumber, out buildNumber);

            try
            {
                AppPatchUpdates.WriteLog("setupNumber: " + setupNumber);
                AppPatchUpdates.WriteLog("buildNumber: " + buildNumber);

                patchFiles = AppPatchUpdates.GetPublishedFiles(setupNumber, buildNumber, out maxBuildNumber, out maxSetupNumber);
                
            }
            catch (Exception ex)
            {
                AppPatchUpdates.WriteLog(ex.ToString());
            }

            if (patchFiles != null && patchFiles.Count > 0)
            {
                AppPatchUpdates.WriteLog("GetPublishedFiles: Patch Files count are : " + patchFiles.Count);

                PatchUpdatesForm patchUpdatesForm = new PatchUpdatesForm(patchFiles);
                patchUpdatesForm.Show();

                AppPatchUpdates.UpdatePatchReleaseInfo(maxSetupNumber, maxBuildNumber);
            }
            else
            {
                AppPatchUpdates.WriteLog("GetPublishedFiles: PatchFiles count is zero or return null values!");
            }

            StartMainApplication();
        }

        private static void StartMainApplication()
        {
            string strLandingAppName = "";
            AppPatchUpdates.WriteLog("StartMainApplication method started!");
            strLandingAppName = ConfigurationManager.AppSettings["PatchUpdatesStartUpEXE"];
            AppPatchUpdates.WriteLog(strLandingAppName);
            string applicationPath = Path.Combine(Application.StartupPath, strLandingAppName);
            AppPatchUpdates.WriteLog(applicationPath);
            AppPatchUpdates.WriteLog("PTT User Name: " +PttUserName);

            try
            {
                if (String.IsNullOrEmpty(PttUserName))
                {
                    Process.Start(applicationPath);
                }
                else
                {
                    Process.Start(applicationPath, string.Format("{0},{1}", PttUserName, PttUserId));
                }
            }
            catch (Exception ex)
            {
                AppPatchUpdates.WriteLog(ex.ToString());
            }
            AppPatchUpdates.WriteLog("StartMainApplication method ended!");
        }
    }
}
