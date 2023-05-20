using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System;
using Rage;
using static CornyFlakezPlugin.CalloutCommons;

namespace CornyFlakezPlugin
{
    public static class DataManagement
    {
        public static string rootDirectory = @"";
        private const string dataFileFolder = @"\Plugins\LSPDFR\CornyFlakezPlugin\";

        public static XmlDocument GetAgencySettings()
        {
            var baseDoc = XDocument.Load(rootDirectory + @"\LSPDFR\data\agency.xml");
            if (Directory.Exists(rootDirectory + @"\LSPDFR\data\custom\"))
            {
                var customDocs = Directory.GetFiles(rootDirectory + @"\LSPDFR\data\custom\")
                    .Where(f => f.Split('\\')[f.Split('\\').Length - 1].StartsWith("agency") && f.EndsWith(".xml"));

                List<string> scriptNames = new List<string>();
                foreach (XElement agency in baseDoc.Descendants("Agency"))
                {
                    if (agency.Element("ScriptName") != null)
                        scriptNames.Add(agency.Element("ScriptName").Value);
                }
                foreach (string customDocPath in customDocs)
                {
                    var customDoc = XDocument.Load(customDocPath);

                    foreach (XElement agency in customDoc.Descendants("Agency"))
                    {
                        if (agency.Element("ScriptName") is null)
                            continue;
                        string currentScriptName = agency.Element("ScriptName").Value;
                        if (scriptNames.Contains(currentScriptName))
                        {
                            XElement xe = baseDoc.Descendants("Agency")
                                .Where(e => e.Element("ScriptName").Value == currentScriptName).Single();
                            xe.ReplaceWith(agency);
                        }
                        else
                        {
                            baseDoc.Element("Agencies").Add(agency);
                            scriptNames.Add(currentScriptName);
                        }
                    }
                }
            }
            XmlDocument returnDoc = new XmlDocument();
            using (XmlReader xr = baseDoc.CreateReader())
            {
                returnDoc.Load(xr);
            }
            return returnDoc;
        }
   
        public static XmlDocument GetFileData(string filename)
        {
            XmlDocument doc = new XmlDocument();
            string dir = rootDirectory + dataFileFolder;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (File.Exists(dir + filename))
                doc.Load(dir + filename);
            else
            {
                switch (filename)
                {
                    case "WeaponLoadouts.xml":
                        doc.LoadXml(
                            "<?xml version=\"1.0\"?>" +
                            "\n<WeaponLoadouts>" +
                            "\n  <OnDuty>" +
                            "\n" +
                            "\n  </OnDuty>" +
                            "\n  <OffDuty>" +
                            "\n" +
                            "\n  </OffDuty>" +
                            "\n</WeaponLoadouts>");
                        break;
                    default:
                        throw new ArgumentException(filename);
                }
                doc.Save(dir + filename);
            }
            return doc;
        }

        public static InitializationFile GetIniFile(string filename = "settings.ini")
        {
            string pathToIniFile = rootDirectory + dataFileFolder + filename;
            InitializationFile ini = new InitializationFile(pathToIniFile);
            if (!File.Exists(pathToIniFile))
            {
                ini.Create();
                ini.Write("Officer", "Name", "Officer");
                ini.Write("Radio", "Division", 1);
                ini.Write("Radio", "Unit", "Lincoln");
                ini.Write("Radio", "Beat", 18);
            }
            return ini;
        }

        public static string GetPlayerName()
        {
            InitializationFile ini = GetIniFile();
            return ini.Read("Officer", "Name", "Officer");
        }

        public static Tuple<int, UnitType, int> GetPlayerCallsign()
        {
            InitializationFile ini = GetIniFile();
            int division = ini.Read("Radio", "Division", 1);
            string unitTypeString = ini.Read("Radio", "Unit", "Lincoln").ToUpper();
            if (!Enum.TryParse(unitTypeString, out UnitType unitType))
                unitType = UnitType.LINCOLN;
            int beat = ini.Read("Radio", "Beat", 18);
            return new Tuple<int, UnitType, int>(division, unitType, beat);
        }
    }
}
