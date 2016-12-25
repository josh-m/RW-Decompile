using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_BondedAnimalMaster : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (directRelations[i].def == PawnRelationDefOf.Bond && !directRelations[i].otherPawn.Dead && directRelations[i].otherPawn.Spawned && directRelations[i].otherPawn.Faction == Faction.OfPlayer && directRelations[i].otherPawn.training.IsCompleted(TrainableDefOf.Obedience) && this.AnimalMasterCheck(p, directRelations[i].otherPawn))
				{
					return ThoughtState.ActiveWithReason(directRelations[i].otherPawn.LabelShort);
				}
			}
			return false;
		}

		protected virtual bool AnimalMasterCheck(Pawn p, Pawn animal)
		{
			return animal.playerSettings.master == p;
		}
	}
}
