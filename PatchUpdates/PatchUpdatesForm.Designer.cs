namespace  GVK_Patch_Updates
{
    partial class PatchUpdatesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPatchUpdateStatus = new System.Windows.Forms.Label();
            this.lblDownloadStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblPatchUpdateStatus
            // 
            this.lblPatchUpdateStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPatchUpdateStatus.ForeColor = System.Drawing.Color.Navy;
            this.lblPatchUpdateStatus.Location = new System.Drawing.Point(63, 33);
            this.lblPatchUpdateStatus.Name = "lblPatchUpdateStatus";
            this.lblPatchUpdateStatus.Size = new System.Drawing.Size(300, 23);
            this.lblPatchUpdateStatus.TabIndex = 0;
            this.lblPatchUpdateStatus.Text = "Checking for updates ...";
            // 
            // lblDownloadStatus
            // 
            this.lblDownloadStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDownloadStatus.ForeColor = System.Drawing.Color.Navy;
            this.lblDownloadStatus.Location = new System.Drawing.Point(63, 66);
            this.lblDownloadStatus.Name = "lblDownloadStatus";
            this.lblDownloadStatus.Size = new System.Drawing.Size(300, 23);
            this.lblDownloadStatus.TabIndex = 0;
            // 
            // PatchUpdatesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(417, 228);
            this.ControlBox = false;
            this.Controls.Add(this.lblDownloadStatus);
            this.Controls.Add(this.lblPatchUpdateStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PatchUpdatesForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PatchUpdatesForm";
            this.Load += new System.EventHandler(this.PatchUpdatesForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblPatchUpdateStatus;
        private System.Windows.Forms.Label lblDownloadStatus;
    }
}