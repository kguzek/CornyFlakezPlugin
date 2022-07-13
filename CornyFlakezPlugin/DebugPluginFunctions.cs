using System.Collections.Generic;
using Rage;

namespace CornyFlakezPlugin
{
    class DebugPluginFunctions
    {
        public static void PassDebugInfo(
            List<Ped> peds = null,
            List<Vehicle> vehicles = null
        )
        {
            CornyFlakezPlugin2.EntryPoint.currentDebugInfo =
                new CornyFlakezPlugin2.EntryPoint.CalloutDebugInfo()
                {
                    peds = peds ?? new List<Ped>(),
                    vehicles = vehicles ?? new List<Vehicle>()
                };
        }
    }
}
