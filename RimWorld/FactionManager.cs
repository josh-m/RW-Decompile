using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class FactionManager : IExposable
	{
		private List<Faction> allFactions = new List<Faction>();

		public List<Faction> AllFactionsListForReading
		{
			get
			{
				return this.allFactions;
			}
		}

		public IEnumerable<Faction> AllFactions
		{
			get
			{
				return this.allFactions;
			}
		}

		public IEnumerable<Faction> AllFactionsVisible
		{
			get
			{
				return from fa in this.allFactions
				where !fa.def.hidden
				select fa;
			}
		}

		public IEnumerable<Faction> AllFactionsInViewOrder
		{
			get
			{
				return (from x in this.AllFactionsVisible
				orderby x.defeated
				select x).ThenByDescending((Faction fa) => fa.def.startingGoodwill.Average);
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Faction>(ref this.allFactions, "allFactions", LookMode.Deep, new object[0]);
		}

		public void Add(Faction faction)
		{
			this.allFactions.Add(faction);
		}

		public void FactionManagerTick()
		{
			for (int i = 0; i < this.allFactions.Count; i++)
			{
				this.allFactions[i].FactionTick();
			}
		}

		public void FactionsDebugDrawOnMap()
		{
			if (DebugViewSettings.drawFactions)
			{
				for (int i = 0; i < this.allFactions.Count; i++)
				{
					this.allFactions[i].DebugDrawOnMap();
				}
			}
		}

		public Faction FirstFactionOfDef(FactionDef facDef)
		{
			for (int i = 0; i < this.allFactions.Count; i++)
			{
				if (this.allFactions[i].def == facDef)
				{
					return this.allFactions[i];
				}
			}
			return null;
		}

		public Faction FactionAtTile(int tileID)
		{
			List<FactionBase> factionBases = Find.WorldObjects.FactionBases;
			for (int i = 0; i < factionBases.Count; i++)
			{
				if (factionBases[i].Tile == tileID)
				{
					return factionBases[i].Faction;
				}
			}
			return null;
		}

		public bool TryGetRandomNonColonyHumanlikeFaction(out Faction faction, bool tryMedievalOrBetter, bool allowDefeated = false)
		{
			IEnumerable<Faction> source = from x in this.AllFactions
			where x != Faction.OfPlayer && !x.def.hidden && x.def.humanlikeFaction && (allowDefeated || !x.defeated)
			select x;
			return source.TryRandomElementByWeight(delegate(Faction x)
			{
				if (tryMedievalOrBetter && x.def.techLevel < TechLevel.Medieval)
				{
					return 0.1f;
				}
				return 1f;
			}, out faction);
		}

		public Faction RandomEnemyFaction(bool allowHidden = false, bool allowDefeated = false)
		{
			return (from x in this.AllFactions
			where (allowHidden || !x.def.hidden) && (allowDefeated || !x.defeated) && x.HostileTo(Faction.OfPlayer)
			select x).RandomElement<Faction>();
		}

		public void LogKidnappedPawns()
		{
			Log.Message("Kidnapped pawns:");
			for (int i = 0; i < this.allFactions.Count; i++)
			{
				this.allFactions[i].kidnapped.LogKidnappedPawns();
			}
		}
	}
}
