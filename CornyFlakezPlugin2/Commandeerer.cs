using System.Windows.Forms;
using Rage;

namespace CornyFlakezPlugin2
{

  public static class Commandeerer
  {
    private static bool startedCommandeeringVehicle = false;

    private static readonly Ped player = Game.LocalPlayer.Character;

    private static void CommandeerVehicle(Vehicle vehicle)
    {
      string modelName = vehicle.Model.Name;
      string notificationBody = $"Commandeering {modelName}...";
      string vehicleTextureDictionary = Util.GetVehicleTextureDictionary(modelName);
      Util.PlayIdentificationSpeech();
      Game.LogTrivial($"Txd of \"{modelName}\": {vehicleTextureDictionary}");
      if (vehicleTextureDictionary == null)
      {
        Game.DisplayNotification(notificationBody);
      }
      else
      {
        Game.DisplayNotification(vehicleTextureDictionary, modelName, "CornyFlakezPlugin", "(civilian vehicle)", notificationBody);
      }
      foreach (Ped vehicleOccupant in vehicle.Occupants)
      {
        LeaveVehicleFlags leaveFlags = LeaveVehicleFlags.None;
        if (vehicleOccupant == vehicle.Driver)
        {
          leaveFlags |= LeaveVehicleFlags.LeaveDoorOpen;
        }
        vehicleOccupant.RelationshipGroup.SetRelationshipWith(RelationshipGroup.Player, Relationship.Respect);
        vehicleOccupant.Tasks.LeaveVehicle(vehicle, leaveFlags);
        vehicleOccupant.Tasks.Wander();
        vehicleOccupant.Dismiss();
      }
    }

    public static void CarjackEventHandler()
    {
      if (Util.WasKeyHeld(Keys.G, 500))
      {
        if (startedCommandeeringVehicle) return;
        startedCommandeeringVehicle = true;

        Vehicle[] nearbyVehicles = player.GetNearbyVehicles(1);
        Vehicle closestVehicle = nearbyVehicles[0];
        if (player.DistanceTo(closestVehicle) > 10f) return;
        if (closestVehicle.Occupants.Length == 0) return;
        CommandeerVehicle(closestVehicle);
      }
      else if (startedCommandeeringVehicle)
      {
        startedCommandeeringVehicle = false;
      }
    }
  }
}