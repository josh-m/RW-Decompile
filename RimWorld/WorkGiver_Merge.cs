using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Merge : WorkGiver_Scanner
	{
		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerMergeables.ThingsPotentiallyNeedingMerging();
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.stackCount == t.def.stackLimit)
			{
				return null;
			}
			if (!HaulAIUtility.PawnCanAutomaticallyHaul(pawn, t, forced))
			{
				return null;
			}
			SlotGroup slotGroup = t.GetSlotGroup();
			if (slotGroup == null)
			{
				return null;
			}
			LocalTargetInfo target = t.Position;
			if (!pawn.CanReserve(target, 1, -1, null, forced))
			{
				return null;
			}
			foreach (Thing current in slotGroup.HeldThings)
			{
				if (current != t)
				{
					if (current.def == t.def)
					{
						if (forced || current.stackCount >= t.stackCount)
						{
							if (current.stackCount < current.def.stackLimit)
							{
								target = current.Position;
								if (pawn.CanReserve(target, 1, -1, null, forced))
								{
									if (pawn.CanReserve(current, 1, -1, null, false))
									{
										if (current.Position.IsValidStorageFor(current.Map, t))
										{
											return new Job(JobDefOf.HaulToCell, t, current.Position)
											{
												count = Mathf.Min(current.def.stackLimit - current.stackCount, t.stackCount),
												haulMode = HaulMode.ToCellStorage
											};
										}
									}
								}
							}
						}
					}
				}
			}
			JobFailReason.Is("NoMergeTarget".Translate(), null);
			return null;
		}
	}
}
