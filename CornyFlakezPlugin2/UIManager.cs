using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace CornyFlakezPlugin2
{
  public class UIManager
  {

    private static string[] speeches = {
      "ANGRY_WITH_PLAYER_TREVOR",
      "ARREST_PLAYER",
      "BLOCKED_GENERIC",
      "BLOCKED_IN_PURSUIT",
      "BUDDY_DOWN",
      "BUMP",
      "CHALLENGE_THREATEN",
      "CHASE_VEHICLE_MEGAPHONE",
      "CHAT_RESP",
      "CHAT_STATE",
      "CLEAR_AREA_MEGAPHONE",
      "CLEAR_AREA_PANIC_MEGAPHONE",
      "COMBAT_TAUNT",
      "COP_ARRIVAL_ANNOUNCE",
      "COP_ARRIVAL_ANNOUNCE_MEGAPHONE",
      "COP_ARRIVAL_ANNOUNCE_MEGAPHONE_NO_REVERB",
      "COP_PEEK",
      "COP_SEES_GRENADE",
      "COP_SEES_GRENADE_LAUNCHER",
      "COP_SEES_GUN",
      "COP_SEES_MINI_GUN",
      "COP_SEES_ROCKET_LAUNCHER",
      "COP_SEES_WEAPON",
      "COVER_ME",
      "COVER_YOU",
      "CRASH_GENERIC",
      "CRIMINAL_APPREHENDED",
      "CRIMINAL_GET_IN_CAR",
      "CRIMINAL_WARNING",
      "DODGE",
      "DRAW_GUN",
      "DUCK",
      "EXPLOSION_IS_IMMINENT",
      "FALL_BACK",
      "FIGHT",
      "FOOT_CHASE",
      "FOOT_CHASE_HEADING_EAST",
      "FOOT_CHASE_HEADING_NORTH",
      "FOOT_CHASE_HEADING_SOUTH",
      "FOOT_CHASE_HEADING_WEST",
      "FOOT_CHASE_LOSING",
      "FOOT_CHASE_RESPONSE",
      "GENERIC_BYE",
      "GENERIC_CURSE_HIGH",
      "GENERIC_CURSE_MED",
      "GENERIC_CURSE_MED",
      "GENERIC_FRIGHTENED_HIGH",
      "GENERIC_FRIGHTENED_MED",
      "GENERIC_HI",
      "GENERIC_INSULT_HIGH",
      "GENERIC_INSULT_MED",
      "GENERIC_SHOCKED_HIGH",
      "GENERIC_SHOCKED_MED",
      "GENERIC_THANKS",
      "GENERIC_WAR_CRY",
      "GENERIC_WHATEVER",
      "GET_HIM",
      "GUN_COOL",
      "JACK_VEHICLE_BACK",
      "JACKED_GENERIC",
      "KIFFLOM_GREET",
      "LOST_SUSPECT_CHOPPER_MEGAPHONE",
      "MOVE_IN",
      "MOVE_IN_PERSONAL",
      "NEED_SOME_HELP",
      "NO_LOITERING_MEGAPHONE",
      "OFFICER_DOWN",
      "PINNED_DOWN",
      "PROVOKE_BUMPED_INTO",
      "PROVOKE_FOLLOWING",
      "PROVOKE_GENERIC",
      "PROVOKE_STARING",
      "PROVOKE_TRESPASS",
      "RELOADING",
      "REQUEST_BACKUP",
      "REQUEST_NOOSE",
      "RESCUE_INJURED_COP",
      "SETTLE_DOWN",
      "SHOOTOUT_OPEN_FIRE",
      "SHOOTOUT_READY",
      "SHOOTOUT_READY_RESP",
      "SHOT_TYRE_CHOPPER_MEGAPHONE",
      "SPOT_SUSPECT_CHOPPER_MEGAPHONE",
      "STOP_ON_FOOT_MEGAPHONE",
      "STOP_VEHICLE_BOAT_MEGAPHONE",
      "STOP_VEHICLE_CAR_MEGAPHONE",
      "STOP_VEHICLE_CAR_WARNING_MEGAPHONE",
      "STOP_VEHICLE_GENERIC_MEGAPHONE",
      "STOP_VEHICLE_GENERIC_WARNING_MEGAPHONE",
      "SURROUNDED",
      "SUSPECT_KILLED",
      "SUSPECT_LOST",
      "SUSPECT_SPOTTED",
      "TAKE_COVER",
      "TRAPPED",
      "WAIT",
      "WON_DISPUTE",
    };

    // S_[MF]_Y_(M|HWAY)?COP_01_(BLACK|WHITE)_(FULL|MINI)_0[1-6]
    private static string[] voices = {
      null,
      "S_F_Y_COP_01_BLACK_FULL_01",
      "S_F_Y_COP_01_BLACK_FULL_02",
      "S_F_Y_COP_01_WHITE_FULL_01",
      "S_F_Y_COP_01_WHITE_FULL_02",
      "S_M_Y_MCOP_01_WHITE_MINI_01",
      "S_M_Y_MCOP_01_WHITE_MINI_02",
      "S_M_Y_MCOP_01_WHITE_MINI_03",
      "S_M_Y_MCOP_01_WHITE_MINI_04",
      "S_M_Y_MCOP_01_WHITE_MINI_05",
      "S_M_Y_MCOP_01_WHITE_MINI_06",
      "S_M_Y_HWAYCOP_01_BLACK_FULL_01",
      "S_M_Y_HWAYCOP_01_BLACK_FULL_02",
      "S_M_Y_HWAYCOP_01_WHITE_FULL_01",
      "S_M_Y_HWAYCOP_01_WHITE_FULL_02",
      "S_M_Y_COP_01_WHITE_FULL_01",
      "S_M_Y_COP_01_WHITE_FULL_02",
      "S_M_Y_COP_01_WHITE_FULL_03",
      "S_M_Y_COP_01_WHITE_FULL_04",
      "S_M_Y_COP_01_WHITE_MINI_01",
      "S_M_Y_COP_01_WHITE_MINI_02",
      "S_M_Y_COP_01_WHITE_MINI_03",
      "S_M_Y_COP_01_WHITE_MINI_04",
      "S_M_Y_COP_01_BLACK_FULL_01",
      "S_M_Y_COP_01_BLACK_FULL_02",
      "S_M_Y_COP_01_BLACK_FULL_03",
      "S_M_Y_COP_01_BLACK_FULL_04",
      "S_M_Y_COP_01_BLACK_MINI_01",
      "S_M_Y_COP_01_BLACK_MINI_02",
      "S_M_Y_COP_01_BLACK_MINI_03",
      "S_M_Y_COP_01_BLACK_MINI_04",
    };

    private static Dictionary<string, string[]> animations = new Dictionary<string, string[]>() {
      {"prop_fib_badge-0", new string[] {"fbi_5b_mcs_1-0"} },
      {"prop_fib_badge-1", new string[] {"fbi_5b_mcs_1-1"} },
      { "prop_fib_badge-2", new string[] { "fbi_5b_mcs_1-2"} },
      { "prop_fib_badge-3", new string[] { "fbi_5b_mcs_1-3"} },
      { "player_one_dual-9", new string[] { "paper_1_rcm_alt1-9"} },
      { "issue_ticket_cop", new string[] {"veh@busted_low", "veh@busted_std", "veh@busted_van"} },
      { "issue_ticket_crim", new string[] {"veh@busted_low", "veh@busted_std", "veh@busted_van"} }
    };

    public static void CreateMainMenu()
    {
      #region Menu Population 
      #region Main Menu
      UIMenu mainMenu = new UIMenu(EntryPoint.PluginName, $"~b~{EntryPoint.PluginName} version ~g~{EntryPoint.PluginVersion}");
      var spawnMenuButton = new UIMenuItem("Spawning", "Options for spawning peds and vehicles.");
      var pedMenuButton = new UIMenuItem("Ped actions", "Options for making peds do stuff.");
      var vehMenuButton = new UIMenuItem("Vehicle actions", "Options relating to spawned vehicles.");
      var calloutsMenuButton = new UIMenuItem("Callout emulator", "Options relating to emulating LSPDFR callouts.");
      var speechMenuButton = new UIMenuItem("Speech menu", "Options for playing dialogue.");
      var animationMenuButton = new UIMenuItem("Animation menu", "Options for playing animations.");
      var clearButton = new UIMenuItem("Dismiss peds", "Dismisses all spawned peds and deletes any spawned vehicles.");
      var reloadButton = new UIMenuItem("Reload plugin", "Reloads this plugin.");
      mainMenu.AddItems(
          spawnMenuButton,
          pedMenuButton,
          vehMenuButton,
          calloutsMenuButton,
          speechMenuButton,
          animationMenuButton,
          clearButton,
          reloadButton);
      #endregion
      #region Spawning Menu
      UIMenu spawnMenu = new UIMenu(EntryPoint.PluginName, "~b~PED AND VEHICLE SPAWNING");
      Model[] pedModels = new Model[]
      {
                "s_m_y_cop_01",
                "s_f_y_cop_01",
                "s_m_y_hwaycop_01",
                "s_m_y_swat_01",
                "s_m_m_pilot_01"
      };
      Model[] vehModels = new Model[]
      {
                "police",
                "police2",
                "police3",
                "policeb",
                "riot",
                "luxor"
      };
      var spawnButton = new UIMenuItem("Spawn", "Spawns the peds and vehicles.");
      var numPedsSelector = new UIMenuNumericScrollerItem<int>("Number of peds to spawn", "", 0, 20, 1);
      var pedsOnlyCheckbox = new UIMenuCheckboxItem("Spawn only peds", false);
      var pedModelSelector = new UIMenuListScrollerItem<Model>("Ped model", "", pedModels)
      {
        Formatter = model => model.Name
      };
      var vehModelSelector = new UIMenuListScrollerItem<Model>("Vehicle model", "", vehModels)
      {
        Formatter = model => model.Name
      };
      spawnMenu.AddItems(
          spawnButton,
          numPedsSelector,
          pedModelSelector,
          pedsOnlyCheckbox,
          vehModelSelector);
      #endregion
      #region Ped Menu
      UIMenu pedMenu = new UIMenu(EntryPoint.PluginName, "~b~PED ACTIONS");
      var pedSelector = new UIMenuNumericScrollerItem<int>("Selected ped", "", 1, 1, 1);
      var selectedPedModel = new UIMenuItem("No spawned peds.", "The currently selected ped's model name.")
      {
        Enabled = false
      };
      var goToVehSelector = new UIMenuNumericScrollerItem<int>("Go to vehicle", "Makes the ped walk towards the vehicle.", 1, 1, 1)
      {
        Formatter = vehicleIndex => $"Vehicle {vehicleIndex}"
      };
      var enterVehSelector = new UIMenuNumericScrollerItem<int>("Enter vehicle seat", "Make this ped enter a spawned vehicle.", -2, 6, 1);
      var enterVehCheckbox = new UIMenuCheckboxItem("Enter vehicle immediately", false);
      var driveAwaySelector = new UIMenuNumericScrollerItem<float>("Drive away", "Makes the selected ped drive away in a vehicle with a given speed.", 5, 120, 5)
      {
        Formatter = speed => $"{speed} MPH"
      };
      var followMeSelector = new UIMenuNumericScrollerItem<float>("Follow me", "Makes the ped drive after the player's vehicle at a given distance.", 1, 50, 1)
      {
        Formatter = distance => $"{distance} m"
      };
      pedMenu.AddItems(
          pedSelector,
          selectedPedModel,
          goToVehSelector,
          enterVehSelector,
          enterVehCheckbox,
          driveAwaySelector,
          followMeSelector);
      #endregion
      #region Vehicle Menu
      UIMenu vehMenu = new UIMenu(EntryPoint.PluginName, "~b~VEHICLE ACTIONS");
      var vehSelector = new UIMenuNumericScrollerItem<int>("Selected vehicle", "", 1, 1, 1);
      var selectedVehModel = new UIMenuItem("No spawned vehicle.", "The spawned vehicle's model name.")
      {
        Enabled = false
      };
      var emptyVehicleButton = new UIMenuItem("Empty vehicle", "Empties the spawned vehicle.");
      KnownColor[] colours = Enum.GetValues(typeof(KnownColor)).OfType<KnownColor>().ToArray();
      var toggleVehicleBlipCheckbox = new UIMenuCheckboxItem("Toggle vehicle blip", false, "Toggles if the vehicle should have a blip that appears on the map.");
      var vehicleBlipColourSelector = new UIMenuListScrollerItem<KnownColor>("Blip colour", "Sets the blip colour for the vehicle.")
      {
        Items = colours,
        Formatter = knownColour => Color.FromKnownColor(knownColour).Name
      };
      toggleVehicleBlipCheckbox.CheckboxEvent += delegate (UIMenuCheckboxItem sender, bool value)
      {
        Functions.SetVehicleBlipToggleStatus(vehSelector.Value - 1, value);
        vehicleBlipColourSelector.Enabled = value;
      };
      vehMenu.AddItems(
          vehSelector,
          selectedVehModel,
          emptyVehicleButton,
          toggleVehicleBlipCheckbox,
          vehicleBlipColourSelector);
      #endregion
      #region Callouts Menu
      UIMenu calloutsMenu = new UIMenu(EntryPoint.PluginName, "~b~CALLOUT EMULATOR");
      var calloutsList = new UIMenuListScrollerItem<Type>("Selected callout", "", EntryPoint.CalloutTypes)
      {
        Formatter = callout => Functions.GetCalloutName(callout),
      };
      var startCalloutButton = new UIMenuItem("Start callout", "Emulates starting the selected callout.");
      var endCalloutButton = new UIMenuItem("End active callout", "Ends the callout emulation.")
      {
        Enabled = false,
      };
      calloutsMenu.AddItems(calloutsList, startCalloutButton, endCalloutButton);
      #endregion
      #region Speech Menu
      UIMenu speechMenu = new UIMenu(EntryPoint.PluginName, "~b~SPEECH MENU");
      var voiceList = new UIMenuListScrollerItem<string>("Voice", "Select the voice to speak with.", voices)
      {
        Formatter = rawValue => Regex.Replace(rawValue ?? "Default voice", "^S_([MF])_Y_", "($1) "),
      };
      var speechList = new UIMenuListScrollerItem<string>("Speech", "Select the piece of dialoge to be said by the player.", speeches);
      var variantSelector = new UIMenuNumericScrollerItem<int>("Variant", "Select the dialogue variant to play", 1, 8, 1);
      speechMenu.AddItems(voiceList, speechList, variantSelector);

      #endregion
      #region Animation menu
      UIMenu animationMenu = new UIMenu(EntryPoint.PluginName, "~b~ANIMATION MENU");
      var animationList = new UIMenuListScrollerItem<string>("Animation", "Select the animation to play.", animations.Keys.ToArray<string>());
      var animDictionaryList = new UIMenuListScrollerItem<string>("Dictionary", "Select the animation dictionary.", new string[] {});
      animationMenu.AddItems(animationList, animDictionaryList);
      #endregion
      #endregion

      MenuPool menuPool = new MenuPool { mainMenu, spawnMenu, pedMenu, vehMenu, calloutsMenu, speechMenu, animationMenu };

      foreach (UIMenu menu in menuPool)
      {
        menu.OnItemSelect += ItemSelectHandler;
        if (menu != mainMenu)
        {
          menu.OnMenuClose += OnMenuClose;
          menu.OnScrollerChange += delegate { updateMenuItems(); };
        }
      }

      void OnMenuClose(UIMenu menu)
      {
        mainMenu.Visible = true;
      }

      void ItemSelectHandler(UIMenu menu, UIMenuItem selectedItem, int selectedItemIndex)
      {
        int menuIndex = menuPool.IndexOf(menu);
        // Game.LogTrivial($"Clicked menu {selectedItem.Text} (idx {selectedItemIndex}) in {menu.TitleText} (idx {menuIndex})");

        updateMenuItems();
        switch (menuIndex)
        {
          case 0: // Main menu
            switch (selectedItemIndex)
            {
              case 6: // Dismiss button
                Functions.ClearPedsAndVehicles();
                break;
              case 7: // Reload plugin button
                Functions.ClearPedsAndVehicles();
                mainMenu.Visible = false;
                Game.ReloadActivePlugin();
                break;
              default:
                // Opens the corresponding menu according to the button index
                mainMenu.Visible = false;
                menuPool.ElementAt(selectedItemIndex + 1).Visible = true;
                break;
            }
            break;
          case 1: // Spawning menu
            switch (selectedItemIndex)
            {
              case 0: // Spawn button
              case 1: // Number of peds selector
              case 2: // Ped model selector
              case 4: // Vehicle model selector
                Functions.SpawnPeds(numPedsSelector.Value, pedModelSelector.SelectedItem);
                if (pedsOnlyCheckbox.Checked)
                  Game.DisplayNotification($"Spawned {numPedsSelector.Value} peds.");
                else
                {
                  Functions.SpawnVehicle(vehModelSelector.SelectedItem);
                  Game.DisplayNotification($"Spawned {numPedsSelector.Value} peds and a {vehModelSelector.SelectedItem.Name}.");
                }
                break;
              case 3: // Peds only checkbox
                /* 
                 * The logic here is inverted because the Checked
                 * attribute is updated after this code runs.
                 * This means that when this method is called,
                 * it has a value opposite of its actual checked state.
                 */
                vehModelSelector.Enabled = pedsOnlyCheckbox.Checked;
                // When the checkbox is actually checked, the selector should be disabled.
                break;
            }
            break;
          case 2: // Ped menu
            switch (selectedItemIndex)
            {
              case 2: // Go to vehicle button
                Functions.MakePedGoToVehicle(
                    pedSelector.Value - 1,
                    goToVehSelector.Value - 1);
                break;
              case 3: // Enter vehicle button
                Functions.MakePedEnterVehicle(
                    pedSelector.Value - 1,
                    goToVehSelector.Value - 1,
                    enterVehSelector.Value,
                    enterVehCheckbox.Checked);
                break;
              case 5: // Drive away button
                Functions.MakePedDriveAwayInVehicle(
                    pedSelector.Value - 1,
                    goToVehSelector.Value - 1,
                    driveAwaySelector.Value);
                break;
              case 6: // Follow me button
                Functions.MakePedFollowPlayer(
                    pedSelector.Value - 1,
                    goToVehSelector.Value - 1,
                    followMeSelector.Value, true);
                break;
            }
            break;
          case 3: // Vehicle menu
            switch (selectedItemIndex)
            {
              case 2: // Empty vehicle button
                Functions.EmptyVehicle(vehSelector.Value - 1);
                break;
              case 4: // Blip colour selector
                Functions.SetVehicleBlipColour(
                    vehSelector.Value - 1,
                    vehicleBlipColourSelector.SelectedItem);
                break;
            }
            break;
          case 4: // Callouts menu
            switch (selectedItemIndex)
            {
              case 1: // Start callout button
                startCalloutButton.Enabled = false;
                endCalloutButton.Enabled = true;
                object callout = System.Activator.CreateInstance(calloutsList.SelectedItem);
                EntryPoint.currentDebugInfo.activeCallout = (Callout)callout;
                EntryPoint.currentDebugInfo.activeCallout.OnBeforeCalloutDisplayed();
                EntryPoint.currentDebugInfo.activeCallout.OnCalloutAccepted();
                break;
              case 2: // End callout button
                endCalloutButton.Enabled = false;
                startCalloutButton.Enabled = true;
                EntryPoint.currentDebugInfo.activeCallout.End();
                break;
            }
            break;
          case 5: // Speech menu
            switch (selectedItemIndex)
            {
              case 0: // Voice selector
              case 1: // Speech selector
              case 2: // Speech variant selector
                string voiceName = voiceList.SelectedItem;
                string speechName = speechList.SelectedItem;
                int speechVariant = variantSelector.Value;
                Game.LocalPlayer.Character.PlayAmbientSpeech(voiceName, speechName, speechVariant, SpeechModifier.Standard);
                break;
            }
            break;
          case 6: // Animation menu
            string animationName = animationList.SelectedItem;
            string animationDictionary = animDictionaryList.SelectedItem;
            Game.LocalPlayer.Character.Tasks.PlayAnimation(animationDictionary, animationName, 1, AnimationFlags.None);
            break;
        }
      }



      void updateMenuItems()
      {
        bool pedsHaveBeenSpawned = Functions.peds.Count != 0;
        bool carsHaveBeenSpawned = Functions.vehicles.Count != 0;
        bool pedsAndCarsHaveBeenSpawned = pedsHaveBeenSpawned && carsHaveBeenSpawned;
        pedSelector.Enabled = pedsHaveBeenSpawned;
        goToVehSelector.Enabled = pedsAndCarsHaveBeenSpawned;
        enterVehSelector.Enabled = pedsAndCarsHaveBeenSpawned;
        enterVehCheckbox.Enabled = pedsAndCarsHaveBeenSpawned;
        bool pedIsBusy = pedsHaveBeenSpawned ? Functions.peds[pedSelector.Value - 1].Tasks.CurrentTaskStatus != TaskStatus.InProgress : false;
        driveAwaySelector.Enabled = pedIsBusy && carsHaveBeenSpawned;
        goToVehSelector.Maximum = vehSelector.Maximum =
            Math.Max(Functions.vehicles.Count, 1); // >= 1
        pedSelector.Maximum = Math.Max(Functions.peds.Count, 1); // >= 1
        selectedPedModel.Text = pedsHaveBeenSpawned ?
            Functions.peds[pedSelector.Value - 1].Model.Name : "No spawned peds.";

        vehSelector.Enabled = carsHaveBeenSpawned;
        emptyVehicleButton.Enabled = carsHaveBeenSpawned;
        toggleVehicleBlipCheckbox.Enabled = carsHaveBeenSpawned;
        vehicleBlipColourSelector.Enabled = carsHaveBeenSpawned && toggleVehicleBlipCheckbox.Checked;
        selectedVehModel.Text = carsHaveBeenSpawned ?
            Functions.vehicles[vehSelector.Value - 1].Key.Model.Name : "No spawned vehicle.";
        animDictionaryList.Items = animations[animationList.SelectedItem];
      }

      GameFiber.StartNew(ProcessMenus);

      void ProcessMenus()
      {
        while (true)
        {
          GameFiber.Yield();

          menuPool.ProcessMenus();
          if (Game.IsKeyDown(System.Windows.Forms.Keys.OemCloseBrackets))
          {
            mainMenu.Visible = !(mainMenu.Visible || UIMenu.IsAnyMenuVisible || TabView.IsAnyPauseMenuVisible);
            updateMenuItems();
          }
          if (EntryPoint.currentDebugInfo.activeCallout != null)
          {
            EntryPoint.currentDebugInfo.activeCallout.Process();
            if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
            {
              endCalloutButton.Activate(calloutsMenu);
            }
          }
          for (int i = 0; i < Functions.vehicles.Count; i++)
          {
            Vehicle vehicle = Functions.vehicles[i].Key;
            Blip blip = Functions.vehicles[i].Value;
            if (!vehicle.Exists() || !vehicle.IsAlive)
            {
              if (blip.Exists())
                blip.Delete();
              continue;
            }
            if (blip.Exists())
            {
              if (blip.Color != Color.FromKnownColor(vehicleBlipColourSelector.SelectedItem))
                Functions.SetVehicleBlipColour(i, vehicleBlipColourSelector.SelectedItem);
            }
            if (vehicle.HasDriver && vehicle.IsSirenOn && vehicle.Driver.Tasks.CurrentTaskStatus == TaskStatus.None)
              vehicle.IsSirenOn = false;
          }
          if (Functions.vehicles.Count > 0)
          {
            emptyVehicleButton.Enabled = !Functions.vehicles[vehSelector.Value - 1].Key.IsEmpty;
            toggleVehicleBlipCheckbox.Checked = Functions.vehicles[vehSelector.Value - 1].Value.Exists();
          }
          followMeSelector.Enabled = Functions.peds.Count > 0 && Game.LocalPlayer.Character.IsInAnyVehicle(true);
          clearButton.Enabled = Functions.peds.Count > 0 || Functions.vehicles.Count > 0;
        }
      }
    }

    public static void CreateDebugMenu()
    {
      #region Menu Population
      #region Debug Main Menu
      UIMenu debugMenu = new UIMenu(EntryPoint.PluginName, "~b~DEBUGGING MENU");
      UIMenuItem debugPeds = new UIMenuItem("Peds");
      UIMenuItem debugVehs = new UIMenuItem("Vehicles");
      debugMenu.AddItems(debugPeds, debugVehs);
      #endregion

      #region Ped Debugging Menu
      UIMenu pedDebuggingMenu = new UIMenu(EntryPoint.PluginName, "~b~PED DEBUGGING");
      var pedSelector = new UIMenuNumericScrollerItem<int>("Selected ped", "", 1, 1, 1);
      var pedModelName = new UIMenuItem("Model name");
      var pedExists = new UIMenuItem("Exist status");
      var pedIsAlive = new UIMenuItem("Alive status");
      pedDebuggingMenu.AddItems(pedSelector, pedModelName, pedExists, pedIsAlive);
      #endregion

      #region Vehicle Debugging Menu
      UIMenu vehDebuggingMenu = new UIMenu(EntryPoint.PluginName, "~b~VEHICLE DEBUGGING");
      var vehSelector = new UIMenuNumericScrollerItem<int>("Selected vehicle", "", 1, 1, 1);
      var vehModelName = new UIMenuItem("Model name");
      var vehExists = new UIMenuItem("Exist status");
      var vehIsAlive = new UIMenuItem("Alive status");
      vehDebuggingMenu.AddItems(vehSelector, vehModelName, vehExists, vehIsAlive);
      #endregion
      #endregion

      MenuPool menuPool = new MenuPool { debugMenu, pedDebuggingMenu, vehDebuggingMenu };

      debugMenu.OnItemSelect += ItemSelectHandler;
      pedDebuggingMenu.OnItemSelect += ItemSelectHandler;
      vehDebuggingMenu.OnItemSelect += ItemSelectHandler;

      void ItemSelectHandler(UIMenu menu, UIMenuItem selectedItem, int selectedItemIndex)
      {
        switch (menuPool.IndexOf(menu))
        {
          case 0: // Debug main menu
            switch (selectedItemIndex)
            {
              case 0: // Ped debugging button
                debugMenu.Visible = false;
                pedDebuggingMenu.Visible = true;
                break;
              case 1: // Vehicle debugging button
                debugMenu.Visible = false;
                vehDebuggingMenu.Visible = true;
                break;
            }
            break;
        }
      }

      Blip selectedPedBlip = null;
      Blip selectedVehBlip = null;

      pedDebuggingMenu.OnMenuClose += OnMenuClose;
      vehDebuggingMenu.OnMenuClose += OnMenuClose;

      void OnMenuClose(UIMenu menu)
      {
        if (menu != debugMenu)
          debugMenu.Visible = true;
        new List<Blip> { selectedPedBlip, selectedVehBlip }
        .Where(b => b.Exists()).ToList().ForEach(b => b.Delete());
      }

      void updateMenuItems()
      {
        List<Ped> peds = EntryPoint.currentDebugInfo.peds;
        List<Vehicle> vehicles = EntryPoint.currentDebugInfo.vehicles;
        void updatePedMenu()
        {
          pedSelector.Maximum = Math.Max(1, peds.Count);
          if (peds.Count == 0)
          {
            pedDebuggingMenu.MenuItems.ForEach(menuItem => menuItem.Enabled = false);
            if (selectedPedBlip.Exists())
              selectedPedBlip.Delete();
            return;
          }
          Ped selectedPed = peds[pedSelector.Value - 1];
          if (selectedPedBlip.Entity != selectedPed)
          {
            selectedPedBlip.Delete();
            selectedPedBlip = selectedPed.AttachBlip();
            selectedPedBlip.Color = Color.SkyBlue;
          }

          pedModelName.Text = "Model name: ~b~" + selectedPed.Model.Name;
          pedExists.Text = "Exist status: ~b~" + (selectedPed.Exists() ? "exists" : "does not exist");
          pedIsAlive.Text = "Alive status: ~b~" + (selectedPed.IsAlive ? "alive" : "dead");


        }
        void updateVehMenu()
        {
          vehSelector.Maximum = Math.Max(1, vehicles.Count);
          if (vehicles.Count == 0)
          {
            vehDebuggingMenu.MenuItems.ForEach(menuItem => menuItem.Enabled = false);
            if (selectedVehBlip.Exists())
              selectedVehBlip.Delete();
            return;
          }
          Vehicle selectedVehicle = vehicles[vehSelector.Value - 1];
          if (selectedVehBlip.Entity != selectedVehicle)
          {
            selectedVehBlip.Delete();
            selectedVehBlip = selectedVehicle.AttachBlip();
            selectedVehBlip.Color = Color.SkyBlue;
          }

          pedModelName.Text = "Model name: ~b~" + selectedVehicle.Model.Name;
          pedExists.Text = "Exist status: ~b~" + (selectedVehicle.Exists() ? "exists" : "does not exist");
          pedIsAlive.Text = "Alive status: ~b~" + (selectedVehicle.IsAlive ? "alive" : "dead");

        }
        updatePedMenu();
        updateVehMenu();
      }

      GameFiber.StartNew(ProcessMenus);

      void ProcessMenus()
      {
        while (true)
        {
          GameFiber.Yield();

          menuPool.ProcessMenus();
          if (Game.IsKeyDown(System.Windows.Forms.Keys.OemOpenBrackets))
          {
            debugMenu.Visible = !(debugMenu.Visible || UIMenu.IsAnyMenuVisible || TabView.IsAnyPauseMenuVisible);
          }
          updateMenuItems();
        }
      }
    }

  }
}
