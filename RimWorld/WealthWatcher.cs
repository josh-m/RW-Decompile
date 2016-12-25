using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WealthWatcher
	{
		private const int MinCountInterval = 5000;

		private Map map;

		private float wealthItems;

		private float wealthBuildings;

		private int totalHealth;

		private float lastCountTick = -99999f;

		public int HealthTotal
		{
			get
			{
				this.RecountIfNeeded();
				return this.totalHealth;
			}
		}

		public float WealthTotal
		{
			get
			{
				this.RecountIfNeeded();
				return this.wealthItems + this.wealthBuildings;
			}
		}

		public float WealthItems
		{
			get
			{
				this.RecountIfNeeded();
				return this.wealthItems;
			}
		}

		public float WealthBuildings
		{
			get
			{
				this.RecountIfNeeded();
				return this.wealthBuildings;
			}
		}

		public WealthWatcher(Map map)
		{
			this.map = map;
		}

		private void RecountIfNeeded()
		{
			if ((float)Find.TickManager.TicksGame - this.lastCountTick > 5000f)
			{
				this.ForceRecount(false);
			}
		}

		public void ForceRecount(bool allowDuringInit = false)
		{
			if (!allowDuringInit && Current.ProgramState != ProgramState.Playing)
			{
				Log.Error("WealthWatcher recount in game mode " + Current.ProgramState);
				return;
			}
			this.wealthItems = 0f;
			this.wealthBuildings = 0f;
			this.totalHealth = 0;
			List<Thing> list = this.map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (!thing.Position.Fogged(thing.Map))
				{
					this.wealthItems += (float)thing.stackCount * thing.MarketValue;
				}
			}
			foreach (Pawn current in this.map.mapPawns.FreeColonists)
			{
				if (current.equipment != null)
				{
					foreach (ThingWithComps current2 in current.equipment.AllEquipment)
					{
						this.wealthItems += current2.MarketValue;
					}
				}
				if (current.apparel != null)
				{
					List<Apparel> wornApparel = current.apparel.WornApparel;
					for (int j = 0; j < wornApparel.Count; j++)
					{
						this.wealthItems += wornApparel[j].MarketValue;
					}
				}
			}
			List<Thing> list2 = this.map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
			for (int k = 0; k < list2.Count; k++)
			{
				Thing thing2 = list2[k];
				if (thing2.Faction == Faction.OfPlayer)
				{
					this.wealthBuildings += thing2.MarketValue;
					this.totalHealth += thing2.HitPoints;
				}
			}
			foreach (Pawn current3 in this.map.mapPawns.FreeColonists)
			{
				this.totalHealth += Mathf.RoundToInt(current3.health.summaryHealth.SummaryHealthPercent * 100f);
			}
			this.lastCountTick = (float)Find.TickManager.TicksGame;
		}
	}
}
