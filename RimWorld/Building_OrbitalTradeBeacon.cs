using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_OrbitalTradeBeacon : Building
	{
		private const float TradeRadius = 7.9f;

		private static List<IntVec3> tradeableCells = new List<IntVec3>();

		public IEnumerable<IntVec3> TradeableCells
		{
			get
			{
				return Building_OrbitalTradeBeacon.TradeableCellsAround(base.Position, base.Map);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Resources>() != null)
			{
				yield return new Command_Action
				{
					action = new Action(this.MakeMatchingStockpile),
					hotKey = KeyBindingDefOf.Misc1,
					defaultDesc = "CommandMakeBeaconStockpileDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Stockpile", true),
					defaultLabel = "CommandMakeBeaconStockpileLabel".Translate()
				};
			}
		}

		private void MakeMatchingStockpile()
		{
			Designator des = DesignatorUtility.FindAllowedDesignator<Designator_ZoneAddStockpile_Resources>();
			des.DesignateMultiCell(from c in this.TradeableCells
			where des.CanDesignateCell(c).Accepted
			select c);
		}

		public static List<IntVec3> TradeableCellsAround(IntVec3 pos, Map map)
		{
			Building_OrbitalTradeBeacon.tradeableCells.Clear();
			if (!pos.InBounds(map))
			{
				return Building_OrbitalTradeBeacon.tradeableCells;
			}
			Region region = pos.GetRegion(map, RegionType.Set_Passable);
			if (region == null)
			{
				return Building_OrbitalTradeBeacon.tradeableCells;
			}
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.portal == null, delegate(Region r)
			{
				foreach (IntVec3 current in r.Cells)
				{
					if (current.InHorDistOf(pos, 7.9f))
					{
						Building_OrbitalTradeBeacon.tradeableCells.Add(current);
					}
				}
				return false;
			}, 13, RegionType.Set_Passable);
			return Building_OrbitalTradeBeacon.tradeableCells;
		}

		[DebuggerHidden]
		public static IEnumerable<Building_OrbitalTradeBeacon> AllPowered(Map map)
		{
			foreach (Building_OrbitalTradeBeacon b in map.listerBuildings.AllBuildingsColonistOfClass<Building_OrbitalTradeBeacon>())
			{
				CompPowerTrader power = b.GetComp<CompPowerTrader>();
				if (power == null || power.PowerOn)
				{
					yield return b;
				}
			}
		}
	}
}
