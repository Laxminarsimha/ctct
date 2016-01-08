using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using DCMTool;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace  GVK_Patch_Updates
{
    public partial class PatchUpdatesForm : Form
    {
        private IList<PatchFile> PatchFiles
        {
            get;
            set;
        }

        private string PttUserName
        {
            get;
            set;
        }

        private int PttUserId
        {
            get;
            set;
        }

        public PatchUpdatesForm()
        {
            InitializeComponent();
        }

        public PatchUpdatesForm(IList<PatchFile> patchFiles)
            : this()
        {
            this.PatchFiles = patchFiles;
        }

        public PatchUpdatesForm(string pttUserName, int pttUserId)
            : this()
        {
            this.PttUserName = pttUserName;
            this.PttUserId = pttUserId;
        }

        private void RunPatchUpdates()
        {
            CheckForIllegalCrossThreadCalls = false;

            this.Refresh();

            this.Visible = true;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                if (this.PatchFiles != null && this.PatchFiles.Count > 0)
                {
                    lblPatchUpdateStatus.Text = "Found new updates ...";
                    Application.DoEvents();

                    System.Threading.Thread.Sleep(500);

                    lblDownloadStatus.Text = "Downloading files ...";
                    Application.DoEvents();

                    AppPatchUpdates.Download(this.PatchFiles);

                    lblDownloadStatus.Text = "Download completed.";
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                AppPatchUpdates.WriteLog(ex.ToString());
                lblPatchUpdateStatus.Text = "Failed to download patch files.";
                lblPatchUpdateStatus.Refresh();
                Thread.Sleep(500);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        //public void ShowMainForm()
        //{
        //    frmMain mainForm = null;
        //    try
        //    {
        //        mainForm = new DCMTool.frmMain();
        //        mainForm.PTTUserName = this.PttUserName;
        //        mainForm.PTTUserId = this.PttUserId;

        //        this.Close();
        //        Application.Run(mainForm);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString(), "Error occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}        

        private void PatchUpdatesForm_Load(object sender, EventArgs e)
        {
            //#if !DEBUG
            RunPatchUpdates();
            //ShowMainForm();
            //StartMainApplication();
            //Thread.Sleep(2000);
            this.Close();
            //#endif
        }
    }
}
