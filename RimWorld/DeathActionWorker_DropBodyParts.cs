using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class DeathActionWorker_DropBodyParts : DeathActionWorker
	{
		public override void PawnDied(Corpse corpse)
		{
			Pawn innerPawn = corpse.InnerPawn;
			PawnKindLifeStage lifeStage = innerPawn.ageTracker.CurKindLifeStage;
			if (lifeStage.dropBodyPart != null)
			{
				if (!lifeStage.dropBodyPart.allowMale && innerPawn.gender == Gender.Male)
				{
					return;
				}
				if (!lifeStage.dropBodyPart.allowFemale && innerPawn.gender == Gender.Female)
				{
					return;
				}
				while (true)
				{
					BodyPartRecord bodyPartRecord = (from x in innerPawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
					where x.IsInGroup(lifeStage.dropBodyPart.bodyPartGroup)
					select x).FirstOrDefault<BodyPartRecord>();
					if (bodyPartRecord == null)
					{
						break;
					}
					innerPawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, innerPawn, bodyPartRecord), null, null);
					Thing thing;
					if (lifeStage.dropBodyPart.thing != null)
					{
						thing = ThingMaker.MakeThing(lifeStage.dropBodyPart.thing, null);
					}
					else
					{
						thing = ThingMaker.MakeThing(bodyPartRecord.def.spawnThingOnRemoved, null);
					}
					Thing forbiddenIfOutsideHomeArea;
					GenPlace.TryPlaceThing(thing, corpse.Position, corpse.Map, ThingPlaceMode.Near, out forbiddenIfOutsideHomeArea, null);
					forbiddenIfOutsideHomeArea.SetForbiddenIfOutsideHomeArea();
				}
			}
		}
	}
}
