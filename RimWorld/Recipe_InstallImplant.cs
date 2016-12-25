using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Recipe_InstallImplant : Recipe_Surgery
	{
		[DebuggerHidden]
		public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
		{
			for (int i = 0; i < recipe.appliedOnFixedBodyParts.Count; i++)
			{
				BodyPartDef part = recipe.appliedOnFixedBodyParts[i];
				List<BodyPartRecord> bpList = pawn.RaceProps.body.AllParts;
				for (int j = 0; j < bpList.Count; j++)
				{
					BodyPartRecord record = bpList[j];
					if (record.def == part)
					{
						if (pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined).Contains(record))
						{
							if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
							{
								if (!pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == this.<record>__4 && x.def == this.recipe.addsHediff))
								{
									yield return record;
								}
							}
						}
					}
				}
			}
		}

		public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients)
		{
			if (billDoer != null)
			{
				if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part))
				{
					return;
				}
				TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
				{
					billDoer,
					pawn
				});
			}
			pawn.health.AddHediff(this.recipe.addsHediff, part, null);
		}
	}
}
