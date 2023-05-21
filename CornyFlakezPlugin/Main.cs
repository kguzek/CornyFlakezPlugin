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
  public const string PluginName = "CornyFlakezPlugin";

  public static readonly Version PluginVersion = Assembly.GetExecutingAssembly().GetName().Version;

  private static List<Action> EventHandlerActions = new List<Action> { };

  private static List<Action> OnDutyEventHandlerActions = new List<Action>
  {
     Commandeerer.CarjackEventHandler
  };

  private static readonly List<Type> CalloutTypes = new List<Type>
  {
    typeof(VehiclePursuit),
    typeof(PoliceEscort)
  };

  public override void Initialize()
  {
    Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
    Game.LogTrivial("");
    Game.LogTrivial("==================================================================================");
    Game.LogTrivial($"{PluginName} {PluginVersion} has been initialised.");
    Game.LogTrivial("==================================================================================");
    Game.LogTrivial("");
    DataManagement.rootDirectory = Environment.CurrentDirectory;
    DataManagement.GetIniFile();
    CalloutCommons.InitialiseColourList();
    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveHandler);
    GameFiber.StartNew(HandleEventHandlerActions);
  }

  public override void Finally()
  {
    Game.LogTrivial($"{PluginName} has been cleaned up.");
  }

  private static void HandleEventHandlerActions()
  {
    Game.LogTrivial("Handling event handlers!");
    while (true)
    {
      GameFiber.Yield();
      
      foreach (Action eventHandler in EventHandlerActions)
      {
        eventHandler();
      }
    }
  }

  private delegate void EventHandlerHandler(Action action);

  private static void OnOnDutyStateChangedHandler(bool OnDuty)
  {
    EventHandlerHandler eventHandlerHandler;
    if (OnDuty)
    {
      GiveLoadout("OnDuty");
      eventHandlerHandler = (Action action) => EventHandlerActions.Add(action);
      RegisterCallouts();
      Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", PluginName, $"~y~v{PluginVersion} ~o~by Konrad Guzek", "Plugin ~g~successfully loaded~w~!");
    }
    else
    {
      GiveLoadout("OffDuty");
      eventHandlerHandler = (Action action) => EventHandlerActions.Remove(action);
    }
    // Start or stop handling on duty-only events
    foreach (Action onDutyOnlyAction in OnDutyEventHandlerActions)
    {
      Game.LogTrivial($"Deregistering event handler action {onDutyOnlyAction}");
      eventHandlerHandler(onDutyOnlyAction);
    }

  }

  private static void RegisterCallouts()
  {
    Game.LogTrivial("");
    Game.LogTrivial("==================================================================================");
    CalloutTypes.ForEach(callout => Functions.RegisterCallout(callout));
    Game.LogTrivial($"Successfully registered all {PluginName} callouts.");
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
      Game.LogTrivial($"{PluginName}: equipping loadout {loadoutName}...");
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
    Game.LogTrivial($"{PluginName}: Could not resolve assembly '{missingAssemblyName}'.");
    Game.LogTrivial("==================================================================================");
    Game.LogTrivial("");
    return null;
  }
}
