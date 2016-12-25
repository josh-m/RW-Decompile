using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_NeedsTracker : IExposable
	{
		private Pawn pawn;

		private List<Need> needs = new List<Need>();

		public Need_Mood mood;

		public Need_Food food;

		public Need_Rest rest;

		public Need_Joy joy;

		public Need_Beauty beauty;

		public Need_Space space;

		public Need_Comfort comfort;

		public List<Need> AllNeeds
		{
			get
			{
				return this.needs;
			}
		}

		private bool ShouldTickNeeds
		{
			get
			{
				return this.pawn.Spawned || (this.pawn.holdingContainer != null && this.pawn.holdingContainer.owner is Pawn_CarryTracker) || this.pawn.IsCaravanMember();
			}
		}

		public Pawn_NeedsTracker()
		{
		}

		public Pawn_NeedsTracker(Pawn newPawn)
		{
			this.pawn = newPawn;
			this.AddOrRemoveNeedsAsAppropriate();
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Need>(ref this.needs, "needs", LookMode.Deep, new object[]
			{
				this.pawn
			});
			this.BindDirectNeedFields();
		}

		private void BindDirectNeedFields()
		{
			this.mood = this.TryGetNeed<Need_Mood>();
			this.food = this.TryGetNeed<Need_Food>();
			this.rest = this.TryGetNeed<Need_Rest>();
			this.joy = this.TryGetNeed<Need_Joy>();
			this.beauty = this.TryGetNeed<Need_Beauty>();
			this.comfort = this.TryGetNeed<Need_Comfort>();
			this.space = this.TryGetNeed<Need_Space>();
		}

		public void NeedsTrackerTick()
		{
			if (this.pawn.IsHashIntervalTick(150) && this.ShouldTickNeeds)
			{
				for (int i = 0; i < this.needs.Count; i++)
				{
					this.needs[i].NeedInterval();
				}
			}
		}

		public T TryGetNeed<T>() where T : Need
		{
			for (int i = 0; i < this.needs.Count; i++)
			{
				if (this.needs[i].GetType() == typeof(T))
				{
					return (T)((object)this.needs[i]);
				}
			}
			return (T)((object)null);
		}

		public Need TryGetNeed(NeedDef def)
		{
			for (int i = 0; i < this.needs.Count; i++)
			{
				if (this.needs[i].def == def)
				{
					return this.needs[i];
				}
			}
			return null;
		}

		public void SetInitialLevels()
		{
			for (int i = 0; i < this.needs.Count; i++)
			{
				this.needs[i].SetInitialLevel();
			}
		}

		public void AddOrRemoveNeedsAsAppropriate()
		{
			List<NeedDef> allDefsListForReading = DefDatabase<NeedDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				NeedDef needDef = allDefsListForReading[i];
				if (this.ShouldHaveNeed(needDef))
				{
					if (this.TryGetNeed(needDef) == null)
					{
						this.AddNeed(needDef);
					}
				}
				else if (this.TryGetNeed(needDef) != null)
				{
					this.RemoveNeed(needDef);
				}
			}
		}

		private bool ShouldHaveNeed(NeedDef nd)
		{
			if (this.pawn.RaceProps.intelligence < nd.minIntelligence)
			{
				return false;
			}
			if (nd.colonistsOnly && (this.pawn.Faction == null || !this.pawn.Faction.IsPlayer))
			{
				return false;
			}
			if (nd.colonistAndPrisonersOnly && (this.pawn.Faction == null || !this.pawn.Faction.IsPlayer) && (this.pawn.HostFaction == null || this.pawn.HostFaction != Faction.OfPlayer))
			{
				return false;
			}
			if (nd.onlyIfCausedByHediff && !this.pawn.health.hediffSet.hediffs.Any((Hediff x) => x.def.causesNeed == nd))
			{
				return false;
			}
			if (nd.neverOnPrisoner && this.pawn.IsPrisoner)
			{
				return false;
			}
			if (nd == NeedDefOf.Food)
			{
				return this.pawn.RaceProps.EatsFood;
			}
			return nd != NeedDefOf.Rest || this.pawn.RaceProps.needsRest;
		}

		private void AddNeed(NeedDef nd)
		{
			Need need = (Need)Activator.CreateInstance(nd.needClass, new object[]
			{
				this.pawn
			});
			need.def = nd;
			this.needs.Add(need);
			need.SetInitialLevel();
			this.BindDirectNeedFields();
		}

		private void RemoveNeed(NeedDef nd)
		{
			Need item = this.TryGetNeed(nd);
			this.needs.Remove(item);
			this.BindDirectNeedFields();
		}
	}
}
