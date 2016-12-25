using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_AssaultColonySappers : LordToil
	{
		private static readonly FloatRange EscortRadiusRanged = new FloatRange(15f, 19f);

		private static readonly FloatRange EscortRadiusMelee = new FloatRange(23f, 26f);

		private LordToilData_AssaultColonySappers Data
		{
			get
			{
				return (LordToilData_AssaultColonySappers)this.data;
			}
		}

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		public LordToil_AssaultColonySappers()
		{
			this.data = new LordToilData_AssaultColonySappers();
		}

		public override void Init()
		{
			base.Init();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
			this.Data.sapperDest = GenAI.RandomRaidDest(base.Map);
		}

		public override void UpdateAllDuties()
		{
			List<Pawn> list = null;
			if (this.Data.sapperDest.IsValid)
			{
				list = new List<Pawn>();
				for (int i = 0; i < this.lord.ownedPawns.Count; i++)
				{
					Pawn pawn = this.lord.ownedPawns[i];
					if (this.IsValidSapper(pawn))
					{
						list.Add(pawn);
					}
				}
			}
			for (int j = 0; j < this.lord.ownedPawns.Count; j++)
			{
				Pawn pawn2 = this.lord.ownedPawns[j];
				if (this.IsValidSapper(pawn2))
				{
					pawn2.mindState.duty = new PawnDuty(DutyDefOf.Sapper, this.Data.sapperDest, -1f);
				}
				else if (!list.NullOrEmpty<Pawn>())
				{
					float randomInRange;
					if (pawn2.equipment != null && pawn2.equipment.Primary != null && pawn2.equipment.Primary.def.IsRangedWeapon)
					{
						randomInRange = LordToil_AssaultColonySappers.EscortRadiusRanged.RandomInRange;
					}
					else
					{
						randomInRange = LordToil_AssaultColonySappers.EscortRadiusMelee.RandomInRange;
					}
					pawn2.mindState.duty = new PawnDuty(DutyDefOf.Escort, list.RandomElement<Pawn>(), randomInRange);
				}
				else
				{
					pawn2.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
				}
			}
		}

		private bool IsValidSapper(Pawn p)
		{
			return this.Data.sapperDest.IsValid && p.equipment.Primary != null && p.equipment.Primary.def.Verbs[0].ai_IsBuildingDestroyer;
		}

		public override void Notify_ReachedDutyLocation(Pawn pawn)
		{
			this.Data.sapperDest = IntVec3.Invalid;
			this.UpdateAllDuties();
		}
	}
}
