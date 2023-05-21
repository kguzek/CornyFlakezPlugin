using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Rage;

namespace CornyFlakezPlugin2
{
  public static class Util
  {
    private static Dictionary<Keys, DateTime?> trackedKeys = new Dictionary<Keys, DateTime?>();

    private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

    // private const string VehiclePreviewsResourceName = "CornyFlakezPlugin2.Resources.vehiclePreviews.json";
    private const string VehiclePreviewsResourceName = "vehiclePreviews.json";

    private static DateTime lastCheck;

    private static readonly Dictionary<string, (string, int[])[]> identificationSpeeches = new Dictionary<string, (string, int[])[]>()
    {
      {"lspd", new (string, int[])[] {
        ("S_M_Y_COP_01_BLACK_FULL_01", new int[] { 1,2,3 }),
        ("S_M_Y_COP_01_BLACK_FULL_02", new int[] { 1 }),
        ("S_M_Y_COP_01_WHITE_FULL_01", new int[] { 2 }),
        ("S_M_Y_COP_01_WHITE_FULL_02", new int[] { 1,2,3 }),
        ("S_M_Y_HWAYCOP_01_BLACK_FULL_02", new int[] { 1 }),
        ("S_M_Y_HWAYCOP_01_WHITE_FULL_01", new int[] { 1,2 }),
        ("S_M_Y_COP_01_BLACK_MINI_01", new int[] { 1,3 }),
        ("S_M_Y_COP_01_BLACK_MINI_02", new int[] { 1,3 }),
        ("S_M_Y_COP_01_BLACK_MINI_04", new int[] { 2 }),
        ("S_M_Y_COP_01_WHITE_MINI_01", new int[] { 3 }),
        ("S_M_Y_COP_01_WHITE_MINI_02", new int[] { 1,2,3 }),
        ("S_M_Y_COP_01_WHITE_MINI_03", new int[] { 1,2,3 }),
        ("S_M_Y_COP_01_WHITE_MINI_04", new int[] { 3 }),
        }},
      {"sahp", new (string, int[])[] {
        ("S_M_Y_HWAYCOP_01_BLACK_FULL_01", new int[] { 1, 2, 3, }),
        ("S_M_Y_HWAYCOP_01_WHITE_FULL_02", new int[] { 2, 3, }),
        }},
      {"sheriff", new (string, int[])[] {
        ("S_M_Y_SHERIFF_01_WHITE_FULL_01", new int[] { 1, 2, }),
        ("S_M_Y_SHERIFF_01_WHITE_FULL_02", new int[] { 1, 2, }),
        }},
      {"nysp", new (string, int[])[] {
        ("S_M_Y_MCOP_01_WHITE_MINI_01", new int[] { 1, }),
        ("S_M_Y_MCOP_01_WHITE_MINI_02", new int[] { 1, }),
        }},
      {"generic", new (string, int[])[] {
        ("S_M_Y_COP_01_BLACK_FULL_02", new int[] { 3 }),
        ("S_M_Y_COP_01_WHITE_FULL_01", new int[] { 1,3 }),
        ("S_M_Y_HWAYCOP_01_BLACK_FULL_02", new int[] { 2,3 }),
        ("S_M_Y_HWAYCOP_01_WHITE_FULL_01", new int[] { 3 }),
        ("S_M_Y_HWAYCOP_01_WHITE_FULL_02", new int[] { 1 }),
        ("S_M_Y_COP_01_BLACK_MINI_01", new int[] { 2 }),
        ("S_M_Y_COP_01_BLACK_MINI_02", new int[] { 2 }),
        ("S_M_Y_COP_01_BLACK_MINI_03", new int[] { 1,2,3 }),
        ("S_M_Y_COP_01_BLACK_MINI_04", new int[] { 1,3 }),
        ("S_M_Y_COP_01_WHITE_MINI_01", new int[] { 1,2 }),
        ("S_M_Y_MCOP_01_WHITE_MINI_02", new int[] { 1 }),
        }},
    };

    static int[] a = { 1, 2, 3 };

    // Iterates through each tracked key, updating the timestamp it was pressed on.
    private static void CheckTrackedKeys()
    {
      if (lastCheck == DateTime.Now) return;
      Dictionary<Keys, DateTime?> newValues = new Dictionary<Keys, DateTime?>();
      foreach (KeyValuePair<Keys, DateTime?> item in trackedKeys)
      {
        bool isKeyDown = Game.IsKeyDownRightNow(item.Key);
        bool currentValueIsNull = item.Value == null;
        if (isKeyDown == currentValueIsNull)
        {
          DateTime? newValue = isKeyDown ? (DateTime?)DateTime.Now : null;
          newValues.Add(item.Key, newValue);
        }
      }
      foreach (KeyValuePair<Keys, DateTime?> item in newValues)
      {
        trackedKeys[item.Key] = item.Value;
      }
      lastCheck = DateTime.Now;
    }

    public static bool WasKeyHeld(Keys key, int durationMillis)
    {
      CheckTrackedKeys();
      DateTime? pressedAt;
      if (!trackedKeys.TryGetValue(key, out pressedAt))
      {
        // Add this previously untracked key to the tracking queue
        bool isPressedNow = Game.IsKeyDown(key);
        pressedAt = isPressedNow ? (DateTime?)DateTime.Now : null;
        trackedKeys.Add(key, pressedAt);
        return false;
      }
      if (pressedAt == null)
      {
        // There isn't a start timestamp for the key being pressed (ie. it isn't being held now)
        return false;
      }
      TimeSpan duration = TimeSpan.FromMilliseconds(durationMillis);
      return pressedAt.Value + duration <= DateTime.Now;
    }


    public static string GetVehicleTextureDictionary(string modelName)
    {
      VehicleTxdInfo vehicleTxdInfo;
      if (!VehiclePreviews.VehicleTxdInfos.TryGetValue(modelName, out vehicleTxdInfo))
      {
        return null;

      }
      return vehicleTxdInfo.TextureDictionary;
    }

    public static void PlayIdentificationSpeech()
    {
      // TODO: Change me
      // string agency = LSPD_First_Response.Mod.API.Functions.GetCurrentAgencyScriptName();
      string agency = "lspd";
      (string, int[])[] speeches;
      bool usingSheriffSpeech = false;
      identificationSpeeches.TryGetValue(agency, out speeches);
      if (speeches == null)
      {
        switch (agency)
        {
          case "lssd":
          case "bcso":
            speeches = identificationSpeeches["sheriff"];
            usingSheriffSpeech = true;
            break;
          default:
            speeches = identificationSpeeches["generic"];
            break;
        }
      }
      int randomVoiceIdx = new Random().Next(speeches.Length);
      (string, int[]) voiceInfo = speeches[randomVoiceIdx];
      string voice = voiceInfo.Item1;
      int randomSpeechVariantIdx = new Random().Next(voiceInfo.Item2.Length);
      int speechVariant = voiceInfo.Item2[randomSpeechVariantIdx];
      string speechName = usingSheriffSpeech ? "WAIT" : "COP_ARRIVAL_ANNOUNCE";
      Game.LocalPlayer.Character.PlayAmbientSpeech(voice, speechName, speechVariant, SpeechModifier.Force);
    }

  }


  public class VehicleTxdInfo
  {
    public string DisplayName { get; set; }
    public string TextureDictionary { get; set; }
    public string StorePageURL { get; set; }
  }
}