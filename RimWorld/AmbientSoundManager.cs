using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class AmbientSoundManager
	{
		private static List<Sustainer> biomeAmbientSustainers = new List<Sustainer>();

		private static Action recreateMapSustainers = new Action(AmbientSoundManager.RecreateMapSustainers);

		private static bool WorldAmbientSoundCreated
		{
			get
			{
				return Find.SoundRoot.sustainerManager.SustainerExists(SoundDefOf.AmbientSpace);
			}
		}

		private static bool AltitudeWindSoundCreated
		{
			get
			{
				return Find.SoundRoot.sustainerManager.SustainerExists(SoundDefOf.AmbientAltitudeWind);
			}
		}

		public static void EnsureWorldAmbientSoundCreated()
		{
			if (!AmbientSoundManager.WorldAmbientSoundCreated)
			{
				SoundDefOf.AmbientSpace.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.None));
			}
		}

		public static void Notify_SwitchedMap()
		{
			LongEventHandler.ExecuteWhenFinished(AmbientSoundManager.recreateMapSustainers);
		}

		private static void RecreateMapSustainers()
		{
			if (!AmbientSoundManager.AltitudeWindSoundCreated)
			{
				SoundDefOf.AmbientAltitudeWind.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.None));
			}
			SustainerManager sustainerManager = Find.SoundRoot.sustainerManager;
			for (int i = 0; i < AmbientSoundManager.biomeAmbientSustainers.Count; i++)
			{
				Sustainer sustainer = AmbientSoundManager.biomeAmbientSustainers[i];
				if (sustainerManager.AllSustainers.Contains(sustainer) && !sustainer.Ended)
				{
					sustainer.End();
				}
			}
			AmbientSoundManager.biomeAmbientSustainers.Clear();
			if (Find.VisibleMap != null)
			{
				List<SoundDef> soundsAmbient = Find.VisibleMap.Biome.soundsAmbient;
				for (int j = 0; j < soundsAmbient.Count; j++)
				{
					Sustainer item = soundsAmbient[j].TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.None));
					AmbientSoundManager.biomeAmbientSustainers.Add(item);
				}
			}
		}
	}
}
