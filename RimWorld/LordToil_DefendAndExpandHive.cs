using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_DefendAndExpandHive : LordToil
	{
		public float distToHiveToAttack = 10f;

		private LordToilData_DefendAndExpandHive Data
		{
			get
			{
				return (LordToilData_DefendAndExpandHive)this.data;
			}
		}

		public LordToil_DefendAndExpandHive()
		{
			this.data = new LordToilData_DefendAndExpandHive();
		}

		public override void UpdateAllDuties()
		{
			this.FilterOutUnspawnedHives();
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Hive hiveFor = this.GetHiveFor(this.lord.ownedPawns[i]);
				PawnDuty duty = new PawnDuty(DutyDefOf.DefendAndExpandHive, hiveFor, this.distToHiveToAttack);
				this.lord.ownedPawns[i].mindState.duty = duty;
			}
		}

		private void FilterOutUnspawnedHives()
		{
			this.Data.assignedHives.RemoveAll((KeyValuePair<Pawn, Hive> x) => x.Value == null || !x.Value.Spawned);
		}

		private Hive GetHiveFor(Pawn pawn)
		{
			Hive hive;
			if (this.Data.assignedHives.TryGetValue(pawn, out hive))
			{
				return hive;
			}
			hive = this.FindClosestHive(pawn);
			if (hive != null)
			{
				this.Data.assignedHives.Add(pawn, hive);
			}
			return hive;
		}

		private Hive FindClosestHive(Pawn pawn)
		{
			return (Hive)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.Hive), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 30f, (Thing x) => x.Faction == pawn.Faction, null, 30, false);
		}
	}
}
