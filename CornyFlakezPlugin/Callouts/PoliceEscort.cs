using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;

namespace CornyFlakezPlugin.Callouts
{
    [CalloutInfo("Police Escort", CalloutProbability.Medium)]
    public class PoliceEscort : Callout
    {
        private readonly Vector3 suvSpawnPoint = new Vector3(261, -379, 44.3f);

        private readonly Vector3 copSpawnPoint = new Vector3(247, -374, 44);

        private readonly Vector3 bdgSpawnPoint = new Vector3(236, -412, 48.2f);

        private readonly Vector3 vipSpawnPoint = new Vector3(235, -410, 48.2f);

        private readonly Vector3
            jetSpawnPoint = new Vector3(-1000, -3000, 14.6f);

        private readonly Vector3
            partialDestination = new Vector3(300, -450, 44f);

        private readonly Vector3 destination = new Vector3(-1003, -2987, 14f);

        private readonly Vector3
            copDestination = new Vector3(-1005, -2973, 14f);

        private readonly Vector3
            jetTakeOffPoint = new Vector3(-1350, -2240, 14.1f);

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

        private Blip copBlip;

        private enum CalloutStage
        {
            None,
            PreArrival,
            WaitingForDoor,
            WaitingForVIP,
            WaitingForBodyguard,
            EnteringVehicleVIP,
            EnteringVehicleBodyguard,
            EscortBeginning,
            Escort,
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
            foreach (Blip
                blip
                in
                new Blip[] { missionBlip, vipBlip, copBlip }
                    .Where(b => b.Exists())
            )
            blip.Delete();

            if (stage == CalloutStage.TakeOff)
            {
                jet.Dismiss();
            }
            else
            {
                if (jet.Exists()) jet.Delete();
            }
            stage = CalloutStage.None;
            suv.Dismiss();
            copCar.IsSirenOn = false;
            copCar.Dismiss();
            foreach (Ped p in peds.Where(p => p.Exists())) p.Dismiss();
        }

        public override bool OnBeforeCalloutDisplayed()
        {
            peds = new List<Ped>();

            suv =
                new Vehicle("GRANGER",
                    suvSpawnPoint,
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
                    suvSpawnPoint,
                    250)
                {
                    IsPersistent = true,
                    KeepTasks = true,
                    BlockPermanentEvents = true
                };
            driverPed.WarpIntoVehicle(suv, -1);
            peds.Add (driverPed);

            Main.CalloutCommons.Loadout copLoadout =
                Main
                    .CalloutCommons
                    .GetLoadoutFromZone(EBackupUnitType.LocalUnit,
                    "LosSantosCity",
                    true);
            copCar =
                new Vehicle(copLoadout.VehicleInfo.Model,
                    copSpawnPoint,
                    250)
                { IsPersistent = true, IsSirenOn = true, IsSirenSilent = true };
            for (int i = 0; i < 2 && i < copLoadout.NumPeds; i++)
            {
                Ped copPed =
                    new Ped(copLoadout.Peds[i].Model, copSpawnPoint, 250);
                peds.Add (copPed);
                copPed.WarpIntoVehicle(copCar, i - 1);
            }

            ShowCalloutAreaBlipBeforeAccepting(suvSpawnPoint, 15f);
            CalloutMessage = "Police escort requested for VIP.";
            CalloutPosition = suvSpawnPoint;

            string callsignAudio = Main.CalloutCommons.GetCallsignAudio();
            Functions
                .PlayScannerAudioUsingPosition($"ATTENTION_UNIT_SPECIFIC {callsignAudio} REQUESTING_ESCORT IN_OR_ON_POSITION",
                suvSpawnPoint);
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
                    bdgSpawnPoint,
                    341)
                {
                    IsPersistent = true,
                    KeepTasks = true,
                    BlockPermanentEvents = true
                };
            vipPed =
                new Ped("cs_bankman",
                    vipSpawnPoint,
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

            peds.Add (bodyguardPed);
            peds.Add (vipPed);
            peds.Add (pilotPed);

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
                        stage = CalloutStage.WaitingForVIP;
                        if (missionBlip.Exists()) missionBlip.Delete();
                        Game
                            .DisplaySubtitle("Wait for the ~b~VIP~w~ to enter the SUV.");
                        vipBlip = vipPed.AttachBlip();
                        vipBlip.Color = Color.SkyBlue;
                        vipBlip.Name = "VIP";
                        copBlip = copCar.AttachBlip();
                        copBlip.Color = Color.SkyBlue;
                        copBlip.Name = "Cop";

                        // bodyguardPed.Tasks.EnterVehicle(suv, 3000, 2, EnterVehicleFlags.DoNotEnter);
                        bodyguardPed
                            .Tasks
                            .GoToOffsetFromEntity(suv, 4f, 260, 1f);

                        // bodyguardPed.Tasks.GoStraightToPosition(suv.RightPosition, 1f, suv.Heading - 180f, 3f, 30000);
                        vipPed
                            .Tasks
                            .FollowToOffsetFromEntity(bodyguardPed,
                            new Vector3(1f, 1f, 0f));
                    }
                    break;
                case CalloutStage.WaitingForVIP:
                    if (bodyguardPed.DistanceTo(suv) < 3f)
                    {
                        stage = CalloutStage.WaitingForDoor;
                        vipPed.Tasks.Clear();
                        bodyguardPed
                            .Tasks
                            .EnterVehicle(suv,
                            3000,
                            2,
                            EnterVehicleFlags.DoNotEnter);
                    }
                    break;
                case CalloutStage.WaitingForDoor:
                    if (
                        bodyguardPed.Tasks.CurrentTaskStatus !=
                        TaskStatus.InProgress
                    )
                    {
                        stage = CalloutStage.WaitingForBodyguard;
                        bodyguardPed
                            .Tasks
                            .GoToOffsetFromEntity(suv, 2.2f, 260, 1f);
                    }
                    break;
                case CalloutStage.WaitingForBodyguard:
                    if (
                        bodyguardPed.Tasks.CurrentTaskStatus !=
                        TaskStatus.InProgress
                    )
                    {
                        stage = CalloutStage.EnteringVehicleVIP;
                        vipPed.Tasks.EnterVehicle(suv, 3000, 2);
                    }
                    break;
                case CalloutStage.EnteringVehicleVIP:
                    if (vipPed.IsInVehicle(suv, false))
                    {
                        stage = CalloutStage.EnteringVehicleBodyguard;
                        bodyguardPed.Tasks.EnterVehicle(suv, 0);
                    }
                    break;
                case CalloutStage.EnteringVehicleBodyguard:
                    if (bodyguardPed.IsInVehicle(suv, false))
                    {
                        stage = CalloutStage.EscortBeginning;
                        Game
                            .DisplaySubtitle("Escort the ~b~VIP~w~ to the ~y~airport~w~.");
                        missionBlip = jet.AttachBlip();
                        missionBlip.Color = Color.Yellow;
                        missionBlip.Name = "LSIA";
                        missionBlip.EnableRoute(Color.Yellow);
                        suv.Doors[3].Close(false);
                        copCar.IsSirenSilent = false;
                        VehicleDrivingFlags flags =
                            VehicleDrivingFlags.Emergency |
                            VehicleDrivingFlags.FollowTraffic |
                            VehicleDrivingFlags.StopAtDestination;
                        driverPed
                            .Tasks
                            .DriveToPosition(suv,
                            partialDestination,
                            40f,
                            flags,
                            5f);
                        Main
                            .CalloutCommons
                            .MakePedFollowTarget(copCar.Driver,
                            vipPed,
                            5f,
                            flags);
                    }
                    break;
                case CalloutStage.EscortBeginning:
                    if (suv.DistanceTo(partialDestination) < 20f)
                    {
                        stage = CalloutStage.Escort;
                        VehicleDrivingFlags flags =
                            VehicleDrivingFlags.Emergency |
                            VehicleDrivingFlags.FollowTraffic |
                            VehicleDrivingFlags.StopAtDestination;
                        driverPed
                            .Tasks
                            .DriveToPosition(suv, destination, 40f, flags, 5f);
                    }
                    break;
                case CalloutStage.Escort:
                    if (suv.DistanceTo(destination) < 6f)
                    {
                        stage = CalloutStage.ArrivedAtDestination;
                        if (missionBlip.Exists()) missionBlip.Delete();
                        Game
                            .DisplaySubtitle("Wait for the ~b~VIP~w~ to board the plane.");
                        bodyguardPed.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                    }
                    else if (suv.DistanceTo(destination) < 50f)
                    {
                        Main.CalloutCommons.StopPedFollowing(copCar.Driver);
                        copCar
                            .Driver
                            .Tasks
                            .DriveToPosition(copDestination,
                            20f,
                            VehicleDrivingFlags.Normal);
                        if (!copCar.IsSirenSilent) copCar.IsSirenSilent = true;
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
                            3000,
                            2,
                            EnterVehicleFlags.DoNotEnter);
                    }
                    break;
                case CalloutStage.ExitingVehicleVIP:
                    if (!vipPed.IsInVehicle(suv, true))
                    {
                        if (jet.Exists() && jet.IsAlive)
                        {
                            stage = CalloutStage.ApproachingPlaneVIP;
                            bodyguardPed
                                .Tasks
                                .FollowToOffsetFromEntity(vipPed,
                                new Vector3(1f, 1f, 0f));
                            vipPed.Tasks.GoToOffsetFromEntity(jet, 5f, 30f, 1f);
                        }
                        else
                        {
                            End();
                        }
                    }
                    break;
                case CalloutStage.ApproachingPlaneVIP:
                    if (vipPed.Tasks.CurrentTaskStatus != TaskStatus.InProgress)
                    {
                        stage = CalloutStage.BoardingPlaneVIP;
                        vipPed.Tasks.EnterVehicle(jet, -2);
                    }
                    break;
                case CalloutStage.BoardingPlaneVIP:
                    if (vipPed.IsInVehicle(jet, true))
                    {
                        stage = CalloutStage.None;
                        GameFiber
                            .StartNew(delegate ()
                            {
                                GameFiber.Sleep(1500);
                                stage = CalloutStage.BoardingPlaneBodyguard;
                                vipPed.WarpIntoVehicle(jet, 3);
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
                                        GameFiber.Sleep(5000); // ms
                                        pilotPed
                                            .Tasks
                                            .DriveToPosition(jetTakeOffPoint,
                                            10f,
                                            VehicleDrivingFlags.Normal,
                                            10f);
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
                    ?? playerPed.VehicleTryingToEnter ?? (Entity) playerPed;
            switch (stage)
            {
                case CalloutStage.PreArrival:
                    playerEntity.Position =
                        World.GetNextPositionOnStreet(suv.LeftPosition);
                    playerEntity.Heading =
                        playerEntity == playerPed ? 160f : 250f;
                    break;
                case CalloutStage.EscortBeginning:
                case CalloutStage.Escort:
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
