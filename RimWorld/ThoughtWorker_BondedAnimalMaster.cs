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
				DirectPawnRelation directPawnRelation = directRelations[i];
				Pawn otherPawn = directPawnRelation.otherPawn;
				if (directPawnRelation.def == PawnRelationDefOf.Bond && !otherPawn.Dead && otherPawn.Spawned && otherPawn.Faction == Faction.OfPlayer && otherPawn.training.IsCompleted(TrainableDefOf.Obedience) && p.skills.GetSkill(SkillDefOf.Animals).Level >= TrainableUtility.MinimumHandlingSkill(otherPawn) && this.AnimalMasterCheck(p, otherPawn))
				{
					return ThoughtState.ActiveWithReason(otherPawn.LabelShort);
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
