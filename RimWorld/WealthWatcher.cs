using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WealthWatcher
	{
		private Map map;

		private float wealthItems;

		private float wealthBuildings;

		private int totalHealth;

		private float lastCountTick = -99999f;

		private const int MinCountInterval = 5000;

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
				this.wealthItems += WealthWatcher.GetEquipmentApparelAndInventoryWealth(current);
			}
			List<Thing> list2 = this.map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
			for (int j = 0; j < list2.Count; j++)
			{
				ThingOwner resourceContainer = ((Frame)list2[j]).resourceContainer;
				for (int k = 0; k < resourceContainer.Count; k++)
				{
					this.wealthItems += (float)resourceContainer[k].stackCount * resourceContainer[k].MarketValue;
				}
			}
			List<Thing> list3 = this.map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
			for (int l = 0; l < list3.Count; l++)
			{
				Thing thing2 = list3[l];
				if (thing2.Faction == Faction.OfPlayer)
				{
					this.wealthBuildings += thing2.MarketValue;
					this.totalHealth += thing2.HitPoints;
				}
			}
			foreach (Pawn current2 in this.map.mapPawns.FreeColonists)
			{
				this.totalHealth += Mathf.RoundToInt(current2.health.summaryHealth.SummaryHealthPercent * 100f);
			}
			this.lastCountTick = (float)Find.TickManager.TicksGame;
		}

		public static float GetEquipmentApparelAndInventoryWealth(Pawn p)
		{
			float num = 0f;
			if (p.equipment != null)
			{
				List<ThingWithComps> allEquipmentListForReading = p.equipment.AllEquipmentListForReading;
				for (int i = 0; i < allEquipmentListForReading.Count; i++)
				{
					num += allEquipmentListForReading[i].MarketValue * (float)allEquipmentListForReading[i].stackCount;
				}
			}
			if (p.apparel != null)
			{
				List<Apparel> wornApparel = p.apparel.WornApparel;
				for (int j = 0; j < wornApparel.Count; j++)
				{
					num += wornApparel[j].MarketValue * (float)wornApparel[j].stackCount;
				}
			}
			if (p.inventory != null)
			{
				ThingOwner<Thing> innerContainer = p.inventory.innerContainer;
				for (int k = 0; k < innerContainer.Count; k++)
				{
					num += innerContainer[k].MarketValue * (float)innerContainer[k].stackCount;
				}
			}
			return num;
		}
	}
}
