using System;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class DangerWatcher
	{
		private Map map;

		private StoryDanger dangerRatingInt;

		private int lastUpdateTick = -10000;

		private int lastColonistHarmedTick = -10000;

		private const int UpdateInterval = 101;

		public StoryDanger DangerRating
		{
			get
			{
				if (Find.TickManager.TicksGame > this.lastUpdateTick + 101)
				{
					this.dangerRatingInt = this.CalculateDangerRating();
					this.lastUpdateTick = Find.TickManager.TicksGame;
				}
				return this.dangerRatingInt;
			}
		}

		public DangerWatcher(Map map)
		{
			this.map = map;
		}

		private StoryDanger CalculateDangerRating()
		{
			float num = (from x in this.map.attackTargetsCache.TargetsHostileToColony
			where this.AffectsStoryDanger(x)
			select x).Sum((IAttackTarget t) => (!(t is Pawn)) ? 0f : ((Pawn)t).kindDef.combatPower);
			if (num == 0f)
			{
				return StoryDanger.None;
			}
			int num2 = (from p in this.map.mapPawns.FreeColonistsSpawned
			where !p.Downed
			select p).Count<Pawn>();
			if (num < 150f && num <= (float)num2 * 18f)
			{
				return StoryDanger.Low;
			}
			if (num > 400f)
			{
				return StoryDanger.High;
			}
			if (this.lastColonistHarmedTick > Find.TickManager.TicksGame - 900)
			{
				return StoryDanger.High;
			}
			foreach (Lord current in this.map.lordManager.lords)
			{
				if (current.faction.HostileTo(Faction.OfPlayer) && current.CurLordToil.ForceHighStoryDanger && current.AnyActivePawn)
				{
					return StoryDanger.High;
				}
			}
			return StoryDanger.Low;
		}

		public void Notify_ColonistHarmedExternally()
		{
			this.lastColonistHarmedTick = Find.TickManager.TicksGame;
		}

		private bool AffectsStoryDanger(IAttackTarget t)
		{
			Pawn pawn = t.Thing as Pawn;
			if (pawn != null)
			{
				Lord lord = pawn.GetLord();
				if (lord != null && lord.LordJob is LordJob_DefendPoint && pawn.CurJobDef != JobDefOf.AttackMelee && pawn.CurJobDef != JobDefOf.AttackStatic)
				{
					return false;
				}
			}
			return GenHostility.IsActiveThreatToPlayer(t);
		}
	}
}
