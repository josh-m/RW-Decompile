using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public static class SoundSlotManager
	{
		private static Dictionary<string, float> allowedPlayTimes = new Dictionary<string, float>();

		public static bool CanPlayNow(string slotName)
		{
			if (slotName == string.Empty)
			{
				return true;
			}
			float num = 0f;
			return !SoundSlotManager.allowedPlayTimes.TryGetValue(slotName, out num) || Time.realtimeSinceStartup >= SoundSlotManager.allowedPlayTimes[slotName];
		}

		public static void Notify_Played(string slot, float duration)
		{
			if (slot == string.Empty)
			{
				return;
			}
			float a;
			if (SoundSlotManager.allowedPlayTimes.TryGetValue(slot, out a))
			{
				SoundSlotManager.allowedPlayTimes[slot] = Mathf.Max(a, Time.realtimeSinceStartup + duration);
			}
			else
			{
				SoundSlotManager.allowedPlayTimes[slot] = Time.realtimeSinceStartup + duration;
			}
		}
	}
}
