using Rage;
using Rage.Attributes;

namespace CornyFlakezPlugin2
{
    public static class ConsoleCommands
    {
        [ConsoleCommand(Description = "Spawns a vehicle and some peds.")]
        public static void Command_CornySpawn(bool pedsOnly = false, int numPeds = 2, string pedModelName = "s_m_m_pilot_01", string vehModelName = "LUXOR")
        {
            Functions.SpawnPeds(numPeds, pedModelName);
            if (!pedsOnly)
                Functions.SpawnVehicle(vehModelName);
        }

        [ConsoleCommand(Description = "Makes a given ped walk to a vehicle.")]
        public static void Command_CornyWalk(int pedIndex = 0, int vehIndex = 0, bool goStraightToVehicle = false, float angle = 0f)
        {
            Functions.MakePedGoToVehicle(pedIndex, vehIndex, goStraightToVehicle, angle);
        }

        [ConsoleCommand(Description = "Makes a given ped enter a vehicle")]
        public static void Command_CornyEnter(int pedIndex = 0, int vehIndex = 0, int seatIndex = -1, bool instantly = false)
        {
            Functions.MakePedEnterVehicle(pedIndex, vehIndex, seatIndex, instantly);
        }

        [ConsoleCommand(Description = "Makes all peds exit a vehicle.")]
        public static void Command_CornyExit(int vehIndex)
        {
            Functions.EmptyVehicle(vehIndex);
        }

        [ConsoleCommand(Description = "Makes a given ped drive away in a vehicle.")]
        public static void Command_CornyGo(int pedIndex = 0, int vehIndex = 0, float speed = 30f)
        {
            Functions.MakePedDriveAwayInVehicle(pedIndex, vehIndex, speed);
        }

        [ConsoleCommand(Description = "Makes a given ped follow the player.")]
        public static void Command_CornyFollowMe(int pedIndex = 0, int vehIndex = 0, float distance = 8f, bool chaseMode = false)
        {
            Functions.MakePedFollowPlayer(pedIndex, vehIndex, distance, chaseMode);
        }

        [ConsoleCommand(Description = "Removes the peds, vehicles and blips created by CornyFlakezPlugin2.")]
        public static void Command_CornyClear()
        {
            Functions.ClearPedsAndVehicles();
        }

        [ConsoleCommand(Description = "Enables/disables the vehicle blip.")] 
        public static void Command_CornyCarBlip(int vehIndex = 0, bool? enabled = null)
        {
            if (Functions.vehicles.Count == 0)
                return;
            // By default the blip is toggled to the inverse of its current state.
            // If the user doesn't specify a state, 'enabled' is null so this expression below
            // sets it to the opposite of the current existence state of the blip.
            Functions.SetVehicleBlipToggleStatus(vehIndex, enabled ?? !Functions.vehicles[vehIndex].Value.Exists());
        }

        [ConsoleCommand(Description = "Reloads CornyFlakezPlugin2.")]
        public static void Command_RL()
        {
            Functions.ClearPedsAndVehicles();
            Game.ReloadActivePlugin();
        }
    }
}
