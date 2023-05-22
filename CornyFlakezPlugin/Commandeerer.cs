using Rage;
using LSPD_First_Response.Mod.API;
using System;

namespace CornyFlakezPlugin
{

  public static class Commandeerer
  {
    private const float MaxCommandeerDistance = 3f;

    private const float CommandeerVehicleDelayMs = 1500;

    private static DateTime? startedCommandeeringVehicleAt = null;

    private static Vehicle vehicleToBeCommandeered = null;

    private static void CommandeerVehicle(Vehicle vehicle)
    {
      string modelName = vehicle.Model.Name;
      string notificationBody = $"~y~{modelName}~w~ has been ~g~commandeered~w~!";
      string agencyName = Functions.GetCurrentAgencyScriptName().ToUpper();
      Game.DisplayNotification(Main.PoliceTxd, Main.PoliceTxd, agencyName, "~b~Official Police Business", notificationBody);
      vehicleToBeCommandeered = vehicle;
    }

    private static void CheckForVehiclesToBeCommandeered(Ped player)
    {
      Vehicle[] nearbyVehicles = player.GetNearbyVehicles(1);
      Vehicle closestVehicle = nearbyVehicles[0];
      if (player.DistanceTo(closestVehicle.FrontPosition) > MaxCommandeerDistance) return;
      startedCommandeeringVehicleAt = DateTime.Now;
      if (closestVehicle.Occupants.Length == 0) return;
      CommandeerVehicle(closestVehicle);
    }

    public static void CarjackEventHandler()
    {
      Ped player = Game.LocalPlayer.Character;
      bool startedCommandeeringVehicle = startedCommandeeringVehicleAt != null;
      // Check if the player is trying to commandeer a vehicle
      if (Functions.IsPedShowingBadge(player))
      {
        if (!startedCommandeeringVehicle)
        {
          CheckForVehiclesToBeCommandeered(player);
        }
        else if (vehicleToBeCommandeered == null)
        {
          // Don't do anything in the period after the vehicle was commandeered and before the animation finishes
          return;
        }
      }
      // Don't do anything else if the player hasn't started commandeering
      if (!startedCommandeeringVehicle) return;
      // Allow the player to commandeer a new vehicle if there was no suitable vehicle found after the attempt
      if (vehicleToBeCommandeered == null)
      {
        startedCommandeeringVehicleAt = null;
        return;
      }
      // Actually make the peds exit the vehicle after the set delay has passed
      if (DateTime.Now < startedCommandeeringVehicleAt + TimeSpan.FromMilliseconds(CommandeerVehicleDelayMs)) return;
      foreach (Ped vehicleOccupant in vehicleToBeCommandeered.Occupants)
      {
        LeaveVehicleFlags leaveFlags = LeaveVehicleFlags.None;
        if (vehicleOccupant == vehicleToBeCommandeered.Driver)
        {
          leaveFlags |= LeaveVehicleFlags.LeaveDoorOpen;
        }
        vehicleOccupant.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Player, Relationship.Respect);
        vehicleOccupant.Tasks.LeaveVehicle(vehicleToBeCommandeered, leaveFlags);
        vehicleOccupant.Tasks.Wander();
        vehicleOccupant.Dismiss();
      }
      vehicleToBeCommandeered = null;
    }

  }
}