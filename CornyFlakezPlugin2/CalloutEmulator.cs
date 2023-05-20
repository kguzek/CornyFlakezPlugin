using Rage;
using System.Linq;

namespace CornyFlakezPlugin2
{
    public class Callout
    {
        
        public string FriendlyName;
        public string CalloutMessage;
        public Vector3 CalloutPosition;
        
        public virtual bool OnBeforeCalloutDisplayed()
        {
        return false;
        }
        
        public virtual bool OnCalloutAccepted()
        {
        return false;
        }
        
        public virtual void OnCalloutNotAccepted()
        {}
        
        public virtual void Process()
        {}
        
        public virtual void End()
        {
          Main.currentDebugInfo.activeCallout = null;
        }
        
        public void ShowCalloutAreaBlipBeforeAccepting(Vector3 position, float radius) {}

        public Callout()
        {
            FriendlyName = Functions.GetCalloutName(this.GetType());
        }
    }

    partial class Functions {
        public static void PlayScannerAudio(string audio) {}
        public static void PlayScannerAudioUsingPosition(string audio, Vector3 position) {}

        public static string GetCalloutName(System.Type calloutType)
        {
            var attributeType = typeof(CalloutInfoAttribute);
            var attribute = calloutType.GetCustomAttributes(attributeType, true).FirstOrDefault() as CalloutInfoAttribute;
            return attribute?.Name;
        }
    }

    public enum CalloutProbability
    {
        Always = 0,
        VeryHigh = 1,
        High = 2,
        Medium = 3,
        Low = 4,
        VeryLow = 5,
        Never = 6,
    }

    public class CalloutInfoAttribute : System.Attribute
    {
        public string Name;
        private CalloutProbability Probability;
        
        public CalloutInfoAttribute(string name, CalloutProbability probability)
        {
            Probability = probability;
            Name = name;
        }
    }

    public enum EBackupUnitType
    {
        LocalUnit = 0,
        StateUnit = 1,
        SwatTeam = 2,
        NooseTeam = 3,
        AirUnit = 4,
        NooseAirUnit = 5,
        Ambulance = 6,
        Firetruck = 7,
    }
}
