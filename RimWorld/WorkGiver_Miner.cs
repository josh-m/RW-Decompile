using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Miner : WorkGiver_Scanner
	{
		private static string NoPathTrans;

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public static void Reset()
		{
			WorkGiver_Miner.NoPathTrans = "NoPath".Translate();
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation des in pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Mine))
			{
				bool mayBeAccessible = false;
				for (int j = 0; j < 8; j++)
				{
					IntVec3 c = des.target.Cell + GenAdj.AdjacentCells[j];
					if (c.InBounds(pawn.Map) && c.Walkable(pawn.Map))
					{
						mayBeAccessible = true;
						break;
					}
				}
				if (mayBeAccessible)
				{
					Mineable i = des.target.Cell.GetFirstMineable(pawn.Map);
					if (i != null)
					{
						yield return i;
					}
				}
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!t.def.mineable)
			{
				return null;
			}
			if (pawn.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.Mine) == null)
			{
				return null;
			}
			if (!pawn.CanReserve(t, 1, -1, null, false))
			{
				return null;
			}
			bool flag = false;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = t.Position + GenAdj.AdjacentCells[i];
				if (intVec.InBounds(pawn.Map) && intVec.Standable(pawn.Map) && ReachabilityImmediate.CanReachImmediate(intVec, t, pawn.Map, PathEndMode.Touch, pawn))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < 8; j++)
				{
					IntVec3 intVec2 = t.Position + GenAdj.AdjacentCells[j];
					if (intVec2.InBounds(t.Map))
					{
						if (ReachabilityImmediate.CanReachImmediate(intVec2, t, pawn.Map, PathEndMode.Touch, pawn))
						{
							if (intVec2.Walkable(t.Map) && !intVec2.Standable(t.Map))
							{
								Thing thing = null;
								List<Thing> thingList = intVec2.GetThingList(t.Map);
								for (int k = 0; k < thingList.Count; k++)
								{
									if (thingList[k].def.designateHaulable && thingList[k].def.passability == Traversability.PassThroughOnly)
									{
										thing = thingList[k];
										break;
									}
								}
								if (thing != null)
								{
									Job job = HaulAIUtility.HaulAsideJobFor(pawn, thing);
									if (job != null)
									{
										return job;
									}
									JobFailReason.Is(WorkGiver_Miner.NoPathTrans);
									return null;
								}
							}
						}
					}
				}
				JobFailReason.Is(WorkGiver_Miner.NoPathTrans);
				return null;
			}
			return new Job(JobDefOf.Mine, t, 1500, true);
		}
	}
}
