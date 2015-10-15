using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

namespace XNAStickyNoteDetector
{
    public class AppParameters
    {
        static public int NOTE_SIZE;
        static public int MARKER_SIZE;
        static public void Load()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("MiscParameters.xml");
            XmlElement root = xmlDoc.DocumentElement;
            NOTE_SIZE = int.Parse(root.SelectSingleNode("NOTE_SIZE").InnerText, CultureInfo.InvariantCulture);
            MARKER_SIZE = int.Parse(root.SelectSingleNode("MARKER_SIZE").InnerText, CultureInfo.InvariantCulture);
        }
    }
}
