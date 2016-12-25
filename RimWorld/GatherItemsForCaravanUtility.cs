using System;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class GatherItemsForCaravanUtility
	{
		public static int CountLeftToTransfer(Pawn pawn, TransferableOneWay transferable)
		{
			if (transferable.countToTransfer <= 0 || !transferable.HasAnyThing)
			{
				return 0;
			}
			return Mathf.Max(transferable.countToTransfer - GatherItemsForCaravanUtility.TransferableCountHauledByOthers(pawn, transferable), 0);
		}

		private static int TransferableCountHauledByOthers(Pawn pawn, TransferableOneWay transferable)
		{
			if (!transferable.HasAnyThing)
			{
				Log.Warning("Can't determine transferable count hauled by others because transferable has 0 things.");
				return 0;
			}
			Lord lord = pawn.GetLord();
			int num = 0;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn2 = lord.ownedPawns[i];
				if (pawn2 != pawn)
				{
					if (pawn2.CurJob != null && pawn2.CurJob.def == JobDefOf.PrepareCaravan_GatherItems)
					{
						Thing toHaul = ((JobDriver_PrepareCaravan_GatherItems)pawn2.jobs.curDriver).ToHaul;
						if (transferable.things.Contains(toHaul) || TransferableUtility.TransferAsOne(transferable.AnyThing, toHaul))
						{
							num += toHaul.stackCount;
						}
					}
				}
			}
			return num;
		}
	}
}
