using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Rage;

namespace CornyFlakezPlugin2
{
  public static class Util
  {
    private static Dictionary<Keys, DateTime?> trackedKeys = new Dictionary<Keys, DateTime?>();

    private static DateTime lastCheck;

    // Iterates through each tracked key, updating the timestamp it was pressed on.
    private static void checkTrackedKeys()
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
      checkTrackedKeys();
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
  }
}