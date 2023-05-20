using System;
using System.Collections.Generic;
using LSPD_First_Response;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;

using static CornyFlakezPlugin.CalloutCommons;

namespace CornyFlakezPlugin.Callouts
{
    [CalloutInfo("Vehicle Pursuit", CalloutProbability.High)]
    public class VehiclePursuit : Callout
    {
        private LHandle pursuit;

        private Vector3 spawnPoint;

        private Ped suspectPed;

        private Vehicle suspectVehicle;

        private enum Direction
        {
            NORTH = 0,
            EAST = 3,
            SOUTH = 2,
            WEST = 1
        }

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnPoint =
                World
                    .GetNextPositionOnStreet(Game
                        .LocalPlayer
                        .Character
                        .Position
                        .AroundBetween(300f, 600f));
            Model[] VehicleModels =
                new Model[] {
                    "NINFEF2",
                    "BALLER",
                    "BALLER2",
                    "BANSHEE",
                    "BJXL",
                    "BENSON",
                    "BOBCATXL",
                    "BUCCANEER",
                    "BUFFALO",
                    "BUFFALO2",
                    "BULLET",
                    "BURRITO",
                    "BURRITO2",
                    "BURRITO3",
                    "BURRITO4",
                    "BURRITO5",
                    "CAVALCADE",
                    "CAVALCADE2",
                    "GBURRITO",
                    "CAMPER",
                    "CARBONIZZARE",
                    "CHEETAH",
                    "COMET2",
                    "COGCABRIO",
                    "COQUETTE",
                    "GRESLEY",
                    "DUNE2",
                    "HOTKNIFE",
                    "DUBSTA",
                    "DUBSTA2",
                    "DOMINATOR",
                    "EMPEROR",
                    "EMPEROR2",
                    "EMPEROR3",
                    "ENTITYXF",
                    "EXEMPLAR",
                    "ELEGY2",
                    "F620",
                    "FBI",
                    "FBI2",
                    "FELON",
                    "FELON2",
                    "FELTZER2",
                    "FIRETRUK",
                    "FQ2",
                    "FUGITIVE",
                    "FUTO",
                    "GRANGER",
                    "GAUNTLET",
                    "HABANERO",
                    "INFERNUS",
                    "INTRUDER",
                    "JACKAL",
                    "JOURNEY",
                    "JB700",
                    "KHAMELION",
                    "LANDSTALKER",
                    "MESA",
                    "MESA2",
                    "MESA3",
                    "MIXER",
                    "MINIVAN",
                    "MIXER2",
                    "MULE",
                    "MULE2",
                    "ORACLE",
                    "ORACLE2",
                    "MONROE",
                    "PATRIOT",
                    "PENUMBRA",
                    "PEYOTE",
                    "PHANTOM",
                    "PHOENIX",
                    "PICADOR",
                    "PRIMO",
                    "RANCHERXL",
                    "RANCHERXL2",
                    "RAPIDGT",
                    "RAPIDGT2",
                    "RUINER",
                    "RIPLEY",
                    "SABREGT",
                    "SADLER",
                    "SADLER2",
                    "SANDKING",
                    "SANDKING2",
                    "SPEEDO",
                    "SPEEDO2",
                    "STINGER",
                    "STINGERGT",
                    "SUPERD",
                    "STRATUM",
                    "SULTAN",
                    "AKUMA",
                    "PCJ",
                    "FAGGIO2",
                    "DAEMON",
                    "BATI2"
                };
            Model randomModel =
                VehicleModels[new Random().Next(VehicleModels.Length)];
            suspectVehicle =
                new Vehicle(randomModel,
                    spawnPoint,
                    new Random().Next(360))
                { IsPersistent = true };
            suspectPed =
                new Ped(spawnPoint)
                { IsPersistent = true, BlockPermanentEvents = true };
            suspectPed.WarpIntoVehicle(suspectVehicle, -1);
            ShowCalloutAreaBlipBeforeAccepting(spawnPoint, 15f);

            CalloutMessage = "Officers requesting backup for vehicle pursuit.";
            CalloutPosition = spawnPoint;

            Functions
                .PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS OFFICERS_REPORT CRIME_RESIST_ARREST IN_OR_ON_POSITION",
                spawnPoint);
            pursuit = Functions.CreatePursuit();
            Functions.AddPedToPursuit (pursuit, suspectPed);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Game.DisplaySubtitle("Join the ~r~pursuit~w~.", 7500);
            Functions.SetPursuitIsActiveForPlayer(pursuit, true);


            #region Relaying suspect details via scanner
            float heading = suspectVehicle.Heading;
            Direction dir = (Direction)(Math.Round(heading / 90) % 4);
            string directionScannerAudio = (new Random().Next(2) == 0)
              ? "SUSPECT_HEADING DIRECTION_HEADING_"
              : "SUSPECT_IS DIRECTION_BOUND_";

            Functions
                .PlayScannerAudio($"{directionScannerAudio}{dir} IN_A {GetVehicleDescription(suspectVehicle)} UNITS_RESPOND_CODE_03");
            #endregion


            Functions
                .RequestBackup(suspectVehicle.Position,
                EBackupResponseType.Pursuit,
                EBackupUnitType.AirUnit);
            Functions
                .RequestBackup(suspectVehicle.Position,
                EBackupResponseType.Pursuit,
                EBackupUnitType.LocalUnit);
            if (IsLSPDFRPluginRunning("CornyFlakezPlugin2.dll"))
            {
                DebugPluginFunctions
                    .PassDebugInfo(new List<Ped> { suspectPed },
                    new List<Vehicle> { suspectVehicle });
            }
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            Functions.ForceEndPursuit (pursuit);
            new List<Entity> { suspectPed, suspectVehicle }
                .ForEach(e =>
                {
                    if (e.Exists()) e.Dismiss();
                });
            base.OnCalloutNotAccepted();
            Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL");
        }

        public override void Process()
        {
            ProcessCallout(this, new List<Ped> { suspectPed });
            if (!Functions.IsPursuitStillRunning(pursuit)) End();
            base.Process();
        }

        public override void End()
        {
            if (suspectPed.Exists()) suspectPed.Dismiss();
            Code statusCode = Code.Code4;
            if (Functions.IsPursuitStillRunning(pursuit))
            {
                Functions.ForceEndPursuit (pursuit);
                statusCode = Code.Code4Adam;
            }
            else
            {
                statusCode |=
                    !suspectPed.Exists() || suspectPed.IsAlive
                        ? Code.SuspectInCustody
                        : Code.SuspectDown;
            }
            EndCallout(this, statusCode);
            base.End();
        }
    }
}
