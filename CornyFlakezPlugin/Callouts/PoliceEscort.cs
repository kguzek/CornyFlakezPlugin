using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;

namespace CornyFlakezPlugin.Callouts
{
  [CalloutInfo("Police Escort", CalloutProbability.Medium)]
  public class PoliceEscort : Callout
  {
    private class CalloutLocation
    {
      public Vector3 suvSpawnPoint { get; set; }

      public Vector3 copSpawnPoint { get; set; }

      public Vector3 bdgSpawnPoint { get; set; }

      public Vector3 vipSpawnPoint { get; set; }

      public Vector3 partialDestination { get; set; }
    }

    private static readonly CalloutLocation
        CityHall =
            new CalloutLocation()
            {
              suvSpawnPoint = new Vector3(259, -378, 44f),
              copSpawnPoint = new Vector3(253f, -376f, 44),
              bdgSpawnPoint = new Vector3(236, -412, 48.2f),
              vipSpawnPoint = new Vector3(235, -410, 48.2f),
              partialDestination = new Vector3(300, -450, 44f)
            };

    private static readonly CalloutLocation[]
        CalloutLocations = new CalloutLocation[] { CityHall };

    private readonly Vector3
        jetSpawnPoint = new Vector3(-1000, -3000, 14.6f);

    private readonly Vector3 airportGateLocation = new Vector3(-968, -2797, 14f);
    private readonly Vector3 destination = new Vector3(-1003, -2987, 14f);

    private readonly Vector3
        copDestination = new Vector3(-1005, -2973, 14f);

    private readonly Vector3
        jetTakeOffPoint = new Vector3(-1350, -2240, 14.1f);

    private VehicleDrivingFlags escortDrivingFlags =
        VehicleDrivingFlags.Emergency |
        VehicleDrivingFlags.StopAtDestination;

    private VehicleDrivingFlags normalDrivingFlags = VehicleDrivingFlags.Normal | VehicleDrivingFlags.StopAtDestination;

    private Ped driverPed;

    private Ped bodyguardPed;

    private Ped vipPed;

    private Ped pilotPed;

    private List<Ped> peds;

    private Vehicle suv;

    private Vehicle copCar;

    private Vehicle jet;

    private Blip missionBlip;

    private Blip vipBlip;

    private CalloutLocation calloutLocation;

    private enum CalloutStage
    {
      None,
      PreArrival,
      WaitingForDoor,
      WaitingForVIP,
      EnteringVehicleVIP,
      EnteringVehicleBodyguard,
      EscortBeginning,
      Escort,
      EscortEnd,
      ArrivedAtDestination,
      ExitingVehicleVIP,
      ApproachingPlaneVIP,
      BoardingPlaneVIP,
      BoardingPlaneBodyguard,
      TakeOff
    }

    private CalloutStage stage = CalloutStage.None;

    private void CleanUpAfterCallout()
    {
      var blipsToDelete = new Blip[] { missionBlip, vipBlip }
        .Where(b => b.Exists());
      foreach (Blip blip in blipsToDelete)
        blip.Delete();

      if (stage == CalloutStage.TakeOff)
      {
        jet.Dismiss();
        Game.LogTrivial("Jet has been dismissed");
      }
      else if (jet.Exists())
      {
        jet.Delete();
        Game.LogTrivial("Jet has been deleted");
      }
      stage = CalloutStage.None;
      if (copCar.Exists()) 
      {
        copCar.IsSirenOn = false;
        copCar.Dismiss();
      }
      foreach (Ped p in peds)
      {
        if (p.Exists())
        {
          p.Dismiss();
        }
      };
    }

    public override bool OnBeforeCalloutDisplayed()
    {
      peds = new List<Ped>();

      int randomLocationIndex =
          new Random().Next(CalloutLocations.Length);
      calloutLocation = CalloutLocations[randomLocationIndex];

      suv =
          new Vehicle("GRANGER",
              calloutLocation.suvSpawnPoint,
              250)
          {
            IsPersistent = true,
            LicensePlate = "GR4NG3R",
            PrimaryColor = Color.Black,
            SecondaryColor = Color.Black,
            PearlescentColor = Color.Black
          };

      driverPed =
          new Ped("cs_fbisuit_01",
              calloutLocation.suvSpawnPoint,
              250)
          {
            IsPersistent = true,
            KeepTasks = true,
            BlockPermanentEvents = true
          };
      driverPed.WarpIntoVehicle(suv, -1);
      peds.Add(driverPed);

      Main.CalloutCommons.Loadout copLoadout =
          Main
              .CalloutCommons
              .GetLoadoutFromZone(EBackupUnitType.LocalUnit,
              "LosSantosCity",
              true);
      copCar =
          new Vehicle(copLoadout.VehicleInfo.Model,
              calloutLocation.copSpawnPoint,
              250)
          { IsPersistent = true, IsSirenOn = true, IsSirenSilent = true };
      for (int i = 0; i < 2 && i < copLoadout.NumPeds; i++)
      {
        Ped copPed =
            new Ped(copLoadout.Peds[i].Model,
                calloutLocation.copSpawnPoint,
                250);
        peds.Add(copPed);
        copPed.WarpIntoVehicle(copCar, i - 1);
      }

      ShowCalloutAreaBlipBeforeAccepting(calloutLocation.suvSpawnPoint,
      15f);
      CalloutMessage = "Police escort requested for VIP.";
      CalloutPosition = calloutLocation.suvSpawnPoint;

      string callsignAudio = Main.CalloutCommons.GetCallsignAudio();
      Functions
          .PlayScannerAudioUsingPosition($"ATTENTION_UNIT_SPECIFIC {callsignAudio} REQUESTING_ESCORT IN_OR_ON_POSITION",
          calloutLocation.suvSpawnPoint);
      return base.OnBeforeCalloutDisplayed();
    }

    public override bool OnCalloutAccepted()
    {
      stage = CalloutStage.PreArrival;
      Game.DisplaySubtitle("Go to the ~y~City Hall~w~.");
      Game
          .DisplayNotification("A high-value asset is en-route to the Los Santos International Airport. Make sure they get there safely.");
      Functions.PlayScannerAudio("UNITS_RESPOND_CODE_02");

      missionBlip = suv.AttachBlip();
      missionBlip.Color = Color.Yellow;
      missionBlip.Name = "City Hall";
      missionBlip.EnableRoute(Color.Yellow);
      bodyguardPed =
          new Ped("cs_fbisuit_01",
              calloutLocation.bdgSpawnPoint,
              341)
          {
            IsPersistent = true,
            KeepTasks = true,
            BlockPermanentEvents = true
          };
      vipPed =
          new Ped("cs_bankman",
              calloutLocation.vipSpawnPoint,
              341)
          {
            IsPersistent = true,
            KeepTasks = true,
            BlockPermanentEvents = true
          };
      pilotPed =
          new Ped("s_m_m_pilot_01",
              jetSpawnPoint,
              60)
          {
            IsPersistent = true,
            KeepTasks = true,
            BlockPermanentEvents = true
          };

      peds.Add(bodyguardPed);
      peds.Add(vipPed);
      peds.Add(pilotPed);

      jet =
          new Vehicle("LUXOR", jetSpawnPoint, 60) { IsPersistent = true };
      jet.Doors[0].Open(false);
      pilotPed.WarpIntoVehicle(jet, -1);
      if (
          Main
              .CalloutCommons
              .IsLSPDFRPluginRunning("CornyFlakezPlugin2.dll")
      )
        DebugPluginFunctions
            .PassDebugInfo(peds,
            new List<Vehicle> { suv, copCar, jet });
      return base.OnCalloutAccepted();
    }

    public override void OnCalloutNotAccepted()
    {
      CleanUpAfterCallout();
      base.OnCalloutNotAccepted();
    }

    public override void Process()
    {
      switch (stage)
      {
        case CalloutStage.None:
          break;
        case CalloutStage.PreArrival:
          if (
              Game.LocalPlayer.Character.Position.DistanceTo(suv) <
              40f
          )
          {
            if (missionBlip.Exists()) missionBlip.Delete();
            Game.DisplaySubtitle("Park up and wait for the ~b~VIP~w~ to enter the SUV.");
            vipBlip = vipPed.AttachBlip();
            vipBlip.Scale = 0.5f;
            vipBlip.Name = "VIP";
            vipBlip.Color = Color.FromKnownColor(KnownColor.MenuHighlight);

            // bodyguardPed.Tasks.EnterVehicle(suv, 3000, 2, EnterVehicleFlags.DoNotEnter);
            bodyguardPed
                .Tasks
                .GoToOffsetFromEntity(suv, 2f, 240f, 1f);

            // bodyguardPed.Tasks.GoStraightToPosition(suv.RightPosition, 1f, suv.Heading - 180f, 3f, 30000);
            vipPed
                .Tasks
                .FollowToOffsetFromEntity(bodyguardPed,
                new Vector3(1f, 1f, 0f));
            stage = CalloutStage.WaitingForVIP;
          }
          break;
        case CalloutStage.WaitingForVIP:
          if (bodyguardPed.DistanceTo(suv) < 3f)
          {
            bodyguardPed.Tasks.Clear();
            vipPed.Tasks.Clear();
            bodyguardPed
                .Tasks
                .EnterVehicle(suv,
                5000,
                2,
                EnterVehicleFlags.DoNotEnter);
            stage = CalloutStage.WaitingForDoor;
          }
          break;
        case CalloutStage.WaitingForDoor:
          if (bodyguardPed.Tasks.CurrentTaskStatus == TaskStatus.NoTask)
          {
            vipPed.Tasks.EnterVehicle(suv, 10000, 2);
            bodyguardPed
              .Tasks
              .GoToOffsetFromEntity(suv, 3f, 230f, 1f);
            stage = CalloutStage.EnteringVehicleVIP;
          }
          break;
        case CalloutStage.EnteringVehicleVIP:
          if (vipPed.IsInVehicle(suv, false))
          {
            bodyguardPed.Tasks.EnterVehicle(suv, 0);
            suv.Doors[3].Close(false);
            stage = CalloutStage.EnteringVehicleBodyguard;
          }
          break;
        case CalloutStage.EnteringVehicleBodyguard:
          if (bodyguardPed.IsInVehicle(suv, false))
          {
            Game.DisplaySubtitle("Escort the ~b~VIP~w~ to the ~y~airport~w~.");
            missionBlip = new Blip(airportGateLocation);
            missionBlip.Color = Color.Yellow;
            missionBlip.Name = "LSIA";
            missionBlip.EnableRoute(Color.Yellow);
            vipBlip.Scale = 1f;
            copCar.IsSirenSilent = false;
            driverPed
                .Tasks
                .DriveToPosition(suv,
                calloutLocation.partialDestination,
                40f,
                escortDrivingFlags,
                5f);
            Main
                .CalloutCommons
                .MakePedFollowTarget(copCar.Driver,
                vipPed,
                5f,
                escortDrivingFlags);
            stage = CalloutStage.EscortBeginning;
          }
          break;
        case CalloutStage.EscortBeginning:
          if (suv.DistanceTo(calloutLocation.partialDestination) < 20f)
          {
            stage = CalloutStage.Escort;
            driverPed
                .Tasks
                .DriveToPosition(suv, airportGateLocation, 40f, escortDrivingFlags, 10f);
          }
          break;
        case CalloutStage.Escort:
          if (suv.DistanceTo(airportGateLocation) < 15f)
          {
            Game.DisplaySubtitle("Escort the ~b~VIP~w~ to the ~y~Jet Charter~w~.");
            missionBlip = new Blip(destination, 20f);
            missionBlip.Name = "Hangar";
            missionBlip.Color = Color.Yellow;
            driverPed.Tasks.DriveToPosition(suv, destination, 25f, normalDrivingFlags, 5f);
            Main.CalloutCommons.StopPedFollowing(copCar.Driver);
            copCar
              .Driver
              .Tasks
              .DriveToPosition(copDestination,
              20f,
              normalDrivingFlags);
            if (!copCar.IsSirenSilent) copCar.IsSirenSilent = true;
            stage = CalloutStage.EscortEnd;
          }
          break;
        case CalloutStage.EscortEnd:
          if (suv.DistanceTo(destination) < 6f)
          {
            if (missionBlip.Exists()) missionBlip.Delete();
            Game.DisplaySubtitle("Wait for the ~b~VIP~w~ to board the plane.");
            bodyguardPed.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
            stage = CalloutStage.ArrivedAtDestination;
          }
          break;
        case CalloutStage.ArrivedAtDestination:
          if (suv.Doors[3].IsOpen)
          {
            driverPed.Tasks.Clear();
            stage = CalloutStage.ExitingVehicleVIP;
            vipPed.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
          }
          else if (
              !bodyguardPed.IsInVehicle(suv, true) &&
              bodyguardPed.Tasks.CurrentTaskStatus !=
              TaskStatus.InProgress
          )
          {
            bodyguardPed
                .Tasks
                .EnterVehicle(suv,
                5000,
                2,
                EnterVehicleFlags.DoNotEnter);
          }
          break;
        case CalloutStage.ExitingVehicleVIP:
          if (!vipPed.IsInVehicle(suv, true))
          {
            if (!jet.Exists() || !jet.IsAlive)
            {
                End();
                break;
            }
            
            stage = CalloutStage.ApproachingPlaneVIP;
            bodyguardPed
                .Tasks
                .FollowToOffsetFromEntity(vipPed,
                new Vector3(1f, 1f, 0f));
            vipPed.Tasks.GoToOffsetFromEntity(jet, 5f, 30f, 1f);
          }
          break;
        case CalloutStage.ApproachingPlaneVIP:
          if (vipPed.Tasks.CurrentTaskStatus != TaskStatus.InProgress)
          {
            stage = CalloutStage.BoardingPlaneVIP;
            vipPed.Tasks.EnterVehicle(jet, -2);
            driverPed.Dismiss();
          }
          break;
        case CalloutStage.BoardingPlaneVIP:
          if (vipPed.IsInVehicle(jet, true))
          {
            stage = CalloutStage.None;
            GameFiber
                .StartNew(delegate ()
                {
                  GameFiber.Sleep(1000);
                  stage = CalloutStage.BoardingPlaneBodyguard;
                  vipPed.WarpIntoVehicle(jet, 2);
                  bodyguardPed.Tasks.EnterVehicle(jet, -2);
                });
          }
          break;
        case CalloutStage.BoardingPlaneBodyguard:
          if (bodyguardPed.IsInVehicle(jet, true))
          {
            stage = CalloutStage.None;
            GameFiber
                .StartNew(delegate ()
                {
                  GameFiber.Sleep(1500);
                  stage = CalloutStage.TakeOff;
                  bodyguardPed.WarpIntoVehicle(jet, 4);
                  jet.Doors[0].Close(false);
                  jet.IsEngineStarting = true;
                  GameFiber
                    .StartNew(delegate ()
                    {
                      GameFiber.Sleep(3000); // ms
                      if (!pilotPed.Exists()) return;
                      // pilotPed
                      //   .Tasks
                      //   .DriveToPosition(jet, jetTakeOffPoint,
                      //   10f,
                      //   VehicleDrivingFlags.Normal,
                      //   10f);
                      pilotPed.Dismiss();
                    });
                  End();
                });
          }
          break;
      }
      Main.CalloutCommons.ProcessCallout(this, peds);
      if (Game.IsKeyDown(System.Windows.Forms.Keys.PageDown))
        ProgressCalloutByCheating();
      if (vipPed.Exists() && !vipPed.IsAlive && vipBlip.Exists())
      {
        vipBlip.Delete();
      }
      base.Process();
    }

    public override void End()
    {
      CleanUpAfterCallout();
      Main
        .CalloutCommons
        .EndCallout(this, Main.CalloutCommons.Code.Complete);
      base.End();
    }

    private void ProgressCalloutByCheating()
    {
      Ped playerPed = Game.LocalPlayer.Character;
      Entity playerEntity =
          playerPed.CurrentVehicle
              ?? playerPed.VehicleTryingToEnter ?? (Entity)playerPed;
      switch (stage)
      {
        case CalloutStage.PreArrival:
          playerEntity.Position =
              World.GetNextPositionOnStreet(suv.LeftPosition);
          playerEntity.Heading =
              playerEntity == playerPed ? 160f : 250f;
          break;
        case CalloutStage.EscortBeginning:
          playerEntity.Position = World.GetNextPositionOnStreet(calloutLocation.partialDestination);
          suv.Position = World.GetNextPositionOnStreet(playerEntity.RearPosition);
          copCar.Position = World.GetNextPositionOnStreet(suv.RearPosition);
          break;
        case CalloutStage.Escort:
          playerEntity.Position = World.GetNextPositionOnStreet(airportGateLocation);
          suv.Position = World.GetNextPositionOnStreet(playerEntity.RearPosition);
          copCar.Position = World.GetNextPositionOnStreet(suv.RearPosition);
          break;
        case CalloutStage.EscortEnd:
          suv.Position = destination;
          suv.Heading = 215f;
          copCar.Position = copDestination;
          copCar.Heading = 223f;
          playerEntity.Position = new Vector3(-1037, -2955, 14);
          playerEntity.Heading = 220f;
          suv.DriveForce = copCar.DriveForce = 0;
          break;
      }
    }
  }
}
