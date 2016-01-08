
namespace GVK_Patch_Updates
{
    /// <summary>
    /// The PatchFile class contains the publish file details.
    /// </summary>
    public class PatchFile
    {
        /// <summary>
        /// Gets or sets the application setup number.
        /// <remarks>If an application have multiple setus with same database schema.</remarks>
        /// </summary>
        public int SetupNumber
        {
            set;
            get;
        }

        /// <summary>
        /// Gets or sets an unique id for path or build.
        /// </summary>
        public int PatchId
        {
            set;
            get;
        }

        /// <summary>
        /// Gets or sets an unique id for patch file.
        /// </summary>
        public int PatchFileId
        {
            set;
            get;
        }

        /// <summary>
        /// Gets or sets build number.
        /// </summary>
        public int PatchNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of file or assembly.
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets assembly or file version.
        /// </summary>
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the publish path of file or assembly.
        /// </summary>
        public string FilePath
        {
            get;
            set;
        }
    }
}
