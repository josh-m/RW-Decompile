using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	internal class MedicalRecipesUtility
	{
		public static bool IsCleanAndDroppable(Pawn pawn, BodyPartRecord part)
		{
			return !pawn.Dead && !pawn.RaceProps.Animal && part.def.spawnThingOnRemoved != null && MedicalRecipesUtility.IsClean(pawn, part);
		}

		public static bool IsClean(Pawn pawn, BodyPartRecord part)
		{
			return !pawn.Dead && !(from x in pawn.health.hediffSet.hediffs
			where x.Part == part
			select x).Any<Hediff>();
		}

		public static void RestorePartAndSpawnAllPreviousParts(Pawn pawn, BodyPartRecord part, IntVec3 pos)
		{
			MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, pos);
			MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, pos);
			BodyPartDamageInfo value = new BodyPartDamageInfo(part, false, null);
			DamageInfo dinfo = new DamageInfo(DamageDefOf.RestoreBodyPart, 1, null, new BodyPartDamageInfo?(value), null);
			pawn.TakeDamage(dinfo);
		}

		public static Thing SpawnNaturalPartIfClean(Pawn pawn, BodyPartRecord part, IntVec3 pos)
		{
			if (MedicalRecipesUtility.IsCleanAndDroppable(pawn, part))
			{
				return GenSpawn.Spawn(part.def.spawnThingOnRemoved, pos);
			}
			return null;
		}

		public static void SpawnThingsFromHediffs(Pawn pawn, BodyPartRecord part, IntVec3 pos)
		{
			if (!pawn.health.hediffSet.GetNotMissingParts(null, null).Contains(part))
			{
				return;
			}
			IEnumerable<Hediff> enumerable = from x in pawn.health.hediffSet.hediffs
			where x.Part == part
			select x;
			foreach (Hediff current in enumerable)
			{
				if (current.def.spawnThingOnRemoved != null)
				{
					GenSpawn.Spawn(current.def.spawnThingOnRemoved, pos);
				}
			}
			for (int i = 0; i < part.parts.Count; i++)
			{
				MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part.parts[i], pos);
			}
		}
	}
}
