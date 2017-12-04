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

		IEnumerable<IntVec3> IPlantToGrowSettable.Cells
		{
			get
			{
				return base.Cells;
			}
		}

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
			Scribe_Defs.Look<ThingDef>(ref this.plantDefToGrow, "plantDefToGrow");
			Scribe_Values.Look<bool>(ref this.allowSow, "allowSow", true, false);
		}

		public override string GetInspectString()
		{
			string text = string.Empty;
			if (!base.Cells.NullOrEmpty<IntVec3>())
			{
				IntVec3 c = base.Cells.First<IntVec3>();
				if (c.UsesOutdoorTemperature(base.Map))
				{
					string text2 = text;
					text = string.Concat(new string[]
					{
						text2,
						"OutdoorGrowingPeriod".Translate(),
						": ",
						Zone_Growing.GrowingQuadrumsDescription(base.Map.Tile),
						"\n"
					});
				}
				if (GenPlant.GrowthSeasonNow(c, base.Map))
				{
					text += "GrowSeasonHereNow".Translate();
				}
				else
				{
					text += "CannotGrowBadSeasonTemperature".Translate();
				}
			}
			return text;
		}

		public static string GrowingQuadrumsDescription(int tile)
		{
			List<Twelfth> list = GenTemperature.TwelfthsInAverageTemperatureRange(tile, 10f, 42f);
			if (list.NullOrEmpty<Twelfth>())
			{
				return "NoGrowingPeriod".Translate();
			}
			if (list.Count == 12)
			{
				return "GrowYearRound".Translate();
			}
			return "PeriodDays".Translate(new object[]
			{
				list.Count * 5 + "/" + 60
			}) + " (" + QuadrumUtility.QuadrumsRangeLabel(list) + ")";
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
				isActive = (() => this.$this.allowSow),
				toggleAction = delegate
				{
					this.$this.allowSow = !this.$this.allowSow;
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
	}
}
