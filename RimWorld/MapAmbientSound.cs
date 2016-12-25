using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public sealed class MapAmbientSound
	{
		public MapAmbientSound()
		{
			SoundDefOf.AmbientAltitudeWind.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.None));
			List<SoundDef> soundsAmbient = Find.Map.Biome.soundsAmbient;
			for (int i = 0; i < soundsAmbient.Count; i++)
			{
				soundsAmbient[i].TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.None));
			}
		}
	}
}
