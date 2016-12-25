using System;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_TakeCombatEnhancingDrug : ThinkNode_JobGiver
	{
		private const int TakeEveryTicks = 20000;

		private bool onlyIfInDanger;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_TakeCombatEnhancingDrug jobGiver_TakeCombatEnhancingDrug = (JobGiver_TakeCombatEnhancingDrug)base.DeepCopy(resolve);
			jobGiver_TakeCombatEnhancingDrug.onlyIfInDanger = this.onlyIfInDanger;
			return jobGiver_TakeCombatEnhancingDrug;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.IsTeetotaler())
			{
				return null;
			}
			if (Find.TickManager.TicksGame - pawn.mindState.lastTakeCombatEnancingDrugTick < 20000)
			{
				return null;
			}
			Thing thing = this.FindCombatEnhancingDrug(pawn);
			if (thing == null)
			{
				return null;
			}
			if (this.onlyIfInDanger)
			{
				Lord lord = pawn.GetLord();
				if (lord == null)
				{
					if (!this.HarmedRecently(pawn))
					{
						return null;
					}
				}
				else
				{
					int num = 0;
					int num2 = Mathf.Clamp(lord.ownedPawns.Count / 2, 1, 4);
					for (int i = 0; i < lord.ownedPawns.Count; i++)
					{
						if (this.HarmedRecently(lord.ownedPawns[i]))
						{
							num++;
							if (num >= num2)
							{
								break;
							}
						}
					}
					if (num < num2)
					{
						return null;
					}
				}
			}
			return new Job(JobDefOf.Ingest, thing)
			{
				count = 1
			};
		}

		private bool HarmedRecently(Pawn pawn)
		{
			return Find.TickManager.TicksGame - pawn.mindState.lastHarmTick < 2500;
		}

		private Thing FindCombatEnhancingDrug(Pawn pawn)
		{
			for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
			{
				Thing thing = pawn.inventory.innerContainer[i];
				CompDrug compDrug = thing.TryGetComp<CompDrug>();
				if (compDrug != null)
				{
					if (compDrug.Props.isCombatEnhancingDrug)
					{
						return thing;
					}
				}
			}
			return null;
		}
	}
}
