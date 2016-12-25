using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_TakeDrug : JoyGiver_Ingest
	{
		private static List<ThingDef> takeableDrugs = new List<ThingDef>();

		protected override Thing BestIngestItem(Pawn pawn, Predicate<Thing> extraValidator)
		{
			Predicate<Thing> predicate = (Thing t) => this.CanIngestForJoy(pawn, t) && (extraValidator == null || extraValidator(t));
			ThingContainer innerContainer = pawn.inventory.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (predicate(innerContainer[i]))
				{
					return innerContainer[i];
				}
			}
			JoyGiver_TakeDrug.takeableDrugs.Clear();
			DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
			for (int j = 0; j < currentPolicy.Count; j++)
			{
				if (currentPolicy[j].allowedForJoy)
				{
					JoyGiver_TakeDrug.takeableDrugs.Add(currentPolicy[j].drug);
				}
			}
			JoyGiver_TakeDrug.takeableDrugs.Shuffle<ThingDef>();
			for (int k = 0; k < JoyGiver_TakeDrug.takeableDrugs.Count; k++)
			{
				List<Thing> list = pawn.Map.listerThings.ThingsOfDef(JoyGiver_TakeDrug.takeableDrugs[k]);
				if (list.Count > 0)
				{
					Predicate<Thing> validator = predicate;
					Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, list, PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null);
					if (thing != null)
					{
						return thing;
					}
				}
			}
			return null;
		}

		public override float GetChance(Pawn pawn)
		{
			int num = 0;
			if (pawn.story != null)
			{
				num = pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
			}
			if (num < 0)
			{
				return 0f;
			}
			float num2 = this.def.baseChance;
			if (num == 1)
			{
				num2 *= 2f;
			}
			if (num == 2)
			{
				num2 *= 5f;
			}
			return num2;
		}

		protected override Job CreateIngestJob(Thing ingestible, Pawn pawn)
		{
			return DrugAIUtility.IngestAndTakeToInventoryJob(ingestible, pawn, 9999);
		}
	}
}
