using Rage;
using System;
using System.Collections.Generic;
using System.Xml;

[assembly: Rage.Attributes.Plugin("CornyFlakezPlugin2", Description = "Test plugin for CornyFlakezPlugin.", Author = "CornyFlakez")]
namespace CornyFlakezPlugin2
{
  public static class EntryPoint
  {
    public const string PLUGIN_NAME = "CornyFlakezPlugin";

    public class CalloutDebugInfo
    {
      public List<Ped> peds { get; set; } = new List<Ped>();
      public List<Vehicle> vehicles { get; set; } = new List<Vehicle>();
      public Callout activeCallout { get; set; }
    }
    public static CalloutDebugInfo currentDebugInfo = new CalloutDebugInfo();


    public static string GetAssemblyVersion()
    {
      System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
      System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
      return fvi.FileVersion;
    }

    public static readonly string VERSION = GetAssemblyVersion();

    public static readonly List<Type> calloutTypes = new List<Type> {
            typeof(PoliceEscort) };

    private static List<Action> EventHandlerActions = new List<Action>
    {
      Commandeerer.CarjackEventHandler,
    };

    private static void HandleEventHandlerActions()
    {
      while (true)
      {
        GameFiber.Yield();

        foreach (Action handler in EventHandlerActions)
        {
          handler();
        }
      }
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

    public static void Main()
    {
      UIManager.CreateMainMenu();
      UIManager.CreateDebugMenu();
      GameFiber.StartNew(HandleEventHandlerActions);
      GameFiber.Hibernate();
    }
  }
}