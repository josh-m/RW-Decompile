using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPawns : IExposable
	{
		private const float PctOfHumanlikesAlwaysKept = 0.1f;

		private const int TendIntervalTicks = 7500;

		private HashSet<Pawn> pawnsAlive = new HashSet<Pawn>();

		private HashSet<Pawn> pawnsDead = new HashSet<Pawn>();

		private HashSet<Pawn> pawnsForcefullyKeptAsWorldPawns = new HashSet<Pawn>();

		private Stack<Pawn> pawnsBeingDiscarded = new Stack<Pawn>();

		private static List<Pawn> tmpPawnsToTick = new List<Pawn>();

		public IEnumerable<Pawn> AllPawnsAliveOrDead
		{
			get
			{
				return this.pawnsAlive.Concat(this.pawnsDead);
			}
		}

		public IEnumerable<Pawn> AllPawnsAlive
		{
			get
			{
				return this.pawnsAlive;
			}
		}

		public IEnumerable<Pawn> AllPawnsDead
		{
			get
			{
				return this.pawnsDead;
			}
		}

		public void WorldPawnsTick()
		{
			WorldPawns.tmpPawnsToTick.Clear();
			foreach (Pawn current in this.pawnsAlive)
			{
				if (!current.Dead && !current.Destroyed)
				{
					WorldPawns.tmpPawnsToTick.Add(current);
				}
			}
			for (int i = 0; i < WorldPawns.tmpPawnsToTick.Count; i++)
			{
				WorldPawns.tmpPawnsToTick[i].Tick();
				if (!WorldPawns.tmpPawnsToTick[i].Dead && !WorldPawns.tmpPawnsToTick[i].Destroyed && WorldPawns.tmpPawnsToTick[i].IsHashIntervalTick(7500))
				{
					TendUtility.DoTend(null, WorldPawns.tmpPawnsToTick[i], null);
				}
			}
			WorldPawns.tmpPawnsToTick.Clear();
			foreach (Pawn current2 in this.pawnsAlive)
			{
				if (current2.Dead || current2.Destroyed)
				{
					if (current2.ThingState == ThingState.Discarded)
					{
						Log.Error("World pawn " + current2 + " has been discarded while still being a world pawn. This should never happen, because discard destroy mode means that the pawn is no longer managed by anything. Pawn should have been removed from the world first.");
					}
					else
					{
						this.pawnsDead.Add(current2);
					}
				}
			}
			this.pawnsAlive.RemoveWhere((Pawn x) => x.Dead || x.Destroyed);
		}

		public void ExposeData()
		{
			Scribe_Collections.LookHashSet<Pawn>(ref this.pawnsForcefullyKeptAsWorldPawns, true, "pawnsForcefullyKeptAsWorldPawns", LookMode.MapReference);
			Scribe_Collections.LookHashSet<Pawn>(ref this.pawnsAlive, "pawnsAlive", LookMode.Deep);
			Scribe_Collections.LookHashSet<Pawn>(ref this.pawnsDead, true, "pawnsDead", LookMode.Deep);
		}

		public bool Contains(Pawn p)
		{
			return this.pawnsAlive.Contains(p) || this.pawnsDead.Contains(p);
		}

		public void PassToWorld(Pawn pawn, PawnDiscardDecideMode discardMode = PawnDiscardDecideMode.Decide)
		{
			if (pawn.Spawned)
			{
				Log.Error("Tried to call PassToWorld with spawned pawn: " + pawn + ". Despawn him first.");
				return;
			}
			if (this.Contains(pawn))
			{
				Log.Error("Tried to pass pawn " + pawn + " to world, but it's already here.");
				return;
			}
			if (discardMode == PawnDiscardDecideMode.Keep && pawn.ThingState == ThingState.Discarded)
			{
				Log.Error("Tried to pass a discarded pawn " + pawn + " to world with discardMode=Keep. Discarded pawns should never be stored in WorldPawns.");
				discardMode = PawnDiscardDecideMode.Decide;
			}
			switch (discardMode)
			{
			case PawnDiscardDecideMode.Decide:
				if (this.ShouldKeep(pawn))
				{
					this.AddPawn(pawn);
				}
				else
				{
					this.DiscardPawn(pawn);
				}
				break;
			case PawnDiscardDecideMode.Keep:
				this.pawnsForcefullyKeptAsWorldPawns.Add(pawn);
				this.AddPawn(pawn);
				break;
			case PawnDiscardDecideMode.Discard:
				this.DiscardPawn(pawn);
				break;
			}
		}

		public void RemovePawn(Pawn p)
		{
			if (!this.Contains(p))
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to remove pawn ",
					p,
					" from ",
					base.GetType(),
					", but it's not here."
				}));
			}
			this.pawnsAlive.Remove(p);
			this.pawnsDead.Remove(p);
			this.pawnsForcefullyKeptAsWorldPawns.Remove(p);
		}

		public WorldPawnSituation GetSituation(Pawn p)
		{
			if (!this.Contains(p))
			{
				return WorldPawnSituation.None;
			}
			if (p.Dead || p.Destroyed)
			{
				return WorldPawnSituation.Dead;
			}
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (allFactionsListForReading[i].leader == p)
				{
					return WorldPawnSituation.FactionLeader;
				}
				if (allFactionsListForReading[i].kidnapped.KidnappedPawnsListForReading.Contains(p))
				{
					return WorldPawnSituation.Kidnapped;
				}
			}
			return WorldPawnSituation.Free;
		}

		public IEnumerable<Pawn> GetPawnsBySituation(WorldPawnSituation situation)
		{
			return from x in this.AllPawnsAliveOrDead
			where this.GetSituation(x) == situation
			select x;
		}

		public int GetPawnsBySituationCount(WorldPawnSituation situation)
		{
			int num = 0;
			foreach (Pawn current in this.pawnsAlive)
			{
				if (this.GetSituation(current) == situation)
				{
					num++;
				}
			}
			foreach (Pawn current2 in this.pawnsDead)
			{
				if (this.GetSituation(current2) == situation)
				{
					num++;
				}
			}
			return num;
		}

		public void DiscardIfUnimportant(Pawn pawn)
		{
			if (!this.Contains(pawn))
			{
				Log.Warning(pawn + " is not a world pawn.");
				return;
			}
			if (!this.ShouldKeep(pawn))
			{
				this.RemovePawn(pawn);
				this.DiscardPawn(pawn);
			}
		}

		public bool IsBeingDiscarded(Pawn p)
		{
			return this.pawnsBeingDiscarded.Contains(p);
		}

		public void LogWorldPawns()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= World Pawns =======");
			stringBuilder.AppendLine("Count: " + this.AllPawnsAliveOrDead.Count<Pawn>());
			WorldPawnSituation[] array = (WorldPawnSituation[])Enum.GetValues(typeof(WorldPawnSituation));
			for (int i = 0; i < array.Length; i++)
			{
				WorldPawnSituation worldPawnSituation = array[i];
				if (worldPawnSituation != WorldPawnSituation.None)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("== " + worldPawnSituation + " ==");
					foreach (Pawn current in from x in this.GetPawnsBySituation(worldPawnSituation)
					orderby (x.Faction != null) ? x.Faction.loadID : -1
					select x)
					{
						stringBuilder.AppendLine(string.Concat(new object[]
						{
							current.Name.ToStringFull,
							", ",
							current.KindLabel,
							", ",
							current.Faction
						}));
					}
				}
			}
			stringBuilder.AppendLine("===========================");
			Log.Message(stringBuilder.ToString());
		}

		private bool ShouldKeep(Pawn pawn)
		{
			if (pawn.ThingState == ThingState.Discarded)
			{
				return false;
			}
			if (pawn.records.GetAsInt(RecordDefOf.TimeAsColonist) > 0)
			{
				return true;
			}
			if (pawn.records.GetAsInt(RecordDefOf.TimeAsPrisoner) > 0)
			{
				return true;
			}
			if (pawn.corpse != null)
			{
				return true;
			}
			if (PawnGenerator.IsBeingGenerated(pawn))
			{
				return true;
			}
			if (this.pawnsForcefullyKeptAsWorldPawns.Contains(pawn))
			{
				return true;
			}
			if (!pawn.Dead && !pawn.Destroyed && pawn.RaceProps.Humanlike)
			{
				Rand.PushSeed();
				Rand.Seed = pawn.thingIDNumber * 681;
				bool flag = Rand.Value < 0.1f;
				Rand.PopSeed();
				if (flag)
				{
					return true;
				}
			}
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (allFactionsListForReading[i].leader == pawn)
				{
					return true;
				}
				if (allFactionsListForReading[i].kidnapped.KidnappedPawnsListForReading.Contains(pawn))
				{
					return true;
				}
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				if (Find.PlayLog.AnyEntryConcerns(pawn))
				{
					return true;
				}
				if (Find.TaleManager.AnyTaleConcerns(pawn))
				{
					return true;
				}
			}
			if (pawn.RaceProps.IsFlesh)
			{
				if (pawn.relations.RelatedPawns.Any((Pawn x) => x.relations.everSeenByPlayer))
				{
					return true;
				}
			}
			return false;
		}

		private void AddPawn(Pawn p)
		{
			if (p.Dead || p.Destroyed)
			{
				this.pawnsDead.Add(p);
			}
			else
			{
				this.pawnsAlive.Add(p);
			}
			if ((p.Faction == null || p.Faction.IsPlayer) && p.RaceProps.Humanlike && !p.Dead && this.GetSituation(p) != WorldPawnSituation.Kidnapped)
			{
				bool tryMedievalOrBetter = p.Faction != null && p.Faction.def.techLevel >= TechLevel.Medieval;
				Faction newFaction;
				if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out newFaction, tryMedievalOrBetter))
				{
					p.SetFaction(newFaction, null);
				}
			}
			p.ClearMind(false);
		}

		private void DiscardPawn(Pawn p)
		{
			this.pawnsBeingDiscarded.Push(p);
			try
			{
				if (!p.Destroyed)
				{
					p.Destroy(DestroyMode.Vanish);
				}
				if (p.ThingState != ThingState.Discarded)
				{
					p.Discard();
				}
			}
			finally
			{
				this.pawnsBeingDiscarded.Pop();
			}
		}
	}
}
