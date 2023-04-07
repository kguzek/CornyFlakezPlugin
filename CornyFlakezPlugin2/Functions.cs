using System;
using System.Collections.Generic;
using Rage;

namespace CornyFlakezPlugin2
{
    class Functions
    {
        public static List<KeyValuePair<Vehicle, Blip>> vehicles = new List<KeyValuePair<Vehicle, Blip>>();
        public static readonly List<Ped> peds = new List<Ped>();
        private static Dictionary<Ped, GameFiber> gameFibres = new Dictionary<Ped, GameFiber>();

        public static void SpawnVehicle(Model vehModel)
        {
            Vector3 playerPosition = Game.LocalPlayer.Character.Position;
            Vector3 spawnLocation = World.GetNextPositionOnStreet(playerPosition.Around(10f, 20f));
            Vehicle veh = new Vehicle(vehModel, spawnLocation);
            vehicles.Add(new KeyValuePair<Vehicle, Blip>(veh, null));
            SetVehicleBlipToggleStatus(vehicles.Count - 1, false);
        }

        public static void SpawnPeds(int numPeds, Model pedModel)
        {
            Vector3 playerPosition = Game.LocalPlayer.Character.Position;
            Vector3 spawnPosition = World.GetNextPositionOnStreet(playerPosition.Around(10f, 20f));
            for (int i = 0; i < numPeds; i++)
            {
                Vector3 pedPosition = World.GetNextPositionOnStreet(spawnPosition.Around2D(5));
                Ped ped = new Ped(pedModel, pedPosition, new Random().Next(360));
                peds.Add(ped);
            }

        }

        public static void MakePedGoToVehicle(int pedIndex, int vehIndex, bool goStraightToVehicle = false, float angle = 0f)
        {
            Ped ped = peds[pedIndex];
            Vehicle vehicle = vehicles[vehIndex].Key;
            if (goStraightToVehicle)
                ped.Tasks.GoStraightToPosition(vehicle.LeftPosition, 1f, angle, 3f, 10000);
            else
                ped.Tasks.GoToOffsetFromEntity(vehicle, 1.5f, 80f, 1f);
        }

        public static void MakePedEnterVehicle(int pedIndex, int vehIndex, int seatIndex, bool instantly)
        {
            Vehicle vehicle = vehicles[vehIndex].Key;
            if (!vehicle.Exists())
            {
                Game.LogTrivial("Error! Vehicle does not exist in the game world.");
                return;
            }
            Ped ped = peds[pedIndex];
            if (!ped.Exists())
            {
                peds.RemoveAt(pedIndex);
                Game.LogTrivial("Error! Ped does not exist in the game world.");
                return;
            }
            if (instantly)
                ped.WarpIntoVehicle(vehicle, seatIndex);
            else
                ped.Tasks.EnterVehicle(vehicle, seatIndex);
            Game.LogTrivial($"Made {ped.Model.Name} enter {vehicle.Model.Name} at seat {seatIndex}.");
        }

        public static void MakePedDriveAwayInVehicle(int pedIndex, int vehIndex, float speed)
        {
            Vehicle vehicle = vehicles[vehIndex].Key;
            if (!vehicle.Exists() || peds.Count <= pedIndex || !peds[pedIndex].Exists())
                return;
            vehicle.IsSirenOn = true;
            vehicle.IsSirenSilent = true;
            vehicle.BlipSiren(true);
            Vector3 destination = World.GetNextPositionOnStreet(peds[pedIndex].Position.Around(200f, 300f));
            peds[pedIndex].Tasks.DriveToPosition(vehicle, destination, speed, VehicleDrivingFlags.Normal, 50f);
        }

        public static void MakePedFollowPlayer(int pedIndex, int vehIndex, float distance, bool chaseMode)
        {
            Ped ped = peds[pedIndex];
            Vehicle vehicle = vehicles[vehIndex].Key;
            if (ped.IsInAnyVehicle(true))
                vehicle = ped.CurrentVehicle ?? ped.VehicleTryingToEnter;
            Ped targetPed = Game.LocalPlayer.Character;
            Vehicle targetVehicle = targetPed.CurrentVehicle;

            VehicleDrivingFlags flags = chaseMode ? VehicleDrivingFlags.Emergency : VehicleDrivingFlags.Normal;
            if (chaseMode)
                vehicle.IsSirenOn = true;

            void FollowTarget()
            {
                bool attemptedToCarjackTarget = false;
                while (true)
                {
                    GameFiber.Sleep(250); // In milliseconds
                    if (!ped.Exists() || !ped.IsAlive || !vehicle.Exists() || !vehicle.IsAlive)
                        break;
                    if (!targetPed.IsInAnyVehicle(true))
                        break;
                    if (ped.DistanceTo(targetPed.Position) > distance + 2f)
                    {
                        attemptedToCarjackTarget = false;
                        vehicle.IsSirenSilent = false;
                        float speed = Math.Max(ped.DistanceTo(targetPed.Position), targetPed.Speed * 1.2f);
                        if (ped.IsInVehicle(vehicle, false) || ped.Tasks.CurrentTaskStatus != TaskStatus.InProgress)
                            ped.Tasks.DriveToPosition(vehicle, targetPed.Position, speed, flags, distance);
                    }
                    else
                    {
                        vehicle.IsSirenSilent = true;
                        if (targetVehicle.Speed == 0 && !attemptedToCarjackTarget && chaseMode)
                        {
                            attemptedToCarjackTarget = true;
                            EnterVehicleFlags carjackFlags = EnterVehicleFlags.AllowJacking | EnterVehicleFlags.DoNotEnter;
                            ped.Tasks.EnterVehicle(targetVehicle, -1, carjackFlags);
                        }
                    }
                }
                if (ped.Exists() && ped.IsAlive && !ped.IsInAnyVehicle(false))
                    ped.Tasks.EnterVehicle(vehicle, -1);
                vehicle.IsSirenOn = false;
            }

            if (gameFibres.ContainsKey(ped))
            {
                gameFibres[ped].Abort();
                gameFibres.Remove(ped);
            }
            gameFibres.Add(ped, GameFiber.StartNew(FollowTarget));
        }

        public static void EmptyVehicle(int vehIndex)
        {
            Vehicle vehicle = vehicles[vehIndex].Key;
            if (!vehicle.Exists())
            {
                Game.LogTrivial("Error! Vehicle does not exist in the game world.");
                return;
            }
            for (int i = 0; i < peds.Count; i++)
            {
                if (!peds[i].Exists())
                    continue;
                if (!peds[i].IsInVehicle(vehicle, true))
                    continue;
                peds[i].Tasks.LeaveVehicle(vehicle, LeaveVehicleFlags.None);
            }
        }

        public static void SetVehicleBlipToggleStatus(int vehIndex, bool blipEnabled)
        {
            Vehicle vehicle = vehicles[vehIndex].Key;
            if (blipEnabled)
            {
                if (vehicle.Exists())
                {
                    char first = char.ToUpper(vehicle.Model.Name[0]);
                    string rest = vehicle.Model.Name.Substring(1);
                    Blip blip = new Blip(vehicle)
                    {
                        Name = first + rest
                    };
                    vehicles[vehIndex] = new KeyValuePair<Vehicle, Blip>(vehicle, blip);
                }
            }
            else
            {
                if (vehicles[vehIndex].Value.Exists())
                    vehicles[vehIndex].Value.Delete();
            }
        }

        public static void SetVehicleBlipColour(int vehIndex, System.Drawing.KnownColor colour)
        {
            Blip blip = vehicles[vehIndex].Value;
            if (blip.Exists())
                blip.Color = System.Drawing.Color.FromKnownColor(colour);
        }

        public static void ClearPedsAndVehicles()
        {
            foreach (Ped ped in gameFibres.Keys)
            {
                GameFiber gameFibre = gameFibres[ped];
                if (!gameFibre.IsAlive)
                    continue;
                gameFibre.Abort();
            }
            for (int i = 0; i < peds.Count; i++)
            {
                Ped ped = peds[i];
                if (!ped.Exists())
                    continue;
                ped.Tasks.Wander();
                ped.Dismiss();
            }
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i].Key.Exists())
                    vehicles[i].Key.Delete();
                if (vehicles[i].Value.Exists())
                    vehicles[i].Value.Delete();
            }
            gameFibres.Clear();
            peds.Clear();
            vehicles.Clear();
        }
    }
}