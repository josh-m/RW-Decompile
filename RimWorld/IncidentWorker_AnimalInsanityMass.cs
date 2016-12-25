using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class IncidentWorker_AnimalInsanityMass : IncidentWorker
	{
		public static bool AnimalUsable(Pawn p)
		{
			return p.Spawned && !p.Position.Fogged(p.Map) && (!p.InMentalState || !p.MentalStateDef.IsAggro) && !p.Downed && p.Faction == null;
		}

		public static void DriveInsane(Pawn p)
		{
			p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, true, false, null);
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (parms.points <= 0f)
			{
				Log.Error("AnimalInsanity running without points.");
				parms.points = (float)((int)(map.strengthWatcher.StrengthRating * 50f));
			}
			float adjustedPoints = parms.points;
			if (adjustedPoints > 250f)
			{
				adjustedPoints -= 250f;
				adjustedPoints *= 0.5f;
				adjustedPoints += 250f;
			}
			IEnumerable<PawnKindDef> source = from def in DefDatabase<PawnKindDef>.AllDefs
			where def.RaceProps.Animal && def.combatPower <= adjustedPoints && (from p in map.mapPawns.AllPawnsSpawned
			where p.kindDef == def && IncidentWorker_AnimalInsanityMass.AnimalUsable(p)
			select p).Count<Pawn>() >= 3
			select def;
			PawnKindDef animalDef;
			if (!source.TryRandomElement(out animalDef))
			{
				return false;
			}
			List<Pawn> list = (from p in map.mapPawns.AllPawnsSpawned
			where p.kindDef == animalDef && IncidentWorker_AnimalInsanityMass.AnimalUsable(p)
			select p).ToList<Pawn>();
			float combatPower = animalDef.combatPower;
			float num = 0f;
			int num2 = 0;
			Pawn pawn = null;
			list.Shuffle<Pawn>();
			foreach (Pawn current in list)
			{
				if (num + combatPower > adjustedPoints)
				{
					break;
				}
				IncidentWorker_AnimalInsanityMass.DriveInsane(current);
				num += combatPower;
				num2++;
				pawn = current;
			}
			if (num == 0f)
			{
				return false;
			}
			string label;
			string text;
			if (num2 == 1)
			{
				label = "LetterLabelAnimalInsanitySingle".Translate() + ": " + pawn.LabelCap;
				text = "AnimalInsanitySingle".Translate(new object[]
				{
					pawn.LabelShort
				});
			}
			else
			{
				label = "LetterLabelAnimalInsanityMultiple".Translate() + ": " + animalDef.LabelCap;
				text = "AnimalInsanityMultiple".Translate(new object[]
				{
					animalDef.label
				});
			}
			Find.LetterStack.ReceiveLetter(label, text, LetterType.BadUrgent, pawn, null);
			if (map == Find.VisibleMap)
			{
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
				Find.CameraDriver.shaker.DoShake(1f);
			}
			return true;
		}
	}
}
