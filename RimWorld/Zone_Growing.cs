using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Zone_Growing : Zone, IPlantToGrowSettable
	{
		private ThingDef plantDefToGrow = ThingDefOf.PlantPotato;

		public bool allowSow = true;

		public override bool IsMultiselectable
		{
			get
			{
				return true;
			}
		}

		protected override Color NextZoneColor
		{
			get
			{
				return ZoneColorUtility.NextGrowingZoneColor();
			}
		}

		public Zone_Growing()
		{
		}

		public Zone_Growing(ZoneManager zoneManager) : base("GrowingZone".Translate(), zoneManager)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<ThingDef>(ref this.plantDefToGrow, "plantDefToGrow");
			Scribe_Values.LookValue<bool>(ref this.allowSow, "allowSow", true, false);
		}

		public override string GetInspectString()
		{
			string text = string.Empty;
			if (!base.Cells.NullOrEmpty<IntVec3>())
			{
				IntVec3 c = base.Cells.First<IntVec3>();
				if (c.UsesOutdoorTemperature(base.Map))
				{
					text = text + "OutdoorGrowingPeriod".Translate() + ": " + Zone_Growing.GrowingMonthsDescription(base.Map.Tile);
				}
				if (GenPlant.GrowthSeasonNow(c, base.Map))
				{
					text = text + "\n" + "GrowSeasonHereNow".Translate();
				}
				else
				{
					text = text + "\n" + "CannotGrowBadSeasonTemperature".Translate();
				}
			}
			return text;
		}

		public static string GrowingMonthsDescription(int tile)
		{
			List<Month> list = GenTemperature.MonthsInTemperatureRange(tile, 10f, 42f);
			if (list.NullOrEmpty<Month>())
			{
				return "NoGrowingPeriod".Translate();
			}
			if (list.Count == 12)
			{
				return "GrowYearRound".Translate();
			}
			return "PeriodDays".Translate(new object[]
			{
				list.Count * 5
			}) + " (" + SeasonUtility.SeasonsRangeLabel(list) + ")";
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			yield return PlantToGrowSettableUtility.SetPlantToGrowCommand(this);
			yield return new Command_Toggle
			{
				defaultLabel = "CommandAllowSow".Translate(),
				defaultDesc = "CommandAllowSowDesc".Translate(),
				hotKey = KeyBindingDefOf.CommandItemForbid,
				icon = TexCommand.Forbidden,
				isActive = (() => this.<>f__this.allowSow),
				toggleAction = delegate
				{
					this.<>f__this.allowSow = !this.<>f__this.allowSow;
				}
			};
		}

		public ThingDef GetPlantDefToGrow()
		{
			return this.plantDefToGrow;
		}

		public void SetPlantDefToGrow(ThingDef plantDef)
		{
			this.plantDefToGrow = plantDef;
		}

		public bool CanAcceptSowNow()
		{
			return true;
		}

		virtual Map get_Map()
		{
			return base.Map;
		}
	}
}
