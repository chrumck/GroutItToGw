using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace GroutItToGw
{
    public class AppSettings
    {
        //Fields and Properties------------------------------------------------------------------------------------------------//

        public string InputFolder { get; set; }
        public string ProcessedFolder { get; set; }
        public string OutputFolder { get; set; }

        private int folderScanSeconds;
        public int FolderScanSeconds 
        {
            get { return this.folderScanSeconds; }
            set { this.folderScanSeconds = (value > 1 && value <= 3600) ? value : folderScanSeconds; }
        }

        public string ExtCLICommand { get; set; }
        public string ExtCLICommandArgs { get; set; }

        //Constructors---------------------------------------------------------------------------------------------------------//

        public AppSettings()
        {
            this.InputFolder = @"\input";
            this.ProcessedFolder = @"\processed";
            this.OutputFolder = @"\output";
            this.folderScanSeconds = 10;

        }
        
        //Methods--------------------------------------------------------------------------------------------------------------//

        //ReadFromXML - overload for default settings file name
        public void ReadFromXML()
        {
            ReadFromXML("AppSettings.xml");
        }

        //ReadFromXML
        public void ReadFromXML(string fileName)
        {
            var settingsFile = new XmlDocument();
            settingsFile.Load(fileName);
            var settingsMainNode = settingsFile.SelectSingleNode("AppSettings");
            if (settingsMainNode == null)
                { throw new ArgumentException(fileName + " is empty of file format not correct. Settings not loaded."); }

            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var childNode in settingsMainNode.Cast<XmlNode>())
            {
                setPropertyFromXmlNode(childNode, properties);
            } 
        }

        //Helpers--------------------------------------------------------------------------------------------------------------//
        #region Helpers

        //savePropertyFromChildNode
        private void setPropertyFromXmlNode(XmlNode node, PropertyInfo[] properties)
        {
            var property = properties.FirstOrDefault(x => x.Name == node.Name);
            if (property != null)
            {
                if (property.PropertyType == typeof(int)) { property.SetValue(this, int.Parse(node.InnerText), null); }
                if (property.PropertyType == typeof(String)) { property.SetValue(this, node.InnerText, null); }

            }
        } 

        #endregion


        
    }
}
