using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAStickyNoteDetector
{
    public class NetworkSettings
    {
        static public string HUB_IP = "";
        static public int HUB_PORT = 0;
        public static void Load()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("NetworkSetting.xml");
            XmlElement rootNote = xmlDoc.DocumentElement;
            HUB_IP = rootNote.SelectSingleNode("METAPLAN_HUB_IP").InnerText;
            HUB_PORT = Int32.Parse(rootNote.SelectSingleNode("METAPLAN_PORT").InnerText);
        }
    }
}
