using CornyFlakezPlugin;
using CornyFlakezPlugin.Callouts;
using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

public class Main : Plugin
{
  public const string PLUGIN_NAME = "CornyFlakezPlugin";

  private static List<Action> EventHandlerActions = new List<Action> { };
  private static List<Action> OnDutyEventHandlerActions = new List<Action>
  {
     Commandeerer.CarjackEventHandler
  };

  public static string GetAssemblyVersion()
  {
    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
    System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
    return fvi.FileVersion;
  }

  public static readonly string VERSION = GetAssemblyVersion();

  private static readonly List<Type> calloutTypes = new List<Type> {
        typeof(VehiclePursuit),
        typeof(PoliceEscort) };

  public override void Initialize()
  {
    Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
    Game.LogTrivial("");
    Game.LogTrivial("==================================================================================");
    Game.LogTrivial($"{PLUGIN_NAME} {VERSION} has been initialised.");
    Game.LogTrivial("==================================================================================");
    Game.LogTrivial("");
    DataManagement.rootDirectory = Environment.CurrentDirectory;
    DataManagement.GetIniFile();
    CalloutCommons.InitialiseColourList();
    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveHandler);
    GameFiber.StartNew(HandleEventHandlerActions);
  }

  private static void HandleEventHandlerActions()
  {
    while (true)
    {
      foreach (Action eventHandler in EventHandlerActions)
      {
        eventHandler();
      }
    }
  }

  public override void Finally()
  {
    Game.LogTrivial($"{PLUGIN_NAME} has been cleaned up.");
  }

  private static void OnOnDutyStateChangedHandler(bool OnDuty)
  {
    if (OnDuty)
    {
      GiveLoadout("OnDuty");
      RegisterCallouts();
      Game.DisplayNotification($"{PLUGIN_NAME} {VERSION} has loaded successfully.");

      // Start handling events that should be handled when on duty
      foreach (Action onDutyOnlyAction in OnDutyEventHandlerActions)
      {
        EventHandlerActions.Add(onDutyOnlyAction);
      }
    }
    else
    {
      GiveLoadout("OffDuty");

      // Stop handling on duty-only events
      foreach (Action onDutyOnlyAction in OnDutyEventHandlerActions)
      {
        EventHandlerActions.Remove(onDutyOnlyAction);
      }
    }

  }

  private static void RegisterCallouts()
  {
    Game.LogTrivial("");
    Game.LogTrivial("==================================================================================");
    calloutTypes.ForEach(callout => Functions.RegisterCallout(callout));
    Game.LogTrivial($"Successfully registered all {PLUGIN_NAME} callouts.");
    Game.LogTrivial("==================================================================================");
    Game.LogTrivial("");
  }

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

  private static Assembly AssemblyResolveHandler(object sender, ResolveEventArgs args)
  {
    string missingAssemblyName = args.Name;
    foreach (Assembly loadedPlugin in Functions.GetAllUserPlugins())
    {
      if (missingAssemblyName.ToLower().Contains(loadedPlugin.GetName().Name.ToLower()))
      {
        return loadedPlugin;
      }
    }
    Game.LogTrivial("");
    Game.LogTrivial("==================================================================================");
    Game.LogTrivial($"{PLUGIN_NAME}: Could not resolve assembly '{missingAssemblyName}'.");
    Game.LogTrivial("==================================================================================");
    Game.LogTrivial("");
    return null;
  }
}
