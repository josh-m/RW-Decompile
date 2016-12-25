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
				return Building_OrbitalTradeBeacon.TradeableCellsAround(base.Position);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			yield return new Command_Action
			{
				action = new Action(this.MakeMatchingStockpile),
				hotKey = KeyBindingDefOf.Misc1,
				defaultDesc = "CommandMakeBeaconStockpileDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Stockpile", true),
				defaultLabel = "CommandMakeBeaconStockpileLabel".Translate()
			};
		}

		private void MakeMatchingStockpile()
		{
			Designator_ZoneAddStockpile_Resources des = new Designator_ZoneAddStockpile_Resources();
			des.DesignateMultiCell(from c in this.TradeableCells
			where des.CanDesignateCell(c).Accepted
			select c);
		}

		public static List<IntVec3> TradeableCellsAround(IntVec3 pos)
		{
			Building_OrbitalTradeBeacon.tradeableCells.Clear();
			if (!pos.InBounds())
			{
				return Building_OrbitalTradeBeacon.tradeableCells;
			}
			Region region = pos.GetRegion();
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
			}, 12);
			return Building_OrbitalTradeBeacon.tradeableCells;
		}

		[DebuggerHidden]
		public static IEnumerable<Building_OrbitalTradeBeacon> AllPowered()
		{
			foreach (Building_OrbitalTradeBeacon b in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_OrbitalTradeBeacon>())
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
