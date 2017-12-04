using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public class DebugArena : WorldObjectComp
	{
		public List<Pawn> lhs;

		public List<Pawn> rhs;

		public Action<ArenaUtility.ArenaResult> callback;

		private int tickCreated;

		private int tickFightStarted;

		public DebugArena()
		{
			this.tickCreated = Find.TickManager.TicksGame;
		}

		public override void CompTick()
		{
			if (this.lhs == null || this.rhs == null)
			{
				Log.ErrorOnce("DebugArena improperly set up", 73785616);
				return;
			}
			if ((this.tickFightStarted == 0 && Find.TickManager.TicksGame - this.tickCreated > 10000) || (this.tickFightStarted != 0 && Find.TickManager.TicksGame - this.tickFightStarted > 60000))
			{
				Log.Message("Fight timed out");
				ArenaUtility.ArenaResult obj = default(ArenaUtility.ArenaResult);
				obj.tickDuration = Find.TickManager.TicksGame - this.tickCreated;
				obj.winner = ArenaUtility.ArenaResult.Winner.Other;
				this.callback(obj);
				Find.WorldObjects.Remove(this.parent);
				return;
			}
			if (this.tickFightStarted == 0)
			{
				foreach (Pawn current in this.lhs.Concat(this.rhs))
				{
					if (current.records.GetValue(RecordDefOf.ShotsFired) > 0f || (current.CurJob != null && current.CurJob.def == JobDefOf.AttackMelee && current.Position.DistanceTo(current.CurJob.targetA.Thing.Position) <= 2f))
					{
						this.tickFightStarted = Find.TickManager.TicksGame;
						break;
					}
				}
			}
			if (this.tickFightStarted != 0)
			{
				bool flag = !this.lhs.Any((Pawn pawn) => !pawn.Dead && !pawn.Downed);
				bool flag2 = !this.rhs.Any((Pawn pawn) => !pawn.Dead && !pawn.Downed);
				if (flag || flag2)
				{
					ArenaUtility.ArenaResult obj2 = default(ArenaUtility.ArenaResult);
					obj2.tickDuration = Find.TickManager.TicksGame - this.tickFightStarted;
					if (flag && !flag2)
					{
						obj2.winner = ArenaUtility.ArenaResult.Winner.Rhs;
					}
					else if (!flag && flag2)
					{
						obj2.winner = ArenaUtility.ArenaResult.Winner.Lhs;
					}
					else
					{
						obj2.winner = ArenaUtility.ArenaResult.Winner.Other;
					}
					this.callback(obj2);
					foreach (Pawn current2 in this.lhs.Concat(this.rhs))
					{
						if (!current2.Destroyed)
						{
							current2.Destroy(DestroyMode.Vanish);
						}
					}
					Find.WorldObjects.Remove(this.parent);
				}
			}
		}
	}
}
