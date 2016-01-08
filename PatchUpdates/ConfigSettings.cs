using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Xml;

namespace GVK_Patch_Updates
{
    [RunInstaller(true)]
    public partial class ConfigSettings : Installer
    {
        public ConfigSettings()
        {
            InitializeComponent();
        }
        public override void Install(System.Collections.IDictionary stateSaver)
        {

            base.Install(stateSaver);

            string targetDirectory = Context.Parameters["targetdir"];

            string strConfigFilename = "CTODDataCollectionV2.exe.config";

            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(targetDirectory + strConfigFilename);

                foreach (XmlElement element in xmlDoc.DocumentElement)
                {
                    if (element.Name.Equals("oracle.dataaccess.client"))
                    {
                        //For Each xElement As XmlElement In XmlDoc.DocumentElement



                        foreach (XmlNode node in element.ChildNodes)
                        {
                            
                            if (node.ChildNodes[0].Attributes[0].Value.Equals("DllPath"))
                            {
                                node.ChildNodes[0].Attributes[1].Value = targetDirectory + "Data_Display_Oracle_Connection\\bin";
                            }


                        }
                    }
                }

                xmlDoc.Save(targetDirectory + strConfigFilename);
                //System.Configuration.ConfigurationManager.RefreshSection("oracle.dataaccess.client");
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
