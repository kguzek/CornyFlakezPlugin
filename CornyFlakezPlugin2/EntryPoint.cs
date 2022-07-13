using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using RAGENativeUI.PauseMenu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

[assembly: Rage.Attributes.Plugin("CornyFlakezPlugin2", Description = "Test plugin for CornyFlakezPlugin.", Author = "CornyFlakez")]
namespace CornyFlakezPlugin2
{
    public static class EntryPoint
    {
        public class CalloutDebugInfo
        {
            public List<Ped> peds { get; set; } = new List<Ped>();
            public List<Vehicle> vehicles { get; set; } = new List<Vehicle>();
        }
        public static CalloutDebugInfo currentDebugInfo = new CalloutDebugInfo();

        private static readonly Version version = Assembly.GetExecutingAssembly().GetName().Version;
        private const string pluginName = "CornyFlakezPlugin2";

        private static void CreateMainMenu()
        {

            #region Menu Population 
            #region Main Menu
            UIMenu mainMenu = new UIMenu(pluginName, $"~b~{pluginName} version ~g~{version}");
            var spawnMenuButton = new UIMenuItem("Spawning", "Options for spawning peds and vehicles.");
            var pedMenuButton = new UIMenuItem("Ped actions", "Options for making peds do stuff.");
            var vehMenuButton = new UIMenuItem("Vehicle actions", "Options relating to spawned vehicles.");
            var clearButton = new UIMenuItem("Dismiss peds", "Dismisses all spawned peds and deletes any spawned vehicles.");
            var reloadButton = new UIMenuItem("Reload plugin", "Reloads this plugin.");
            mainMenu.AddItems(
                spawnMenuButton,
                pedMenuButton,
                vehMenuButton,
                clearButton,
                reloadButton);
            #endregion
            #region Spawning Menu
            UIMenu spawnMenu = new UIMenu(pluginName, "~b~PED AND VEHICLE SPAWNING");
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
            UIMenu pedMenu = new UIMenu(pluginName, "~b~PED ACTIONS");
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
            UIMenu vehMenu = new UIMenu(pluginName, "~b~VEHICLE ACTIONS");
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
            #endregion

            MenuPool menuPool = new MenuPool { mainMenu, spawnMenu, pedMenu, vehMenu };

            mainMenu.OnItemSelect += ItemSelectHandler;
            spawnMenu.OnItemSelect += ItemSelectHandler;
            pedMenu.OnItemSelect += ItemSelectHandler;
            vehMenu.OnItemSelect += ItemSelectHandler;

            void ItemSelectHandler(UIMenu menu, UIMenuItem selectedItem, int selectedItemIndex)
            {
                updateMenuItems();
                switch (menuPool.IndexOf(menu))
                {
                    case 0: // Main menu
                        switch (selectedItemIndex)
                        {
                            case 0: // Spawning menu button
                                mainMenu.Visible = false;
                                spawnMenu.Visible = true;
                                break;
                            case 1: // Ped menu button
                                mainMenu.Visible = false;
                                pedMenu.Visible = true;
                                break;
                            case 2: // Vehicle menu button
                                mainMenu.Visible = false;
                                vehMenu.Visible = true;
                                break;
                            case 3: // Dismiss button
                                Functions.ClearPedsAndVehicles();
                                break;
                            case 4: // Reload plugin button
                                Functions.ClearPedsAndVehicles();
                                mainMenu.Visible = false;
                                Game.ReloadActivePlugin();
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
                }
            }

            spawnMenu.OnMenuClose += OnMenuClose;
            pedMenu.OnMenuClose += OnMenuClose;
            vehMenu.OnMenuClose += OnMenuClose;

            void OnMenuClose(UIMenu menu)
            {
                mainMenu.Visible = true;
            }

            void updateMenuItems()
            {
                bool pedsHaveBeenSpawned = Functions.peds.Count != 0;
                bool carsHaveBeenSpawned = Functions.vehicles.Count != 0;
                pedSelector.Enabled = pedsHaveBeenSpawned;
                goToVehSelector.Enabled = pedsHaveBeenSpawned && carsHaveBeenSpawned;
                enterVehSelector.Enabled = pedsHaveBeenSpawned && carsHaveBeenSpawned;
                enterVehCheckbox.Enabled = pedsHaveBeenSpawned && carsHaveBeenSpawned;
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

        private static void CreateDebugMenu()
        {
            #region Menu Population
            #region Debug Main Menu
            UIMenu debugMenu = new UIMenu(pluginName, "~b~DEBUGGING MENU");
            UIMenuItem debugPeds = new UIMenuItem("Peds");
            UIMenuItem debugVehs = new UIMenuItem("Vehicles");
            debugMenu.AddItems(debugPeds, debugVehs);
            #endregion

            #region Ped Debugging Menu
            UIMenu pedDebuggingMenu = new UIMenu(pluginName, "~b~PED DEBUGGING");
            var pedSelector = new UIMenuNumericScrollerItem<int>("Selected ped", "", 1, 1, 1);
            var pedModelName = new UIMenuItem("Model name");
            var pedExists = new UIMenuItem("Exist status");
            var pedIsAlive = new UIMenuItem("Alive status");
            pedDebuggingMenu.AddItems(pedSelector, pedModelName, pedExists, pedIsAlive);
            #endregion

            #region Vehicle Debugging Menu
            UIMenu vehDebuggingMenu = new UIMenu(pluginName, "~b~VEHICLE DEBUGGING");
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
                List<Ped> peds = currentDebugInfo.peds;
                List<Vehicle> vehicles = currentDebugInfo.vehicles;
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

        public static void Main()
        {
            CreateMainMenu();
            CreateDebugMenu();
            GameFiber.Hibernate();
        }
    }
}
