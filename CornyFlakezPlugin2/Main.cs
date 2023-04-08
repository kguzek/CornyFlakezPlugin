using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace CornyFlakezPlugin2
{
    public class Main
    {
        private const string PLUGIN_NAME = "CornyFlakezPlugin";

        public static string GetAssemblyVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        public static readonly string VERSION = GetAssemblyVersion();

        public static readonly List<Type> calloutTypes = new List<Type> { 
            typeof(PoliceEscort) };
        

        private static void GiveLoadout(string loadoutName)
        {
            XmlDocument xmlDocument = DataManagement.GetFileData("WeaponLoadouts.xml");
            try
            {
                Game.LogTrivial("");
                Game.LogTrivial("==================================================================================");
                Game.LogTrivial($"{PLUGIN_NAME}: equipping loadout {loadoutName}...");
                XmlNode loadoutNode = xmlDocument.SelectSingleNode($"//WeaponLoadouts/{loadoutName}");
                if (loadoutNode.ChildNodes.Count > 0)
                {
                    Game.LocalPlayer.Character.Inventory.Weapons.Clear();
                    Game.LogTrivial("Removed all current weapons.");
                }
                else
                {
                    Game.LogTrivial("Loadout is empty. No changes have been made.");
                }
                foreach (XmlNode weaponNode in loadoutNode.ChildNodes)
                {
                    string weaponName;
                    if (weaponNode["Name"] != null)
                        weaponName = weaponNode["Name"].InnerText;
                    else
                        weaponName = weaponNode.InnerText;
                    short ammoCount = 36; // Default value
                    if (weaponNode.Attributes.GetNamedItem("ammo") != null)
                        ammoCount = short.Parse(weaponNode.Attributes.GetNamedItem("ammo").InnerText);
                    WeaponHash weaponHash = (WeaponHash)Enum.Parse(typeof(WeaponHash), weaponName, true);
                    Game.LocalPlayer.Character.Inventory.GiveNewWeapon(weaponHash, ammoCount, false);
                    Game.LogTrivial($"Gave weapon {weaponName} with {ammoCount} bullets.");
                    if (weaponNode["Components"] == null)
                        continue;
                    foreach (XmlNode componentNode in weaponNode["Components"].ChildNodes)
                    {
                        string wpn = "WEAPON_" + weaponName.ToUpper();
                        string cmp = componentNode.InnerText.ToUpper();
                        Game.LocalPlayer.Character.Inventory.AddComponentToWeapon(wpn, cmp);
                        Game.LogTrivial($"    + component {cmp}");
                    }
                }
                Game.LogTrivial("Successfully equipped loadout!");
                Game.LogTrivial("==================================================================================");
                Game.LogTrivial("");
            }
            catch (Exception e)
            {
                Game.DisplayHelp($"~r~Error!~w~ Loadout '~b~{loadoutName}~w~' could not be loaded.");
                Game.LogTrivial($"Error! Loadout '{loadoutName}' could not be loaded. Exception message and stack trace:");
                Game.LogTrivial(e.Message);
                Game.LogTrivial(e.StackTrace);
                Game.LogTrivial("==================================================================================");
                Game.LogTrivial("");
            }
        }


        public static class CalloutCommons
        {
            private static readonly List<KnownColor> coloursWithAudio = Enum.GetValues(typeof(KnownColor)).OfType<KnownColor>().ToList();
            
            private static readonly Dictionary<VehicleClass, string> vehicleCategories = new Dictionary<VehicleClass, string>()
                {
                    { VehicleClass.Cycle, "BICYCLE" },
                    { VehicleClass.Industrial, "INDUSTRIAL_VEHICLE" },
                    { VehicleClass.Military, "MILITARY_VEHICLE" },
                    { VehicleClass.Muscle, "MUSCLE_CAR" },
                    { VehicleClass.OffRoad, "OFF_ROAD_VEHICLE" },
                    { VehicleClass.Super, "PERFORMANCE_CAR" },
                    { VehicleClass.Service, "SERVICE_VEHICLE" },
                    { VehicleClass.Sport, "SPORTS_CAR" },
                    { VehicleClass.SportClassic, "SPORTS_CAR" },
                    { VehicleClass.Rail, "TRAIN" },
                    { VehicleClass.Utility, "UTILITY_VEHICLE" }
                };

            private static readonly Dictionary<Ped, GameFiber> followGameFibers = new Dictionary<Ped, GameFiber>();

            public static void InitialiseColourList()
            {
                string[] colourAudioFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @".\LSPDFR\audio\scanner\COLOUR");
                
                coloursWithAudio.RemoveAll(c => !colourAudioFiles.Any(
                    f => Path.GetFileName(f) == GetColourAudioName(c) + "_01.wav"));
            }

            public class Loadout
            {
                public class Vehicle
                {
                    public Model Model { get; set; }

                    private int livery;

                    public int Livery
                    {
                        get { return livery; }
                        set { livery = value - 1; }
                    }

                    public Vehicle(Model model, int liv = 0)
                    {
                        Model = model;
                        livery = liv;
                    }
                }
                public class Ped
                {
                    public Model Model { get; set; }
                    public string Outfit { get; set; }
                    public string Inventory { get; set; }
                    public Ped(Model model, string outfit = null, string inventory = null)
                    {
                        Model = model;
                        Outfit = outfit;
                        Inventory = inventory;
                    }
                }
                public Ped[] Peds { get; set; }
                public Vehicle VehicleInfo { get; set; }
                public int NumPeds { get; set; }
            }

            public enum UnitType
            {
                ADAM,
                BOY,
                CHARLES,
                DAVID,
                EDWARD,
                FRANK,
                GEORGE,
                HENRY,
                IDA,
                JOHN,
                KING,
                LINCOLN,
                MARY,
                NORA,
                OCEAN,
                PAUL,
                QUEEN,
                ROBERT,
                SAM,
                TOM,
                UNION,
                VICTOR,
                WILLIAM,
                XRAY,
                YOUNG,
                ZEBRA
            }

            public static List<int> DeserialiseIndexes(string serialisedIndexes)
            {
                List<int> returnIndexes = new List<int>();
                string[] indexes = serialisedIndexes.Split(',');
                foreach (string index in indexes)
                {
                    try
                    {
                        returnIndexes.Add(int.Parse(index));
                    }
                    catch (FormatException)
                    {
                        string[] indexRange = index.Split('-');
                        if (indexRange.Length != 2)
                            throw new ArgumentException("Invalid index range.", nameof(serialisedIndexes));
                        int rangeStart = int.Parse(indexRange[0]);
                        int rangeEnd = int.Parse(indexRange[1]);
                        if (rangeStart >= rangeEnd)
                            throw new ArgumentException("Invalid index range.", nameof(serialisedIndexes));
                        for (int i = rangeStart; i <= rangeEnd; i++)
                            returnIndexes.Add(i);
                    }
                }
                return returnIndexes;
            }

            // Converts KnownColor into a colour name string with the conventions of the .wav files in the LSPDFR scanner folder.
            private static string GetColourAudioName(KnownColor kc)
            {
                StringBuilder builder = new StringBuilder();
                foreach (char c in Color.FromKnownColor(kc).Name)
                {
                    if (char.IsUpper(c) && builder.Length > 0) builder.Append('_');
                    builder.Append(c);
                }
                return "COLOR_" + builder.ToString().ToUpper();
            }

            private static string GetRegionZoneIsIn(string zoneName)
            {
                XmlDocument regionsDoc = new XmlDocument();
                regionsDoc.Load(@"\LSPDFR\Data\regions.xml");
                foreach (XmlElement region in regionsDoc["Regions"])
                {
                    foreach (XmlElement zone in region["Zones"])
                    {
                        if (zone.InnerText.ToUpper() == zoneName.ToUpper())
                            return region["Name"].InnerText;
                    }
                }
                return null;
            }

            public static string GetAgencyNameFromZone(EBackupUnitType backupType, string zone)
            {
                if (zone is null)
                {
                    throw new ArgumentNullException(nameof(zone));
                }
                XmlDocument backupDoc = new XmlDocument();
                backupDoc.Load(@"LSPDFR\Data\backup.xml");
                Dictionary<EBackupUnitType, string> dict = new Dictionary<EBackupUnitType, string>
                {
                    { EBackupUnitType.LocalUnit, "LocalPatrol" },
                    { EBackupUnitType.StateUnit, "StatePatrol" },
                    { EBackupUnitType.SwatTeam, "LocalSWAT" },
                    { EBackupUnitType.NooseTeam, "NooseSWAT" },
                    { EBackupUnitType.AirUnit, "LocalAir" },
                    { EBackupUnitType.NooseAirUnit, "NooseAir" },
                    { EBackupUnitType.Ambulance, "Ambulance" },
                    { EBackupUnitType.Firetruck, "Firetruck" }
                };
                XmlElement regions = backupDoc["BackupUnits"][dict[backupType]];
                string zoneName = zone;
                if (regions[zoneName] is null)
                    zoneName = GetRegionZoneIsIn(zoneName);
                if (zoneName is null)
                {
                    Game.DisplayHelp($"~r~Error~w~! Invalid backup agency set for zone ~b~{zone}~w~. Revert any changes made to ~y~regions.xml~w~.");
                    return null;
                }
                XmlNodeList agencies = regions[zoneName].ChildNodes;
                int randomIndex = new Random().Next(agencies.Count);
                return agencies[randomIndex].InnerText;
            }

            public static Loadout GetLoadoutFromZone(EBackupUnitType backupType, string zoneName, bool ignoreMpPeds = false)
            {
                Loadout selectedLoadout = new Loadout();
                XmlDocument doc = DataManagement.GetAgencySettings();
                string agencyScriptName = GetAgencyNameFromZone(backupType, zoneName);

                XmlElement SelectRandomElement(string elementName, XmlElement parentElement)
                {
                    List<XmlElement> elements = new List<XmlElement>();
                    bool useChances = false;
                    foreach (var child in parentElement)
                    {
                        if (child.GetType() != typeof(XmlElement))
                            continue;
                        XmlElement childElement = (XmlElement)child;
                        if (childElement.Name == elementName && childElement.HasAttribute("chance"))
                        {
                            useChances = true;
                            break;
                        }
                    }
                    foreach (var child in parentElement)
                    {
                        if (child.GetType() != typeof(XmlElement))
                            continue;
                        XmlElement childElement = (XmlElement)child;
                        if (childElement.Name != elementName)
                            continue;
                        if (elementName == "Ped" && ignoreMpPeds && childElement.InnerText.Contains("freemode"))
                            continue;
                        if (useChances)
                        {
                            if (!childElement.HasAttribute("chance"))
                                continue;
                            int chanceForThisElement = int.Parse(childElement.GetAttribute("chance"));
                            for (int i = 0; i < chanceForThisElement; i++)
                                elements.Add(childElement);
                        }
                        else
                        {
                            elements.Add(childElement);
                        }
                    }
                    return elements[new Random().Next(elements.Count)];
                }

                foreach (var element in doc["Agencies"])
                {
                    if (element.GetType() != typeof(XmlElement))
                        continue;
                    XmlElement agency = (XmlElement)element;
                    if (agency["ScriptName"].InnerText != agencyScriptName)
                        continue;

                    XmlElement loadout = SelectRandomElement("Loadout", agency);
                    XmlElement vehicle = SelectRandomElement("Vehicle", loadout["Vehicles"]);
                    int minPeds = int.Parse(loadout["NumPeds"].GetAttribute("min"));
                    int maxPeds = int.Parse(loadout["NumPeds"].GetAttribute("max"));
                    int numPeds = new Random().Next(minPeds, maxPeds + 1);
                    XmlElement[] pedElements = new XmlElement[numPeds];
                    for (int i = 0; i < numPeds; i++)
                    {
                        XmlElement pedElement = SelectRandomElement("Ped", loadout["Peds"]);
                        pedElements[i] = pedElement;
                    }
                    selectedLoadout.VehicleInfo = new Loadout.Vehicle(vehicle.InnerText);
                    if (vehicle.HasAttribute("livery"))
                    {
                        List<int> indexes = DeserialiseIndexes(vehicle.GetAttribute("livery"));
                        selectedLoadout.VehicleInfo.Livery = indexes[new Random().Next(indexes.Count)];
                    }
                    Loadout.Ped[] peds = new Loadout.Ped[pedElements.Length];
                    for (int i = 0; i < pedElements.Length; i++)
                    {
                        XmlElement pedElement = pedElements[i];
                        Loadout.Ped ped = new Loadout.Ped(pedElement.InnerText);
                        if (pedElement.HasAttribute("Outfit"))
                            ped.Outfit = pedElement.GetAttribute("Outfit");
                        if (pedElement.HasAttribute("Inventory"))
                            ped.Inventory = pedElement.GetAttribute("Inventory");
                        else
                            ped.Inventory = agency["Inventory"].InnerText;
                        peds[i] = ped;
                    }
                    selectedLoadout.Peds = peds;
                    selectedLoadout.NumPeds = numPeds;
                    break;
                }
                return selectedLoadout;
            }

            public static string GetCallsignAudio(Tuple<int, UnitType, int> callsign = null)
            {
                if (callsign is null)
                    callsign = DataManagement.GetPlayerCallsign();
                int division = callsign.Item1;
                UnitType unitType = callsign.Item2;
                int beat = callsign.Item3;
                string callsignAudioName = $"DIV_{division:D2} {unitType} BEAT_{beat:D2}";
                try
                {
                string[] presetCallsigns = Directory.GetFiles(@"LSPDFR\audio\scanner\CAR_CODE_COMPOSITE");
                if (presetCallsigns.Any(c => c.Contains($"{division:D2}_{unitType}_{beat:D2}")))
                    callsignAudioName = $"{division:D2}_{unitType}_{beat:D2}";
                }
                catch {}
                return callsignAudioName;
            }

            public static string GetVehicleDescription(Vehicle vehicle)
            {
                string vehClass = vehicleCategories.ContainsKey(vehicle.Class) ? 
                    vehicleCategories[vehicle.Class] : vehicle.Class.ToString();
                string vehInfo = $"VEHICLE_CATEGORY_{vehClass}";

                string[] modelAudioFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @".\LSPDFR\audio\scanner\CAR_MODEL");
                if (modelAudioFiles.Any(f => Path.GetFileName(f) == vehicle.Model.Name.ToUpper() + "_01.wav"))
                    vehInfo = vehicle.Model.Name;
                
                KnownColor closestColour = coloursWithAudio.OrderBy(
                    c => ColourDiff(Color.FromKnownColor(c), vehicle.PrimaryColor)).First();

                // Distance in RGB space
                int ColourDiff(Color c1, Color c2)
                {
                    return (int)Math.Sqrt(
                        Math.Pow(c1.R - c2.R, 2) +
                        Math.Pow(c1.G - c2.G, 2) +
                        Math.Pow(c1.B - c2.B, 2)
                    );
                }

                return $"{GetColourAudioName(closestColour)} {vehInfo.ToUpper()}";
            }

            public static void MakePedFollowTarget(Ped ped, Ped targetPed, float distance, VehicleDrivingFlags flags)
            {
                Vehicle vehicle = ped.CurrentVehicle ?? ped.VehicleTryingToEnter;
                Vehicle targetVehicle = targetPed.CurrentVehicle ?? targetPed.VehicleTryingToEnter;

                void FollowTarget()
                {
                    while (true)
                    {
                        GameFiber.Sleep(250); // In milliseconds
                        if (!ped.Exists() || !ped.IsAlive || !vehicle.Exists() || !vehicle.IsAlive)
                            break;
                        if (!targetPed.IsInAnyVehicle(true))
                            break;
                        if (ped.DistanceTo(targetPed.Position) > distance + 2f)
                        {
                            vehicle.IsSirenSilent = false;
                            float speed = Math.Max(ped.DistanceTo(targetPed.Position), targetPed.Speed * 1.2f);
                            ped.Tasks.DriveToPosition(vehicle, targetPed.Position, speed, flags, distance);
                        }
                        else
                        {
                            vehicle.IsSirenSilent = true;
                        }
                    }
                }

                StopPedFollowing(ped);
                followGameFibers.Add(ped, GameFiber.StartNew(FollowTarget));
            }

            public static bool StopPedFollowing(Ped ped)
            {
                ped.Tasks.Clear();
                if (!followGameFibers.ContainsKey(ped))
                    return false;
                followGameFibers[ped].Abort();
                followGameFibers.Remove(ped);
                return true;
            }


            public static void ProcessCallout(Callout callout, List<Ped> pedsToProcess = null)
            {
                if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
                {
                  callout.End();
                }
                if (pedsToProcess == null) return;
                foreach (Ped ped in pedsToProcess) {
                  if (ped.Exists() && ped.IsAlive) continue;
                  callout.End();
                  Game.LogTrivial($"Aborting callout: {ped.Model.Name} is missing.");
                  break;
                }
            }

            [Flags]
            public enum Code
            {
                Complete = 1,
                Code4 = 2,
                Code4Adam = 4,
                SuspectInCustody = 8,
                SuspectDown = 16
            }

            public static void EndCallout(Callout callingCallout, Code code = Code.Code4)
            {
                string codeMsg = "";
                string audioMsg = "";
                if (code.HasFlag(Code.Complete))
                {
                    codeMsg = "~g~complete";
                }
                else if (code.HasFlag(Code.Code4))
                {
                    List<string> suspectStatuses = new List<string> { "" };
                    if (code.HasFlag(Code.SuspectDown))
                    {
                        suspectStatuses = new List<string> { "SUSPECT_DOWN" };
                    }
                    else if (code.HasFlag(Code.SuspectInCustody))
                    {
                        suspectStatuses = new List<string> {
                            "SUSPECT_APPREHENDED",
                            "SUSPECT_ARRESTED",
                            "SUSPECT_IN_CUSTODY" };
                    }
                    string suspectStatus = suspectStatuses[new Random().Next(suspectStatuses.Count)];
                    audioMsg = $"ATTENTION_ALL_UNITS CODE_04 {suspectStatus} RETURN_TO_PATROL NO_FURTHER_UNITS";
                    codeMsg = "~g~CODE 4";
                }
                else if (code.HasFlag(Code.Code4Adam))
                {
                    audioMsg = "ATTENTION_ALL_UNITS CODE_04_ADAM REMAIN_IN_THE_AREA NO_FURTHER_UNITS";
                    codeMsg = "~o~CODE 4-Adam";
                }
                Game.DisplayHelp($"{callingCallout.FriendlyName} is {codeMsg}~w~.", 4000);
            }
        }
    }
}