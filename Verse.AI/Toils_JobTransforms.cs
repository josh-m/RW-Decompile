using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Verse.AI
{
	public static class Toils_JobTransforms
	{
		private static List<IntVec3> yieldedIngPlaceCells = new List<IntVec3>();

		public static Toil ExtractNextTargetFromQueue(TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				List<TargetInfo> targetQueue = curJob.GetTargetQueue(ind);
				if (targetQueue.NullOrEmpty<TargetInfo>())
				{
					return;
				}
				curJob.maxNumToCarry = curJob.numToBringList[0];
				curJob.SetTarget(ind, targetQueue[0].Thing);
				targetQueue.RemoveAt(0);
				curJob.numToBringList.RemoveAt(0);
			};
			return toil;
		}

		[DebuggerHidden]
		private static IEnumerable<IntVec3> IngredientPlaceCellsInOrder(IBillGiver billGiver)
		{
			Toils_JobTransforms.yieldedIngPlaceCells.Clear();
			IntVec3 interactCell = ((Thing)billGiver).InteractionCell;
			foreach (IntVec3 c3 in from c in billGiver.IngredientStackCells
			orderby (c - this.<interactCell>__0).LengthHorizontalSquared
			select c)
			{
				Toils_JobTransforms.yieldedIngPlaceCells.Add(c3);
				yield return c3;
			}
			for (int i = 0; i < 200; i++)
			{
				IntVec3 c2 = interactCell + GenRadial.RadialPattern[i];
				if (!Toils_JobTransforms.yieldedIngPlaceCells.Contains(c2))
				{
					Building ed = c2.GetEdifice();
					if (ed == null || ed.def.passability != Traversability.Impassable || ed.def.surfaceType != SurfaceType.None)
					{
						yield return c2;
					}
				}
			}
		}

		public static Toil SetTargetToIngredientPlaceCell(TargetIndex billGiverInd, TargetIndex carryItemInd, TargetIndex cellTargetInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(carryItemInd).Thing;
				IBillGiver billGiver = curJob.GetTarget(billGiverInd).Thing as IBillGiver;
				IntVec3 vec = IntVec3.Invalid;
				foreach (IntVec3 current in Toils_JobTransforms.IngredientPlaceCellsInOrder(billGiver))
				{
					if (!vec.IsValid)
					{
						vec = current;
					}
					bool flag = false;
					List<Thing> list = Find.ThingGrid.ThingsListAt(current);
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].def.category == ThingCategory.Item && (list[i].def != thing.def || list[i].stackCount == list[i].def.stackLimit))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						curJob.SetTarget(cellTargetInd, current);
						return;
					}
				}
				curJob.SetTarget(cellTargetInd, vec);
			};
			return toil;
		}
	}
}
