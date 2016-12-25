using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	internal class Building_SunLamp : Building
	{
		public IEnumerable<IntVec3> GrowableCells
		{
			get
			{
				return GenRadial.RadialCellsAround(base.Position, this.def.specialDisplayRadius, true);
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo baseGizmo in base.GetGizmos())
			{
				yield return baseGizmo;
			}
			yield return new Command_Action
			{
				action = new Action(this.MakeMatchingGrowZone),
				hotKey = KeyBindingDefOf.Misc2,
				defaultDesc = "CommandSunLampMakeGrowingZoneDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Growing", true),
				defaultLabel = "CommandSunLampMakeGrowingZoneLabel".Translate()
			};
		}

		private void MakeMatchingGrowZone()
		{
			Designator_ZoneAdd_Growing designator = new Designator_ZoneAdd_Growing();
			designator.DesignateMultiCell(from tempCell in this.GrowableCells
			where designator.CanDesignateCell(tempCell).Accepted
			select tempCell);
		}
	}
}
